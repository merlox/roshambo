using Chip.Net;
using Chip.Net.Data;
using Chip.Net.Services;
using Roshambo.Common.Controllers.RoomList.Net.Models;
using Roshambo.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roshambo.Common.Controllers.RoomList.Net
{
    public class NetRoomListControllerService : NetService
    {
		public MessageChannel<CreateRoomRequest> CreateRoomRequestChannel { get; private set; }
		public MessageChannel<CreateRoomResponse> CreateRoomResponseChannel { get; private set; }
		public MessageChannel<GetRoomListRequest> GetRoomListRequestChannel { get; private set; }
		public MessageChannel<GetRoomListResponse> GetRoomListResponseChannel { get; private set; }
		public MessageChannel<JoinRoomRequest> JoinRoomRequestChannel { get; private set; }
		public MessageChannel<JoinRoomResponse> JoinRoomResponseChannel { get; private set; }
		public MessageChannel<UserId> SetUserIdChannel { get; private set; }
		public MessageChannel<UserState> SetUserStateChannel { get; private set; }

		public override void InitializeService(INetContext context) {
			base.InitializeService(context);

			this.CreateRoomRequestChannel = this.Router.Route<CreateRoomRequest>();
			this.CreateRoomResponseChannel = this.Router.Route<CreateRoomResponse>();
			this.GetRoomListRequestChannel = this.Router.Route<GetRoomListRequest>();
			this.GetRoomListResponseChannel = this.Router.Route<GetRoomListResponse>();
			this.JoinRoomRequestChannel = this.Router.Route<JoinRoomRequest>();
			this.JoinRoomResponseChannel = this.Router.Route<JoinRoomResponse>();
			this.SetUserIdChannel = this.Router.Route<UserId>();
			this.SetUserStateChannel = this.Router.Route<UserState>();
		}
	}
}
