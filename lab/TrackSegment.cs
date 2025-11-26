using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lab
{
    public class TrackSegment
    {
        public double Length { get; set; }
        public SegmentType Type { get; set; }
        
        // 1.0 для Straight, 0.5 для Corner, 0.0 для PitLane (тоді Car лише гальмує до MinSpeedLimit)
        public double AccelerationFactor { get; set; }
        
        // якщо це поворот, машина гальмує до цього значення; 
        // якщо це PitLane, вона не може перевищувати це значення.
        public double MinSpeedLimit { get; set; } 
        
        public float WearFactor { get; set; } // наскільки сильно зношуються шини на цій ділянці, типу на поворотах вище

        public TrackSegment(SegmentType type, double length, double accelFactor, double minSpeedLimit, float wearFactor)
        {
            Type = type;
            Length = length;
            AccelerationFactor = accelFactor;
            MinSpeedLimit = minSpeedLimit;
            WearFactor = wearFactor;
        }
    }
}
