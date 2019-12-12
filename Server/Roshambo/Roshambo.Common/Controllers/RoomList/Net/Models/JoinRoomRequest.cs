using Chip.Net;
using Chip.Net.Data;
using Roshambo.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roshambo.Common.Controllers.RoomList.Net.Models
{
	public class JoinRoomRequest : ISerializable
	{
		public RoomId Room { get; set; }

		public void ReadFrom(DataBuffer buffer) {
			this.Room = new RoomId();
			this.Room.ReadFrom(buffer);
		}

		public void WriteTo(DataBuffer buffer) {
			this.Room.WriteTo(buffer);
		}
	}
}
