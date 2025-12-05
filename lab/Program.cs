using System.Collections.Generic;

using System;

namespace lab
{
    class Program
    {
        static void Main()
        {
            GameData game = new GameData();

            Console.WriteLine("ADDING DRIVERS------------");

            game.AddDriver(new Driver("Max Verstappen", 1, true));
            game.AddDriver(new Driver("Lewis Hamilton", 44, false));
            game.AddDriver(new Driver("Charles Leclerc", 16, false));

            Console.WriteLine("\nADDING CARS---------------");

            game.AddCar(new Car("RB20", "Red Bull", 2024, 1050, 8, 350, 798));
            game.AddCar(new Car("W15", "Mercedes", 2024, 1000, 7, 340, 812));
            game.AddCar(new Car("SF-24", "Ferrari", 2024, 1030, 7, 345, 800));

            Console.WriteLine("\nSAVING----------------");

            game.DriverSaveToFile("drivers.json");
            game.CarSaveToFile("cars.json");

            Console.WriteLine("\nLOADING----------------");

            game.DriverLoadFromFile("drivers.json");
            game.CarLoadFromFile("cars.json");

            Console.WriteLine("\nCHECK LOADED DATA---------------");

            Console.WriteLine("Drivers:");
            foreach (var d in game.Drivers) Console.WriteLine($"#{d.Number} {d.Name}, Lock: {d.Lock}");

            Console.WriteLine("\nCars:");
            foreach (var c in game.Cars) Console.WriteLine($"{c.Model}, {c.Year}, HP: {c.Horsepower}");

            game.RemoveDriver(1);
            game.RemoveDriver(2);
            Console.WriteLine("Drivers:");
            foreach (var d in game.Drivers) Console.WriteLine($"#{d.Number} {d.Name}, Lock: {d.Lock}");
        }
    }
}
