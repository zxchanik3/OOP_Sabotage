using System;
using System.Collections.Generic;
using System.Numerics;

namespace lab
{
    public enum Button
    {
        Forward,
        Backward,
        Left,
        Right
    }

    public class Driver
    {
        public string Name { get; set; } = "Unknown";
        public int Number { get; set; }
        public string Team { get; set; } = "Independent";
        public int Wins { get; set; } = 0;
        public int Races { get; set; } = 0;
        public int Podiums { get; set; } = 0;
        public int Position { get; set; }
        public bool Lock { get; set; }

        public Driver() { }

        public Driver(string name, int number, bool lockStatus)
        {
            Name = name;
            Number = number;
            Lock = lockStatus;
        }

        public void SetTeam(string team)
        {
            Team = team;
        }

        public void AddRaceResult()
        {
            if (Position == 1) Wins++;
            if (Position <= 3) Podiums++;
            Races++;
        }

        public virtual void Drive(Car car, float dT) { }
    }

    public class UserDriver : Driver
    {
        private Dictionary<Button, bool> ButtonState = new Dictionary<Button, bool>();

        public UserDriver(string name, int number)
        {
            Name = name;
            Number = number;
            foreach (Button b in Enum.GetValues(typeof(Button)))
                ButtonState[b] = false;
        }

        public void Press(Button button)
        {
            if (ButtonState.ContainsKey(button)) ButtonState[button] = true;
        }

        public void Release(Button button)
        {
            if (ButtonState.ContainsKey(button)) ButtonState[button] = false;
        }

        public override void Drive(Car car, float dT)
        {
            if (car == null) return;

            float accelInput = 0f;
            float turnInput = 0f;

            if (ButtonState[Button.Forward]) accelInput += 1f;
            if (ButtonState[Button.Backward]) accelInput -= 1f;
            if (ButtonState[Button.Left]) turnInput -= 1f;
            if (ButtonState[Button.Right]) turnInput += 1f;

            car.UpdateSpeed(accelInput, dT);
            car.UpdateDirection(dT, turnInput);
            car.Move(dT);
        }
    }

    public class NPCDriver : Driver
    {
        private IDriveStrategy strategy;
        private RaceContext raceContext;

        public NPCDriver(string name, int number, IDriveStrategy strategy)
        {
            Name = name;
            Number = number;
            Lock = true;
            this.strategy = strategy;
            raceContext = new RaceContext();
        }

        public void SetStrategy(IDriveStrategy newStrategy)
        {
            strategy = newStrategy;
        }

        public override void Drive(Car car, float dT)
        {
            if (car == null) return;
            strategy.Drive(car, dT, raceContext.TimeToOpponent, raceContext.CurrentSegment);
        }
    }

    public interface IDriveStrategy
    {
        void Drive(Car car, float dT, float timeToOpponent, TrackSegment trackSegment);
    }

    public class AttackStrategy : IDriveStrategy
    {
        public void Drive(Car car, float dT, float timeToOpponent, TrackSegment trackSegment)
        {
            float accelInput = 1f;
            float turnInput = 0f;

            if (trackSegment.Type == SegmentType.Corner)
            {
                accelInput = 0.8f;
                turnInput = 0.7f;
            }

            car.UpdateSpeed(accelInput, dT);
            car.UpdateDirection(dT, turnInput);
            car.Move(dT);
        }
    }

    public class NormalStrategy : IDriveStrategy
    {
        public void Drive(Car car, float dT, float timeToOpponent, TrackSegment trackSegment)
        {
            float accelInput = 0.8f;
            float turnInput = 0f;

            if (trackSegment.Type == SegmentType.Corner)
            {
                accelInput = 0.6f;
                turnInput = 0.5f;
            }

            car.UpdateSpeed(accelInput, dT);
            car.UpdateDirection(dT, turnInput);
            car.Move(dT);
        }
    }

    public class DefenseStrategy : IDriveStrategy
    {
        public void Drive(Car car, float dT, float timeToOpponent, TrackSegment trackSegment)
        {
            float accelInput = 0.6f;
            float turnInput = 0f;

            if (trackSegment.Type == SegmentType.Corner)
            {
                accelInput = 0.4f;
                turnInput = 0.3f;
            }

            car.UpdateSpeed(accelInput, dT);
            car.UpdateDirection(dT, turnInput);
            car.Move(dT);
        }
    }

}
