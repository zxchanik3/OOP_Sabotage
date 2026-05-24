namespace lab;

public class TrackData
{
    public string TrackName { get; init; } = string.Empty;
    public List<NodePoint> Nodes { get; init; } = new();
    public List<SegmentData> Segments { get; init; } = new();
}

public class NodePoint
{
    public float X { get; init; }
    public float Y { get; init; }
}

public class SegmentData
{
    public string Type { get; init; } = string.Empty;
    public int StartIndex { get; init; }
    public int EndIndex { get; init; }
    public float CornerLimit { get; init; }
}