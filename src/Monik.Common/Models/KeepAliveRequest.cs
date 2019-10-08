namespace Monik.Service
{
    public class KeepAliveRequest
    {
        public short[] Groups { get; set; } = new short[0];
        public int[] Instances { get; set; } = new int[0];
    }
}