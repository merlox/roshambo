using Chip.Net;
using Chip.Net.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roshambo.Common.Controllers.Matches.Net.Models
{
	public class StartMatchRequest : MatchMessageBase
	{
		public override void ReadFrom(DataBuffer buffer) {
			base.ReadFrom(buffer);
		}

		public override void WriteTo(DataBuffer buffer) {
			base.WriteTo(buffer);
		}
	}
}
