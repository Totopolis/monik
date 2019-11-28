namespace Monik.Service
{
    public class MetricRequest
    {
        public short[] Groups { get; set; } = new short[0];
        public int[] Instances { get; set; } = new int[0];
    }
}