using Chip.Net;
using Chip.Net.Data;
using Roshambo.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roshambo.Common.Controllers.Matches.Net.Models
{
	public class MatchMessageBase : ISerializable
	{
		public MatchId Match { get; set; }
		public MatchState State { get; set; }

		public virtual void ReadFrom(DataBuffer buffer) {
			this.Match = new MatchId();
			this.Match.ReadFrom(buffer);

			this.State = new MatchState();
			this.State.ReadFrom(buffer);
		}

		public virtual void WriteTo(DataBuffer buffer) {
			this.Match.WriteTo(buffer);
			this.State.WriteTo(buffer);
		}
	}
}
