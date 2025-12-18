using System;
using System.Collections.Generic;
using System.Linq;

namespace lab
{
    public class Race
    {
        public Track CurrentTrack { get; set; }

        private List<Driver> Participants { get; set; } = new List<Driver>();
        private List<Car> Cars { get; set; } = new List<Car>();

        public RaceStatus Status { get; set; }
        public Dictionary<int, double> Results { get; private set; }

        public Race()
        {
            Status = RaceStatus.NotStarted;
            Results = new Dictionary<int, double>();
        }

        public Race(Track track) : this()
        {
            CurrentTrack = track;
        }

        public void AddParticipant(Driver driver, Car car)
        {
            if (Participants.Contains(driver))
            {
                Console.WriteLine($"Учасник '{driver.Name}' вже в гонці!");
                return;
            }
            Participants.Add(driver);
            Cars.Add(car);

            if (!Results.ContainsKey(driver.Number))
            {
                Results.Add(driver.Number, 0.0);
            }

            Console.WriteLine($"До гонки додано: {driver.Name} на {car.Model}");
        }

        public void StartRace()
        {
            Console.WriteLine($"\n--- Старт гонки на трасі {CurrentTrack.Name}! ---");
            Console.WriteLine($"Дистанція: {CurrentTrack.RequiredLapCount} кіл.");
            Status = RaceStatus.Active;
            
            for (int i = 0; i < Participants.Count; i++)
            {
                var driver = Participants[i];
                var car = Cars[i];
                double totalTime = 0;
                double currentSpeed = 0;
                
                for (int lap = 1; lap <= CurrentTrack.RequiredLapCount; lap++)
                {
                    // Цикл по сегментах траси
                    foreach (var segment in CurrentTrack.Segments)
                    {
                        // Ми передаємо currentSpeed як ref, щоб сегмент міг її змінити
                        segment.ApplyEffect(car, ref currentSpeed);
                        
                        double speedForCalc = Math.Max(currentSpeed, 10.0); 
                        double timeOnSegment = segment.Length / speedForCalc;
                        
                        totalTime += timeOnSegment;
                    }
                }

                Results[driver.Number] = totalTime;
            }
            
            FinishRace();
        }

        public void FinishRace()
        {
            Status = RaceStatus.Finished;
            Console.WriteLine("\n🏁 ГОНКА ЗАВЕРШЕНА! Підрахунок результатів...");
            
            var sortedResults = Results.OrderBy(x => x.Value).ToList();
            
            for (int i = 0; i < sortedResults.Count; i++)
            {
                int driverNum = sortedResults[i].Key;
                var driver = Participants.Find(d => d.Number == driverNum);

                if (driver != null)
                {
                    driver.Position = i + 1;
                    driver.Races++;

                    if (driver.Position == 1) 
                        driver.Wins++;
                    
                    if (driver.Position <= 3) 
                        driver.Podiums++;
                }
            }
        }
    }
}
