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
    public class JoinRoomResponse : ISerializable
	{
		public bool Success { get; set; }
		public RoomId Room { get; set; }

		public void ReadFrom(DataBuffer buffer) {
			this.Success = buffer.ReadByte() == 1;
			if (Success == false)
				return;

			this.Room = new RoomId();
			this.Room.ReadFrom(buffer);
		}

		public void WriteTo(DataBuffer buffer) {
			buffer.Write((byte)(Success ? 1 : 0));
			if (Success == false)
				return;

			this.Room.WriteTo(buffer);
		}
	}
}
