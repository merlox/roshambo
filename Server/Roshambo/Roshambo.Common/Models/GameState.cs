using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roshambo.Common.Models
{
    public class RoomState
    {
        public enum State
		{
			New, Active, Ended
		}

		public State CurrentState { get; set; } 
		public List<UserState> Users { get; set; }
		public RoomId Id { get; set; }

		public RoomState() {
			this.Users = new List<UserState>();
			this.CurrentState = State.New;
		}
    }
}
