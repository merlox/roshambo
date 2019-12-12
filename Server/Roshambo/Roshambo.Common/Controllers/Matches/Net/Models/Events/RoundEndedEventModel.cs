using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chip.Net.Data;
using Roshambo.Common.Models;

namespace Roshambo.Common.Controllers.Matches.Net.Models.Events
{
    public class RoundEndedEventModel : MatchMessageBase
    {
		public RoundState RoundState { get; set; }

		public override void WriteTo(DataBuffer buffer) {
			base.WriteTo(buffer);
			RoundState.WriteTo(buffer);
		}

		public override void ReadFrom(DataBuffer buffer) {
			base.ReadFrom(buffer);
			RoundState = new RoundState();
			RoundState.ReadFrom(buffer);
		}
	}
}
