using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lab
{
    public class Track
    {
        public string Name { get; set; }
        public int RequiredLapCount { get; set; }
        public double Length { get; set; }

        public double StartLineX { get; set; }
        public double StartLineY { get; set; }

        public int SectorsCount { get; set; }

        public List<TrackSegment> Segments { get; set; } = new();
        
        public Track()
        {
            Name = "Default Track";
            RequiredLapCount = 1;
            Length = 0.0;
            StartLineX = 0.0;
            StartLineY = 0.0;
            SectorsCount = 0;
        }
        public Track(string name, int lapCount, double length, double startX, double startY, int secC)
        {
            Name = name;
            RequiredLapCount = lapCount;
            Length = length;
            StartLineX = startX;
            StartLineY = startY;
            SectorsCount = secC;
        }

        public double TotalDistance()
        {
            double distance = Length * RequiredLapCount;
            return distance;
        }

        public void DisplayTrackInfo()
        {
            Console.WriteLine($"\n--- Інформація про Трек ---");
            Console.WriteLine($"Назва: {Name}");
            Console.WriteLine($"Довжина кола: {Length} км");
            Console.WriteLine($"Кількість кіл: {RequiredLapCount}");
            Console.WriteLine($"Секторів: {SectorsCount}");
            Console.WriteLine($"Координати старту: ({StartLineX}, {StartLineY})");
        }
        
        public void InitializeSegments() 
        {
            Segments.Add(new TrackSegment(SegmentType.StartFinish, 1.0, 1.0, 0, 0.01f)); // Довга пряма
            Segments.Add(new TrackSegment(SegmentType.Corner, 0.5, 0.4, 150.0, 0.05f)); // Поворот
            Segments.Add(new TrackSegment(SegmentType.Straight, 2.0, 1.0, 0, 0.01f));  // Пряма
        }
    }
}
