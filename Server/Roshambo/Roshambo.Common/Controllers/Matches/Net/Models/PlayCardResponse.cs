using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chip.Net.Data;

namespace Roshambo.Common.Controllers.Matches.Net.Models
{
    public class PlayCardResponse : MatchMessageBase
    {
		public enum Result
		{
			Accepted,
			TimerExpired,
			CardAlreadySelected,
			NoActiveRound,
		}

		public Result ResultCode { get; set; }

		public override void WriteTo(DataBuffer buffer) {
			base.WriteTo(buffer);
			buffer.Write((int)this.ResultCode);
		}

		public override void ReadFrom(DataBuffer buffer) {
			base.ReadFrom(buffer);
			this.ResultCode = (Result)buffer.ReadInt32();
		}
	}
}
