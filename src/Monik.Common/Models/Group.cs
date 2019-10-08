using System.Collections.Generic;

namespace Monik.Service
{
    public class Group
    {
        public short ID { get; set; }
        public string Name { get; set; }
        public bool IsDefault { get; set; }
        public string Description { get; set; }

        public List<int> Instances { get; set; } = new List<int>();
    }
}