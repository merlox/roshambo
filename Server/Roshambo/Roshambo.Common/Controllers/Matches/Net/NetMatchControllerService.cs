using Chip.Net;
using Chip.Net.Data;
using Chip.Net.Services;
using Roshambo.Common.Controllers.Matches.Net.Models;
using Roshambo.Common.Controllers.Matches.Net.Models.Events;
using Roshambo.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roshambo.Common.Controllers.Matches.Net
{
	public class NetMatchControllerService : NetService
	{
		public Dictionary<Guid, INetMatchController> MatchMap { get; private set; }
		public NetMatchControllerChannels Channels { get; private set; }

		public override void InitializeService(INetContext context) {
			base.InitializeService(context);
			MatchMap = new Dictionary<Guid, INetMatchController>();
			this.Channels = new NetMatchControllerChannels(this.Router);
			this.Channels.UserLeftEventChannel.Receive += OnUserLeftEventReceived;
			this.Channels.UserJoinedEventChannel.Receive += OnUserJoinedEventReceived;

			this.Channels.RoundEndedEventChannel.Receive += OnRoundEndedEventReceived;
			this.Channels.RoundCreatedEventChannel.Receive += OnRoundCreatedEventReceived;

			this.Channels.MatchStartedEventChannel.Receive += OnMatchStartedEventReceived;
			this.Channels.MatchEndedEventChannel.Receive += OnMatchEndedEventReceived;

			this.Channels.EndMatchRequestChannel.Receive += OnEndMatchRequestReceived;
			this.Channels.StartMatchRequestChannel.Receive += OnStartMatchRequestReceived;

			this.Channels.PlayCardRequestChannel.Receive += OnPlayCardRequestReceived;
			this.Channels.PlayCardResponseChannel.Receive += OnPlayCardResponseReceived;
		}

		private void OnUserLeftEventReceived(IncomingMessage<UserLeftEventModel> incoming) {
			if (this.MatchMap.ContainsKey(incoming.Data.Match.GUID) == false)
				return;

			var match = this.MatchMap[incoming.Data.Match.GUID];
			match.HandleUserLeftEvent(incoming);
		}

		private void OnUserJoinedEventReceived(IncomingMessage<UserJoinedEventModel> incoming) {
			if (this.MatchMap.ContainsKey(incoming.Data.Match.GUID) == false)
				return;

			var match = this.MatchMap[incoming.Data.Match.GUID];
			match.HandleUserJoinedEvent(incoming);
		}

		private void OnRoundEndedEventReceived(IncomingMessage<RoundEndedEventModel> incoming) {
			if (this.MatchMap.ContainsKey(incoming.Data.Match.GUID) == false)
				return;

			var match = this.MatchMap[incoming.Data.Match.GUID];
			match.HandleRoundEndedEvent(incoming);
		}

		private void OnRoundCreatedEventReceived(IncomingMessage<RoundCreatedEventModel> incoming) {
			if (this.MatchMap.ContainsKey(incoming.Data.Match.GUID) == false)
				return;

			var match = this.MatchMap[incoming.Data.Match.GUID];
			match.HandleRoundCreatedEvent(incoming);
		}

		private void OnMatchStartedEventReceived(IncomingMessage<MatchStartedEventModel> incoming) {
			if (this.MatchMap.ContainsKey(incoming.Data.Match.GUID) == false)
				return;

			var match = this.MatchMap[incoming.Data.Match.GUID];
			match.HandleMatchStartedEvent(incoming);
		}

		private void OnMatchEndedEventReceived(IncomingMessage<MatchEndedEventModel> incoming) {
			if (this.MatchMap.ContainsKey(incoming.Data.Match.GUID) == false)
				return;

			var match = this.MatchMap[incoming.Data.Match.GUID];
			match.HandleMatchEndedEvent(incoming);
		}

		private void OnEndMatchRequestReceived(IncomingMessage<EndMatchRequest> incoming) {
			if (this.MatchMap.ContainsKey(incoming.Data.Match.GUID) == false)
				return;

			var match = this.MatchMap[incoming.Data.Match.GUID];
			match.HandleEndMatchRequest(incoming);
		}

		private void OnStartMatchRequestReceived(IncomingMessage<StartMatchRequest> incoming) {
			if (this.MatchMap.ContainsKey(incoming.Data.Match.GUID) == false)
				return;

			var match = this.MatchMap[incoming.Data.Match.GUID];
			match.HandleStartMatchRequest(incoming);
		}

		private void OnPlayCardRequestReceived(IncomingMessage<PlayCardRequest> incoming) {
			if (this.MatchMap.ContainsKey(incoming.Data.Match.GUID) == false)
				return;

			var match = this.MatchMap[incoming.Data.Match.GUID];
			match.HandlePlayCardRequest(incoming);
		}

		private void OnPlayCardResponseReceived(IncomingMessage<PlayCardResponse> incoming) {
			if (this.MatchMap.ContainsKey(incoming.Data.Match.GUID) == false)
				return;

			var match = this.MatchMap[incoming.Data.Match.GUID];
			match.HandlePlayCardResponse(incoming);
		}

		public NetClientMatchController CreateClientMatchController(MatchId id, MatchOptions options) {
			var controller = new NetClientMatchController(id, options, this.Channels);
			this.MatchMap.Add(id.GUID, controller);

			return controller;
		}

		public NetServerMatchController CreateServerMatchController(LocalMatchController source) {
			var controller = new NetServerMatchController(source, this.Channels);
			this.MatchMap.Add(source.Id.GUID, controller);

			controller.MatchStartedEvent += (s, e) => {
				this.Channels.MatchStartedEventChannel.Send(new Models.Events.MatchStartedEventModel() {
					Match = e.Id,
					State = e.State,
				});
			};

			controller.MatchEndedEvent += (s, e) => {
				this.Channels.MatchEndedEventChannel.Send(new Models.Events.MatchEndedEventModel() {
					Match = e.Match.Id,
					State = e.Match.State,
					Winner = e.Winner
				});
			};

			controller.RoundCreatedEvent += (s, e) => {
				this.Channels.RoundCreatedEventChannel.Send(new Models.Events.RoundCreatedEventModel() {
					Match = e.Match.Id,
					State = e.Match.State,
				});
			};

			controller.UserJoinedEvent += (s, e) => {
				this.Channels.UserJoinedEventChannel.Send(new Models.Events.UserJoinedEventModel() {
					Match = e.Match.Id,
					State = e.Match.State,
					User = e.User
				});
			};

			controller.UserLeftEvent += (s, e) => {
				this.Channels.UserLeftEventChannel.Send(new Models.Events.UserLeftEventModel() {
					Match = e.Match.Id,
					State = e.Match.State,
					User = e.User
				});
			};

			return controller;
		}
	}
}
