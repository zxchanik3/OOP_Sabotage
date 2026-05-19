using System;
using System.Collections.Generic;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

using lab;

namespace RaceGUI
{
    public partial class MainWindow : Window
    {
        private DispatcherTimer _timer;
        private List<Car> _cars;
        private List<Driver> _drivers;
        private Dictionary<Car, Rectangle> _carVisuals;
        private UserDriver _userDriver;
        private Dictionary<Car, int> _carLaps = new();
        private Dictionary<Car, int> _carLastWaypoint = new();
        private bool _isRaceFinished = false;
        private Rect _pitArea = new Rect(350, 470, 170, 50); 
        private DispatcherTimer _pitTimer; // Окремий таймер для обслуговування
        private bool _isPitServing = false; // Чи обслуговується машина в цей момент
        private int _pitTimeLeft = 0;
        private bool _hasBeenServed = false;
        private TrackSegment _lastSegment = null;
        private double _targetSpeed = 1000;
        
        // Для відслідковування поточного сегменту кожного боліда (потрібно для вашого ШІ)
        private Dictionary<Car, int> _carSegmentIndices;
        private Track _track;

        public MainWindow()
        {
            InitializeComponent();
            InitializeGame();
        }
        
        private void TrackSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (GameCanvas == null || TrackSelector.SelectedItem == null) return;

            string selectedTrack = ((ComboBoxItem)TrackSelector.SelectedItem).Content.ToString();
            ImageBrush bgBrush = new ImageBrush { Stretch = Stretch.Fill };

            if (selectedTrack.Contains("Winter"))
            {
                bgBrush.ImageSource = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/Images/WinterMap.jpg"));
            }
            else if (selectedTrack.Contains("Forest"))
            {
                bgBrush.ImageSource = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/Images/ForestMap.jpg"));
            }

            GameCanvas.Background = bgBrush;
        }
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
        
        private int GetClosestNodeIndex(Vector2 pos, out float distance)
        {
            int bestIdx = 0;
            distance = float.MaxValue;
            for (int i = 0; i < _trackNodes.Count; i++)
            {
                float d = Vector2.Distance(pos, _trackNodes[i].Position);
                if (d < distance) 
                { 
                    distance = d; 
                    bestIdx = i; 
                }
            }
            return bestIdx;
        }

        private List<TrackNode> _trackNodes;

        private void InitializeGame()
        {
            _cars = new List<Car>();
            _drivers = new List<Driver>();
            _carVisuals = new Dictionary<Car, Rectangle>();
            _carSegmentIndices = new Dictionary<Car, int>();
            
            
            StraightSegment s = new StraightSegment(100);
            CornerSegment c = new CornerSegment(50.0, 2);

            _trackNodes = new List<TrackNode>
            {
                new TrackNode(new Vector2(552, 560), s), new TrackNode(new Vector2(593, 559), s),
                new TrackNode(new Vector2(637, 562), s), new TrackNode(new Vector2(680, 562), s),
                new TrackNode(new Vector2(731, 562), s), new TrackNode(new Vector2(780, 560), s),
                new TrackNode(new Vector2(823, 546), c), new TrackNode(new Vector2(859, 516), c),
                new TrackNode(new Vector2(888, 481), c), new TrackNode(new Vector2(915, 435), c),
                new TrackNode(new Vector2(933, 389), c), new TrackNode(new Vector2(934, 344), c),
                new TrackNode(new Vector2(900, 322), c), new TrackNode(new Vector2(844, 324), c),
                new TrackNode(new Vector2(806, 350), c), new TrackNode(new Vector2(775, 381), c),
                new TrackNode(new Vector2(732, 403), c), new TrackNode(new Vector2(691, 382), c),
                new TrackNode(new Vector2(676, 330), c), new TrackNode(new Vector2(688, 273), c),
                new TrackNode(new Vector2(724, 222), c), new TrackNode(new Vector2(772, 189), c),
                new TrackNode(new Vector2(825, 151), c), new TrackNode(new Vector2(848, 88), c),
                new TrackNode(new Vector2(799, 42), c), new TrackNode(new Vector2(728, 32), s),
                new TrackNode(new Vector2(654, 38), s), new TrackNode(new Vector2(591, 54), s),
                new TrackNode(new Vector2(540, 88), s), new TrackNode(new Vector2(527, 146), c),
                new TrackNode(new Vector2(542, 198), c), new TrackNode(new Vector2(560, 261), c),
                new TrackNode(new Vector2(560, 318), c), new TrackNode(new Vector2(517, 355), c),
                new TrackNode(new Vector2(460, 341), c), new TrackNode(new Vector2(432, 287), c),
                new TrackNode(new Vector2(412, 223), c), new TrackNode(new Vector2(384, 166), c),
                new TrackNode(new Vector2(339, 116), c), new TrackNode(new Vector2(280, 97), c),
                new TrackNode(new Vector2(207, 110), c), new TrackNode(new Vector2(154, 134), c),
                new TrackNode(new Vector2(124, 177), c), new TrackNode(new Vector2(107, 238), c),
                new TrackNode(new Vector2(124, 304), c), new TrackNode(new Vector2(164, 338), c),
                new TrackNode(new Vector2(198, 382), c), new TrackNode(new Vector2(187, 429), c),
                new TrackNode(new Vector2(169, 483), c), new TrackNode(new Vector2(189, 536), c),
                new TrackNode(new Vector2(244, 556), c), new TrackNode(new Vector2(304, 559), c),
                new TrackNode(new Vector2(362, 559), s), new TrackNode(new Vector2(416, 561), s),
                new TrackNode(new Vector2(467, 561), s), new TrackNode(new Vector2(509, 561), s),
                new TrackNode(new Vector2(380, 494), s), new TrackNode(new Vector2(432, 493), s),
                new TrackNode(new Vector2(482, 493), s)
            };

            Vector2 startPos = _trackNodes[0].Position;
            Vector2 startDir = Vector2.Normalize(_trackNodes[1].Position - _trackNodes[0].Position);

            TrackBuilder builder = new TrackBuilder();
            _track = builder.SetName("Winter GP").SetLaps(5).AddStartFinish(1.0).Build();

            _userDriver = new UserDriver("Користувач (Ти)", 77);
            
            var startContext = new RaceContext { CurrentSegment = _track.Segments[0], TimeToOpponent = 1f };
            
            var userCar = new Car("Player Car", "Your Team", 2026, 1000, 25, 280, 798);

            userCar.ChangeTyres(TyreType.Soft);
            
            userCar.SetPosition(new Vector2(startPos.X, startPos.Y - 15), startDir);

            _drivers.Add(_userDriver);
            _cars.Add(userCar);

            for (int i = 0; i < _cars.Count; i++)
            {
                var car = _cars[i];
                Rectangle rect = new Rectangle
                {
                    Width = 24, Height = 14,
                    Fill = Brushes.Red,
                    Stroke = Brushes.Black, StrokeThickness = 1
                };
                _carVisuals[car] = rect;
                GameCanvas.Children.Add(rect);
            }
            _pitTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(100) }; // Тікає кожні 0.1 секунди
            _pitTimer.Tick += PitTimer_Tick; // Додаємо обробник
            
