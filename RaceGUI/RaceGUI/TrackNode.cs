using System.Numerics;

namespace lab
{
    public class TrackNode
    {
        public Vector2 Position { get; }
        public TrackSegment Logic { get; }

        public TrackNode(Vector2 position, TrackSegment logic)
        {
            Position = position;
            Logic = logic;
        }
    }
}