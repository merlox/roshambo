using Chip.Net.Controllers.Basic;
using Chip.Net.Data;
using Chip.Net.Providers.Lidgren;
using Chip.Net.Providers.TCP;
using Roshambo.Common.Controllers.RoomList.Net.Models;
using Roshambo.Common.Controllers.Rooms;
using Roshambo.Common.Controllers.Rooms.Net;
using Roshambo.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roshambo.Common.Controllers.RoomList.Net
{
    public class NetServerRoomListController : IRoomListController
    {
		public BasicServer Server { get; private set; }
		private NetRoomListControllerService service;
		private Dictionary<Guid, NetServerRoomController> rooms;

		public List<IRoomController> RoomList => this.rooms.Values.Cast<IRoomController>().ToList();

		public EventHandler<IRoomController> RoomCreatedEvent { get; set; }
		public EventHandler<IRoomController> RoomEndedEvent { get; set; }
		public EventHandler<IRoomListController> RoomListUpdatedEvent { get; set; }

		public NetServerRoomListController(int port) {
			var ctx = SharedNetContext.CreateContext("", 12345);
			this.Server = new BasicServer();
			this.Server.InitializeServer(ctx, new LidgrenServerProvider());
			this.service = ctx.Services.Get<NetRoomListControllerService>();
			this.rooms = new Dictionary<Guid, NetServerRoomController>();

			this.service.CreateRoomRequestChannel.Receive += OnCreateRoomRequest;
			this.service.GetRoomListRequestChannel.Receive += OnGetRoomListRequest;
			this.service.JoinRoomRequestChannel.Receive += OnJoinRoomRequest;
			this.service.SetUserIdChannel.Receive += OnUserIdSet;

			this.Server.StartServer();
		}

		private void OnJoinRoomRequest(IncomingMessage<JoinRoomRequest> incoming) {
			if(incoming.Sender.GetUserId() == null) {
				this.service.JoinRoomResponseChannel.Send(incoming.Sender, new JoinRoomResponse() {
					Success = false,
				});

				return;
			}

			if(this.rooms.ContainsKey(incoming.Data.Room.GUID) == false) {
				this.service.JoinRoomResponseChannel.Send(incoming.Sender, new JoinRoomResponse() {
					Success = false,
				});

				return;
			}

			var room = this.rooms[incoming.Data.Room.GUID];
			if(room.State.CurrentState == RoomState.State.Ended) {
				this.service.JoinRoomResponseChannel.Send(incoming.Sender, new JoinRoomResponse() {
					Success = false,
				});

				return;
			}

			room.AddUser(incoming.Sender, incoming.Sender.GetUserState());
			this.service.JoinRoomResponseChannel.Send(incoming.Sender, new JoinRoomResponse() {
				Success = true,
				Room = room.State.Id,
			});
		}

		private void OnUserIdSet(IncomingMessage<UserId> incoming) {
			incoming.Sender.SetUserId(incoming.Data);
			var state = new UserState();
			state.Id = incoming.Data;
			state.Stars = 3;
			state.UserHand = new HandState();
			state.UserHand.RockCount = 9;
			state.UserHand.ScissorsCount = 9;
			state.UserHand.PaperCount = 9;
			incoming.Sender.SetUserState(state);
			this.service.SetUserStateChannel.Send(incoming.Sender, state);
		}

		private void OnGetRoomListRequest(IncomingMessage<GetRoomListRequest> incoming) {
			this.service.GetRoomListResponseChannel.Send(incoming.Sender, new GetRoomListResponse() {
				RoomList = this.rooms.Values.Select(i => i.State.Id).ToList()
			});
		}

		private void OnCreateRoomRequest(IncomingMessage<CreateRoomRequest> incoming) {
			this.Create(incoming.Data.Id);
		}

		public void Create(RoomId withId = null) {
			if (withId == null)
				withId = new RoomId() {
					GUID = Guid.NewGuid(),
					Name = "New Room",
				};

			var controller = this.Server.Context.Services.Get<NetRoomControllerService>().CreateServerRoomController(withId);
			this.rooms.Add(withId.GUID, controller);

			this.RoomCreatedEvent?.Invoke(this, controller);
			this.RoomListUpdatedEvent?.Invoke(this, this);

			controller.RoomEndedEvent += (s, e) => {
				this.RoomEndedEvent?.Invoke(this, e);
				this.rooms.Remove(e.State.Id.GUID);
				this.RoomListUpdatedEvent?.Invoke(this, this);
			};

			this.service.CreateRoomResponseChannel.Send(new Models.CreateRoomResponse() {
				Success = true,
				Id = controller.State.Id,
			});
		}

		public void Update() {
			if (this.Server != null)
				this.Server.UpdateServer();

			var tmp = rooms.Values.ToList();
			foreach (var room in tmp)
				room.UpdateMatches();
		}

		public void Refresh() {
			//serverside, do nothing
		}
	}
}
