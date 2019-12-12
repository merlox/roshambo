using Roshambo.Common.Controllers.Matches;
using Roshambo.Common.Controllers.RoomList.Net;
using Roshambo.Common.Controllers.Rooms;
using Roshambo.Common.Controllers.Rooms.Net;
using Roshambo.Common.Controllers.Rounds;
using Roshambo.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Roshambo.Server
{
    public class TestClient
    {
		public NetClientRoomListController roomlist;
		private DebugHooks hooks;
		private bool create;

		public TestClient(string name, int port) {
			this.roomlist = new NetClientRoomListController(new Common.Models.UserId() {
				GUID = Guid.NewGuid(),
				Name = name,
			}, "localhost", port);

			this.hooks = new DebugHooks("Client " + name, roomlist, (msg) => Console.WriteLine(msg));
			this.roomlist.RoomCreatedEvent += OnRoomCreated;
		}

		private void OnRoomCreated(object sender, IRoomController e) {
			this.roomlist.Join(e.State.Id);

			e.MatchCreatedEvent += OnMatchCreated;
			if(create)
				e.CreateMatch(new Common.Models.MatchOptions() {
					Name = "Test match",
					Rounds = 3,
					Style = Common.Models.MatchStyle.Rounds,
					TimerLength = 20,
				});
		}

		private void OnMatchCreated(object sender, (IRoomController Room, IMatchController Match) e) {
			var ctlr = e.Room as NetClientRoomController;
			e.Match.UserJoinedEvent += OnUserJoinedMatch;
			e.Match.RoundCreatedEvent += OnRoundCreated;
			ctlr.JoinMatch(e.Match.Id);
		}

		private void OnRoundCreated(object sender, (IMatchController Match, IRoundController Round) e) {
			e.Round.PlayCard(roomlist.LocalUser, CardType.Paper);
		}

		private void OnUserJoinedMatch(object sender, (IMatchController Match, PlayerState User) e) {

		}

		public Thread StartThread(bool create) {
			this.create = create;
			Thread th = new Thread(() => {
				System.Threading.Thread.Sleep(200);
				this.roomlist.Connect();
				if(create)
					this.roomlist.Create(new Common.Models.RoomId() {
						GUID = Guid.NewGuid(),
						Name = "Test Room"
					});

				while(this.roomlist.Client.IsConnected) {
					this.roomlist.Update();
					System.Threading.Thread.Sleep(100);
				}
			});

			th.Start();
			return th;
		}
    }
}
