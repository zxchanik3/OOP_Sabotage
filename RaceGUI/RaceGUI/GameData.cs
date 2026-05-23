using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace lab
{
    public class CarConfig
    {
        public string Model { get; set; } = "";
        public string Team { get; set; } = "";
        public int Year { get; set; }
        public int Horsepower { get; set; }
        public float Acceleration { get; set; }
        public int TopSpeed { get; set; }
        public int Weight { get; set; }
        public string ImagePath { get; set; } = "";
    }
    public class DriverConfig
    {
        public string Name { get; set; } = "";
        public int Number { get; set; }
        public bool Lock { get; set; }
    }
    public class TrackNodeConfig
    {
        public float X { get; set; }
        public float Y { get; set; }
    }
    public class TrackConfig
    {
        public string Name { get; set; } = "";
        public int Laps { get; set; }
        public List<TrackNodeConfig> Nodes { get; set; } = new();
    }
    public class GameData
    {
        public List<DriverConfig> Drivers { get; private set; } = new();
        public List<CarConfig> Cars { get; private set; } = new();
        public List<TrackConfig> Tracks { get; private set; } = new();

        public GameData() { }

        #region Drivers

        public void AddDriver(DriverConfig driver)
        {
            if (Drivers.Any(d => d.Number == driver.Number))
            {
                Console.WriteLine($"Номер {driver.Number} вже зайнятий.");
                return;
            }

            Drivers.Add(driver);
            Console.WriteLine($"Гонщика додано: {driver.Name}");
        }

        public void RemoveDriver(int number)
        {
            var driver = Drivers.FirstOrDefault(d => d.Number == number);

            if (driver == null)
            {
                Console.WriteLine("Гонщика з таким номером не знайдено.");
                return;
            }

            if (driver.Lock)
            {
                Console.WriteLine($"Неможливо видалити стандартного гонщика ({driver.Name}).");
                return;
            }

            Drivers.Remove(driver);
            Console.WriteLine($"Гонщика {driver.Name} видалено.");
        }

        public void DriverSaveToFile(string path)
        {
            Save(Drivers, path);
        }

        public void DriverLoadFromFile(string path)
        {
            Drivers = Load<List<DriverConfig>>(path) ?? new();
        }

        #endregion

        #region Cars

        public void AddCar(CarConfig car)
        {
            Cars.Add(car);
            Console.WriteLine($"Додано автомобіль: {car.Model}");
        }

        public void RemoveCar(string model)
        {
            var car = Cars.FirstOrDefault(c => c.Model == model);
            if (car == null)
            {
                Console.WriteLine("Автомобіль не знайдено.");
                return;
            }

            Cars.Remove(car);
        }

        public void CarSaveToFile(string path)
        {
            Save(Cars, path);
        }

        public void CarLoadFromFile(string path)
        {
            Cars = Load<List<CarConfig>>(path) ?? new();
        }

        #endregion

        #region Tracks

        public void AddTrack(TrackConfig track)
        {
            Tracks.Add(track);
            Console.WriteLine($"Додано трек: {track.Name}");
        }

        public void RemoveTrack(string name)
        {
            var track = Tracks.FirstOrDefault(t => t.Name == name);
            if (track == null)
            {
                Console.WriteLine("Трек не знайдено.");
                return;
            }

            Tracks.Remove(track);
        }

        public void TrackSaveToFile(string path)
        {
            Save(Tracks, path);
        }

        public void TrackLoadFromFile(string path)
        {
            Tracks = Load<List<TrackConfig>>(path) ?? new();
        }

        #endregion

        #region Generic JSON

        private void Save<T>(T data, string path)
        {
            string json = JsonSerializer.Serialize(data, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            File.WriteAllText(path, json);
        }

        private T? Load<T>(string path)
        {
            if (!File.Exists(path))
                return default;

            string json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<T>(json);
        }

        #endregion
    }

}
