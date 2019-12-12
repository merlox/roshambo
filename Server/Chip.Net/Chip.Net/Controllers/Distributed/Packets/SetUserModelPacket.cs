using Chip.Net.Controllers.Distributed.Models;
using Chip.Net.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chip.Net.Controllers.Distributed.Packets
{
    public class SetUserModelPacket<TUser> : Packet where TUser : IUserModel
    {
		public TUser Model { get; set; }

		public SetUserModelPacket() {
			Model = Activator.CreateInstance<TUser>();
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
