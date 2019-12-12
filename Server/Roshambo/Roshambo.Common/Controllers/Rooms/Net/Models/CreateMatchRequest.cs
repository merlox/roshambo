using Chip.Net;
using Chip.Net.Data;
using Roshambo.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roshambo.Common.Controllers.Rooms.Net.Models
{
	public class CreateMatchRequest : RoomMessageBase, ISerializable
	{
		public MatchOptions Options { get; set; }

		public override void ReadFrom(DataBuffer buffer) {
			base.ReadFrom(buffer);
			Options = new MatchOptions();
			Options.ReadFrom(buffer);
		}

		public override void WriteTo(DataBuffer buffer) {
			base.WriteTo(buffer);
			Options.WriteTo(buffer);
		}
	}
}
