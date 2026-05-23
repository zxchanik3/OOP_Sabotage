using System;
using System.Collections.Generic;
using System.Numerics;

namespace lab
{
    public interface IDriveStrategy
    {
        CarInput GetInput(
            Car car,
            float dT,
            float timeToOpponent,
            TrackSegment trackSegment
        );
    }

    // =========================
    // NPC DRIVER (без змін фізики)
    // =========================
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

        public void UpdateContext(RaceContext context)
        {
            raceContext = context;
        }

        public override CarInput GetInput(Car car, float dT)
        {
            if (car == null || strategy == null)
                return new CarInput();

            return strategy.GetInput(
                car,
                dT,
                raceContext.TimeToOpponent,
                raceContext.CurrentSegment
            );
        }
    }

    // =========================
    // BASE WAYPOINT STRATEGY
    // =========================
    public class BaseWaypointStrategy : IDriveStrategy
    {
        protected List<Vector2> _waypoints;
        protected int _currentWpIndex;

        protected float SpeedModifier = 1.0f;
        protected float BrakeSensitivity = 0.95f;

        public BaseWaypointStrategy(List<Vector2> waypoints, int startIndex = 1)
        {
            _waypoints = waypoints;
            _currentWpIndex = startIndex;
        }

        public virtual CarInput GetInput(
            Car car,
            float dT,
            float timeToOpponent,
            TrackSegment trackSegment)
        {
            CarInput input = new CarInput();

            if (_waypoints == null || _waypoints.Count == 0)
                return input;

            Vector2 target = _waypoints[_currentWpIndex];

            float dist = Vector2.Distance(car.Position, target);

            if (dist < 50f)
            {
                _currentWpIndex = (_currentWpIndex + 1) % _waypoints.Count;
                target = _waypoints[_currentWpIndex];
            }

            Vector2 toTarget = Vector2.Normalize(target - car.Position);

            float cross =
                (car.Direction.X * toTarget.Y) -
                (car.Direction.Y * toTarget.X);

            float dot = Vector2.Dot(car.Direction, toTarget);

            input.Steering = Math.Clamp(cross * 4.5f, -1f, 1f);

            if (dot < BrakeSensitivity)
            {
                input.Brake = 0.5f;
                input.Throttle = 0f;
            }
            else
            {
                input.Throttle = SpeedModifier;
                input.Brake = 0f;
            }

            return input;
        }
    }

    // =========================
    // ATTACK STRATEGY
    // =========================
    public class AttackStrategy : BaseWaypointStrategy
    {
        public AttackStrategy(List<Vector2> waypoints, int startIndex = 1)
            : base(waypoints, startIndex)
        {
            SpeedModifier = 1.15f;
            BrakeSensitivity = 0.92f;
        }

        public override CarInput GetInput(
            Car car,
            float dT,
            float timeToOpponent,
            TrackSegment trackSegment)
        {
            CarInput input =
                base.GetInput(car, dT, timeToOpponent, trackSegment);

            // агресивна поведінка біля суперника
            if (timeToOpponent > 0 && timeToOpponent < 1.5f)
            {
                input.Brake *= 0.7f;
                input.Throttle *= 1.1f;
            }

            // знос шин через агресію
            if (car.Tyres != null && car.Speed > 50f)
            {
                car.Tyres.WearDown(car.Speed, dT);
            }

            return input;
        }
    }

    // =========================
    // NORMAL STRATEGY
    // =========================
    public class NormalStrategy : BaseWaypointStrategy
    {
        public NormalStrategy(List<Vector2> waypoints, int startIndex = 1)
            : base(waypoints, startIndex)
        {
            SpeedModifier = 1.0f;
            BrakeSensitivity = 0.96f;
        }
    }

    // =========================
    // DEFENSE STRATEGY
    // =========================
    public class DefenseStrategy : BaseWaypointStrategy
    {
        public DefenseStrategy(List<Vector2> waypoints, int startIndex = 1)
            : base(waypoints, startIndex)
        {
            SpeedModifier = 0.9f;
            BrakeSensitivity = 0.98f;
        }

        public override CarInput GetInput(
            Car car,
            float dT,
            float timeToOpponent,
            TrackSegment trackSegment)
        {
            CarInput input =
                base.GetInput(car, dT, timeToOpponent, trackSegment);

            // обережніше при близьких суперниках
            if (timeToOpponent > 0 && timeToOpponent < 1.5f)
            {
                input.Brake += 0.2f;
            }

            return input;
        }
    }
}