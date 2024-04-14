namespace Checkers.Tools.Models
{
    public class GamePiece : Entity
    {
        public Colors Color { get; }

        public Position CurrentPosition { get; set; }

        public List<Position> Moves { get; set; }

        protected GamePiece(Colors color) : base(Guid.NewGuid())
        {
            Color = color;
        }
    }
}