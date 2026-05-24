using System.Windows;
using System.Windows.Threading;

namespace lab
{
    public class PitStopManager
    {
        private readonly Rect _pitArea = new(350, 470, 170, 50);
        private readonly DispatcherTimer _pitTimer = new();
        private int _pitTimeLeft;

        public bool IsPitServing { get; private set; }
        public bool HasBeenServed { get; set; }
        public bool AutoPitLimiter { get; set; } = true;

        public event Action<int>? PitTick;
        public event Action? PitCompleted;

        public PitStopManager()
        {
            _pitTimer.Interval = TimeSpan.FromMilliseconds(100);
            _pitTimer.Tick += (_, _) =>
            {
                _pitTimeLeft--;
                PitTick?.Invoke(_pitTimeLeft);
                if (_pitTimeLeft <= 0)
                {
                    _pitTimer.Stop();
                    IsPitServing = false;
                    HasBeenServed = true;
                    PitCompleted?.Invoke();
                }
            };
        }

        public void HandlePitStop(Car car, int carIndex)
        {
            var carPoint = new Point(car.Position.X, car.Position.Y);
            if (_pitArea.Contains(carPoint))
            {
                if (carIndex == 0) // Логіка гравця
                {
                    if (AutoPitLimiter && car.Speed > 40f) car.Speed -= 2f;
                    if (!IsPitServing && !HasBeenServed && car.Speed < 40f)
                    {
                        IsPitServing = true;
                        _pitTimeLeft = 20;
                        _pitTimer.Start();
                    }
                }
                else // Швидкий обслуговування ботів
                {
                    car.Speed *= 0.95f;
                    if (car.Speed < 50f) car.ChangeTyres(TyreType.Medium);
                }
            }
            else if (carIndex == 0)
            {
                HasBeenServed = false;
            }
        }

        public void Reset()
        {
            _pitTimer.Stop();
            IsPitServing = false;
            HasBeenServed = false;
        }
    }
}