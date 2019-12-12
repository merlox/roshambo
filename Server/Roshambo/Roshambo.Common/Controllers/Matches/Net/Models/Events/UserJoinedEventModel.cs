using Chip.Net;
using Chip.Net.Data;
using Roshambo.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roshambo.Common.Controllers.Matches.Net.Models.Events
{
	public class UserJoinedEventModel : MatchMessageBase
	{
		public PlayerState User { get; set; }

		public override void ReadFrom(DataBuffer buffer) {
			base.ReadFrom(buffer);

			if (buffer.ReadByte() == 1) {
				this.User = new PlayerState();
				this.User.ReadFrom(buffer);
			}
		}

		public override void WriteTo(DataBuffer buffer) {
			base.WriteTo(buffer);
			buffer.Write((byte)(this.User == null ? 0 : 1));
			if (this.User != null)
				this.User.WriteTo(buffer);
		}
	}
}
