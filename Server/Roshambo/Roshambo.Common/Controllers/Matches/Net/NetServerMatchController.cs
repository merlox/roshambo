using Chip.Net.Data;
using Roshambo.Common.Controllers.Matches.Net.Models;
using Roshambo.Common.Controllers.Matches.Net.Models.Events;
using Roshambo.Common.Controllers.Rounds;
using Roshambo.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roshambo.Common.Controllers.Matches.Net
{
    public class NetServerMatchController : INetMatchController
	{
		public EventHandler<IMatchController> MatchStartedEvent { get; set; }
		public EventHandler<(IMatchController Match, IRoundController Round)> RoundCreatedEvent { get; set; }
		public EventHandler<(IMatchController Match, PlayerState User)> UserJoinedEvent { get; set; }
		public EventHandler<(IMatchController Match, PlayerState User)> UserLeftEvent { get; set; }
		public EventHandler<(IMatchController Match, PlayerState Winner)> MatchEndedEvent { get; set; }
		public NetMatchControllerChannels Channels { get; }
		private LocalMatchController Source { get; }
		public MatchId Id => Source.Id;
		public MatchOptions Options => Source.Options;
		public MatchState State => Source.State;

		public IRoundController CurrentRound => Source.CurrentRound;

		public NetServerMatchController(LocalMatchController source, NetMatchControllerChannels channels) {
			this.Channels = channels;
			this.Source = source;

			this.Source.MatchStartedEvent += (_, e) => this.MatchStartedEvent?.Invoke(this, this);
			this.Source.UserJoinedEvent += (_, e) => this.UserJoinedEvent?.Invoke(this, (this, e.User));
			this.Source.UserLeftEvent += (_, e) => this.UserLeftEvent?.Invoke(this, (this, e.User));
			this.Source.MatchEndedEvent += (_, e) => this.MatchEndedEvent?.Invoke(this, (this, e.Winner));

			this.Source.RoundCreatedEvent += OnRoundCreated;
		}

		private void OnRoundCreated(object sender, (IMatchController Match, IRoundController Round) e) {
			e.Round.RoundEndedEvent += (s, ee) => OnRoundEnded(e.Match, e.Round);
			this.RoundCreatedEvent?.Invoke(this, (this, e.Round));
		}

		private void OnRoundEnded(IMatchController controller, IRoundController e) {
			this.Channels.RoundEndedEventChannel.Send(new RoundEndedEventModel() {
				RoundState = e.State,
				Match = controller.Id,
				State = controller.State
			});
		}

		public PlayerState AddPlayer(UserState user) {
			var newPlayer = this.Source.AddPlayer(user);
			return newPlayer;
		}
		
		public void StartMatch() {
			this.Source.StartMatch();
		}

		public void EndMatch() {
			this.Source.EndMatch();
		}

		#region Events
		public void HandleMatchStartedEvent(IncomingMessage<MatchStartedEventModel> message) {
			throw new NotImplementedException();
		}

		public void HandleMatchEndedEvent(IncomingMessage<MatchEndedEventModel> message) {
			throw new NotImplementedException();
		}

		public void HandleUserJoinedEvent(IncomingMessage<UserJoinedEventModel> message) {
			throw new NotImplementedException();
		}

		public void HandleUserLeftEvent(IncomingMessage<UserLeftEventModel> message) {
			throw new NotImplementedException();
		}

		public void HandleRoundCreatedEvent(IncomingMessage<RoundCreatedEventModel> message) {
			throw new NotImplementedException();
		}

		public void HandleRoundEndedEvent(IncomingMessage<RoundEndedEventModel> message) {
			throw new NotImplementedException();
		}
		#endregion

		#region Requests/Responses
		public void HandleStartMatchRequest(IncomingMessage<StartMatchRequest> message) {
			this.Source.StartMatch();
		}

		public void HandleEndMatchRequest(IncomingMessage<EndMatchRequest> message) {
			this.Source.EndMatch();
		}

		public void HandlePlayCardRequest(IncomingMessage<PlayCardRequest> message) {
			this.Source.CurrentRound.PlayCard(message.Sender.GetUserId(), message.Data.Card);
		}

		public void HandlePlayCardResponse(IncomingMessage<PlayCardResponse> message) {
			throw new NotImplementedException();
		}
		#endregion
	}
}
