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
    public class GetMatchListResponse : RoomMessageBase, ISerializable
	{
		public List<(MatchId Id, MatchOptions Options)> MatchList { get; set; }

		public override void ReadFrom(DataBuffer buffer) {
			base.ReadFrom(buffer);
			MatchList = new List<(MatchId Id, MatchOptions Options)>();
			var ct = buffer.ReadInt32();
			for (int i = 0; i < ct; i++) {
				MatchId id = new MatchId();
				id.ReadFrom(buffer);
				MatchOptions options = new MatchOptions();
				options.ReadFrom(buffer);
				MatchList.Add((id, options));
			}
		}

		public override void WriteTo(DataBuffer buffer) {
			base.WriteTo(buffer);
			buffer.Write((int)MatchList.Count);
			foreach (var match in MatchList) {
				match.Id.WriteTo(buffer);
				match.Options.WriteTo(buffer);
			}
		}
	}
}
