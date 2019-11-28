using System;

namespace Monik.Service
{
    public class Source
    {
        public short ID { get; set; }
        public DateTime Created { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public short? DefaultGroupID { get; set; }
    }
}