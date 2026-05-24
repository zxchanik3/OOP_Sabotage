using lab;
using System.IO;
using System.Numerics;
using System.Text.Json;
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
        private GameData _gameData = new();
        private readonly string _dataFolder = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");

        private List<Car> _cars = new();
        private Dictionary<Car, Image> _carVisuals = new(); // Візуалізація через картинки від колеги

        private List<Car> _carOptions = new();
        private int _carIndex = 0;
        private Car? _selectedCarTemplate;

        private UserDriver _userDriver = new("Player", 77);

        private Track _track = new TrackBuilder().SetName("GP").SetLaps(5).Build();
        private List<TrackNode> _trackNodes = new();

        private DispatcherTimer _timer = new();

        private bool _isRaceFinished;
        private int _selectedLaps = 5;
        private string _selectedTrackName = "GP";
        private Dictionary<Car, int> _carLaps = new();
        private Dictionary<Car, int> _carLastWaypoint = new();
        private bool _trackHasPitStop = true;
        private float _raceTime = 0f;

        private Rect _pitArea = new Rect(350, 470, 170, 50);
        private DispatcherTimer _pitTimer = new();
        private bool _isPitServing = false;
        private int _pitTimeLeft = 0;
        private bool _hasBeenServed = false;
        private bool _autoPitLimiter = true;
        
        private MediaPlayer _menuMusicPlayer = new();
        private MediaPlayer _raceMusicPlayer = new();

        private List<Ellipse> _dustParticles = new(); // Твої частинки пилу

        public MainWindow()
        {
            InitializeComponent();
            InitializeGame();
            InitializeMusic();

            _timer.Interval = TimeSpan.FromMilliseconds(16);
            _timer.Tick += Timer_Tick;
            
            _pitTimer.Interval = TimeSpan.FromMilliseconds(100);
            _pitTimer.Tick += PitTimer_Tick;
        }

        private void InitializeGame()
        {
            Directory.CreateDirectory(_dataFolder);
            _gameData.AddCar(new CarConfig("Car1", "Blue", 2020, 670, 3, 250, 780, "Images/car1.png"));
            _gameData.AddCar(new CarConfig("Car2", "Green", 2021, 720, 2, 260, 750, "Images/car2.png"));
            _gameData.AddCar(new CarConfig("Car3", "Purple", 2019, 650, 4, 240, 800, "Images/car3.png"));
            _gameData.AddCar(new CarConfig("Car4", "Yellow", 2022, 700, 3.5f, 255, 770, "Images/car4.png"));
            _gameData.AddCar(new CarConfig("Car5", "Red", 2023, 750, 2.5f, 270, 730, "Images/car5.png"));

            _gameData.AddDriver(new DriverConfig("Player", 77, false));
            _gameData.AddDriver(new DriverConfig("Lewis", 11, true));

            _gameData.CarSaveToFile(System.IO.Path.Combine(_dataFolder, "CarsData.json")); 
            _gameData.DriverSaveToFile(System.IO.Path.Combine(_dataFolder, "DriversData.json"));
            _gameData.CarLoadFromFile(System.IO.Path.Combine(_dataFolder, "CarsData.json"));
            
            _carOptions = _gameData.Cars
                .Select(c => new Car(c.Model, c.Team, c.Year, c.Horsepower, (int)c.Acceleration, c.TopSpeed, c.Weight, c.ImagePath))
                .ToList();

            _selectedCarTemplate = _carOptions.FirstOrDefault();
            _trackNodes = new List<TrackNode>();
        }

        private void InitializeMusic()
        {
            try
            {
                string basePath = AppDomain.CurrentDomain.BaseDirectory;
                _menuMusicPlayer.Open(new Uri(System.IO.Path.Combine(basePath, "Music", "MenuSong.mp3")));
                _raceMusicPlayer.Open(new Uri(System.IO.Path.Combine(basePath, "Music", "RaceSong.mp3")));
                _menuMusicPlayer.Volume = 0.2;
                _raceMusicPlayer.Volume = 0.2;
                _menuMusicPlayer.MediaEnded += (s, e) => _menuMusicPlayer.Position = TimeSpan.Zero;
                _raceMusicPlayer.MediaEnded += (s, e) => _raceMusicPlayer.Position = TimeSpan.Zero;
                _menuMusicPlayer.Play();
            }
            catch { }
        }

        // ================= ІГРОВИЙ ЦИКЛ =================
        private void Timer_Tick(object? sender, EventArgs e)
        {
            if (_cars.Count == 0 || _isRaceFinished) return;

            var car = _cars[0];
            
            var input = _userDriver.GetInput(car, 0.07f);
            car.Update(input, 0.07f, new StraightSegment(100));
            
            if (!_isRaceFinished)
            {
                _raceTime += 0.016f;
                TimeSpan ts = TimeSpan.FromSeconds(_raceTime);
                TxtTimer.Text = $"Час: {ts.ToString(@"mm\:ss\.ff")}";
            }
            
            float padding = 15f;
            float clampedX = Math.Max(padding, Math.Min(car.Position.X, (float)GameCanvas.ActualWidth - padding));
            float clampedY = Math.Max(padding, Math.Min(car.Position.Y, (float)GameCanvas.ActualHeight - padding));
            if (car.Position.X != clampedX || car.Position.Y != clampedY)
            {
                car.SetPosition(new Vector2(clampedX, clampedY), car.Direction);
            }

            int closestNodeIndex = GetClosestNodeIndex(car.Position);

            if (closestNodeIndex >= 0)
            {
                if (!_carLaps.ContainsKey(car))
                {
                    _carLaps[car] = 1;
                    _carLastWaypoint[car] = 0;
                }

                int lastWp = _carLastWaypoint[car];

                if (closestNodeIndex > lastWp && closestNodeIndex <= lastWp + 5)
                {
                    _carLastWaypoint[car] = closestNodeIndex;
                }
                else if (lastWp >= _trackNodes.Count - 5 && closestNodeIndex <= 5)
                {
                    _carLaps[car]++;
                    _carLastWaypoint[car] = closestNodeIndex;

                    if (_carLaps[car] > _track.RequiredLapCount)
                    {
                        _isRaceFinished = true;
                        _timer.Stop();

                        TxtStatus.Text = "ФІНІШ!";
                        TxtResultPlace.Text = "Місце: 1";

                        TimeSpan ts = TimeSpan.FromSeconds(_raceTime);
                        TxtResultTime.Text = $"Час: {ts.ToString(@"mm\:ss\.ff")}";

                        ResultMenuPanel.Visibility = Visibility.Visible;
                        return;
                    }
                }
            }

            if (closestNodeIndex >= 0)
            {
                Vector2 closestNodePos = _trackNodes[closestNodeIndex].Position;
                float distToTrack = Vector2.Distance(car.Position, closestNodePos);

                if (distToTrack > 45f) 
                {
                    if (car.Speed > 30f)
                    {
                        SpawnDust(car.Position);
                    }
                    car.Speed *= 0.94f;
                }

                if (distToTrack > 55f)
                {
                    Vector2 pushDir = Vector2.Normalize(closestNodePos - car.Position);
                    car.SetPosition(car.Position + pushDir * 3f, car.Direction);
                }
            }

            // Логіка автоматичного піт-стопу
            if (_trackHasPitStop)
            {
                Point carPoint = new Point(car.Position.X, car.Position.Y);
                if (_pitArea.Contains(carPoint))
                {
                    if (_autoPitLimiter && car.Speed > 40f)
                    {
                        car.Speed -= 2f; 
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
            }
            else
            {
                if (car.Tyres != null && car.Tyres.Durability < 100)
                    car.ChangeTyres(car.Tyres.Type);
            }

            // Оновлення частинок пилу
            for (int i = _dustParticles.Count - 1; i >= 0; i--)
            {
                Ellipse p = _dustParticles[i];
                p.Opacity -= 0.05;
                p.Width += 0.5;
                p.Height += 0.5;
                Canvas.SetLeft(p, Canvas.GetLeft(p) - 0.25);
                Canvas.SetTop(p, Canvas.GetTop(p) - 0.25);

                if (p.Opacity <= 0)
                {
                    GameCanvas.Children.Remove(p);
                    _dustParticles.RemoveAt(i);
                }
            }

            UpdateCarVisual(car);
            
            string tyreText = _trackHasPitStop ? $"{(int)car.Tyres.Durability}%" : "—";
            int currentLap = _carLaps.ContainsKey(car) ? _carLaps[car] : 1;
            TxtStatus.Text = $"Коло: {currentLap}/{_track.RequiredLapCount} | Швидкість: {Math.Abs((int)car.Speed)} км/год | Шини: {tyreText}";
        }

        private int GetClosestNodeIndex(Vector2 pos)
        {
            if (_trackNodes.Count == 0) return -1;
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

        private void PitTimer_Tick(object? sender, EventArgs e)
        {
            _pitTimeLeft--;
            if (_pitTimeLeft <= 0)
            {
                _pitTimer.Stop();
                _isPitServing = false;
                _hasBeenServed = true;
                TxtPitStatus.Visibility = Visibility.Collapsed;
                if (_cars.Count > 0) _cars[0].ChangeTyres(TyreType.Medium);
            }
        }

        // ВИПРАВЛЕНО: Машинка тепер крутиться ІДЕАЛЬНО навколо своєї осі (прибрано подвійний центр)
        private void UpdateCarVisual(Car car)
        {
            if (!_carVisuals.TryGetValue(car, out var img)) return;

            Canvas.SetLeft(img, car.Position.X - img.Width / 2);
            Canvas.SetTop(img, car.Position.Y - img.Height / 2);

            double angle = Math.Atan2(car.Direction.Y, car.Direction.X) * 180 / Math.PI;
            img.RenderTransform = new RotateTransform(angle); 
        }

        private void SpawnDust(Vector2 position)
        {
            Ellipse dust = new Ellipse
            {
                Width = 10, Height = 10,
                Fill = _selectedTrackName == "Winter" ? Brushes.White : Brushes.SaddleBrown,
                Opacity = 0.8
            };
            Canvas.SetLeft(dust, position.X - 5);
            Canvas.SetTop(dust, position.Y - 5);
            GameCanvas.Children.Add(dust);
            _dustParticles.Add(dust);
        }

        // ================= ЗАВАНТАЖЕННЯ ТА СТАРТ ГОНКИ =================
        // ПОВЕРНУТО: Метод став async для підтримки красивого зворотного відліку 3...2...1
        private async void StartGameOnMap(string mapType)
        {
            _selectedTrackName = mapType;
            _trackHasPitStop = (_selectedTrackName == "Winter");
            _track.RequiredLapCount = _selectedLaps;

            string uriPath = _selectedTrackName == "Winter" ? "/Images/WinterMap.jpg" : "/Images/ForestMap.jpg";
            GameCanvas.Background = new ImageBrush { ImageSource = new BitmapImage(new Uri($"pack://application:,,,{uriPath}")), Stretch = Stretch.Fill };

            // Завантаження точок з вашого JSON файлу
            string filePath = $"{mapType}.json";
            if (File.Exists(filePath))
            {
                string jsonText = File.ReadAllText(filePath);
                TrackData data = JsonSerializer.Deserialize<TrackData>(jsonText);
                _trackNodes.Clear();

                TrackSegment defaultSeg = new StraightSegment(100);
                foreach (var node in data.Nodes)
                {
                    _trackNodes.Add(new TrackNode(new Vector2(node.X, node.Y), defaultSeg));
                }

                foreach (var seg in data.Segments)
                {
                    TrackSegment logic = seg.Type == "Straight" ? new StraightSegment(100) : new CornerSegment(seg.CornerLimit, 2);
                    for (int i = seg.StartIndex; i <= seg.EndIndex && i < _trackNodes.Count; i++)
                    {
                        _trackNodes[i].Logic = logic;
                    }
                }
            }
            else
            {
                MessageBox.Show($"Файл карти {filePath} не знайдено!", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            MainMenuGrid.Visibility = Visibility.Collapsed;
            MapSelectionPanel.Visibility = Visibility.Collapsed;
            GameScreenGrid.Visibility = Visibility.Visible;

            if (_menuMusicPlayer != null && _raceMusicPlayer != null)
            {
                _menuMusicPlayer.Stop();
                _raceMusicPlayer.Play();
            }

            SpawnCar(); 
            ResetGameState();

            // ПОВЕРНУТО: Логіка зворотного відліку перед стартом гонки!
            _timer.Stop(); 
            for (int i = 3; i > 0; i--)
            {
                TxtStatus.Text = $"СТАРТ ЧЕРЕЗ: {i}";
                await Task.Delay(1000); 
            }
            TxtStatus.Text = "СТАРТ!";
            _raceTime = 0f;
            _timer.Start(); 
        }

        private void SpawnCar()
        {
            var cfg = _selectedCarTemplate ?? _carOptions.FirstOrDefault();
            if (cfg == null) return;

            var car = new Car(cfg.Model, cfg.Team, cfg.Year, cfg.Horsepower, cfg.Acceleration, cfg.TopSpeed, cfg.Weight, cfg.ImagePath);

            _cars.Clear();
            foreach (var image in _carVisuals.Values.ToList())
            {
                if (GameCanvas.Children.Contains(image)) GameCanvas.Children.Remove(image);
            }
            _carVisuals.Clear();
            _cars.Add(car);

            var img = new Image { Width = 40, Height = 20, RenderTransformOrigin = new Point(0.5, 0.5) };
            try
            {
                if (!string.IsNullOrWhiteSpace(cfg.ImagePath))
                    img.Source = new BitmapImage(new Uri(cfg.ImagePath, UriKind.RelativeOrAbsolute));
            }
            catch { img.Source = null; }

            Panel.SetZIndex(img, 100);
            _carVisuals[car] = img;
            GameCanvas.Children.Add(img);
        }

        private void ResetGameState()
        {
            if (_cars.Count == 0 || _trackNodes.Count < 2) return;
            _isRaceFinished = false;
            var car = _cars[0];
            car.Speed = 0f;
            _carLastWaypoint[car] = 0;
            car.ChangeTyres(TyreType.Medium); 
            
            Vector2 startPos = _trackNodes[0].Position;
            Vector2 startDir = Vector2.Normalize(_trackNodes[1].Position - _trackNodes[0].Position);
            car.SetPosition(new Vector2(startPos.X, startPos.Y - 15), startDir);
            
            _isPitServing = false;
            TxtPitStatus.Visibility = Visibility.Collapsed;

            UpdateCarVisual(car);
            
            _raceTime = 0f;
            TxtTimer.Text = "Час: 00:00.00";
            ResultMenuPanel.Visibility = Visibility.Collapsed;
        }

        // ================= КНОПКИ ТА ОБРОБНИКИ =================
        private void BtnPlayGame_Click(object sender, RoutedEventArgs e)
        {
            BtnPlayGame.Visibility = Visibility.Collapsed;
            MapSelectionPanel.Visibility = Visibility.Visible;
        }

        private void BtnMapWinter_Click(object sender, RoutedEventArgs e) => StartGameOnMap("Winter");
        private void BtnMapForest_Click(object sender, RoutedEventArgs e) => StartGameOnMap("Forest");

        private void BtnSettings_Click(object sender, RoutedEventArgs e) => SettingsPanel.Visibility = Visibility.Visible;
        private void BtnCloseSettings_Click(object sender, RoutedEventArgs e)
        {
            if (ComboLaps.SelectedIndex == 0) _selectedLaps = 3;
            else if (ComboLaps.SelectedIndex == 1) _selectedLaps = 5;
            else _selectedLaps = 15;
            _autoPitLimiter = CheckAutoPit.IsChecked == true;
            SettingsPanel.Visibility = Visibility.Collapsed;
        }

        private void BtnRestart_Click(object sender, RoutedEventArgs e)
        {
            PauseMenuPanel.Visibility = Visibility.Collapsed;
            ResetGameState();
            _timer.Start();
        }

        private void BtnMainMenu_Click(object sender, RoutedEventArgs e)
        {
            PauseMenuPanel.Visibility = Visibility.Collapsed;
            _timer.Stop();
            ResetGameState();
            GameScreenGrid.Visibility = Visibility.Collapsed;
            MainMenuGrid.Visibility = Visibility.Visible;
            BtnPlayGame.Visibility = Visibility.Visible;
            _raceMusicPlayer.Stop();
            _menuMusicPlayer.Play();
        }

        private void BtnCarGarage_Click(object sender, RoutedEventArgs e)
        {
            CarGaragePanel.Visibility = Visibility.Visible;
            UpdateCarPreview();
        }

        private void UpdateCarPreview()
        {
            if (_carOptions.Count == 0 || _carIndex < 0 || _carIndex >= _carOptions.Count) return;
            string path = _carOptions[_carIndex].ImagePath ?? string.Empty;
            try { CarPreviewImage.Source = new BitmapImage(new Uri(path, UriKind.Relative)); }
            catch { }
        }

        private void BtnNextCar_Click(object sender, RoutedEventArgs e)
        {
            if (_carOptions.Count == 0) return;
            _carIndex++;
            if (_carIndex >= _carOptions.Count) _carIndex = 0;
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
            if (_carOptions.Count > 0)
            {
                _selectedCarTemplate = _carOptions[_carIndex];
                CarGaragePanel.Visibility = Visibility.Collapsed;
            }
        }

        // ================= КЕРУВАННЯ =================
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            // ПОВЕРНУТО: Пауза на клавішу Escape
            if (e.Key == Key.Escape)
            {
                if (_timer.IsEnabled)
                {
                    _timer.Stop();
                    PauseMenuPanel.Visibility = Visibility.Visible;
                    TxtStatus.Text = "ПАУЗА";
                }
                else if (PauseMenuPanel.Visibility == Visibility.Visible)
                {
                    PauseMenuPanel.Visibility = Visibility.Collapsed;
                    _timer.Start();
                }
                return; 
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
        
        private void BtnResultMainMenu_Click(object sender, RoutedEventArgs e)
        {
            ResultMenuPanel.Visibility = Visibility.Collapsed;
            _timer.Stop();
            ResetGameState();
            GameScreenGrid.Visibility = Visibility.Collapsed;
            MainMenuGrid.Visibility = Visibility.Visible;
            BtnPlayGame.Visibility = Visibility.Visible;
            _raceMusicPlayer.Stop();
            _menuMusicPlayer.Play();
        }
    }
}