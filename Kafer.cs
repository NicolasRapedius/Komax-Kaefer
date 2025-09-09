namespace Komax_Kaefer
{
    public enum Direction
    {
        Up,
        Right,
        Down,
        Left
    }

    public class Kafer
    {
        public int X { get; set; }
        public int Y { get; set; }
        public Direction Richtung { get; set; }

        private readonly int width;
        private readonly int height;
        private readonly bool[,] grid;

        public Kafer(int startX, int startY, Direction startRichtung, bool[,] grid)
        {
            X = startX;
            Y = startY;
            Richtung = startRichtung;
            this.grid = grid;
            width = grid.GetLength(0);
            height = grid.GetLength(1);
        }

        public void Schritt()
        {
            if (!grid[X, Y])
            {
                grid[X, Y] = true; // Schwarz f‰rben
                Richtung = (Direction)(((int)Richtung + 1) % 4); // Rechts drehen
            }
            else
            {
                grid[X, Y] = false; // Weiﬂ f‰rben
                Richtung = (Direction)(((int)Richtung + 3) % 4); // Links drehen
            }

            switch (Richtung)
            {
                case Direction.Up:
                    Y = (Y - 1 + height) % height;
                    break;
                case Direction.Right:
                    X = (X + 1) % width;
                    break;
                case Direction.Down:
                    Y = (Y + 1) % height;
                    break;
                case Direction.Left:
                    X = (X - 1 + width) % width;
                    break;
            }
        }
    }
}
