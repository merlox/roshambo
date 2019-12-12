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
	public class GetRoomListResponse : ISerializable
	{
		public List<RoomId> RoomList { get; set; }

		public void ReadFrom(DataBuffer buffer) {
			this.RoomList = new List<RoomId>();
			var ct = buffer.ReadInt32();
			for(int i = 0; i < ct; i++) {
				var id = new RoomId();
				id.ReadFrom(buffer);
				this.RoomList.Add(id);
			}
		}

		public void WriteTo(DataBuffer buffer) {
			buffer.Write((int)RoomList.Count);
			foreach (var room in RoomList) {
				room.WriteTo(buffer);
			}
		}
	}
}
