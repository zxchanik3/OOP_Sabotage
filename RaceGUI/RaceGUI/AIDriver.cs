using System;
using System.Numerics;

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
    public class WaypointStrategy : IDriveStrategy
    {
        private List<Vector2> _waypoints;
        private int _currentWpIndex = 0;

        public WaypointStrategy(List<Vector2> waypoints, int startIndex = 1)
        {
            _waypoints = waypoints;
            _currentWpIndex = startIndex;
        }

        public void Drive(Car car, float dT, float timeToOpponent, TrackSegment segment)
        {
            if (_waypoints == null || _waypoints.Count == 0) return;

            Vector2 target = _waypoints[_currentWpIndex];
            float dist = Vector2.Distance(car.Position, target);

            // Збільшуємо радіус зарахування точки до 40 пікселів, бо машини швидкі
            if (dist < 40f)
            {
                _currentWpIndex = (_currentWpIndex + 1) % _waypoints.Count;
                target = _waypoints[_currentWpIndex];
            }

            Vector2 toTarget = Vector2.Normalize(target - car.Position);
            float crossProduct = (car.Direction.X * toTarget.Y) - (car.Direction.Y * toTarget.X);
    
            // Скалярний добуток: показує, чи ціль попереду (ближче до 1), чи збоку/позаду (ближче до 0 чи мінус)
            float dotProduct = Vector2.Dot(car.Direction, toTarget);

            // Кермування (що більший crossProduct, то сильніше крутимо кермо)
            float turnInput = Math.Clamp(crossProduct * 4f, -1f, 1f); 

            // ЛОГІКА ГАЗУ ТА ГАЛЬМ
            float accelInput = 1.0f;

            // Якщо поворот занадто крутий (ми дивимося в бік від точки, dotProduct < 0.85)
            if (dotProduct < 0.97f)
            {
                // Тиснемо на гальма! Передаємо мінусове значення в UpdateSpeed
                accelInput = -1.0f; 
            }
            else
            {
                // Повний газ на прямій
                accelInput = 1.0f; 
            }

            // Запобіжник: якщо бот уже сильно загальмував (швидкість менше 60), 
            // даємо трохи газу, щоб він не зупинився повністю посеред траси
            if (car.Speed < 60f && accelInput < 0)
            {
                accelInput = 0.3f; 
            }

            car.UpdateSpeed(accelInput, dT);
            car.UpdateDirection(dT, turnInput);
            car.Move(dT);
        }
    }
}
