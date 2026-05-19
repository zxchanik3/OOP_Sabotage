using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace lab
{
    [JsonDerivedType(typeof(StraightSegment), typeDiscriminator: "Straight")]
    [JsonDerivedType(typeof(CornerSegment), typeDiscriminator: "Corner")]
    [JsonDerivedType(typeof(PitLaneSegment), typeDiscriminator: "PitLane")]
    [JsonDerivedType(typeof(StartFinishSegment), typeDiscriminator: "StartFinish")]
    public abstract class TrackSegment
    {
        public double Length { get; set; }
        public SegmentType Type { get; set; } 

        public TrackSegment(SegmentType type, double length)
        {
            Type = type;
            Length = length;
        }

        // Кожен сегмент сам вирішує, як змінити швидкість машини
        public abstract void ApplyEffect(Car car, ref double currentSpeed);
    }
    
    public class StraightSegment : TrackSegment
    {
        public StraightSegment(double length) : base(SegmentType.Straight, length) { }

        public override void ApplyEffect(Car car, ref double currentSpeed)
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

        public override void ApplyEffect(Car car, ref double currentSpeed)
        {
            // Обмеження швидкості в повороті залежить від шин
            double grip = Math.Max(car.Tyres.GripLevel, 10);
            double gripFactor = car.Tyres.GripLevel / 100.0;
            
            double cornerLimit = (300 * gripFactor) / Difficulty; 
            
            if (currentSpeed > cornerLimit)
            {
                currentSpeed = cornerLimit;
                car.Tyres.WearDown(); 
            }
            car.Tyres.WearDown();
        }
    }
    
    public class PitLaneSegment : TrackSegment
    {
        public PitLaneSegment(double length) : base(SegmentType.PitLane, length) { }

        public override void ApplyEffect(Car car, ref double currentSpeed)
        {
            if (currentSpeed > 80) currentSpeed = 80;
            
            if (car.Tyres.Durability < 40)
            {
                var oldType = car.Tyres.Type;
                
                car.ChangeTyres(TyreType.Soft);
                
                Console.WriteLine($"PIT-STOP {car.Model}: Заміна шин ({oldType} -> Soft).");
                
                currentSpeed = 5; 
            }
        }
    }
    
    public class StartFinishSegment : TrackSegment
    {
        public StartFinishSegment(double length) : base(SegmentType.StartFinish, length) { }

        public override void ApplyEffect(Car car, ref double currentSpeed)
        {
            if (currentSpeed < car.TopSpeed)
                currentSpeed += car.Acceleration;
        }
    }
}
