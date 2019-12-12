using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chip.Net.Data;
using Roshambo.Common.Models;

namespace Roshambo.Common.Controllers.Matches.Net.Models
{
    public class PlayCardRequest : MatchMessageBase
    {
		public CardType Card { get; set; }

		public override void WriteTo(DataBuffer buffer) {
			base.WriteTo(buffer);
		}

		public override void ReadFrom(DataBuffer buffer) {
			base.ReadFrom(buffer);
		}
	}
}
