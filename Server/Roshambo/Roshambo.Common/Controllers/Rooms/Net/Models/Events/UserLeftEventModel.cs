using Chip.Net;
using Chip.Net.Data;
using Roshambo.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roshambo.Common.Controllers.Rooms.Net.Models.Events
{
	public class UserLeftEventModel : RoomMessageBase, ISerializable
	{
		public UserState User { get; set; }

		public override void ReadFrom(DataBuffer buffer) {
			base.ReadFrom(buffer);
			User = new UserState();
			User.ReadFrom(buffer);
		}

		public override void WriteTo(DataBuffer buffer) {
			base.WriteTo(buffer);
			User.WriteTo(buffer);
		}
	}
}
