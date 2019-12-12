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
	public class RoomEndedEventModel : RoomMessageBase, ISerializable
	{

		public override void ReadFrom(DataBuffer buffer) {
			base.ReadFrom(buffer);
		}

		public override void WriteTo(DataBuffer buffer) {
			base.WriteTo(buffer);
		}
	}
}
