using System;

namespace lab
{
    public class RaceContext
    {
        public float TimeToOpponent { get; set; }
        public TrackSegment CurrentSegment { get; set; }

        public RaceContext()
        {
            TimeToOpponent = 0;
            CurrentSegment = null;
        }
    }
}

