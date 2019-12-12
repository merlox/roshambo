using Roshambo.Common.Controllers.Rounds;
using Roshambo.Common.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roshambo.Common.Controllers.Matches
{
    public class LocalMatchController : IMatchController
    {
		public EventHandler<IMatchController> MatchStartedEvent { get; set; }
		public EventHandler<(IMatchController Match, IRoundController Round)> RoundCreatedEvent { get; set; }
		public EventHandler<(IMatchController Match, PlayerState User)> UserJoinedEvent { get; set; }
		public EventHandler<(IMatchController Match, PlayerState User)> UserLeftEvent { get; set; }
		public EventHandler<(IMatchController Match, PlayerState Winner)> MatchEndedEvent { get; set; }

		public MatchId Id { get; private set; }
		public MatchOptions Options { get; private set; }
		public MatchState State { get; private set; }
		public IRoundController CurrentRound { get; private set; }

		private MatchState.State previousState;
		private Stopwatch stateTimer;
		private int round;

		public LocalMatchController(MatchOptions options) {
			this.stateTimer = new Stopwatch();
			this.previousState = MatchState.State.New;
			this.stateTimer.Start();

			this.State = new MatchState();
			this.State.CurrentState = MatchState.State.New;

			this.Id = new MatchId();
			this.Id.Name = options.Name;

			this.Options = options;
			this.round = 0;
		}

		public PlayerState AddPlayer(UserState user) {
			if (this.State.PlayerList.Any(i => i.Id.GUID == user.Id.GUID))
				return this.State.PlayerList.First(i => i.Id.GUID == user.Id.GUID);

			var player = new PlayerState();
			player.PlayerHand = new HandState();
			player.PlayerHand.Put(CardType.Rock, user.UserHand.Take(CardType.Rock, 3));
			player.PlayerHand.Put(CardType.Paper, user.UserHand.Take(CardType.Paper, 3));
			player.PlayerHand.Put(CardType.Scissors, user.UserHand.Take(CardType.Scissors, 3));
			player.Stars = user.Stars;
			player.Id = user.Id;

			this.State.PlayerList.Add(player);
			this.UserJoinedEvent?.Invoke(this, (this, player));
			return player;
		}

		public bool RemovePlayer(UserId id) {
			if (this.State.CurrentState != MatchState.State.New)
				return false;

			this.State.PlayerList = this.State.PlayerList.Where(i => i.Id.GUID != id.GUID).ToList();
			return true;
		}

		public void StartMatch() {
			if (this.State.CurrentState != MatchState.State.New)
				return;

			this.State.CurrentState = MatchState.State.Active;
			this.MatchStartedEvent?.Invoke(this, this);
		}

		private void StartRound() {
			this.CurrentRound = new LocalRoundController();
			this.CurrentRound.PlayerWinRoundEvent += (s, e) => this.State.PlayerList.First(i => i.Id.GUID == e.User.GUID).Stars++;
			this.CurrentRound.PlayerLoseRoundEvent += (s, e) => this.State.PlayerList.First(i => i.Id.GUID == e.User.GUID).Stars--;
			this.CurrentRound.RoundEndedEvent += OnRoundEnded;
			this.State.CurrentState = MatchState.State.Active;
			this.RoundCreatedEvent?.Invoke(this, (this, this.CurrentRound));
			this.round++;
		}

		private void OnRoundEnded(object sender, IRoundController e) {
			this.State.CurrentState = MatchState.State.RoundResults;
			if (e.State.Winner == null)
				return;
		}

		public void UpdateMatch() {
			if (this.State.CurrentState == MatchState.State.Ended)
				return;

			if(this.previousState != this.State.CurrentState) {
				if (this.previousState == MatchState.State.Ended)
					return;

				this.previousState = this.State.CurrentState;
				this.stateTimer.Restart();
			}

			switch(this.State.CurrentState) {
				case MatchState.State.Active:
					if(this.CurrentRound == null || this.CurrentRound.State.IsActive == false) {
						if(this.stateTimer.Elapsed.TotalSeconds > 1f) {
							this.StartRound();
							this.stateTimer.Restart();
						}
					}
					break;

				case MatchState.State.RoundResults:
					if (this.stateTimer.Elapsed.TotalSeconds > 3f) {
						if (this.Options.Style == MatchStyle.Rounds && this.round >= this.Options.Rounds) {
							EndMatch();
							return;
						}

						if (this.State.PlayerList.Any(i => i.PlayerHand.HasCards == false)) {
							EndMatch();
							return;
						}

						if (this.State.PlayerList.Any(i => i.Stars == 0)) {
							EndMatch();
							return;
						}

						this.StartRound();
						this.stateTimer.Restart();
					}
					break;
			}
		}

		public void EndMatch() {
			if (this.State.CurrentState == MatchState.State.Ended)
				return;

			this.State.CurrentState = MatchState.State.Ended;
			this.MatchEndedEvent?.Invoke(this, (this, null));
		}
    }
}
