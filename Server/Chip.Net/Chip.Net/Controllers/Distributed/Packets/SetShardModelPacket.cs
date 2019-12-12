using Chip.Net.Controllers.Distributed.Models;
using Chip.Net.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chip.Net.Controllers.Distributed.Packets
{
    public class SetShardModelPacket<TShard> : Packet where TShard : IShardModel
    {
		public TShard Model { get; set; }

		public SetShardModelPacket() {
			Model = Activator.CreateInstance<TShard>();
		}

		public override void WriteTo(DataBuffer buffer) {
			base.WriteTo(buffer);
			Model.WriteTo(buffer);
		}

		public override void ReadFrom(DataBuffer buffer) {
			base.ReadFrom(buffer);
			Model.ReadFrom(buffer);
		}
	}
}
