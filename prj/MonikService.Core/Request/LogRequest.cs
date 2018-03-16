namespace MonikService.Core.Request
{
    public class LogRequest
    {
        public short[] Groups    { get; set; } = new short[0];
        public int[]   Instances { get; set; } = new int[0];

        public long? LastId         { get; set; }
        public byte? SeverityCutoff { get; set; }
        public byte? Level          { get; set; }
        public int?  Top            { get; set; }
    }
}