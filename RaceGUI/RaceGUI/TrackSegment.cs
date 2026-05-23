using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace lab
{
    [JsonDerivedType(typeof(StraightSegment), typeDiscriminator: "Straight")]
    [JsonDerivedType(typeof(CornerSegment), typeDiscriminator: "Corner")]
    public abstract class TrackSegment
    {
        public double Length { get; set; }
        public SegmentType Type { get; set; } 

        public TrackSegment(SegmentType type, double length)
        {
            Type = type;
            Length = length;
        }

        public abstract void ApplyEffect(Car car, ref double currentSpeed, float dT);
    }
    
    public class StraightSegment : TrackSegment
    {
        public StraightSegment(double length) : base(SegmentType.Straight, length) { }

        public override void ApplyEffect(Car car, ref double currentSpeed, float dT)
        {
            double acceleration = car.Acceleration * 2.0; 
            currentSpeed += acceleration;
            
            if (currentSpeed > car.TopSpeed) 
                currentSpeed = car.TopSpeed;
        }
    }
    
    public class CornerSegment : TrackSegment
    {
        public int Difficulty { get; set; } // 1 - легкий, 3 - важкий

        public CornerSegment(double length, int difficulty) : base(SegmentType.Corner, length)
        {
            Difficulty = difficulty;
        }

        public override void ApplyEffect(Car car, ref double currentSpeed, float dT)
        {
            double grip = Math.Max(car.Tyres.GripLevel, 10);
            double gripFactor = car.Tyres.GripLevel / 100.0;
            
            double cornerLimit = (300 * gripFactor) / Difficulty; 
            
            if (currentSpeed > cornerLimit)
            {
                currentSpeed = cornerLimit;
                car.Tyres.WearDown((float)currentSpeed, dT); 
            }
            car.Tyres.WearDown((float)currentSpeed, dT);
        }
    }
}
