using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chip.Net.Data;
using Roshambo.Common.Controllers.Matches.Net.Models;
using Roshambo.Common.Controllers.Matches.Net.Models.Events;
using Roshambo.Common.Controllers.Rounds;
using Roshambo.Common.Controllers.Rounds.Net;
using Roshambo.Common.Models;

namespace Roshambo.Common.Controllers.Matches.Net
{
	public class NetClientMatchController : INetMatchController
	{
		public EventHandler<IMatchController> MatchStartedEvent { get; set; }
		public EventHandler<(IMatchController Match, IRoundController Round)> RoundCreatedEvent { get; set; }
		public EventHandler<(IMatchController Match, PlayerState User)> UserJoinedEvent { get; set; }
		public EventHandler<(IMatchController Match, PlayerState User)> UserLeftEvent { get; set; }
		public EventHandler<(IMatchController Match, PlayerState Winner)> MatchEndedEvent { get; set; }

		public MatchId Id { get; private set; }

		public MatchOptions Options { get; private set; }
		public NetMatchControllerChannels Channels { get; private set; }
		public MatchState State { get; private set; }

		public IRoundController CurrentRound { get; private set; }

		public NetClientMatchController(MatchId id, MatchOptions options, NetMatchControllerChannels channels) {
			this.Id = id;
			this.Options = options;
			this.Channels = channels;
			this.State = new MatchState();
		}

		#region Events
		public void HandleMatchStartedEvent(IncomingMessage<MatchStartedEventModel> message) {
			this.State = message.Data.State;
			this.MatchStartedEvent?.Invoke(this, this);
		}

		public void HandleMatchEndedEvent(IncomingMessage<MatchEndedEventModel> message) {
			this.State = message.Data.State;
			this.MatchEndedEvent?.Invoke(this, (this, message.Data.Winner));
		}

		public void HandleUserJoinedEvent(IncomingMessage<UserJoinedEventModel> message) {
			this.State = message.Data.State;
			this.UserJoinedEvent?.Invoke(this, (this, message.Data.User));
		}

		public void HandleUserLeftEvent(IncomingMessage<UserLeftEventModel> message) {
			this.State = message.Data.State;
			this.UserLeftEvent?.Invoke(this, (this, message.Data.User));
		}

		public void HandleRoundCreatedEvent(IncomingMessage<RoundCreatedEventModel> message) {
			this.State = message.Data.State;
			var round = new NetClientRoundController(this.Id);
			round.CardPlayedEvent += (s, e) => {
				this.Channels.PlayCardRequestChannel.Send(new PlayCardRequest() {
					Card = e.Card,
					Match = e.Match,
					State = new MatchState(),
				});
			};

			this.CurrentRound = round;
			this.RoundCreatedEvent?.Invoke(this, (this, this.CurrentRound));
		}

		public void HandleRoundEndedEvent(IncomingMessage<RoundEndedEventModel> message) {
			this.State = message.Data.State;
			this.CurrentRound.State.IsActive = false;
			if (message.Data.RoundState.Cards.Count == 0 || message.Data.RoundState.Winner == null) {
				this.CurrentRound.RoundDrawEvent?.Invoke(this, this.CurrentRound);
				return;
			}

			foreach(var card in message.Data.RoundState.Cards) {
				if (card.User.GUID == message.Data.RoundState.Winner.GUID)
					this.CurrentRound.PlayerWinRoundEvent?.Invoke(this, (this.CurrentRound, card.User));
				else
					this.CurrentRound.PlayerLoseRoundEvent?.Invoke(this, (this.CurrentRound, card.User));
			}
		}
		#endregion

		#region Requests/Responses
		public void HandleStartMatchRequest(IncomingMessage<StartMatchRequest> message) {
			throw new NotImplementedException();
		}

		public void HandleEndMatchRequest(IncomingMessage<EndMatchRequest> message) {
			throw new NotImplementedException();
		}

		public void HandlePlayCardRequest(IncomingMessage<PlayCardRequest> message) {
			throw new NotImplementedException();
		}

		public void HandlePlayCardResponse(IncomingMessage<PlayCardResponse> message) {
			this.State = message.Data.State;
			throw new NotImplementedException();
		}
		#endregion
	}
}
