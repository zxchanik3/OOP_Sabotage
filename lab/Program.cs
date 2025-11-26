using System.Collections.Generic;
using lab;

public class Program
{
    public static void Main()
    {
        Track monza = new Track("Monza", 50, 5.793, 0, 0, 3);
        monza.InitializeSegments();
        Console.WriteLine($"Трек '{monza.Name}' створено і скомпоновано з {monza.Segments.Count} сегментів.");
        
        Driver max = new Driver("Max Verstappen", 33, false);
        
        Tyre softs = new Tyre("Soft", 100, 10, 0.5f); 
        Car rb19 = new Car("Red Bull RB19", 2023, 900, softs, 15);
        
        Race grandPrix = new Race(monza);
        
        grandPrix.AddParticipant(max, rb19); 
        
        Console.WriteLine("\n--- Демонстрація роботи об'єктів ---");
        grandPrix.StartRace();
    }
}