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
	public class JoinMatchRequest : RoomMessageBase, ISerializable
	{
		public MatchId Match { get; set; }

		public override void ReadFrom(DataBuffer buffer) {
			base.ReadFrom(buffer);
			this.Match = new MatchId();
			this.Match.ReadFrom(buffer);
		}

		public override void WriteTo(DataBuffer buffer) {
			base.WriteTo(buffer);
			this.Match.WriteTo(buffer);
		}
	}
}
