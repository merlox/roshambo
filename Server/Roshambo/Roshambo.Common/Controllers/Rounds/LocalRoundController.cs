using Roshambo.Common.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roshambo.Common.Controllers.Rounds
{
    public class LocalRoundController : IRoundController
	{
		public EventHandler<IRoundController> RoundEndedEvent { get; set; }
		public EventHandler<IRoundController> RoundDrawEvent { get; set; }
		public EventHandler<(IRoundController Round, UserId User)> PlayerWinRoundEvent { get; set; }
		public EventHandler<(IRoundController Round, UserId User)> PlayerLoseRoundEvent { get; set; }

		public RoundState State { get; set; }

		public LocalRoundController() {
			this.State = new RoundState();
			this.State.IsActive = true;
		}

		public bool PlayCard(UserId user, CardType card) {
			if (this.State.IsActive == false)
				return false;

			if (this.State.Cards.Any(i => i.User.GUID == user.GUID))
				return false;

			this.State.Cards.Add((user, card));
			if(this.State.Cards.Count > 1) {
				this.Determine();
			}

			return true;
		}

		private void Determine() {
			//if (this.State.Cards.Count == 0) {
			//	this.RoundDrawEvent?.Invoke(this, this);
			//	this.RoundEndedEvent?.Invoke(this, this);
			//	this.State.IsActive = false;
			//	return;
			//}

			//if(this.State.Cards.Count == 1) {
			//	this.PlayerWinRoundEvent?.Invoke(this, (this, this.State.Cards.First().User));
			//	this.RoundEndedEvent?.Invoke(this, this);
			//	this.State.IsActive = false;
			//	return;
			//}

			var first = this.State.Cards.First();
			var second = this.State.Cards.Skip(1).First();

			if(first.Card == second.Card) {
				this.RoundDrawEvent?.Invoke(this, this);
				this.RoundEndedEvent?.Invoke(this, this);
				this.State.IsActive = false;
				return;
			}

			if(first.Card == CardType.None) {
				this.State.Winner = second.User;
			}

			if(second.Card == CardType.None) {
				this.State.Winner = first.User;
			}

			if (first.Card == CardType.Rock) {
				if (second.Card == CardType.Paper) this.State.Winner = first.User;
				if (second.Card == CardType.Scissors) this.State.Winner = second.User;
			}

			if (first.Card == CardType.Paper) {
				if (second.Card == CardType.Rock) this.State.Winner = first.User;
				if (second.Card == CardType.Scissors) this.State.Winner = second.User;
			}

			if (first.Card == CardType.Scissors) {
				if (second.Card == CardType.Paper) this.State.Winner = first.User;
				if (second.Card == CardType.Rock) this.State.Winner = second.User;
			}

			if (this.State.Winner == null) {
				this.RoundDrawEvent?.Invoke(this, this);
			} else {
				this.PlayerWinRoundEvent?.Invoke(this, (this, this.State.Winner));
				foreach (var user in this.State.Cards)
					if (user.User.GUID != this.State.Winner.GUID)
						this.PlayerLoseRoundEvent?.Invoke(this, (this, user.User));
			}

			this.RoundEndedEvent?.Invoke(this, this);
			this.State.IsActive = false;
		}
    }
}
