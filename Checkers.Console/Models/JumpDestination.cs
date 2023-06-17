namespace Checkers.Console.Models
{
    public class JumpDestination
    {
        public JumpDestination(int x, int y, int xNext, int yNext)
        {
            X = x;
            Y = y;
            XNext = xNext;
            YNext = yNext;
        }

        public int X { get; private set; }
        public int Y { get; private set; }
        public int XNext { get; private set; }
        public int YNext { get; private set; }
    }
}