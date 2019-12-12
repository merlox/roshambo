using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chip.Net.Data;
using Roshambo.Common.Controllers.Matches;
using Roshambo.Common.Controllers.Matches.Net;
using Roshambo.Common.Controllers.Rooms.Net.Models;
using Roshambo.Common.Controllers.Rooms.Net.Models.Events;
using Roshambo.Common.Models;

namespace Roshambo.Common.Controllers.Rooms.Net
{
	public class NetClientRoomController : INetRoomController
	{
		private NetMatchControllerService matchService;
		private UserState localUser;
		private NetRoomControllerChannels channels;
		private Dictionary<Guid, NetClientMatchController> MatchMap;

		public string Name { get; set; }

		public EventHandler<IRoomController> RoomStartedEvent { get; set; }
		public EventHandler<IRoomController> RoomEndedEvent { get; set; }
		public EventHandler<(IRoomController Room, UserState User)> UserJoinedEvent { get; set; }
		public EventHandler<(IRoomController Room, UserState User)> UserLeftEvent { get; set; }
		public EventHandler<(IRoomController Room, IMatchController Match)> MatchCreatedEvent { get; set; }

		public RoomState State { get; private set; }

		public NetClientRoomController(UserState localUser, RoomId id, NetRoomControllerChannels channels, NetMatchControllerService matchService) {
			this.matchService = matchService;
			this.localUser = localUser;
			this.MatchMap = new Dictionary<Guid, NetClientMatchController>();
			this.State = new RoomState();
			this.State.Id = id;
			this.channels = channels;
		}

		#region Handlers
		public void HandleCloseRoomRequest(CloseRoomRequest data) {
			throw new NotImplementedException();
		}

		public void HandleStartRoomRequest(StartRoomRequest data) {
			throw new NotImplementedException();
		}

		public void HandleGetMatchListRequest(IncomingMessage<GetMatchListRequest> data) {
			throw new NotImplementedException();
		}

		public void HandleCreateMatchRequest(IncomingMessage<CreateMatchRequest> data) {
			throw new NotImplementedException();
		}

		public void HandleUserJoinMatchRequest(JoinMatchRequest data, UserState user) {
			throw new NotImplementedException();
		}

		public void HandleGetMatchListResponse(GetMatchListResponse data) {
			foreach (var match in data.MatchList) {
				if (this.MatchMap.ContainsKey(match.Id.GUID) == false) {
					this.CreateMatchWrapper(match.Id, match.Options);
				}
			}
		}

		private NetClientMatchController CreateMatchWrapper(MatchId id, MatchOptions options) {
			if (this.MatchMap.ContainsKey(id.GUID))
				throw new Exception("Match wrapper already exists");

			var controller = matchService.CreateClientMatchController(id, options);
			this.MatchMap.Add(id.GUID, controller);
			this.MatchCreatedEvent?.Invoke(this, (this, controller));
			return controller;
		}

		public void HandleMatchCreatedEvent(MatchCreatedEventModel data) {
			this.CreateMatchWrapper(data.MatchId, data.Options);
		}

		public void HandleRoomEndedEvent(RoomEndedEventModel data) {
			this.State.CurrentState = RoomState.State.Ended;
			this.RoomEndedEvent?.Invoke(this, this);
		}

		public void HandleRoomStartedEvent(RoomStartedEventModel data) {
			this.State.CurrentState = RoomState.State.Active;
			this.RoomStartedEvent?.Invoke(this, this);
		}

		public void HandleUserJoinedEvent(UserJoinedEventModel data) {
			this.State.Users.Add(data.User);
			this.UserJoinedEvent?.Invoke(this, (this, data.User));
		}

		public void HandleUserLeftEvent(UserLeftEventModel data) {
			this.State.Users = this.State.Users.Where(i => i.Id.GUID != data.User.Id.GUID).ToList();
			this.UserLeftEvent?.Invoke(this, (this, data.User));
		}
		#endregion

		public void CloseRoom() {
			this.channels.CloseRoomRequestChannel.Send(new CloseRoomRequest() {
				Id = this.State.Id,
			});
		}

		public void CreateMatch(MatchOptions options) {
			this.channels.CreateMatchRequestChannel.Send(new CreateMatchRequest() {
				Id = this.State.Id,
				Options = options,
			});
		}

		public void JoinMatch(MatchId id) {
			this.channels.JoinMatchRequestChannel.Send(new JoinMatchRequest() {
				Id = this.State.Id,
				Match = id
			});
		}

		public List<IMatchController> GetMatches() {
			return this.MatchMap.Values.Cast<IMatchController>().ToList();
		}

		public void StartRoom() {
			this.channels.StartRoomRequestChannel.Send(new StartRoomRequest() {
				Id = this.State.Id,
			});
		}

		public void UpdateMatches() {

		}
	}
}
