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
	public class MatchCreatedEventModel : RoomMessageBase, ISerializable
	{
		public MatchId MatchId { get; set; }
		public MatchOptions Options { get; set; }

		public override void ReadFrom(DataBuffer buffer) {
			base.ReadFrom(buffer);
			this.MatchId = new MatchId();
			this.MatchId.ReadFrom(buffer);

			this.Options = new MatchOptions();
			this.Options.ReadFrom(buffer);
		}

		public override void WriteTo(DataBuffer buffer) {
			base.WriteTo(buffer);
			this.MatchId.WriteTo(buffer);
			this.Options.WriteTo(buffer);
		}
	}
}
