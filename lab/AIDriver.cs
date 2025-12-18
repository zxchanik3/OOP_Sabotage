using System;

namespace lab
{
    public interface IDriveStrategy
    {
        void Drive(Car car, float dT, float timeToOpponent, TrackSegment trackSegment);
    }

    public class NPCDriver : Driver
    {
        private IDriveStrategy strategy;
        private RaceContext raceContext;

        public NPCDriver(string name, int number, IDriveStrategy startStrategy)
        {
            Name = name;
            Number = number;
            Lock = true;

            strategy = startStrategy;
            raceContext = new RaceContext();
        }

        public void SetStrategy(IDriveStrategy newStrategy)
        {
            if (newStrategy != null)
                strategy = newStrategy;
        }

        public override void Drive(Car car, float dT)
        {
            if (car == null || strategy == null) return;
            if (raceContext.CurrentSegment == null) return;

            strategy.Drive(
                car,
                dT,
                raceContext.TimeToOpponent,
                raceContext.CurrentSegment
            );
        }

        public void UpdateContext(RaceContext context)
        {
            raceContext = context;
        }
    }

    public class AttackStrategy : IDriveStrategy
    {
        public void Drive(Car car, float dT, float timeToOpponent, TrackSegment segment)
        {
            float accel = 1.0f;
            float turn = 0.0f;

            if (segment.Type == SegmentType.Corner)
            {
                accel = 0.8f;
                turn = 0.7f;
            }

            car.UpdateSpeed(accel, dT);
            car.UpdateDirection(dT, turn);
            car.Move(dT);
        }
    }

    public class NormalStrategy : IDriveStrategy
    {
        public void Drive(Car car, float dT, float timeToOpponent, TrackSegment segment)
        {
            float accel = 0.8f;
            float turn = 0.0f;

            if (segment.Type == SegmentType.Corner)
            {
                accel = 0.6f;
                turn = 0.5f;
            }

            car.UpdateSpeed(accel, dT);
            car.UpdateDirection(dT, turn);
            car.Move(dT);
        }
    }

    public class DefenseStrategy : IDriveStrategy
    {
        public void Drive(Car car, float dT, float timeToOpponent, TrackSegment segment)
        {
            float accel = 0.6f;
            float turn = 0.0f;

            if (segment.Type == SegmentType.Corner)
            {
                accel = 0.4f;
                turn = 0.3f;
            }

            car.UpdateSpeed(accel, dT);
            car.UpdateDirection(dT, turn);
            car.Move(dT);
        }
    }
}
