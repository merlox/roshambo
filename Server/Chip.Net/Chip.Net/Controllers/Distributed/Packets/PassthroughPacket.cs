using Chip.Net.Controllers.Distributed.Models;
using Chip.Net.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chip.Net.Controllers.Distributed.Packets {
	public class PassthroughPacket<T> : Packet where T : Packet {
		public T Data { get; set; }
		public int RecipientId { get; set; }

		public PassthroughPacket() {
			Data = Activator.CreateInstance<T>();
		}

		public PassthroughPacket(T Data, int recipient = -1) {
			this.Data = Data;
			this.RecipientId = recipient;
		}

		public PassthroughPacket(T Data, IShardModel shard) {
			this.RecipientId = shard.Id;
			this.Data = Data;
		}

		public PassthroughPacket(T Data, IUserModel user) {
			this.RecipientId = user.Id;
			this.Data = Data;
		}

		public override void WriteTo(DataBuffer buffer) {
			base.WriteTo(buffer);
			buffer.Write((int)RecipientId);
			Data.WriteTo(buffer);
		}

		public override void ReadFrom(DataBuffer buffer) {
			base.ReadFrom(buffer);
			RecipientId = buffer.ReadInt32();
			Data.ReadFrom(buffer);
		}
	}
}
