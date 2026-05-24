namespace lab;

public class TrackData
{
    public string TrackName { get; set; }
    public List<NodePoint> Nodes { get; set; }
    public List<SegmentData> Segments { get; set; }
}

public class NodePoint
{
    public float X { get; set; }
    public float Y { get; set; }
}

public class SegmentData
{
    public string Type { get; set; }
    public int StartIndex { get; set; }
    public int EndIndex { get; set; }
    public float CornerLimit { get; set; }
}