            _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(16) };
            _timer.Tick += Timer_Tick;
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            float dT = 0.1f;

            // Оскільки ми очистили код від ботів, у нас завжди тільки один водій і одна машина
            if (_drivers.Count == 0) return;
            var driver = _userDriver;
            var car = _cars[0];

            if (_isRaceFinished) return;

            // --- ОБРОБЛЯЄМО КЕРУВАННЯ ТА РУХ (Тільки якщо не обслуговуємося) ---
            if (!_isPitServing)
            {
                driver.Drive(car, dT);
            }
            else
            {
                // Під час обслуговування машина примусово гальмує
                car.UpdateSpeed(-2f, dT);
                car.Move(dT);
            }

            // --- ОНОВЛЮЄМО ГРАФІКУ (Тільки червона машинка) ---
            Rectangle rect = _carVisuals[car];
            double x = car.Position.X - rect.Width / 2;
            double y = car.Position.Y - rect.Height / 2;

            Canvas.SetLeft(rect, x);
            Canvas.SetTop(rect, y);

            double angle = Math.Atan2(car.Direction.Y, car.Direction.X) * (180 / Math.PI);
            rect.RenderTransform = new RotateTransform(angle, rect.Width / 2, rect.Height / 2);

            // --- ЛОГІКА ТРАСИ: СИСТЕМА КОЛО + ФІНІШ ---
            // (Ми це реалізували раніше, я залишаю цей код)
            if (!_carLastWaypoint.ContainsKey(car)) _carLastWaypoint[car] = 0;
            if (!_carLaps.ContainsKey(car)) _carLaps[car] = 0;

            // Логіка підрахунку кіл
            int closestWp = GetClosestNodeIndex(car.Position, out float distToWp);
            int lastWp = _carLastWaypoint[car];
            TrackNode currentNode = _trackNodes[closestWp];

            // 1. ДЕТЕКТОР ЗОН: Викликаємо бекенд ТІЛЬКИ в момент переходу (з прямої в поворот або навпаки)
            if (currentNode.Logic != _lastSegment && !_isPitServing)
            {
                _lastSegment = currentNode.Logic; // Запам'ятовуємо, що ми в новій зоні
                double tempSpeed = car.Speed;

                // Бекенд відпрацьовує рівно 1 раз на весь довгий поворот!
                currentNode.Logic.ApplyEffect(car, ref tempSpeed);

                _targetSpeed = tempSpeed; // Отримуємо ліміт для цієї ділянки
            }

            // 2. ПЛАВНЕ ГАЛЬМУВАННЯ: Якщо після розрахунку бекенду ми летимо зашвидко
            if (car.Speed > _targetSpeed && !_isPitServing)
            {
                // Машину "душить" електроніка, поки швидкість не впаде до безпечної
                car.UpdateSpeed(-2.0f, dT); 
            }

            if (distToWp > 45f)
            {
                car.UpdateSpeed(-1.5f, dT);
            }

            if (lastWp >= _trackNodes.Count - 8 && closestWp <= 5)
            {
                _carLaps[car]++;
                _carLastWaypoint[car] = closestWp; 

                if (_carLaps[car] >= _track.RequiredLapCount)
                {
                    _isRaceFinished = true;
                    _timer.Stop();
                    return;
                }
            }
            else if (Math.Abs(closestWp - lastWp) < 10) 
            {
                _carLastWaypoint[car] = closestWp;
            }

            // --- === НОВА ЛОГІКА: ЗАЇЗД НА ПІТ-СТОП === ---
            // Перевіряємо, чи ми вже обслуговуємося
            if (_isPitServing) return;

            Point carPoint = new System.Windows.Point(car.Position.X, car.Position.Y);
            if (_pitArea.Contains(carPoint))
            {
                // Додали перевірку: !_hasBeenServed
                if (!_hasBeenServed && car.Speed < 50f)
                {
                    _isPitServing = true; 
                    TxtPitStatus.Visibility = Visibility.Visible; 
                    car.UpdateSpeed(-2f, dT); 

                    _pitTimeLeft = 20; 
                    _pitTimer.Start(); 
                }
            }
            else
            {
                _hasBeenServed = false; 
            }
            int currentLap = _carLaps.ContainsKey(_cars[0]) ? _carLaps[_cars[0]] + 1 : 1;
            int speed = (int)_cars[0].Speed;
            int tyreDurability = (int)_cars[0].Tyres.Durability;

            TxtStatus.Text = $"Коло: {currentLap}/{_track.RequiredLapCount} | Швидкість: {speed} км/год | Шини: {tyreDurability}%";
        }
        private void PitTimer_Tick(object? sender, EventArgs e)
        {
            _pitTimeLeft--;

            if (_pitTimeLeft <= 0)
            {
                // === ОБСЛУГОВУВАННЯ ЗАВЕРШЕНО! ===
                _pitTimer.Stop(); // Зупиняємо таймер піт-стопу
                _isPitServing = false; // Машина знову вільна
                TxtPitStatus.Visibility = Visibility.Collapsed; // Приховуємо текст

                // ВІДНОВЛЮЄМО ШИНИ!
                // Ми беремо машину гравця (_cars[0]) і викликаємо ChangeTyres.
                // Ми використовуємо її поточний тип гуми, але це відновить Durability до 100%.
                _hasBeenServed = true;
                _cars[0].ChangeTyres(_cars[0].Tyres.Type);
            }
        }

        // Обробка натискань кнопок інтерфейсу
        private void BtnStart_Click(object sender, RoutedEventArgs e)
        {
            TrackSelector.IsEnabled = false; 
            this.Focus();
            
            _timer.Start();
            TxtStatus.Text = "Статус: Гонка активована!";
        }

        private void BtnStop_Click(object sender, RoutedEventArgs e)
        {
            _timer.Stop();
            
            TrackSelector.IsEnabled = true;
            TxtStatus.Text = "Статус: Пауза";
        }

        // Обробка керування з клавіатури (WASD або стрілочки)
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (_userDriver == null) return;
            if (_isPitServing) return;
            
            switch (e.Key)
            {
                case Key.W: case Key.Up:
                    _userDriver.Press(lab.Button.Forward);
                    break;
                case Key.S: case Key.Down:
                    _userDriver.Press(lab.Button.Backward);
                    break;
                case Key.A: case Key.Left:
                    _userDriver.Press(lab.Button.Left);
                    break;
                case Key.D: case Key.Right:
                    _userDriver.Press(lab.Button.Right);
                    break;
            }
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (_userDriver == null) return;

            switch (e.Key)
            {
                case Key.W: case Key.Up:
                    _userDriver.Release(lab.Button.Forward);
                    break;
                case Key.S: case Key.Down:
                    _userDriver.Release(lab.Button.Backward);
                    break;
                case Key.A: case Key.Left:
                    _userDriver.Release(lab.Button.Left);
                    break;
                case Key.D: case Key.Right:
                    _userDriver.Release(lab.Button.Right);
                    break;
            }
        }
        private void GameCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // Отримуємо координати кліку відносно нашого Canvas
            Point clickPoint = e.GetPosition(GameCanvas);
            
            // Виводимо їх у консоль Rider у зручному форматі
            Console.WriteLine($"new Vector2({(int)clickPoint.X}, {(int)clickPoint.Y}),");

            // Малюємо маленьку червону крапку там, де ти клікнув, щоб бачити маршрут
            Ellipse dot = new Ellipse
            {
                Width = 6,
                Height = 6,
                Fill = Brushes.Red
            };
            Canvas.SetLeft(dot, clickPoint.X - 3);
            Canvas.SetTop(dot, clickPoint.Y - 3);
            GameCanvas.Children.Add(dot);
        }
    }
}