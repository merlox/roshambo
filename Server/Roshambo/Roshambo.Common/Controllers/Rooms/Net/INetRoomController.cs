using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chip.Net.Data;
using Roshambo.Common.Controllers.Rooms.Net.Models;
using Roshambo.Common.Controllers.Rooms.Net.Models.Events;
using Roshambo.Common.Models;

namespace Roshambo.Common.Controllers.Rooms.Net
{
	public interface INetRoomController : IRoomController
	{
		void HandleUserJoinedEvent(UserJoinedEventModel data);
		void HandleGetMatchListResponse(GetMatchListResponse data);
		void HandleGetMatchListRequest(IncomingMessage<GetMatchListRequest> data);
		void HandleCreateMatchRequest(IncomingMessage<CreateMatchRequest> data);
		void HandleCloseRoomRequest(CloseRoomRequest data);
		void HandleStartRoomRequest(StartRoomRequest data);
		void HandleMatchCreatedEvent(MatchCreatedEventModel data);
		void HandleRoomEndedEvent(RoomEndedEventModel data);
		void HandleRoomStartedEvent(RoomStartedEventModel data);
		void HandleUserLeftEvent(UserLeftEventModel data);
		void HandleUserJoinMatchRequest(JoinMatchRequest data, UserState user);
	}
}
