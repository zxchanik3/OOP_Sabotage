using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace lab
{
    public class GameData
    {
        public List<Driver> Drivers { get; private set; } = new();
        public List<Car> Cars { get; private set; } = new();
        public List<Track> Tracks { get; private set; } = new();

        public GameData() { }

        // Add a driver if the number is not already used.
        public void AddDriver(Driver driver)
        {
            if (Drivers.Any(d => d.Number == driver.Number))
            {
                Console.WriteLine($"Number {driver.Number} is already taken.");
                return;
            }

            Drivers.Add(driver);
            Console.WriteLine($"Driver added: {driver.Name}");
        }

        // Remove a driver by racing number.
        public void RemoveDriver(int number)
        {
            var driver = Drivers.FirstOrDefault(d => d.Number == number);

            if (driver == null)
            {
                Console.WriteLine("Driver with that number not found.");
                return;
            }

            if (driver.Lock)
            {
                Console.WriteLine($"Cannot remove locked/default driver ({driver.Name}).");
                return;
            }

            Drivers.Remove(driver);
            Console.WriteLine($"Driver {driver.Name} removed.");
        }

        //Save drivers list to file.
        public void DriverSaveToFile(string path)
        {
            string json = JsonSerializer.Serialize(Drivers, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(path, json);
            Console.WriteLine("Drivers saved.");
        }

        // Load drivers list from file, or create empty list if file missing.
        public void DriverLoadFromFile(string path)
        {
            if (!File.Exists(path))
            {
                Console.WriteLine("File does not exist.");
                Drivers = new List<Driver>();
                return;
            }

            string json = File.ReadAllText(path);
            Drivers = JsonSerializer.Deserialize<List<Driver>>(json) ?? new List<Driver>();
            Console.WriteLine("Drivers loaded.");
        }

        // Add a car to the collection.
        public void AddCar(Car car)
        {
            Cars.Add(car);
            Console.WriteLine($"Car {car.Model} added.");
        }

        //Remove the car that matches the model.
        public void RemoveCar(string model)
        {
            var car = Cars.FirstOrDefault(c => c.Model == model);
            if (car == null)
            {
                Console.WriteLine("Car cannot be deleted (not found).");
                return;
            }
            Cars.Remove(car);
            Console.WriteLine($"Car {car.Model} deleted.");
        }

        //Save cars list to file.
        public void CarSaveToFile(string path)
        {
            string json = JsonSerializer.Serialize(Cars, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(path, json);
            Console.WriteLine("Cars saved.");
        }

        // Load cars list from file, or create empty list if file missing.
        public void CarLoadFromFile(string path)
        {
            if (!File.Exists(path))
            {
                Console.WriteLine("File does not exist.");
                Cars = new List<Car>();
                return;
            }
            string json = File.ReadAllText(path);
            Cars = JsonSerializer.Deserialize<List<Car>>(json) ?? new List<Car>();
            Console.WriteLine("Cars loaded.");
        }

        // Add a track to the collection.
        public void AddTrack(Track track)
        {
            Tracks.Add(track);
            Console.WriteLine($"Track {track.Name} added.");
        }

        //Remove the track that matches the name.
        public void RemoveTrack(string name)
        {
            var track = Tracks.FirstOrDefault(t => t.Name == name);
            if(track == Null)
            {
                Console.WriteLine("Track cannot be deleted (not found)");
            }
            Tracks.Remove(track);
            Console.WriteLine($"Track {track.Name} deleted.");
        }

        //Save tracks to file.
        public void TrackSaveToFile(string path)
        {
            string json = JsonSerializer.Serialize(Tracks, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(path, json);
            Console.WriteLine("Tracks saved.");
        }

        //Load tracks from file, or create empty list if file missing.
        
        public void TrackLoadFromFile(string path)
        {
            if (!File.Exists(path))
            {
                Console.WriteLine("File does not exist.");
                Tracks = new List<Track>();
                return;
            }
            string json = File.ReadAllText(path);
            Tracks = JsonSerializer.Deserialize<List<Track>>(json) ?? new List<Track>();
            Console.WriteLine("Tracks loaded.");
        }

    }
}
