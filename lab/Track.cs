using System;
using System.Collections.Generic;

namespace lab
{
    public class Track
    {
        public string Name { get; set; }
        public int RequiredLapCount { get; set; }
        public double Length { get; set; }

        public int SectorsCount { get; set; }

        public List<TrackSegment> Segments { get; set; } = new();
        
        public Track() { }
        public Track(string name, int lapCount)
        {
            Name = name;
            RequiredLapCount = lapCount;
        }

        public double TotalDistance()
        {
            double distance = Length * RequiredLapCount;
            return distance;
        }

        public void DisplayTrackInfo()
        {
            Console.WriteLine($"Назва: {Name}");
            Console.WriteLine($"Довжина кола: {Length} км");
            Console.WriteLine($"Кількість кіл: {RequiredLapCount}");
            Console.WriteLine($"Секторів: {SectorsCount}");
        }
        
     
    }
}
