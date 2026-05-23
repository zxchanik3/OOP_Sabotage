using lab;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace RaceGUI
{
    public partial class MainWindow : Window
    {
        // ================= DATA =================
        private GameData _gameData = new();
        private readonly string _dataFolder = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");

        private List<Car> _cars = new();
        private Dictionary<Car, Image> _carVisuals = new();

        private List<Car> _carOptions = new();
        private int _carIndex = 0;
        private Car? _selectedCarTemplate;

        private UserDriver _userDriver = new("Player", 77);

        private Track _track = null!;
        private List<TrackNode> _trackNodes = new();

        private DispatcherTimer _timer = new();

        private Dictionary<Car, int> _carLastWaypoint = new();
        private Dictionary<Car, int> _carLaps = new();
        private Rect _pitArea = new Rect(400, 480, 200, 50);
        private TrackSegment? _lastSegment;
        private double _targetSpeed;
        private double _pitLaneSpeedLimit = 60;
        private int _pitTimeLeft;
        private bool _hasBeenServed = false;
        private DispatcherTimer _pitTimer = new();

        // ================= STATE =================
        private bool _isRaceFinished;
        private int _selectedLaps = 5;
        private string _selectedTrackName = "GP";

        private bool _isPitServing = false;
        private bool _autoPitLimiter = true;
        private MediaPlayer _menuMusicPlayer = new();
        private MediaPlayer _raceMusicPlayer = new();

        // ================= INIT =================
        public MainWindow()
        {
            InitializeComponent();

            InitializeGame();
            InitializeMusic();

            _timer.Interval = TimeSpan.FromMilliseconds(16);
            _timer.Tick += Timer_Tick;
        }

        private void InitializeGame()
        {
            _gameData.CarLoadFromFile(System.IO.Path.Combine(_dataFolder, "CarsData.json"));
            _gameData.TrackLoadFromFile(System.IO.Path.Combine(_dataFolder, "TracksData.json"));
            _gameData.DriverLoadFromFile(System.IO.Path.Combine(_dataFolder, "DriversData.json"));

            _carOptions = _gameData.Cars
                .Select(c => new Car(
                    c.Model, c.Team, c.Year,
                    c.Horsepower, c.Acceleration,
                    c.TopSpeed, c.Weight, c.ImagePath))
                .ToList();

            _selectedCarTemplate = _carOptions.FirstOrDefault();

            InitializeTrack();

        }

        private void InitializeTrack()
        {
            var config = _gameData.Tracks.FirstOrDefault();

            _track = new TrackBuilder()
                .SetName(config?.Name ?? "GP")
                .SetLaps(config?.Laps ?? 5)
                .Build();

            _trackNodes = new();

            if (config?.Nodes == null) return;

            foreach (var n in config.Nodes)
            {
                _trackNodes.Add(new TrackNode(
                    new Vector2(n.X, n.Y),
                    new StraightSegment(0)
                ));
            }
        }

        private void InitializeMusic()
        {
            try
            {
                string basePath = AppDomain.CurrentDomain.BaseDirectory;
                _menuMusicPlayer.Open(new Uri(System.IO.Path.Combine(basePath, "Music", "MenuSong.mp3")));
                _raceMusicPlayer.Open(new Uri(System.IO.Path.Combine(basePath, "Music", "RaceSong.mp3")));
                _menuMusicPlayer.Volume = 0.02;
                _raceMusicPlayer.Volume = 0.02;
                _menuMusicPlayer.MediaEnded += (s, e) => _menuMusicPlayer.Position = TimeSpan.Zero;
                _raceMusicPlayer.MediaEnded += (s, e) => _raceMusicPlayer.Position = TimeSpan.Zero;
                _menuMusicPlayer.Play();
            }
            catch
            {
                // Ignore music initialization failures
            }
        }

        //LOOP
        private void Timer_Tick(object? sender, EventArgs e)
        {
            float dT = 0.1f;
            if (_cars.Count == 0 || _isRaceFinished) return;

            var driver = _userDriver;
            var car = _cars[0];

            int closestWpForLogic = GetClosestNodeIndex(car.Position, out float distToWp);
            TrackNode currentNode = _trackNodes[closestWpForLogic];
            TrackSegment currentSegment = currentNode.Logic;

            CarInput input = _isPitServing
                ? new CarInput { Throttle = 0f, Brake = 1f, Steering = 0f }
                : driver.GetInput(car, dT);

            if (!_isPitServing)
            {
                double tempSpeed = car.Speed;
                currentSegment.ApplyEffect(car, ref tempSpeed, dT);

                if (currentSegment is StraightSegment)
                {
                    _targetSpeed = car.TopSpeed;
                }
                else
                {
                    _targetSpeed = tempSpeed;
                }

                if (car.Speed > _targetSpeed)
                {
                    input.Throttle = 0f;
                    input.Brake = Math.Max(input.Brake, 0.4f);
                }
            }

            if (distToWp > 55f)
            {
                Vector2 pushDirection = Vector2.Normalize(currentNode.Position - car.Position);

                Vector2 nextDirection = car.Direction;
                if (_trackNodes.Count > (closestWpForLogic + 1))
                {
                    nextDirection = Vector2.Normalize(_trackNodes[closestWpForLogic + 1].Position - currentNode.Position);
                }
                else
                {
                    nextDirection = Vector2.Normalize(_trackNodes[0].Position - currentNode.Position);
                }

                car.SetPosition(currentNode.Position - pushDirection * 3f, nextDirection);
            }
            float maxAllowedDistance = 45f;

            if (distToWp > maxAllowedDistance)
            {
                Vector2 pushDir = Vector2.Normalize(currentNode.Position - car.Position);

                float penetrationDepth = distToWp - maxAllowedDistance;

                Vector2 correction = pushDir * (penetrationDepth * 0.3f);
                Vector2 newPosition = car.Position + correction;

                input.Brake = 1.0f;
                input.Throttle = 0.0f;

                car.SetPosition(newPosition, car.Direction);
            }
            else if (distToWp > 35f && car.Speed > 30f)
            {
                input.Throttle *= 0.5f;
                input.Brake = Math.Max(input.Brake, 0.3f);
            }

            Point carPoint = new Point(car.Position.X, car.Position.Y);
            if (_pitArea.Contains(carPoint))
            {
                if (_autoPitLimiter && car.Speed > _pitLaneSpeedLimit)
                {
                    input.Throttle = 0f;
                    input.Brake = Math.Max(input.Brake, 0.5f);
                }

                if (!_isPitServing && !_hasBeenServed && car.Speed < 40f)
                {
                    _isPitServing = true;
                    TxtPitStatus.Visibility = Visibility.Visible;
                    _pitTimeLeft = 20;
                    _pitTimer.Start();
                }
            }
            else
            {
                _hasBeenServed = false;
            }

            car.Update(input, dT, currentSegment);

            if (_carVisuals.TryGetValue(car, out Image? img))
            {
                double x = car.Position.X - (img.Width / 2);
                double y = car.Position.Y - (img.Height / 2);

                Canvas.SetLeft(img, x);
                Canvas.SetTop(img, y);

                double angle = Math.Atan2(car.Direction.Y, car.Direction.X) * (180 / Math.PI);

                img.RenderTransform = new RotateTransform(angle);
            }
            if (!_carLastWaypoint.ContainsKey(car)) _carLastWaypoint[car] = 0;
            if (!_carLaps.ContainsKey(car)) _carLaps[car] = 0;

            int lastWp = _carLastWaypoint[car];

            if (lastWp >= _trackNodes.Count - 8 && closestWpForLogic <= 5)
            {
                _carLaps[car]++;
                _carLastWaypoint[car] = closestWpForLogic;

                if (_carLaps[car] >= _track.RequiredLapCount)
                {
                    _isRaceFinished = true;
                    _timer.Stop();
                    if (_raceMusicPlayer != null) _raceMusicPlayer.Stop();
                    if (_menuMusicPlayer != null) _menuMusicPlayer.Play();
                    TxtStatus.Text = "🏁 ФІНІШ! Гонка завершена!";
                    return;
                }
            }
            else if (Math.Abs(closestWpForLogic - lastWp) < 10)
            {
                _carLastWaypoint[car] = closestWpForLogic;
            }

            int currentLap = _carLaps.ContainsKey(car) ? _carLaps[car] + 1 : 1;
            int currentSpeed = (int)car.Speed;
            int tyreDurability = car.Tyres != null ? (int)car.Tyres.Durability : 100;

            TxtStatus.Text = $"Коло: {currentLap}/{_track.RequiredLapCount} | Швидкість: {currentSpeed} км/год | Шини: {tyreDurability}%";
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

        private void UpdateCarVisual(Car car)
        {
            if (!_carVisuals.TryGetValue(car, out var img)) return;

            Canvas.SetLeft(img, car.Position.X - img.Width / 2);
            Canvas.SetTop(img, car.Position.Y - img.Height / 2);

            double angle = Math.Atan2(car.Direction.Y, car.Direction.X) * 180 / Math.PI;
            img.RenderTransform = new RotateTransform(angle, img.Width / 2, img.Height / 2);
        }

        private void UpdateHUD(Car car)
        {
            TxtStatus.Text =
                $"Speed: {(int)car.Speed} | Tyres: {(int)car.Tyres.Durability}%";
        }

        // ================= BUTTONS =================

        private void BtnStart_Click(object sender, RoutedEventArgs e)
        {
            SpawnCar();
            _timer.Start();
        }

        private void BtnStop_Click(object sender, RoutedEventArgs e)
        {
            _timer.Stop();
        }

        private void SpawnCar()
        {
            if (_carOptions.Count == 0) return;

            var cfg = _carOptions[_carIndex];

            var car = new Car(
                cfg.Model, cfg.Team, cfg.Year,
                cfg.Horsepower, cfg.Acceleration,
                cfg.TopSpeed, cfg.Weight, cfg.ImagePath
            );

            _cars.Clear();
            foreach (var image in _carVisuals.Values.ToList())
            {
                if (GameCanvas.Children.Contains(image))
                    GameCanvas.Children.Remove(image);
            }
            _carVisuals.Clear();

            _cars.Add(car);

            var img = new Image
            {
                Width = 40,
                Height = 20,
                RenderTransformOrigin = new Point(0.5, 0.5)
            };

            try
            {
                if (!string.IsNullOrWhiteSpace(cfg.ImagePath))
                    img.Source = new BitmapImage(new Uri(cfg.ImagePath, UriKind.RelativeOrAbsolute));
            }
            catch
            {
                img.Source = null;
            }

            Vector2 start = _trackNodes.Count > 0 ? _trackNodes[0].Position : new Vector2(100, 100);
            Canvas.SetLeft(img, start.X - img.Width / 2);
            Canvas.SetTop(img, start.Y - img.Height / 2);
            Panel.SetZIndex(img, 100);

            _carVisuals[car] = img;
            GameCanvas.Children.Add(img);

            UpdateCarPreview();
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

            string uriPath = _selectedTrackName == "Winter" ? "/Images/WinterMap.jpg" : "/Images/ForestMap.jpg";

            GameCanvas.Background = new ImageBrush
            {
                ImageSource = new BitmapImage(new Uri($"pack://application:,,,{uriPath}")),
                Stretch = Stretch.Fill
            };

            try
            {
                _track.RequiredLapCount = _selectedLaps;
            }
            catch { }

            MainMenuGrid.Visibility = Visibility.Collapsed;
            GameScreenGrid.Visibility = Visibility.Visible;

            if (_menuMusicPlayer != null && _raceMusicPlayer != null)
            {
                try
                {
                    _menuMusicPlayer.Stop();
                    _raceMusicPlayer.Play();
                }
                catch { }
            }
        }

        private void BtnNextCar_Click(object sender, RoutedEventArgs e)
        {
            if (_carOptions.Count == 0) return;

            // Збільшуємо індекс. Якщо дійшли до кінця — скидаємо на 0 (циклічне перемикання)
            _carIndex++;
            if (_carIndex >= _carOptions.Count)
            {
                _carIndex = 0;
            }

            UpdateCarPreview();
        }

        private void BtnPrevCar_Click(object sender, RoutedEventArgs e)
        {
            if (_carOptions.Count == 0) return;

            _carIndex--;
            if (_carIndex < 0)
            {
                _carIndex = _carOptions.Count - 1;
            }

            UpdateCarPreview();
        }

        private void BtnSelectCar_Click(object sender, RoutedEventArgs e)
        {
            if (_carOptions.Count > 0)
            {
                _selectedCarTemplate = _carOptions[_carIndex];

                CarGaragePanel.Visibility = Visibility.Collapsed;
            }
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
                case Key.W:
                case Key.Up:
                    _userDriver.Press(lab.Button.Forward);
                    break;
                case Key.S:
                case Key.Down:
                    _userDriver.Press(lab.Button.Backward);
                    break;
                case Key.A:
                case Key.Left:
                    _userDriver.Press(lab.Button.Left);
                    break;
                case Key.D:
                case Key.Right:
                    _userDriver.Press(lab.Button.Right);
                    break;
            }
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (_userDriver == null) return;

            switch (e.Key)
            {
                case Key.W:
                case Key.Up:
                    _userDriver.Release(lab.Button.Forward);
                    break;
                case Key.S:
                case Key.Down:
                    _userDriver.Release(lab.Button.Backward);
                    break;
                case Key.A:
                case Key.Left:
                    _userDriver.Release(lab.Button.Left);
                    break;
                case Key.D:
                case Key.Right:
                    _userDriver.Release(lab.Button.Right);
                    break;
            }
        }

        private void GameCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var p = e.GetPosition(GameCanvas);

            Console.WriteLine($"new Vector2({(int)p.X}, {(int)p.Y}),");

            var dot = new Ellipse
            {
                Width = 6,
                Height = 6,
                Fill = Brushes.Red
            };

            Canvas.SetLeft(dot, p.X - 3);
            Canvas.SetTop(dot, p.Y - 3);
            GameCanvas.Children.Add(dot);
        }

        private void BtnCarGarage_Click(object sender, RoutedEventArgs e)
        {
            CarGaragePanel.Visibility = Visibility.Visible;
            UpdateCarPreview();
        }

        //оновлення прев'ю машини при виборі
        private void UpdateCarPreview()
        {
            if (_carOptions.Count == 0 || _carIndex < 0 || _carIndex >= _carOptions.Count) return;

            string path = _carOptions[_carIndex].ImagePath ?? string.Empty;

            try
            {
                CarPreviewImage.Source = new BitmapImage(new Uri(path, UriKind.Relative));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Помилка завантаження ресурсу: {ex.Message}");
            }
        }
    }
}