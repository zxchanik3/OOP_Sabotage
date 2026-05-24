using System.Numerics;

namespace lab
{
    public interface IDriveStrategy
    {
        CarInput GetInput(Car car, float dT);
    }

    public class NpcDriver : Driver
    {
        private IDriveStrategy _strategy;

        public NpcDriver(string name, int number, IDriveStrategy startStrategy)
        {
            Name = name;
            Number = number;
            Lock = true;
            _strategy = startStrategy;
        }

        public override CarInput GetInput(Car car, float dT)
        {
            return _strategy.GetInput(car, dT);
        }
    }

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

        public virtual CarInput GetInput(Car car, float dT)
        {
            CarInput input = new CarInput();

            if (_waypoints.Count == 0)
                return input;

            Vector2 target = _waypoints[_currentWpIndex];
            float dist = Vector2.Distance(car.Position, target);

            if (dist < 50f)
            {
                _currentWpIndex = (_currentWpIndex + 1) % _waypoints.Count;
                target = _waypoints[_currentWpIndex];
            }

            Vector2 toTarget = Vector2.Normalize(target - car.Position);
            float cross = (car.Direction.X * toTarget.Y) - (car.Direction.Y * toTarget.X);
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

    public class SmartStrategy : BaseWaypointStrategy
    {
        private List<Vector2> _mainWaypoints;
        private List<Vector2> _pitWaypoints;
        private bool _isPitting;

        public SmartStrategy(List<Vector2> mainWaypoints, List<Vector2> pitWaypoints, int startIndex = 1) 
            : base(mainWaypoints, startIndex)
        {
            _mainWaypoints = mainWaypoints;
            _pitWaypoints = pitWaypoints;
            SpeedModifier = 0.85f; 
            BrakeSensitivity = 0.90f; 
        }

        public override CarInput GetInput(Car car, float dT)
        {
            if (car.Tyres.Durability < 30 && !_isPitting && _pitWaypoints.Count > 0)
            {
                _isPitting = true;
                _waypoints = _pitWaypoints;
                _currentWpIndex = 0;
            }
            
            if (_isPitting && car.Tyres.Durability > 90)
            {
                _isPitting = false;
                _waypoints = _mainWaypoints;
                _currentWpIndex = GetClosest(car.Position, _mainWaypoints);
            }

            SpeedModifier = _isPitting ? 0.5f : 0.85f;

            return base.GetInput(car, dT);
        }

        private int GetClosest(Vector2 pos, List<Vector2> wps)
        {
            int best = 0;
            float bestDist = float.MaxValue;
            for (int i = 0; i < wps.Count; i++)
            {
                float d = Vector2.Distance(pos, wps[i]);
                if (d < bestDist) { bestDist = d; best = i; }
            }
            return best;
        }
    }

    public class DumbStrategy : BaseWaypointStrategy
    {
        public DumbStrategy(List<Vector2> waypoints, int startIndex = 1) 
            : base(waypoints, startIndex)
        {
            SpeedModifier = 1.0f; 
            BrakeSensitivity = -1f; 
        }
    }
}