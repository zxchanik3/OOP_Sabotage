namespace lab
{
    public class Track
    {
        public string Name { get; set; } = string.Empty;
        public int RequiredLapCount { get; set; }
        public double Length { get; set; }

        public int SectorsCount => Segments.Count;

        public List<TrackSegment> Segments { get; set; } = new();
    }
}
