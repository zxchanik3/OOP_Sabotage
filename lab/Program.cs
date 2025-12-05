using System.Collections.Generic;
using System.Text;
using lab;

public class Program
{
    public static void Main()
    {
        Console.OutputEncoding = Encoding.UTF8;
        
        Console.WriteLine("Тестовий Сценарій\n");
        
        // Адміністрування даних (GameData)

        Console.WriteLine("1. Менеджер Даних");
        GameData game = new GameData();

        Console.WriteLine("\nДодавання водіїв...");
        // Створення водіїв
        game.AddDriver(new Driver("Max Verstappen", 1, true)); // Lock = true
        game.AddDriver(new Driver("Lewis Hamilton", 44, false));
        game.AddDriver(new Driver("Charles Leclerc", 16, false));

        Console.WriteLine("\nДодавання автомобілів...");
        // Створення автомобілів (використовуємо новий конструктор з Car.cs)
        game.AddCar(new Car("RB20", "Red Bull", 2024, 1050, 8, 350, 798));
        game.AddCar(new Car("W15", "Mercedes", 2024, 1000, 7, 340, 812));
        game.AddCar(new Car("SF-24", "Ferrari", 2024, 1030, 7, 345, 800));

        Console.WriteLine("\nЗбереження та Завантаження...");
        game.DriverSaveToFile("drivers.json");
        game.CarSaveToFile("cars.json");
        
        Console.WriteLine("\nПеревірка завантажених даних:");
        Console.WriteLine("Водії:");
        foreach (var d in game.Drivers) Console.WriteLine($" - #{d.Number} {d.Name}, Lock: {d.Lock}");
        
        Console.WriteLine("Автомобілі:");
        foreach (var c in game.Cars) Console.WriteLine($" - {c.Model} ({c.Team}), {c.Horsepower} HP");

        
        // Етап 2: Підготовка до гонки (Ваш код: Агрегація та Композиція)
        Console.WriteLine("\n\n2. Підготовка треку та команд");

        // 1. Композиція: Створення Траси
        Track monza = new Track("Monza", 53, 5.793, 0, 0, 3);
        monza.InitializeSegments(); // Track сам створює свої сегменти
        Console.WriteLine($"\nТрек '{monza.Name}' підготовлено. Сегментів: {monza.Segments.Count} (Композиція).");

        // 2. Агрегація: Вибір учасників із бази даних GameData
        // Ми беремо об'єкти, які створив gamedata, і використовуємо їх у гонці.
        
        // Команда 1: Max + RB20
        Driver driver1 = game.Drivers.FirstOrDefault(d => d.Name.Contains("Verstappen"));
        Car car1 = game.Cars.FirstOrDefault(c => c.Model == "RB20");
        car1.ChangeTyres(TyreType.Soft); // шини

        // Команда 2: Lewis + W15
        Driver driver2 = game.Drivers.FirstOrDefault(d => d.Name.Contains("Hamilton"));
        Car car2 = game.Cars.FirstOrDefault(c => c.Model == "W15");
        car2.ChangeTyres(TyreType.Medium);

        Console.WriteLine($"\nОбрано учасників з бази даних:");
        Console.WriteLine($" 1. {driver1.Name} на {car1.Model} (Шини: {car1.Tyres.Type})");
        Console.WriteLine($" 2. {driver2.Name} на {car2.Model} (Шини: {car2.Tyres.Type})");
        
        // Етап 3: Гонка
        Console.WriteLine("\n\n3. Проведення Гонки");

        // Створення гонки (Агрегація з Track)
        Race italianGP = new Race(monza);

        // Додавання учасників (Агрегація з Driver та Car)
        italianGP.AddParticipant(driver1, car1);
        italianGP.AddParticipant(driver2, car2);

        italianGP.StartRace();

        // Симуляція фізики
        Console.WriteLine($"\nЗнос шин RB20 до: {car1.Tyres.Durability}%");
        car1.Tyres.WearDown();
        Console.WriteLine($"Знос шин RB20 після кола: {car1.Tyres.Durability}%");

        italianGP.FinishRace();

        Console.WriteLine("\nРезультати Гран-прі");
        foreach (var kvp in italianGP.Results)
        {
            string name = game.Drivers.FirstOrDefault(d => d.Number == kvp.Key)?.Name ?? "Unknown";
            Console.WriteLine($"#{kvp.Key} {name}: {kvp.Value:F3} сек");
        }
        
        // Етап 4: Очищення
        Console.WriteLine("\n\n4. Тест видалення");
        
        // Спроба видалити коли Lock = true)
        game.RemoveDriver(1); 
        
        // Видалення
        game.RemoveDriver(44);

        Console.WriteLine("\nФінальний список водіїв:");
        foreach (var d in game.Drivers) Console.WriteLine($" - #{d.Number} {d.Name}");
        
    }
}