using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using lab;

public class Program
{
    public static void Main()
    {
        Console.OutputEncoding = Encoding.UTF8;
        Console.WriteLine("=== F1 MANAGER 2025: FINAL BUILD ===\n");
        
        Console.WriteLine("[1] Завантаження бази даних...");
        GameData game = new GameData();

        // Створюємо водіїв (використовуємо різні стратегії, якщо треба, але тут базові)
        var driver1 = new NPCDriver("Max Verstappen", 1, new AttackStrategy());
        var driver2 = new NPCDriver("Lewis Hamilton", 44, new DefenseStrategy());
        var driver3 = new NPCDriver("Charles Leclerc", 16, new NormalStrategy());

        game.AddDriver(driver1);
        game.AddDriver(driver2);
        game.AddDriver(driver3);

        // Створюємо боліди
        var car1 = new Car("RB20", "Red Bull", 2024, 10, 20, 350, 798);
        var car2 = new Car("W15", "Mercedes", 2024, 980, 20, 340, 800);
        var car3 = new Car("SF-24", "Ferrari", 2024, 990, 20, 345, 798);
        
        // Взуваємо шини (стратегія шин)
        car1.ChangeTyres(TyreType.Soft);   // Швидкі, але швидко зношуються
        car2.ChangeTyres(TyreType.Hard);   // Повільні, але живучі
        car3.ChangeTyres(TyreType.Medium); // Баланс

        game.AddCar(car1);
        game.AddCar(car2);
        game.AddCar(car3);


        // --- ЧАСТИНА 2: TRACK BUILDING (Твій код: Builder) ---
        Console.WriteLine("\n[2] Будівництво траси...");
        
        TrackBuilder builder = new TrackBuilder();
        // Будуємо складну трасу, щоб перевірити піт-стопи
        Track monza = builder
            .SetName("Monza GP")
            .SetLaps(15) // Достатньо кіл, щоб Soft стерся
            .AddStartFinish(1.0)
            .AddStraight(2.0)
            .AddCorner(0.5, difficulty: 2)
            .AddStraight(1.5)
            .AddCorner(0.3, difficulty: 3) // Важкий поворот
            .AddPitLane(0.4) // Піт-лейн є на трасі
            .Build();

        monza.DisplayTrackInfo();


        // --- ЧАСТИНА 3: RACING (Твій код: Simulation) ---
        Console.WriteLine("\n[3] Підготовка до гонки...");
        
        Race grandPrix = new Race(monza);
        grandPrix.AddParticipant(driver1, car1);
        grandPrix.AddParticipant(driver2, car2);
        grandPrix.AddParticipant(driver3, car3);

        // Запуск
        grandPrix.StartRace();
        
        // --- ЧАСТИНА 4: РЕЗУЛЬТАТИ ---
        Console.WriteLine("\n--- 🏆 РЕЗУЛЬТАТИ ГРАН-ПРІ ---");
        
        // Виводимо відсортовані результати з Race
        var leaderboard = grandPrix.Results.OrderBy(r => r.Value);
        
        int pos = 1;
        foreach (var entry in leaderboard)
        {
            var dr = game.Drivers.First(d => d.Number == entry.Key);
            var cr = (dr == driver1) ? car1 : (dr == driver2) ? car2 : car3; // спрощений пошук авто
            
            Console.WriteLine($"{pos}. {dr.Name} [Time: {entry.Value:F2}]");
            Console.WriteLine($"   - Шини на фініші: {cr.Tyres.Type} (Знос: {cr.Tyres.Durability}%)");
            Console.WriteLine($"   - Статистика: Wins={dr.Wins}, Podiums={dr.Podiums}");
            pos++;
        }

        Console.WriteLine("\nСимуляція завершена успішно.");
    }
}