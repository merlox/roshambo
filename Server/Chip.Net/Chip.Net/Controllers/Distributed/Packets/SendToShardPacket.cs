using Chip.Net.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chip.Net.Controllers.Distributed.Packets {
	public class SendToShardPacket : Packet {
		public int UserId { get; set; }
		public byte PacketType { get; set; }
		public byte[] PacketData { get; set; }

		public override void WriteTo(DataBuffer buffer)
		{
			base.WriteTo(buffer);
			buffer.Write((int)UserId);
			buffer.Write((byte)PacketType);
			buffer.Write((byte[])PacketData);
		}

		public override void ReadFrom(DataBuffer buffer)
		{
			base.ReadFrom(buffer);
			UserId = buffer.ReadInt32();
			PacketType = buffer.ReadByte();
			PacketData = buffer.ReadByteArray();
		}
	}
}
