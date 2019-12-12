using Chip.Net.Controllers.Basic;
using Chip.Net.Data;
using Roshambo.Common.Controllers.Matches;
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
    public class NetServerRoomController : INetRoomController
	{
		private NetRoomControllerService roomService;
		private Dictionary<Guid, NetServerMatchController> matchMap;
		public string Name => this.State.Id.Name;

		public EventHandler<IRoomController> RoomStartedEvent { get; set; }
		public EventHandler<IRoomController> RoomEndedEvent { get; set; }
		public EventHandler<(IRoomController Room, UserState User)> UserJoinedEvent { get; set; }
		public EventHandler<(IRoomController Room, UserState User)> UserLeftEvent { get; set; }
		public EventHandler<(IRoomController Room, IMatchController Match)> MatchCreatedEvent { get; set; }

		public RoomState State => this.Source.State;

		private NetMatchControllerService matchService;

		private LocalRoomController Source { get; }

		public NetServerRoomController(RoomId id, NetRoomControllerService roomService, NetMatchControllerService matchService) {
			this.roomService = roomService;
			this.matchMap = new Dictionary<Guid, NetServerMatchController>();
			this.matchService = matchService;
			this.Source = new LocalRoomController(id);
			this.AddSourceEventHooks(this.Source);
		}

		public void CloseRoom() {
			this.Source.CloseRoom();
		}

		public void CreateMatch(MatchOptions options) {
			this.Source.CreateMatch(options);
		}

		public List<IMatchController> GetMatches() {
			return this.Source.GetMatches();
		}

		#region Handlers
		public void HandleCloseRoomRequest(CloseRoomRequest data) {
			this.Source.CloseRoom();
		}

		public void HandleCreateMatchRequest(IncomingMessage<CreateMatchRequest> data) {
			this.Source.CreateMatch(data.Data.Options);
		}

		public void HandleGetMatchListRequest(IncomingMessage<GetMatchListRequest> data) {
			this.SendMatchListToUser(data.Sender);
		}

		public void HandleGetMatchListResponse(GetMatchListResponse data) {
			throw new NotImplementedException();
		}

		public void HandleMatchCreatedEvent(MatchCreatedEventModel data) {
			throw new NotImplementedException();
		}

		public void HandleRoomEndedEvent(RoomEndedEventModel data) {
			throw new NotImplementedException();
		}

		public void HandleRoomStartedEvent(RoomStartedEventModel data) {
			throw new NotImplementedException();
		}

		public void HandleStartRoomRequest(StartRoomRequest data) {
			this.Source.StartRoom();
		}

		public void HandleUserJoinedEvent(UserJoinedEventModel data) {
			throw new NotImplementedException();
		}

		public void HandleUserLeftEvent(UserLeftEventModel data) {
			throw new NotImplementedException();
		}

		public void HandleUserJoinMatchRequest(JoinMatchRequest data, UserState user) {
			if (this.matchMap.ContainsKey(data.Match.GUID) == false)
				return;

			var match = this.matchMap[data.Match.GUID] as NetServerMatchController;
			match.AddPlayer(user);

			if (match.State.PlayerList.Count == 2)
				match.StartMatch();
		}
		#endregion

		public void StartRoom() {
			this.Source.StartRoom();
		}

		public void UpdateMatches() {
			this.Source.UpdateMatches();
		}

		public void AddUser(NetUser endpoint, UserState state) {
			this.Source.AddExistingPlayer(state);
			this.SendMatchListToUser(endpoint);
		}

		private void AddSourceEventHooks(LocalRoomController source) {
			source.MatchCreatedEvent += (s, e) => {
				var wrapper = this.matchService.CreateServerMatchController((LocalMatchController)e.Match);
				this.matchMap.Add(wrapper.Id.GUID, wrapper);
				this.MatchCreatedEvent?.Invoke(this, (this, wrapper));
			};

			source.RoomEndedEvent += (s, e) => {
				this.RoomEndedEvent?.Invoke(this, this);
			};

			source.RoomStartedEvent += (s, e) => {
				this.RoomStartedEvent?.Invoke(this, this);
			};

			source.UserJoinedEvent += (s, e) => {
				this.UserJoinedEvent?.Invoke(this, (this, e.User));
			};

			source.UserLeftEvent += (s, e) => {
				this.UserLeftEvent?.Invoke(this, (this, e.User));
			};
		}

		private void SendMatchListToUser(NetUser user) {
			this.roomService.Channels.GetMatchListResponseChannel.Send(user, new GetMatchListResponse() {
				Id = this.Source.State.Id,
				MatchList = this.GetMatches().Select(i => (i.Id, i.Options)).ToList(),
			});
		}
	}
}
