using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Roshambo.Common.Models;

namespace Roshambo.Common.Controllers.Rounds.Net
{
	public class NetClientRoundController : IRoundController
	{
		public EventHandler<IRoundController> RoundEndedEvent { get; set; }
		public EventHandler<IRoundController> RoundDrawEvent { get; set; }
		public EventHandler<(IRoundController Round, UserId User)> PlayerWinRoundEvent { get; set; }
		public EventHandler<(IRoundController Round, UserId User)> PlayerLoseRoundEvent { get; set; }

		public EventHandler<(MatchId Match, CardType Card)> CardPlayedEvent;
		private MatchId parentMatch;

		public RoundState State { get; set; }

		public NetClientRoundController(MatchId parentMatch) {
			this.parentMatch = parentMatch;
			this.State = new RoundState();
		}

		public bool PlayCard(UserId user, CardType card) {
			this.CardPlayedEvent?.Invoke(this, (parentMatch, card));
			return true;
		}
	}
}
