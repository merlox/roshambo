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
    public class CreateRoomResponse : ISerializable
    {
		public bool Success { get; set; }
		public RoomId Id { get; set; }

		public void ReadFrom(DataBuffer buffer) {
			this.Success = buffer.ReadByte() == 1;
			this.Id = new RoomId();
			this.Id.ReadFrom(buffer);
		}

		public void WriteTo(DataBuffer buffer) {
			buffer.Write((byte)(Success ? 1 : 0));
			this.Id.WriteTo(buffer);
		}
	}
}
