using System;
using System.Collections.Generic;

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
	}

	public class Group
	{
		public short ID { get; set; }
		public string Name { get; set; }
		public bool IsDefault { get; set; }
		public string Description { get; set; }

		public List<int> Instances { get; set; } = new List<int>();
	}

	public class Log_
	{
		public long ID { get; set; }
		public DateTime Created { get; set; }
		public DateTime Received { get; set; }
		public byte Level { get; set; }
		public byte Severity { get; set; }
		public int InstanceID { get; set; }
		public byte Format { get; set; }
		public string Body { get; set; }
		public string Tags { get; set; }
	}

	public class KeepAlive_
	{
		public long ID { get; set; }
		public DateTime Created { get; set; }
		public DateTime Received { get; set; }
		public int InstanceID { get; set; }
	}
}
