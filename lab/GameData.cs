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
        public List<Tyre> Tyres { get; private set; } = new();

        public void AddDriver(Driver driver)
        {
            // ��������, �� ����� ������
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
            string json = JsonSerializer.Serialize(Drivers, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(path, json);
            Console.WriteLine("Дані збережено.");
        }

        public void DriverLoadFromFile(string path)
        {
            if (!File.Exists(path))
            {
                Console.WriteLine("Файл не знайдено.");
                Drivers = new List<Driver>();
                return;
            }

            string json = File.ReadAllText(path);
            Drivers = JsonSerializer.Deserialize<List<Driver>>(json) ?? new List<Driver>();
            Console.WriteLine("Дані завантажено.");
        }

        public void AddCar(Car car)
        {
            Cars.Add(car);
            Console.WriteLine($"Додано автомобіль: {car.Model}");
        }

        public void RemoveCar(string model)
        {
            var car = Cars.FirstOrDefault(c => c.Model == model);
            if (car == null)
            {
                Console.WriteLine("Автомобіль з такою моделлю не знайдено.");
                return;
            }
            Cars.Remove(car);
            Console.WriteLine($"Автомобіль {car.Model} видалено.");
        }

        public void CarSaveToFile(string path)
        {
            string json = JsonSerializer.Serialize(Cars, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(path, json);
            Console.WriteLine("Дані збережено.");
        }

        public void CarLoadFromFile(string path)
        {
            if (!File.Exists(path))
            {
                Console.WriteLine("Файл не знайдено.");
                Cars = new List<Car>();
                return;
            }
            string json = File.ReadAllText(path);
            Cars = JsonSerializer.Deserialize<List<Car>>(json) ?? new List<Car>();
            Console.WriteLine("Дані завантажено.");
        }

        public void AddTyre(Tyre tyre)
        {
            Tyres.Add(tyre);
            Console.WriteLine($"Додано шину типу: {tyre.Type}");
        }

        public void RemoveTyre(string type)
        {
            var tyre = Tyres.FirstOrDefault(t => t.Type == type);
            if (tyre == null)
            {
                Console.WriteLine("Шину такого типу не знайдено.");
                return;
            }
            Tyres.Remove(tyre);
            Console.WriteLine($"Шину типу {tyre.Type} видалено.");
        }

        public void TyreSaveToFile(string path)
        {
            string json = JsonSerializer.Serialize(Tyres, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(path, json);
            Console.WriteLine("Дані збережено.");
        }

        public void TyreLoadFromFile(string path)
        {
            if (!File.Exists(path))
            {
                Console.WriteLine("Файл не знайдено.");
                Tyres = new List<Tyre>();
                return;
            }
            string json = File.ReadAllText(path);
            Tyres = JsonSerializer.Deserialize<List<Tyre>>(json) ?? new List<Tyre>();
            Console.WriteLine("Дані завантажено.");
        }
    }
}
