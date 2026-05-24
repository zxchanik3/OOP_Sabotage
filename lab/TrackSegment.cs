using System.Text.Json.Serialization;

namespace lab
{
    [JsonDerivedType(typeof(StraightSegment), typeDiscriminator: "Straight")]
    [JsonDerivedType(typeof(CornerSegment), typeDiscriminator: "Corner")]
    public abstract class TrackSegment
    {
        public float Length { get; init; }

        public TrackSegment(float length)
        {
            Length = length;
        }
    }
    
    public class StraightSegment : TrackSegment
    {
        public StraightSegment(float length) : base(length) { }
    }
    
    public class CornerSegment : TrackSegment
    {
        public int Difficulty { get; init; }

        public CornerSegment(float length, int difficulty) : base(length)
        {
            Difficulty = difficulty;
        }
    }
}