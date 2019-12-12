using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Chip.Net.Data {
	public class SerializedPacket : Packet {
		public override void WriteTo(DataBuffer buffer) {
			DynamicSerializer.Instance.Write(buffer, GetType(), this);
		}

		public override void ReadFrom(DataBuffer buffer) {
			DynamicSerializer.Instance.Read(buffer, GetType(), this);
		}
	}
}