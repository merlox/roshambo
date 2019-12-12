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
    public class CreateRoomRequest : ISerializable
    {
		public UserId Creator { get; set; }
        public RoomId Id { get; set; }

		public void ReadFrom(DataBuffer buffer) {
			this.Id = new RoomId();
			this.Id.ReadFrom(buffer);
		}

		public void WriteTo(DataBuffer buffer) {
			this.Id.WriteTo(buffer);
		}
	}
}
