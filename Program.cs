using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Media;

namespace TetrisGame
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new TetrisGame());
        }
    }

    public class TetrisGame : Form 
    {
        private Timer timer;
        private const int GridWidth = 10;
        private const int GridHeight = 20;
        private const int CellSize = 30;
        private int[,] grid = new int[GridWidth, GridHeight];
        private Tetromino currentTetromino;
        private Grid Grid;
        private bool GameState;
        

        public TetrisGame() 
        {
            this.Text = "Tetris";
            this.ClientSize = new Size(GridWidth * CellSize, GridHeight * CellSize);
            this.DoubleBuffered = true;

            Grid = new Grid(GridWidth, GridHeight);
            SpawnNewTetromino();

            timer = new Timer();
            timer.Interval = 500;
            timer.Tick += UpdateGame;
            timer.Start();

            this.Paint += DrawGame;

            if (!GameState) 
            {
                NewGame();
            }
        }

        private void SpawnNewTetromino() 
        {
            currentTetromino = TetrominoFactory.GetRandomTetromino();
            if(currentTetromino == null)
            {
                throw new Exception("Tetronomifactory returned null!");
            }
            currentTetromino.X = Grid.Width / 2 - currentTetromino.Shape.GetLength(0) / 2;
            currentTetromino.Y = 0;

            // check for gameover state
            if (!CanPlaceTetromino(currentTetromino)) 
            {
                timer.Stop();
                MessageBox.Show("Game Over!");
            }
        }

        private MessageBoxButtons NewGame()
        {
            var msgBtn = MessageBoxButtons.YesNoCancel;
            return msgBtn;
        }

        private bool CanPlaceTetromino(Tetromino tetromino)
        {
            for(int x = 0; x < tetromino.Shape.GetLength(0); x++)
            {
                for(int y = 0; y < tetromino.Shape.GetLength(1); y++)
                {
                    if (tetromino.Shape[x,y] != 0)
                    {
                        int gridX = tetromino.X + x;
                        int gridY = tetromino.Y + y;

                        if (gridX < 0 || gridX >= Grid.Width || gridY < 0 || gridY >= Grid.Height)
                        {
                            return false;
                        }
                        if (Grid.Cells[gridX, gridY]!= 0)
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }


        private void UpdateGame(object sender, EventArgs e) 
        {
            currentTetromino.Y++;

            if (!CanPlaceTetromino(currentTetromino))
            {
                currentTetromino.Y--;
                PlaceTetrominoInGrid();
                SpawnNewTetromino();
            }
            Invalidate();
        }

        private void DrawGame(object sender, PaintEventArgs e) 
        {
            Graphics g = e.Graphics;

            for(int i = 0; i < GridWidth; i++) 
            {
                for (int j = 0; j < GridHeight; j++)
                {
                    Rectangle rect = new Rectangle(i * CellSize, j * CellSize, CellSize, CellSize);
                    // if the row is full, we draw the cells
                    if (Grid.Cells[i, j] != 0)
                    {
                        g.FillRectangle(currentTetromino.Color, rect);
                    }
                    else
                    {
                        g.FillRectangle(Brushes.Black, rect);

                        g.DrawRectangle(Pens.White, rect);
                    }
                }
            }

            if(currentTetromino != null)
            {
                for(int x = 0; x < currentTetromino.Shape.GetLength(0); x++)
                {
                    for(int y = 0; y < currentTetromino.Shape.GetLength(1); y++)
                    {
                        if (currentTetromino.Shape[x, y] != 0)
                        {
                            int gridX = currentTetromino.X + x;
                            int gridY = currentTetromino.Y + y;
                            if (gridY >= 0 && gridY < GridHeight)
                            {
                                Rectangle rect = new Rectangle(gridX * CellSize, gridY * CellSize, CellSize, CellSize);
                                g.FillRectangle(currentTetromino.Color, rect);
                                g.DrawRectangle(Pens.White, rect);
                            }
                        }
                    }
                }
            }
        }

        public bool PlaceTetromino(Grid grid, Tetromino tetromino)
        {
            for(int x = 0; x < tetromino.Shape.GetLength(0); x++)
            {
                for(int y = 0; y < tetromino.Shape.GetLength(1);y++)
                {
                    if (tetromino.Shape[x,y] != 0)
                    {
                        int gridX = tetromino.X + x;
                        int gridY = tetromino.Y + y;
                        // check for collision
                        if(gridX < 0 || gridX >= grid.Width || gridY >= grid.Height || grid.Cells[gridX, gridY] != 0)
                        {
                            return false;
                        }
                    }
                }
            }

            // if there is no collision, we can add a tetris block
            for(int x = 0; x < tetromino.Shape.GetLength(0); x++)
            {
                for(int y = 0; y < tetromino.Shape.GetLength(1); y++)
                {
                    if (tetromino.Shape[x,y] != 0)
                    {
                        grid.Cells[tetromino.X + x, tetromino.Y + y] = tetromino.Shape[x, y];
                    }
                }
            }
            return true;
        }

        private void PlaceTetrominoInGrid()
        {
            for(int x = 0; x < currentTetromino.Shape.GetLength(0); x++)
            {
                for(int y = 0; y < currentTetromino.Shape.GetLength(1);y++)
                {
                    if (currentTetromino.Shape[x,y] != 0)
                    {
                        int gridX = currentTetromino.X + x;
                        int gridY = currentTetromino.Y + y;
                        Grid.Cells[gridX, gridY] = currentTetromino.Shape[x, y];

                        if (gridX >= 0 && gridX < GridWidth && gridY >= 0 && gridY < GridHeight)
                        {
                            grid[gridX, gridY] = 1; // cell filled
                        }
                    }
                }
            }

            for(int y = 0; y < Grid.Height; y++)
            {
                if (Grid.IsRowFull(y))
                {
                    Grid.ClearRow(y);
                }
            }
            ClearFullRows();
        }

        

        private void PaintTetronomiWhenStacked()
        {
            // todo: implement this method
            for(int x = 0; x < currentTetromino.Shape.GetLength(0); x++)
            {
                for(int y = 0; y < currentTetromino.Shape.GetLength(1); y++)
                {
                    if (currentTetromino.Shape[x,y] != 0)
                    {
                        int gridX = currentTetromino.X + x;
                        int gridY = currentTetromino.Y + y;
                        Grid.Cells[gridX, gridY] = currentTetromino.Shape[x, y];
                    }
                }
            }
        }

        private void ClearFullRows()
        {
            bool isRowFull = true;
            for (int y = Grid.Height - 1; y >= 0; y--)
            {

                for (int x = 0; x < Grid.Width; x++)
                {
                    if (grid[x, y] == 0)
                    {
                        isRowFull = false; break;
                    }
                }


                if (isRowFull)
                {
                    for (int yy = y; yy > 0; yy--)
                    {
                        for(int x = 0; x < Grid.Width; x++)
                        {
                            grid[x, yy] = grid[x, yy -1];
                        }
                    }

                    for(int x = 0; x < Grid.Width; x++)
                    {
                        grid[x, 0] = 0;
                    }
                }
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if(currentTetromino == null)
            {
                return;
            }
            switch (e.KeyCode)
            {
                case Keys.Left:
                    currentTetromino.X--;
                    if (!CanPlaceTetromino(currentTetromino))
                    {
                        currentTetromino.X++;
                    }
                    break;
                case Keys.Right:
                    currentTetromino.X++;
                    if (!CanPlaceTetromino(currentTetromino))
                    {
                        currentTetromino.X--;
                    }
                    break;
                case Keys.Down:
                    currentTetromino.Y++;
                    if (!CanPlaceTetromino(currentTetromino))
                    {
                        currentTetromino.Y--;
                    }
                    break;
                case Keys.Up:
                    if(currentTetromino.CanRotate(Grid))
                    {
                        currentTetromino.Rotate();
                    }
                    break;
            }
            this.Invalidate();
        }

    }

    public class Grid
    {
        public int[,] Cells { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }

        public Grid(int width, int heigth)
        {
            Width = width;
            Height = heigth;
            Cells = new int[width, heigth];
        }

        public bool IsRowFull(int row)
        {
            for (int i = 0; i < Width; i++)
            {
                if (Cells[i, row] == 0)
                {
                    return false;
                }

            }
            return true;
        }

        public void ClearRow(int row)
        {
            for (int i = 0; i < Width; i++)
            {
                Cells[i, row] = 0;

                for (int j = 0; j > 0; j--)
                {
                    for (int k = 0; k < Width; k++)
                    {
                        Cells[i, k] = Cells[i, j - 1];
                    }
                }
            }
        }
    
    }


    

    public class Tetromino
    {
        public int[,] Shape { get; private set; }
        public int X { get; set; }
        public int Y { get; set; }
        public Brush Color { get; private set; }

        public Tetromino(int[,] shape, Brush color)
        {
            Shape = shape;
            Color = color;
            X = 0;
            Y = 0;
        }

        public bool CanRotate(Grid grid)
        {
            int size = Shape.GetLength(0);
            int[,] rotated = new int[size, size];

            for(int x = 0; x < size; x++)
            {
                for(int y = 0; y < size; y++)
                {
                    rotated[x, y] = Shape[size - y - 1, x];
                }
            }

            for(int x = 0; x < size; x++)
            {
                for(int y = 0; y < size; y++)
                {
                    if (rotated[x,y] != 0)
                    {
                        int gridX = X + x;
                        int gridY = Y + y;

                        if(gridX < 0 || gridX >= grid.Width || gridY < 0 || gridY >= grid.Height)
                        {
                            return false;
                        }
                        // check for collisions
                        if (grid.Cells[gridX, gridY] != 0)
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        public void Rotate()
        {
            int size = Shape.GetLength(0);
            int[,] rotated = new int[size, size];
            for(int x = 0; x < size; x++)
            {
                for(int y = 0; y < size; y++)
                {
                    rotated[x, y] = Shape[size - y - 1, x];
                }
            }
            Shape = rotated;
        }
    }

    public static class TetrominoFactory
    {
        public static Tetromino GetRandomTetromino()
        {
            Random random = new Random();
            int type = random.Next(7);
            switch(type)
            {
                case 0:
                    return new Tetromino(new int[,] { { 1, 1 }, { 1, 1 } }, Brushes.Yellow); // O
                case 1:
                    return new Tetromino(new int[,] { { 0, 1, 0 }, { 1, 1, 1 } }, Brushes.Purple); // T
                case 2:
                    return new Tetromino(new int[,] { { 1, 1, 0 }, { 0, 1, 1 } }, Brushes.Green); // S
                case 3:
                    return new Tetromino(new int[,] { { 0, 1, 1 }, { 1, 1, 0 } }, Brushes.Cyan); // Z
                case 4:
                    return new Tetromino(new int[,] { {1,1,1,1 } }, Brushes.Orange); // I
                case 5:
                    return new Tetromino(new int[,] { { 1, 0, 0 }, { 1, 1, 1 } }, Brushes.Blue); // L
                case 6:
                    return new Tetromino(new int[,] { { 0, 0, 1 }, { 1, 1, 1 } }, Brushes.Red); // J
            }
            return null;
        }
    }
}
