using Chip.Net.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chip.Net.Services.NetTime
{
	public class P_GetNetTime : Packet { }

	public class P_SetNetTime : Packet {
		public double NetTime { get; set; }
		public override void WriteTo(DataBuffer buffer) {
			base.WriteTo(buffer);
			buffer.Write((double)NetTime);
		}

		public override void ReadFrom(DataBuffer buffer) {
			base.ReadFrom(buffer);
			NetTime = buffer.ReadDouble();
		}
	}
}
