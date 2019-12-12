using Chip.Net.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chip.Net.Services.SharedData {
	public class P_SetData : Packet {
		public double NetTime { get; set; }
		public byte[] Data { get; set; }
		public int Key { get; set; }
		public int UserId { get; set; }

		public override void WriteTo(DataBuffer buffer) {
			base.WriteTo(buffer);
			buffer.Write((double)NetTime);
			buffer.Write((byte[])Data);
			buffer.Write((int)Key);
			buffer.Write((int)UserId);
		}

		public override void ReadFrom(DataBuffer buffer) {
			base.ReadFrom(buffer);
			NetTime = buffer.ReadDouble();
			Data = buffer.ReadByteArray();
			Key = buffer.ReadInt32();
			UserId = buffer.ReadInt32();
		}
	}

	public class P_AddUser : Packet {
		public Dictionary<int, byte[]> Data { get; set; }
		public Dictionary<int, double> Times { get; set; }

		public P_AddUser() {
			Data = new Dictionary<int, byte[]>();
			Times = new Dictionary<int, double>();
		}

		public override void WriteTo(DataBuffer buffer) {
			base.WriteTo(buffer);
			buffer.Write((short)Data.Count);
			foreach(var kvp in Data) {
				buffer.Write((int)kvp.Key);
				buffer.Write((byte[])kvp.Value);
			}

			buffer.Write((short)Times.Count);
			foreach(var kvp in Times) {
				buffer.Write((int)kvp.Key);
				buffer.Write((double)kvp.Value);
			}
		}

		public override void ReadFrom(DataBuffer buffer) {
			base.ReadFrom(buffer);
			int count = buffer.ReadInt16();
			for(int i = 0; i < count; i++) {
				Data.Add(buffer.ReadInt32(), buffer.ReadByteArray());
			}

			count = buffer.ReadInt16();
			for(int i= 0; i < count; i++) {
				Times.Add(buffer.ReadInt32(), buffer.ReadDouble());
			}
		}
	}

	public class P_RemoveUser : Packet {
		public int Key { get; set; }

		public override void WriteTo(DataBuffer buffer) {
			base.WriteTo(buffer);
			buffer.Write((int)Key);
		}

		public override void ReadFrom(DataBuffer buffer) {
			base.ReadFrom(buffer);
			Key = buffer.ReadInt32();
		}
	}
}
