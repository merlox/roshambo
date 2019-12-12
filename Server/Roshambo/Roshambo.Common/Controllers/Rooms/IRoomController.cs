using Roshambo.Common.Controllers.Matches;
using Roshambo.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roshambo.Common.Controllers.Rooms
{
    public interface IRoomController
    {
		string Name { get; }

		EventHandler<IRoomController> RoomStartedEvent { get; set; }
		EventHandler<IRoomController> RoomEndedEvent { get; set; }
		EventHandler<(IRoomController Room, UserState User)> UserJoinedEvent { get; set; }
		EventHandler<(IRoomController Room, UserState User)> UserLeftEvent { get; set; }
		EventHandler<(IRoomController Room, IMatchController Match)> MatchCreatedEvent { get; set; }

		RoomState State { get; }

		void StartRoom();

		void CreateMatch(MatchOptions options);

		List<IMatchController> GetMatches();

		void UpdateMatches();

		void CloseRoom();
    }
}
