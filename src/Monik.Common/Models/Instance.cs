using System;
using System.Collections.Concurrent;

namespace Monik.Service
{
    public class Instance
    {
        public int ID { get; set; }
        public DateTime Created { get; set; }
        public short SourceID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        private Source FSourceRef = null;

        public Source SourceRef()
        {
            return FSourceRef;
        }

        public void SourceRef(Source aSrc)
        {
            FSourceRef = aSrc;
        }

        public readonly ConcurrentDictionary<string, IMetricObject> Metrics = new ConcurrentDictionary<string, IMetricObject>();
    }
}