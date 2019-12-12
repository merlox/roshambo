using Chip.Net;
using Chip.Net.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roshambo.Common.Models
{
    public class MatchState : ISerializable
    {
        public enum State
		{
			New, Active, RoundResults, Ended
		}

		public State CurrentState { get; set; }
		public List<PlayerState> PlayerList { get; set; }

		public MatchState() {
			this.CurrentState = State.New;
			this.PlayerList = new List<PlayerState>();
		}

		public void WriteTo(DataBuffer buffer) {
			buffer.Write((int)this.CurrentState);
			buffer.Write((int)this.PlayerList.Count);
			foreach (var p in PlayerList)
				p.WriteTo(buffer);
		}

		public void ReadFrom(DataBuffer buffer) {
			this.CurrentState = (State)buffer.ReadInt32();
			this.PlayerList = new List<PlayerState>();
			var ct = buffer.ReadInt32();
			for(int i = 0; i < ct; i ++) {
				PlayerState state = new PlayerState();
				state.ReadFrom(buffer);
				this.PlayerList.Add(state);
			}
		}
	}
}
