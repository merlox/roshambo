using Roshambo.Common.Controllers.Rooms;
using Roshambo.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roshambo.Common.Controllers.RoomList
{
    public interface IRoomListController
    {
		List<IRoomController> RoomList { get; }

		EventHandler<IRoomController> RoomCreatedEvent { get; set; }
		EventHandler<IRoomController> RoomEndedEvent { get; set; }
		EventHandler<IRoomListController> RoomListUpdatedEvent { get; set; }

		void Refresh();
		void Update();
		void Create(RoomId withId = null);
    }
}
