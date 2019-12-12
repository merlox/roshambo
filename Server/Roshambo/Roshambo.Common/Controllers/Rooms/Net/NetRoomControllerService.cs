using Chip.Net;
using Chip.Net.Data;
using Chip.Net.Services;
using Roshambo.Common.Controllers.Matches.Net;
using Roshambo.Common.Controllers.Rooms.Net.Models;
using Roshambo.Common.Controllers.Rooms.Net.Models.Events;
using Roshambo.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roshambo.Common.Controllers.Rooms.Net
{
    public class NetRoomControllerService : NetService
    {
		public Dictionary<Guid, INetRoomController> RoomMap { get; private set; }
		public NetRoomControllerChannels Channels { get; private set; }

		public override void InitializeService(INetContext context) {
			base.InitializeService(context);
			this.RoomMap = new Dictionary<Guid, INetRoomController>();

			this.Channels = new NetRoomControllerChannels(this.Router);
			this.Channels.RoomStartedEventChannel.Receive += OnRoomStartedEvent;
			this.Channels.RoomEndedEventChannel.Receive += OnRoomEndedEvent;
			this.Channels.StartRoomRequestChannel.Receive += OnStartRoomRequest;
			this.Channels.CloseRoomRequestChannel.Receive += OnCloseRoomRequest;

			this.Channels.MatchCreatedEventChannel.Receive += OnMatchCreatedEvent;
			this.Channels.CreateMatchRequestChannel.Receive += OnCreateMatchRequest;

			this.Channels.GetMatchListRequestChannel.Receive += OnGetMatchListRequest;
			this.Channels.GetMatchListResponseChannel.Receive += OnGetMatchListResponse;

			this.Channels.UserJoinedEventChannel.Receive += OnUserJoinedEvent;
			this.Channels.UserLeftEventChannel.Receive += OnUserLeftEvent;
			this.Channels.JoinMatchRequestChannel.Receive += OnUserJoinMatchRequest;
		}

		public NetClientRoomController CreateClientRoomController(RoomId roomId, UserState localUser) {
			var controller = new NetClientRoomController(localUser, roomId, this.Channels, Context.Services.Get<NetMatchControllerService>());
			this.RoomMap.Add(roomId.GUID, controller);
			return controller;
		}

		public NetServerRoomController CreateServerRoomController(RoomId roomId) {
			NetServerRoomController controller = new NetServerRoomController(roomId, this, Context.Services.Get<NetMatchControllerService>());

			controller.MatchCreatedEvent += (s, e) => {
				this.Channels.MatchCreatedEventChannel.Send(new MatchCreatedEventModel() {
					MatchId = e.Match.Id,
					Id = controller.State.Id,
					Options = e.Match.Options,
				});
			};

			controller.RoomEndedEvent += (s, e) => {
				this.Channels.RoomEndedEventChannel.Send(new RoomEndedEventModel() {
					Id = controller.State.Id,
				});
			};

			controller.RoomStartedEvent += (s, e) => {
				this.Channels.RoomStartedEventChannel.Send(new RoomStartedEventModel() {
					Id = controller.State.Id,
				});
			};

			controller.UserJoinedEvent += (s, e) => {
				this.Channels.UserJoinedEventChannel.Send(new UserJoinedEventModel() {
					Id = controller.State.Id,
					User = e.User,
				});
			};

			controller.UserLeftEvent += (s, e) => {
				this.Channels.UserLeftEventChannel.Send(new UserLeftEventModel() {
					Id = controller.State.Id,
					User = e.User,
				});
			};

			this.RoomMap.Add(roomId.GUID, controller);
			return controller;
		}

		private void OnUserJoinedEvent(IncomingMessage<UserJoinedEventModel> incoming) {
			if (RoomMap.ContainsKey(incoming.Data.Id.GUID) == false)
				return;

			RoomMap[incoming.Data.Id.GUID].HandleUserJoinedEvent(incoming.Data);
		}

		private void OnUserLeftEvent(IncomingMessage<UserLeftEventModel> incoming) {
			if (RoomMap.ContainsKey(incoming.Data.Id.GUID) == false)
				return;

			RoomMap[incoming.Data.Id.GUID].HandleUserLeftEvent(incoming.Data);
		}

		private void OnRoomStartedEvent(IncomingMessage<RoomStartedEventModel> incoming) {
			if (RoomMap.ContainsKey(incoming.Data.Id.GUID) == false)
				return;

			RoomMap[incoming.Data.Id.GUID].HandleRoomStartedEvent(incoming.Data);
		}

		private void OnRoomEndedEvent(IncomingMessage<RoomEndedEventModel> incoming) {
			if (RoomMap.ContainsKey(incoming.Data.Id.GUID) == false)
				return;

			RoomMap[incoming.Data.Id.GUID].HandleRoomEndedEvent(incoming.Data);
		}

		private void OnMatchCreatedEvent(IncomingMessage<MatchCreatedEventModel> incoming) {
			if (RoomMap.ContainsKey(incoming.Data.Id.GUID) == false)
				return;

			RoomMap[incoming.Data.Id.GUID].HandleMatchCreatedEvent(incoming.Data);
		}

		private void OnStartRoomRequest(IncomingMessage<StartRoomRequest> incoming) {
			if (RoomMap.ContainsKey(incoming.Data.Id.GUID) == false)
				return;

			RoomMap[incoming.Data.Id.GUID].HandleStartRoomRequest(incoming.Data);
		}

		private void OnCloseRoomRequest(IncomingMessage<CloseRoomRequest> incoming) {
			if (RoomMap.ContainsKey(incoming.Data.Id.GUID) == false)
				return;

			RoomMap[incoming.Data.Id.GUID].HandleCloseRoomRequest(incoming.Data);
		}
		private void OnCreateMatchRequest(IncomingMessage<CreateMatchRequest> incoming) {
			if (RoomMap.ContainsKey(incoming.Data.Id.GUID) == false)
				return;

			RoomMap[incoming.Data.Id.GUID].HandleCreateMatchRequest(incoming);
		}

		private void OnGetMatchListRequest(IncomingMessage<GetMatchListRequest> incoming) {
			if (RoomMap.ContainsKey(incoming.Data.Id.GUID) == false)
				return;

			RoomMap[incoming.Data.Id.GUID].HandleGetMatchListRequest(incoming);
		}

		private void OnGetMatchListResponse(IncomingMessage<GetMatchListResponse> incoming) {
			if (RoomMap.ContainsKey(incoming.Data.Id.GUID) == false)
				return;

			RoomMap[incoming.Data.Id.GUID].HandleGetMatchListResponse(incoming.Data);
		}

		private void OnUserJoinMatchRequest(IncomingMessage<JoinMatchRequest> incoming) {
			if (RoomMap.ContainsKey(incoming.Data.Id.GUID) == false)
				return;

			RoomMap[incoming.Data.Id.GUID].HandleUserJoinMatchRequest(incoming.Data, incoming.Sender.GetUserState());
		}
	}
}
