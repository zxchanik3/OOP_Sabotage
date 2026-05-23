using lab;
using System;
using System.Collections.Generic;
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

        private List<Car> _cars = new();
        private Dictionary<Car, Image> _carVisuals = new();

        private List<Car> _carOptions = new();
        private int _carIndex = 0;
        private Car? _selectedCarTemplate;

        private UserDriver _userDriver = new("Player", 77);

        private Track _track = null!;
        private List<TrackNode> _trackNodes = new();

        private DispatcherTimer _timer = new();

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
            Car car1 = new Car("Car1", "Blue", 2020, 670, 3, 250, 780, "Images/car1.png");
            Car car2 = new Car("Car2", "Green", 2021, 720, 2, 260, 750, "Images/car2.png");
            Car car3 = new Car("Car3", "Purple", 2019, 650, 4, 240, 800, "Images/car3.png");
            Car car4 = new Car("Car4", "Yellow", 2022, 700, 3.5f, 255, 770, "Images/car4.png");
            Car car5 = new Car("Car5", "Red", 2023, 750, 2.5f, 270, 730, "Images/car5.png");
            Car car6 = new Car("Car6", "Orange", 2020, 680, 3.2f, 245, 790, "Images/car6.png");
            Car car7 = new Car("Car7", "Cyan", 2021, 710, 2.8f, 265, 760, "Images/car7.png");

            _gameData.Cars.AddRange(new[]
            {
                new CarConfig
                {
                    Model = car1.Model,
                    Team = car1.Team,
                    Year = car1.Year,
                    Horsepower = car1.Horsepower,
                    Acceleration = car1.Acceleration,
                    TopSpeed = car1.TopSpeed,
                    Weight = car1.Weight,
                    ImagePath = car1.ImagePath
                },
                new CarConfig
                {
                    Model = car2.Model,
                    Team = car2.Team,
                    Year = car2.Year,
                    Horsepower = car2.Horsepower,
                    Acceleration = car2.Acceleration,
                    TopSpeed = car2.TopSpeed,
                    Weight = car2.Weight,
                    ImagePath = car2.ImagePath
                },
                new CarConfig
                {
                    Model = car3.Model,
                    Team = car3.Team,
                    Year = car3.Year,
                    Horsepower = car3.Horsepower,
                    Acceleration = car3.Acceleration,
                    TopSpeed = car3.TopSpeed,
                    Weight = car3.Weight,
                    ImagePath = car3.ImagePath
                },
                new CarConfig
                {
                    Model = car4.Model,
                    Team = car4.Team,
                    Year = car4.Year,
                    Horsepower = car4.Horsepower,
                    Acceleration = car4.Acceleration,
                    TopSpeed = car4.TopSpeed,
                    Weight = car4.Weight,
                    ImagePath = car4.ImagePath
                },
                new CarConfig
                {
                    Model = car5.Model,
                    Team = car5.Team,
                    Year = car5.Year,
                    Horsepower = car5.Horsepower,
                    Acceleration = car5.Acceleration,
                    TopSpeed = car5.TopSpeed,
                    Weight = car5.Weight,
                    ImagePath = car5.ImagePath
                },
                new CarConfig
                {
                    Model = car6.Model,
                    Team = car6.Team,
                    Year = car6.Year,
                    Horsepower = car6.Horsepower,
                    Acceleration = car6.Acceleration,
                    TopSpeed = car6.TopSpeed,
                    Weight = car6.Weight,
                    ImagePath = car6.ImagePath
                },
                new CarConfig
                {
                    Model = car7.Model,
                    Team = car7.Team,
                    Year = car7.Year,
                    Horsepower = car7.Horsepower,
                    Acceleration = car7.Acceleration,
                    TopSpeed = car7.TopSpeed,
                    Weight = car7.Weight,
                    ImagePath = car7.ImagePath
                }
            });

            _gameData.Drivers.AddRange(new[]
            {
                new DriverConfig { Name = "Player", Number = 77, Lock = false },
                new DriverConfig { Name = "Lewis", Number = 11, Lock = true },
                new DriverConfig { Name = "Kimi", Number = 22, Lock = true },
                new DriverConfig { Name = "Max", Number = 33, Lock = true },
                new DriverConfig { Name = "Toto", Number = 44, Lock = true },
                new DriverConfig { Name = "Nico", Number = 55, Lock = true },
                new DriverConfig { Name = "George", Number = 66, Lock = true }
            });
            

            _gameData.CarSaveToFile("CarsData.json");
            _gameData.DriverSaveToFile("DriversData.json");
            _gameData.TrackSaveToFile("TracksData.json");

            _gameData.CarLoadFromFile("CarsData.json");
            _gameData.TrackLoadFromFile("TracksData.json");

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
            if (_cars.Count == 0 || _isRaceFinished) return;

            var car = _cars[0];

            int closest = GetClosestNodeIndex(car.Position);
            var node = _trackNodes[closest];

            Vector2 dir = node.Position - car.Position;

            if (dir.Length() > 1f)
            {
                dir = Vector2.Normalize(dir);
                car.SetPosition(car.Position + dir * 3f, car.Direction);
            }

            var input = _userDriver.GetInput(car, 0.1f);
            car.Update(input, 0.1f, new StraightSegment(100));

            UpdateCarVisual(car);
            UpdateHUD(car);
        }

        private int GetClosestNodeIndex(Vector2 pos)
        {
            int best = 0;
            float bestDist = float.MaxValue;

            for (int i = 0; i < _trackNodes.Count; i++)
            {
                float d = Vector2.Distance(pos, _trackNodes[i].Position);
                if (d < bestDist)
                {
                    bestDist = d;
                    best = i;
                }
            }

            return best;
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
            _carIndex = (_carIndex + 1) % _carOptions.Count;
            UpdateCarPreview();
        }

        private void BtnPrevCar_Click(object sender, RoutedEventArgs e)
        {
            if (_carOptions.Count == 0) return;
            _carIndex--;
            if (_carIndex < 0) _carIndex = _carOptions.Count - 1;
            UpdateCarPreview();
        }

        private void BtnSelectCar_Click(object sender, RoutedEventArgs e)
        {
            if (_carOptions.Count == 0) return;
            _selectedCarTemplate = _carOptions[_carIndex];
            TxtStatus.Text = $"Selected: {_selectedCarTemplate.Model}";
            CarGaragePanel.Visibility = Visibility.Collapsed;
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

        private void UpdateCarPreview()
        {
            if (_carOptions.Count == 0) return;

            string path = _carOptions[_carIndex].ImagePath ?? string.Empty;
            try
            {
                CarPreviewImage.Source = new BitmapImage(new Uri(path, UriKind.RelativeOrAbsolute));
            }
            catch
            {
                CarPreviewImage.Source = null;
            }
        }

    }
}