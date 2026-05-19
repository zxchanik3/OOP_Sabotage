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
        private DispatcherTimer _pitTimer; 
        private bool _isPitServing = false; 
        private int _pitTimeLeft = 0;
        private bool _hasBeenServed = false;
        private TrackSegment _lastSegment = null;
        private double _targetSpeed = 1000;
        private Track _track;
        private List<TrackNode> _trackNodes;
        private string _selectedTrackName = "";
        private int _selectedLaps = 5;
        private bool _autoPitLimiter = true;
        private double _pitLaneSpeedLimit = 60.0;
        private MediaPlayer _menuMusicPlayer = new MediaPlayer();
        private MediaPlayer _raceMusicPlayer = new MediaPlayer();

        public MainWindow()
        {
            InitializeComponent();
            InitializeGame();
            InitializeMusic();
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
        private void InitializeMusic()
        {
            string basePath = AppDomain.CurrentDomain.BaseDirectory;
            string menuSongPath = System.IO.Path.Combine(basePath, "Music", "MenuSong.mp3");
            string raceSongPath = System.IO.Path.Combine(basePath, "Music", "RaceSong.mp3");

            _menuMusicPlayer.Open(new Uri(menuSongPath));
            _raceMusicPlayer.Open(new Uri(raceSongPath));

            _menuMusicPlayer.MediaEnded += (s, e) => { _menuMusicPlayer.Position = TimeSpan.Zero; _menuMusicPlayer.Play(); };
            _raceMusicPlayer.MediaEnded += (s, e) => { _raceMusicPlayer.Position = TimeSpan.Zero; _raceMusicPlayer.Play(); };

            _menuMusicPlayer.Volume = 0.02;
            _raceMusicPlayer.Volume = 0.02;

            _menuMusicPlayer.Play();
        }
        private void InitializeGame()
        {
            _cars = new List<Car>();
            _drivers = new List<Driver>();
            _carVisuals = new Dictionary<Car, Rectangle>();
            
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
            _track = builder.SetName("Winter GP").SetLaps(5).Build();

            _userDriver = new UserDriver("Користувач (Ти)", 77);
            var userCar = new Car("Player Car", "Your Team", 2026, 1000, 25, 280, 798);
            userCar.ChangeTyres(TyreType.Soft);
            userCar.SetPosition(new Vector2(startPos.X, startPos.Y - 15), startDir);

            _drivers.Add(_userDriver);
            _cars.Add(userCar);

            Rectangle rect = new Rectangle
            {
                Width = 24, Height = 14,
                Fill = Brushes.Red,
                Stroke = Brushes.Black, StrokeThickness = 1
            };
            _carVisuals[userCar] = rect;
            GameCanvas.Children.Add(rect);

            _pitTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(100) };
            _pitTimer.Tick += PitTimer_Tick; 
            
            _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(16) };
            _timer.Tick += Timer_Tick;
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            float dT = 0.1f;
            if (_drivers.Count == 0) return;
            var driver = _userDriver;
            var car = _cars[0];

            if (_isRaceFinished) return;

            if (!_isPitServing)
            {
                driver.Drive(car, dT);
            }
            else
            {
                car.UpdateSpeed(-2f, dT);
                car.Move(dT);
            }

            Rectangle rect = _carVisuals[car];
            double x = car.Position.X - rect.Width / 2;
            double y = car.Position.Y - rect.Height / 2;

            Canvas.SetLeft(rect, x);
            Canvas.SetTop(rect, y);

            double angle = Math.Atan2(car.Direction.Y, car.Direction.X) * (180 / Math.PI);
            rect.RenderTransform = new RotateTransform(angle, rect.Width / 2, rect.Height / 2);

            if (!_carLastWaypoint.ContainsKey(car)) _carLastWaypoint[car] = 0;
            if (!_carLaps.ContainsKey(car)) _carLaps[car] = 0;

            int closestWp = GetClosestNodeIndex(car.Position, out float distToWp);
            int lastWp = _carLastWaypoint[car];
            TrackNode currentNode = _trackNodes[closestWp];

            if (currentNode.Logic != _lastSegment && !_isPitServing)
            {
                _lastSegment = currentNode.Logic; 
                double tempSpeed = car.Speed;
                currentNode.Logic.ApplyEffect(car, ref tempSpeed);

                if (currentNode.Logic is StraightSegment)
                {
                    _targetSpeed = car.TopSpeed; 
                }
                else
                {
                    _targetSpeed = tempSpeed; 
                }
            }

            if (car.Speed > _targetSpeed && !_isPitServing)
            {
                car.UpdateSpeed(-2.0f, dT); 
            }

            if (distToWp > 55f) 
            {
                car.UpdateSpeed(-1000f, dT); 
                Vector2 pushDirection = Vector2.Normalize(currentNode.Position - car.Position);
                car.SetPosition(car.Position + pushDirection * 3f, car.Direction);
            }
            else if (distToWp > 45f) 
            {
                if (car.Speed > 20f) 
                {
                    car.UpdateSpeed(-6.0f, dT); 
                }
            }

            if (lastWp >= _trackNodes.Count - 8 && closestWp <= 5)
            {
                _carLaps[car]++;
                _carLastWaypoint[car] = closestWp; 

                if (_carLaps[car] >= _track.RequiredLapCount)
                {
                    _isRaceFinished = true;
                    _timer.Stop();
                    TxtStatus.Text = "🏁 ФІНІШ! Гонка завершена!";
                    return;
                }
            }
            else if (Math.Abs(closestWp - lastWp) < 10) 
            {
                _carLastWaypoint[car] = closestWp;
            }

            Point carPoint = new System.Windows.Point(car.Position.X, car.Position.Y);
            if (_pitArea.Contains(carPoint))
            {
                if (_autoPitLimiter && car.Speed > _pitLaneSpeedLimit)
                {
                    car.UpdateSpeed(-2.5f, dT); 
                }

                if (!_isPitServing && !_hasBeenServed && car.Speed < 40f)
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
                _pitTimer.Stop(); 
                _isPitServing = false; 
                TxtPitStatus.Visibility = Visibility.Collapsed; 
                _hasBeenServed = true;
                _cars[0].ChangeTyres(_cars[0].Tyres.Type);
            }
        }

        private void BtnSettings_Click(object sender, RoutedEventArgs e)
        {
            SettingsPanel.Visibility = Visibility.Visible;
        }

        private void BtnCloseSettings_Click(object sender, RoutedEventArgs e)
        {
            if (ComboLaps.SelectedIndex == 0) _selectedLaps = 3;
            else if (ComboLaps.SelectedIndex == 1) _selectedLaps = 5;
            else _selectedLaps = 15;

            _autoPitLimiter = CheckAutoPit.IsChecked == true;
            SettingsPanel.Visibility = Visibility.Collapsed;
        }

        private void BtnPlayGame_Click(object sender, RoutedEventArgs e)
        {
            BtnPlayGame.Visibility = Visibility.Collapsed;
            MapSelectionPanel.Visibility = Visibility.Visible;
        }
        private void BtnMapWinter_Click(object sender, RoutedEventArgs e)
        {
            StartGameOnMap("Winter");
        }

        private void BtnMapForest_Click(object sender, RoutedEventArgs e)
        {
            StartGameOnMap("Forest");
        }

        private void StartGameOnMap(string mapType)
        {
            _selectedTrackName = mapType;

            ImageBrush bgBrush = new ImageBrush { Stretch = Stretch.Fill };
            string uriPath = _selectedTrackName == "Winter" ? "/Images/WinterMap.jpg" : "/Images/ForestMap.jpg";
            bgBrush.ImageSource = new System.Windows.Media.Imaging.BitmapImage(new Uri($"pack://application:,,,{uriPath}"));
            GameCanvas.Background = bgBrush;

            _track.RequiredLapCount = _selectedLaps;

            MainMenuGrid.Visibility = Visibility.Collapsed;
            GameScreenGrid.Visibility = Visibility.Visible;

            if (_menuMusicPlayer != null && _raceMusicPlayer != null)
            {
                _menuMusicPlayer.Stop();
                _raceMusicPlayer.Play();
            }
        }

        private void BtnStart_Click(object sender, RoutedEventArgs e)
        {
            this.Focus();
            _timer.Start();
            TxtStatus.Text = "Статус: Гонка активована!";
        }

        private void BtnStop_Click(object sender, RoutedEventArgs e)
        {
            _timer.Stop();
            TxtStatus.Text = "Статус: Пауза";
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.C && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                Application.Current.Shutdown();
            }

            if (_userDriver == null || _isPitServing) return;
            
            switch (e.Key)
            {
                case Key.W: case Key.Up: _userDriver.Press(lab.Button.Forward); break;
                case Key.S: case Key.Down: _userDriver.Press(lab.Button.Backward); break;
                case Key.A: case Key.Left: _userDriver.Press(lab.Button.Left); break;
                case Key.D: case Key.Right: _userDriver.Press(lab.Button.Right); break;
            }
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (_userDriver == null) return;

            switch (e.Key)
            {
                case Key.W: case Key.Up: _userDriver.Release(lab.Button.Forward); break;
                case Key.S: case Key.Down: _userDriver.Release(lab.Button.Backward); break;
                case Key.A: case Key.Left: _userDriver.Release(lab.Button.Left); break;
                case Key.D: case Key.Right: _userDriver.Release(lab.Button.Right); break;
            }
        }

        private void GameCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Point clickPoint = e.GetPosition(GameCanvas);
            Console.WriteLine($"new Vector2({(int)clickPoint.X}, {(int)clickPoint.Y}),");

            Ellipse dot = new Ellipse { Width = 6, Height = 6, Fill = Brushes.Red };
            Canvas.SetLeft(dot, clickPoint.X - 3);
            Canvas.SetTop(dot, clickPoint.Y - 3);
            GameCanvas.Children.Add(dot);
        }
    }
}