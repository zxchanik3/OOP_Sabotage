using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lab
{
    public class Race
    {
        public Track CurrentTrack { get; set; }

        public List<Driver> Participants { get; set; } = new List<Driver>();
        public List<Car> Cars { get; set; } = new List<Car>();

        public RaceStatus Status { get; set; }
        public Dictionary<string, double> Results { get; set; }

        public Race()
        {
            Status = RaceStatus.NotStarted;
            Results = new Dictionary<string, double>();
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

            Results.Add(driver.Name, 0.0);

            Console.WriteLine($"До гонки додано: {driver.Name} на {car.Model}");
        }

        public void StartRace()
        {
            if (Status == RaceStatus.NotStarted)
            {
                Status = RaceStatus.Active;
                Console.WriteLine("Гонка почалася!");
            }
            else
                Console.WriteLine($"Гонка вже має статус {Status}");
        }

        public void FinishRace()
        {
            if (Status == RaceStatus.Active)
            {
                var random = new Random();
                var keys = new List<string>(Results.Keys);

                for (int i = 0; i < keys.Count; i++)
                {
                    Results[keys[i]] = (random.NextDouble() * 30 + 60) * CurrentTrack.RequiredLapCount;
                }

                Status = RaceStatus.Finished;
                Console.WriteLine("Гонка завершена. Результати зібрано.");
            }
            else
            {
                Console.WriteLine($"Гонка не була активною, статус: {Status}.");
            }
        }
    }
}
