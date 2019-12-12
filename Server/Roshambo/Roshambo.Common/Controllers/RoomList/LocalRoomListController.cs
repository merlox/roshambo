using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Roshambo.Common.Controllers.Rooms;
using Roshambo.Common.Models;

namespace Roshambo.Common.Controllers.RoomList
{
	public class LocalRoomListController : IRoomListController
	{
		public List<IRoomController> RoomList => throw new NotImplementedException();

		public EventHandler<IRoomController> RoomCreatedEvent { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		public EventHandler<IRoomController> RoomEndedEvent { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		public EventHandler<IRoomListController> RoomListUpdatedEvent { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

		public void Create(RoomId withId = null) {
			throw new NotImplementedException();
		}

		public void Refresh() {
			throw new NotImplementedException();
		}

		public void Update() {
			throw new NotImplementedException();
		}
	}
}
