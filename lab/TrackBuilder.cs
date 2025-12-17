namespace lab;

public class TrackBuilder
{
    private Track _track;

    public TrackBuilder()
    {
        _track = new Track();
    }

    public TrackBuilder SetName(string name)
    {
        _track.Name = name;
        return this;
    }

    public TrackBuilder SetLaps(int laps)
    {
        _track.RequiredLapCount = laps;
        return this;
    }

    public TrackBuilder AddStraight(double length)
    {
        _track.Segments.Add(new StraightSegment(length));
        _track.Length += length;
        return this;
    }

    public TrackBuilder AddCorner(double length, int difficulty = 2)
    {
        _track.Segments.Add(new CornerSegment(length, difficulty));
        _track.Length += length;
        return this;
    }

    public TrackBuilder AddPitLane(double length)
    {
        _track.Segments.Add(new PitLaneSegment(length));
        _track.Length += length;
        return this;
    }
        
    public TrackBuilder AddStartFinish(double length)
    {
        _track.Segments.Add(new StartFinishSegment(length));
        _track.Length += length;
        return this;
    }

    public Track Build()
    {
        // Можна додати валідацію, чи замкнута траса, тощо
        _track.SectorsCount = _track.Segments.Count;
        return _track;
    }
}