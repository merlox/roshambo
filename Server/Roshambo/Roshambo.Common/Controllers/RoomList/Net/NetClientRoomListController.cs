using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chip.Net;
using Chip.Net.Controllers.Basic;
using Chip.Net.Data;
using Chip.Net.Providers.Lidgren;
using Chip.Net.Providers.TCP;
using Roshambo.Common.Controllers.RoomList.Net.Models;
using Roshambo.Common.Controllers.Rooms;
using Roshambo.Common.Controllers.Rooms.Net;
using Roshambo.Common.Models;

namespace Roshambo.Common.Controllers.RoomList.Net
{
	public class NetClientRoomListController : IRoomListController
	{
		public BasicClient Client { get; private set; }

		public UserId LocalUser { get; private set; }
		private UserState localUserState;
		private NetRoomListControllerService service;

		public List<IRoomController> RoomList { get; private set; }

		public EventHandler<IRoomController> LocalUserJoinRoomEvent { get; set; }
		public EventHandler<IRoomController> RoomCreatedEvent { get; set; }
		public EventHandler<IRoomController> RoomEndedEvent { get; set; }
		public EventHandler<IRoomListController> RoomListUpdatedEvent { get; set; }

		public NetClientRoomListController(UserId user, string ipAddress, int port) {
			this.LocalUser = user;
			this.RoomList = new List<IRoomController>();

			var ctx = SharedNetContext.CreateContext(ipAddress, port);
			this.Client = new BasicClient();
			this.Client.InitializeClient(ctx, new LidgrenClientProvider());
			this.service = ctx.Services.Get<NetRoomListControllerService>();

			this.service.CreateRoomResponseChannel.Receive += OnCreateRoomResponse;
			this.service.GetRoomListResponseChannel.Receive += OnGetRoomListResponse;
			this.service.JoinRoomResponseChannel.Receive += OnJoinRoomResponse;
			this.service.SetUserStateChannel.Receive += OnSetUserState;

			this.Client.OnConnected += OnConnectedToServer;
		}

		public void Connect() {
			this.Client.StartClient();
		}

		private void OnJoinRoomResponse(IncomingMessage<JoinRoomResponse> incoming) {
			if(incoming.Data.Success) {
				this.LocalUserJoinRoomEvent?.Invoke(this, this.RoomList.FirstOrDefault(i => i.State.Id.GUID == incoming.Data.Room.GUID));
			}
		}

		private void OnSetUserState(IncomingMessage<UserState> incoming) {
			this.localUserState = incoming.Data;
		}

		private void OnConnectedToServer(object sender, NetEventArgs e) {
			this.service.SetUserIdChannel.Send(this.LocalUser);
			this.service.GetRoomListRequestChannel.Send(new GetRoomListRequest());
		}

		private void OnCreateRoomResponse(IncomingMessage<CreateRoomResponse> incoming) {
			if (incoming.Data.Success == false)
				return;

			CreateRoomController(incoming.Data.Id);
		}

		private void OnGetRoomListResponse(IncomingMessage<GetRoomListResponse> incoming) {
			var newRooms = incoming.Data.RoomList.Where(i => this.RoomList.Any(r => r.State.Id.GUID == i.GUID) == false);

			foreach(var id in newRooms) {
				CreateRoomController(id);
			}
		}

		public void Create(RoomId withId = null) {
			this.service.CreateRoomRequestChannel.Send(new CreateRoomRequest() {
				Id = withId,
				Creator = LocalUser,
			});
		}

		public void Refresh() {
			this.service.GetRoomListRequestChannel.Send(new GetRoomListRequest());
		}

		public void Update() {
			if (this.Client != null)
				this.Client.UpdateClient();

			foreach (var room in RoomList)
				room.UpdateMatches();
		}

		public void Join(RoomId room) {
			this.service.JoinRoomRequestChannel.Send(new JoinRoomRequest() {
				Room = room,
			});
		}

		private NetClientRoomController CreateRoomController(RoomId id) {
			var controller = this.Client.Context.Services.Get<NetRoomControllerService>().CreateClientRoomController(id, this.localUserState);
			this.RoomList.Add(controller);

			this.RoomCreatedEvent?.Invoke(this, controller);
			this.RoomListUpdatedEvent?.Invoke(this, this);
			controller.RoomEndedEvent += (s, e) => {
				this.RoomEndedEvent?.Invoke(this, e);
				this.RoomList.Remove(e);
				this.RoomListUpdatedEvent?.Invoke(this, this);
			};

			return controller;
		}
	}
}
