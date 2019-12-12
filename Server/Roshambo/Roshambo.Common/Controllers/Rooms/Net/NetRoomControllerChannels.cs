using Chip.Net.Data;
using Roshambo.Common.Controllers.Rooms.Net.Models;
using Roshambo.Common.Controllers.Rooms.Net.Models.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roshambo.Common.Controllers.Rooms.Net
{
    public class NetRoomControllerChannels
    {
		public MessageChannel<MatchCreatedEventModel> MatchCreatedEventChannel { get; private set; }
		public MessageChannel<RoomEndedEventModel> RoomEndedEventChannel { get; private set; }
		public MessageChannel<RoomStartedEventModel> RoomStartedEventChannel { get; private set; }
		public MessageChannel<UserJoinedEventModel> UserJoinedEventChannel { get; private set; }
		public MessageChannel<UserLeftEventModel> UserLeftEventChannel { get; private set; }

		public MessageChannel<CloseRoomRequest> CloseRoomRequestChannel { get; private set; }
		public MessageChannel<CreateMatchRequest> CreateMatchRequestChannel { get; private set; }
		public MessageChannel<GetMatchListRequest> GetMatchListRequestChannel { get; private set; }
		public MessageChannel<GetMatchListResponse> GetMatchListResponseChannel { get; private set; }
		public MessageChannel<StartRoomRequest> StartRoomRequestChannel { get; private set; }
		public MessageChannel<JoinMatchRequest> JoinMatchRequestChannel { get; private set; }

		public NetRoomControllerChannels(PacketRouter router) {
			this.MatchCreatedEventChannel = router.Route<MatchCreatedEventModel>();
			this.RoomEndedEventChannel = router.Route<RoomEndedEventModel>();
			this.RoomStartedEventChannel = router.Route<RoomStartedEventModel>();
			this.UserJoinedEventChannel = router.Route<UserJoinedEventModel>();
			this.UserLeftEventChannel = router.Route<UserLeftEventModel>();

			this.CloseRoomRequestChannel = router.Route<CloseRoomRequest>();
			this.CreateMatchRequestChannel = router.Route<CreateMatchRequest>();
			this.GetMatchListRequestChannel = router.Route<GetMatchListRequest>();
			this.GetMatchListResponseChannel = router.Route<GetMatchListResponse>();
			this.StartRoomRequestChannel = router.Route<StartRoomRequest>();
			this.JoinMatchRequestChannel = router.Route<JoinMatchRequest>();
		}
    }
}
