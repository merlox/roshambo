using Roshambo.Common.Controllers.Rooms;
using Roshambo.Common.Controllers.Matches;
using Roshambo.Common.Controllers.Rounds;
using Roshambo.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Roshambo.Common.Controllers.RoomList.Net;
using Roshambo.Common.Controllers.RoomList;
using System.Threading;
using Lidgren.Network;

namespace Roshambo.Server
{
	class Program
	{
		static List<UserState> users;
		static MatchOptions options;
		static Random random = new Random();

		static void Main(string[] args) {
			//var cfg = new NetPeerConfiguration("test");
			//cfg.Port = 12345;

			//NetServer sv = new NetServer(cfg);
			//sv.Start();

			//var cfg2 = new NetPeerConfiguration("test");
			//NetClient cl = new NetClient(cfg2);
			//cl.Start();
			//cl.Connect("localhost", 12345);

			//	System.Threading.Thread.Sleep(1000);
			//while (true) {
			//	System.Threading.Thread.Sleep(100);
			//}

			//return;



			//users = new List<UserState>();
			//var ids = Enumerable.Range(0, 10).Select(_ => new UserId() {
			//	GUID = Guid.NewGuid(),
			//}).ToList();

			//LocalRoomController testroom = new LocalRoomController("Test");

			NetServerRoomListController roomlist = new NetServerRoomListController(12345);
			var svnodes = roomlist.Server.Router.Nodes.ToList();
			DebugHooks hooks = new DebugHooks("Server", roomlist, (msg) => Console.WriteLine(msg));

			Thread svThread = new Thread(() => {
				while (roomlist.Server.IsActive) {
					roomlist.Update();
					System.Threading.Thread.Sleep(50);
				}
			});

			svThread.Start();

			TestClient client = new TestClient("Test", 12345);
			var clnodes = client.roomlist.Client.Router.Nodes.ToList();
			var clThread = client.StartThread(true);

			for(int i = 0; i < svnodes.Count; i++) {
				if (svnodes[i].Key != clnodes[i].Key || svnodes[i].Order != clnodes[i].Order || svnodes[i].SourceType != clnodes[i].SourceType)
					throw new Exception();
			}

			//TestClient client2 = new TestClient("Test 2", 12345);
			//var clThread2 = client2.StartThread(true);

			while (svThread.IsAlive)
				System.Threading.Thread.Sleep(500);
		}

	}
}
