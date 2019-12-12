using Chip.Net;
using Chip.Net.Data;
using Roshambo.Common.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Roshambo.Common.Controllers.Rooms.Net.Models
{
	public class RoomMessageBase : ISerializable
	{
		public RoomId Id { get; set; }

		public virtual void ReadFrom(DataBuffer buffer) {
			this.Id = new RoomId();
			this.Id.ReadFrom(buffer);
		}

		public virtual void WriteTo(DataBuffer buffer) {
			this.Id.WriteTo(buffer);
		}
	}
}
