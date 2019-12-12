using System;
using System.Collections.Generic;
using System.Text;

namespace Chip.Net.Data
{
	public class Packet : ISerializable {

		public virtual void ReadFrom(DataBuffer buffer) { }

		public virtual void WriteTo(DataBuffer buffer) { }
	}
}
