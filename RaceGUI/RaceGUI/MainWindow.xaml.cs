using lab;
using System.IO;
using System.Numerics;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace RaceGUI
{
    public partial class MainWindow : Window
    {
        private readonly GameData _gameData = new();
        private readonly string _dataFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");

        private readonly List<Car> _cars = new();
        private readonly Dictionary<Car, Image> _carVisuals = new();
        private List<Car> _carOptions = new();
        private int _carIndex;
        private Car? _selectedCarTemplate;

        private readonly UserDriver _userDriver = new("Player", 77);
        private NpcDriver? _botDumb;
        private NpcDriver? _botSmart;

        private readonly Track _track = new() { Name = "GP", RequiredLapCount = 5 };
        private readonly List<TrackNode> _trackNodes = new();
        private readonly List<Vector2> _pitRoute = new();
        private readonly DispatcherTimer _timer = new();

        private int _selectedLaps = 3;
        private string _selectedTrackName = "GP";
        private bool _trackHasPitStop = true;
        private bool _autoPitLimiter = true;

        private readonly AudioManager _audioManager = new();
        private readonly RaceEngine _raceEngine = new();
        private readonly PitStopManager _pitStopManager = new();
        private ParticleManager _particleManager = null!;

        public MainWindow()
        {
            InitializeComponent();
            InitializeGame();
            
            _particleManager = new ParticleManager(GameCanvas);
            _audioManager.PlayMenuMusic();

            _timer.Interval = TimeSpan.FromMilliseconds(16);
            _timer.Tick += Timer_Tick;

            _pitStopManager.PitTick += (_) => TxtPitStatus.Visibility = Visibility.Visible;
            _pitStopManager.PitCompleted += () =>
            {
                TxtPitStatus.Visibility = Visibility.Collapsed;
                if (_cars.Count > 0) _cars[0].ChangeTyres(TyreType.Medium);
            };
        }

        private void InitializeGame()
        {
            Directory.CreateDirectory(_dataFolder);
            _gameData.AddCar(new CarConfig("Car1", "Blue", 2020, 670, 3, 265, 780, "Images/car1.png"));
            _gameData.AddCar(new CarConfig("Car2", "Green", 2021, 720, 2, 280, 750, "Images/car2.png"));
            _gameData.AddCar(new CarConfig("Car3", "Purple", 2019, 650, 4, 260, 800, "Images/car3.png"));
            _gameData.AddCar(new CarConfig("Car4", "Red", 2022, 700, 3.5f, 270, 770, "Images/car4.png"));
            _gameData.AddCar(new CarConfig("Car5", "DarkBlue", 2023, 750, 2.5f, 285, 730, "Images/car5.png"));

            _gameData.CarSaveToFile(Path.Combine(_dataFolder, "CarsData.json")); 
            _gameData.CarLoadFromFile(Path.Combine(_dataFolder, "CarsData.json"));
            
            _carOptions = _gameData.Cars
                .Select(c => new Car(c.Model, c.Team, c.Year, c.Horsepower, (int)c.Acceleration, c.TopSpeed, c.Weight, c.ImagePath))
                .ToList();

            _selectedCarTemplate = _carOptions.FirstOrDefault();
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            if (_cars.Count == 0 || _raceEngine.IsRaceFinished) return;

            for (int i = 0; i < _cars.Count; i++)
            {
                var car = _cars[i];
                CarInput input = new CarInput();

                if (_raceEngine.IsCarFinished(car))
                {
                    car.Speed *= 0.88f;
                    if (MathF.Abs(car.Speed) < 1f) car.Speed = 0f;
                    car.Update(new CarInput(), 0.07f);
                    UpdateCarVisual(car);
                    continue;
                }
                else if (i == 0) input = _userDriver.GetInput(car, 0.07f);
                else if (i == 1 && _botDumb != null) input = _botDumb.GetInput(car, 0.07f);
                else if (i == 2 && _botSmart != null) input = _botSmart.GetInput(car, 0.07f);

                car.Update(input, 0.07f);

                float padding = 15f;
                float clampedX = Math.Max(padding, Math.Min(car.Position.X, (float)GameCanvas.ActualWidth - padding));
                float clampedY = Math.Max(padding, Math.Min(car.Position.Y, (float)GameCanvas.ActualHeight - padding));
                if (Vector2.Distance(car.Position, new Vector2(clampedX, clampedY)) > 0.0001f)
                {
                    car.SetPosition(new Vector2(clampedX, clampedY), car.Direction);
                }

                int closestNodeIndex = GetClosestNodeIndex(car.Position);
                if (closestNodeIndex >= 0)
                {
                    bool finished = _raceEngine.CheckLapAndCheckpoints(car, i, closestNodeIndex, _trackNodes.Count, _track.RequiredLapCount);
                    if (finished)
                    {
                        _timer.Stop();
                        TxtStatus.Text = "ФІНІШ!";
                        int place = _raceEngine.FinishOrder.IndexOf(car) + 1;
                        TxtResultPlace.Text = $"Місце: {place}";
                        TimeSpan ts = TimeSpan.FromSeconds(_raceEngine.RaceTime);
                        TxtResultTime.Text = $"Час: {ts:mm\\:ss\\.ff}";
                        ResultMenuPanel.Visibility = Visibility.Visible;
                        return;
                    }

                    Vector2 closestNodePos = _trackNodes[closestNodeIndex].Position;
                    float distToTrack = Vector2.Distance(car.Position, closestNodePos);

                    if (distToTrack > 45f) 
                    {
                        if (car.Speed > 30f) _particleManager.SpawnDust(car.Position, _selectedTrackName);
                        car.Speed *= 0.94f; 
                    }

                    if (distToTrack > 55f)
                    {
                        Vector2 pushDir = Vector2.Normalize(closestNodePos - car.Position);
                        car.SetPosition(car.Position + pushDir * 3f, car.Direction);
                    }
                }

                if (_trackHasPitStop)
                {
                    _pitStopManager.HandlePitStop(car, i);
                }
                else if (car.Tyres.Durability < 100) 
                {
                    car.ChangeTyres(car.Tyres.Type);
                }

                UpdateCarVisual(car);
            }

            _particleManager.UpdateParticles();

            if (!_raceEngine.IsRaceFinished && _cars.Count > 0)
            {
                _raceEngine.UpdateTime(0.025f);
                TimeSpan ts = TimeSpan.FromSeconds(_raceEngine.RaceTime);
                TxtTimer.Text = $"Час: {ts:mm\\:ss\\.ff}";
                
                var playerCar = _cars[0];
                string tyreText = _trackHasPitStop ? $"{playerCar.Tyres.Durability}%" : "—";
                int currentLap = _raceEngine.CarLaps.ContainsKey(playerCar) ? _raceEngine.CarLaps[playerCar] : 1;
                TxtStatus.Text = $"Коло: {currentLap}/{_track.RequiredLapCount} | Швидкість: {Math.Abs((int)playerCar.Speed)} км/год | Шини: {tyreText}";
            }
        }

        private int GetClosestNodeIndex(Vector2 pos)
        {
            if (_trackNodes.Count == 0) return -1;
            int best = 0;
            float bestDist = float.MaxValue;
            for (int i = 0; i < _trackNodes.Count; i++)
            {
                float d = Vector2.Distance(pos, _trackNodes[i].Position);
                if (d < bestDist) { bestDist = d; best = i; }
            }
            return best;
        }

        private void UpdateCarVisual(Car car)
        {
            if (!_carVisuals.TryGetValue(car, out var img)) return;
            Canvas.SetLeft(img, car.Position.X - img.Width / 2);
            Canvas.SetTop(img, car.Position.Y - img.Height / 2);
            double angle = Math.Atan2(car.Direction.Y, car.Direction.X) * 180 / Math.PI;
            img.RenderTransform = new RotateTransform(angle); 
        }

        private async void StartGameOnMap(string mapType)
        {
            _selectedTrackName = mapType;
            _trackHasPitStop = (_selectedTrackName == "Winter");
            _track.RequiredLapCount = _selectedLaps;

            string uriPath = _selectedTrackName == "Winter" ? "/Images/WinterMap.jpg" : "/Images/ForestMap.jpg";
            GameCanvas.Background = new ImageBrush { ImageSource = new BitmapImage(new Uri($"pack://application:,,,{uriPath}")), Stretch = Stretch.Fill };

            string filePath = $"{mapType}.json";
            if (File.Exists(filePath))
            {
                string jsonText = File.ReadAllText(filePath);
                TrackData? data = JsonSerializer.Deserialize<TrackData>(jsonText);
                if (data != null)
                {
                    _trackNodes.Clear();
                    _pitRoute.Clear();
                    TrackSegment defaultSeg = new StraightSegment(100);
                    int mainNodesCount = _trackHasPitStop ? data.Nodes.Count - 3 : data.Nodes.Count;

                    for (int i = 0; i < mainNodesCount; i++)
                    {
                        _trackNodes.Add(new TrackNode(new Vector2(data.Nodes[i].X, data.Nodes[i].Y), defaultSeg));
                    }

                    if (_trackHasPitStop && data.Nodes.Count >= 3)
                    {
                        for (int i = data.Nodes.Count - 3; i < data.Nodes.Count; i++)
                        {
                            _pitRoute.Add(new Vector2(data.Nodes[i].X, data.Nodes[i].Y));
                        }
                    }
                    
                    foreach (var seg in data.Segments)
                    {
                        TrackSegment logic = seg.Type == "Straight" ? new StraightSegment(100) : new CornerSegment(seg.CornerLimit, 2);
                        for (int i = seg.StartIndex; i <= seg.EndIndex && i < _trackNodes.Count; i++) _trackNodes[i].Logic = logic;
                    }
                }
            }

            MainMenuGrid.Visibility = Visibility.Collapsed;
            MapSelectionPanel.Visibility = Visibility.Collapsed;
            GameScreenGrid.Visibility = Visibility.Visible;

            _audioManager.PlayRaceMusic();
            SpawnCars(); 
            ResetGameState();

            _timer.Stop(); 
            for (int i = 3; i > 0; i--)
            {
                TxtStatus.Text = $"СТАРТ ЧЕРЕЗ: {i}";
                await Task.Delay(1000); 
            }
            TxtStatus.Text = "МАРШ!";
            _timer.Start(); 
        }

        private void SpawnCars()
        {
            var pCfg = _selectedCarTemplate ?? _carOptions.FirstOrDefault();
            if (pCfg == null) return;

            var playerCar = new Car(pCfg.Model, pCfg.Team, pCfg.Year, pCfg.Horsepower, pCfg.Acceleration, pCfg.TopSpeed, pCfg.Weight, pCfg.ImagePath);
            var dumbCar = new Car("Bot1", "AI", 2020, 680, 3, 290, 790, "Images/car6.png");
            var smartCar = new Car("Bot2", "AI", 2021, 710, 3, 290, 760, "Images/car7.png");

            _cars.Clear();
            _cars.Add(playerCar);
            _cars.Add(dumbCar);
            _cars.Add(smartCar);

            foreach (var image in _carVisuals.Values.ToList()) if (GameCanvas.Children.Contains(image)) GameCanvas.Children.Remove(image);
            _carVisuals.Clear();

            foreach (var c in _cars)
            {
                var img = new Image { Width = 40, Height = 20, RenderTransformOrigin = new Point(0.5, 0.5) };
                try { img.Source = new BitmapImage(new Uri(c.ImagePath, UriKind.RelativeOrAbsolute)); } catch { }
                Panel.SetZIndex(img, 100);
                _carVisuals[c] = img;
                GameCanvas.Children.Add(img);
            }
        }

        private void ResetGameState()
        {
            if (_cars.Count < 3 || _trackNodes.Count < 2) return;
            
            _raceEngine.Reset();
            _pitStopManager.Reset();
            _particleManager.Clear();

            TxtTimer.Text = "Час: 00:00.00";
            ResultMenuPanel.Visibility = Visibility.Collapsed;

            var mainRoute = _trackNodes.Select(n => n.Position).ToList();

            _botDumb = new NpcDriver("Dumb", 6, new DumbStrategy(mainRoute));

            if (_selectedTrackName == "Forest")
            {
                _botSmart = new NpcDriver("Smart", 7, new DumbStrategy(mainRoute));
            }
            else
            {
                _botSmart = new NpcDriver("Smart", 7, new SmartStrategy(mainRoute, _pitRoute));
            }

            Vector2 startPos = _trackNodes[0].Position;
            Vector2 startDir = Vector2.Normalize(_trackNodes[1].Position - _trackNodes[0].Position);
            
            _cars[0].SetPosition(new Vector2(startPos.X, startPos.Y - 15), startDir);
            _cars[1].SetPosition(new Vector2(startPos.X - 40, startPos.Y + 15), startDir);
            _cars[2].SetPosition(new Vector2(startPos.X - 80, startPos.Y - 15), startDir);

            foreach (var car in _cars)
            {
                car.Speed = 0f;
                car.ChangeTyres(TyreType.Medium); 
                _raceEngine.RegisterCarInRace(car);
                UpdateCarVisual(car);
            }
            
            TxtPitStatus.Visibility = Visibility.Collapsed;
        }

        private void BtnPlayGame_Click(object sender, RoutedEventArgs e) { BtnPlayGame.Visibility = Visibility.Collapsed; MapSelectionPanel.Visibility = Visibility.Visible; }
        private void BtnMapWinter_Click(object sender, RoutedEventArgs e) => StartGameOnMap("Winter");
        private void BtnMapForest_Click(object sender, RoutedEventArgs e) => StartGameOnMap("Forest");
        private void BtnSettings_Click(object sender, RoutedEventArgs e) => SettingsPanel.Visibility = Visibility.Visible;
        
        private void BtnCloseSettings_Click(object sender, RoutedEventArgs e)
        {
            _selectedLaps = ComboLaps.SelectedIndex switch
            {
                0 => 3,
                1 => 5,
                _ => 15
            };
            _autoPitLimiter = CheckAutoPit.IsChecked == true;
            _pitStopManager.AutoPitLimiter = _autoPitLimiter;
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
            GameScreenGrid.Visibility = Visibility.Collapsed;
            MainMenuGrid.Visibility = Visibility.Visible;
            BtnPlayGame.Visibility = Visibility.Visible;
            _audioManager.PlayMenuMusic();
        }

        private void BtnResultMainMenu_Click(object sender, RoutedEventArgs e)
        {
            ResultMenuPanel.Visibility = Visibility.Collapsed;
            _timer.Stop();
            GameScreenGrid.Visibility = Visibility.Collapsed;
            MainMenuGrid.Visibility = Visibility.Visible;
            BtnPlayGame.Visibility = Visibility.Visible;
            _audioManager.PlayMenuMusic();
        }

        private void BtnCarGarage_Click(object sender, RoutedEventArgs e) { CarGaragePanel.Visibility = Visibility.Visible; UpdateCarPreview(); }
        
        private void UpdateCarPreview()
        {
            if (_carOptions.Count == 0 || _carIndex < 0 || _carIndex >= _carOptions.Count) return;
            try { CarPreviewImage.Source = new BitmapImage(new Uri(_carOptions[_carIndex].ImagePath, UriKind.Relative)); } catch { }
        }
        
        private void BtnNextCar_Click(object sender, RoutedEventArgs e) { if (_carOptions.Count > 0) { _carIndex = (_carIndex + 1) % _carOptions.Count; UpdateCarPreview(); } }
        private void BtnPrevCar_Click(object sender, RoutedEventArgs e) { if (_carOptions.Count > 0) { _carIndex = (_carIndex - 1 + _carOptions.Count) % _carOptions.Count; UpdateCarPreview(); } }
        private void BtnSelectCar_Click(object sender, RoutedEventArgs e) { if (_carOptions.Count > 0) { _selectedCarTemplate = _carOptions[_carIndex]; CarGaragePanel.Visibility = Visibility.Collapsed; } }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                if (_timer.IsEnabled) { _timer.Stop(); PauseMenuPanel.Visibility = Visibility.Visible; TxtStatus.Text = "ПАУЗА"; }
                else if (PauseMenuPanel.Visibility == Visibility.Visible) { PauseMenuPanel.Visibility = Visibility.Collapsed; _timer.Start(); }
                return; 
            }
            if (_pitStopManager.IsPitServing) return;
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
            switch (e.Key)
            {
                case Key.W: case Key.Up: _userDriver.Release(lab.Button.Forward); break;
                case Key.S: case Key.Down: _userDriver.Release(lab.Button.Backward); break;
                case Key.A: case Key.Left: _userDriver.Release(lab.Button.Left); break;
                case Key.D: case Key.Right: _userDriver.Release(lab.Button.Right); break;
            }
        }
    }
}