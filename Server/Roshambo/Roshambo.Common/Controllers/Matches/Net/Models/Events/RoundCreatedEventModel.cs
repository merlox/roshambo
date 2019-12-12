using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chip.Net.Data;

namespace Roshambo.Common.Controllers.Matches.Net.Models.Events
{
    public class RoundCreatedEventModel : MatchMessageBase
    {
		public override void WriteTo(DataBuffer buffer) {
			base.WriteTo(buffer);
		}

		public override void ReadFrom(DataBuffer buffer) {
			base.ReadFrom(buffer);
		}
	}
}
