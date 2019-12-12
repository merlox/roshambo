using Chip.Net.Data;
using Chip.Net.Controllers.Basic;
using Chip.Net.Providers.Sockets;
using Chip.Net.Providers.TCP;
using Chip.Net.Services.NetTime;
using Chip.Net.Services.Ping;
using Chip.Net.Services.RFC;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Chip.Net.Controllers;
using Chip.Net.Services;
using Chip.Net.Services.UserList;
using Chip.Net.Controllers.Distributed.Models;
using Chip.Net.Controllers.Distributed;
using Chip.Net.Providers.Direct;
using Chip.Net.Controllers.Distributed.Services;
using Chip.Net.Controllers.Distributed.Services.ModelTracking;
using Chip.Net.Providers.Lidgren;

namespace Chip.Net.Testbed {
	public class TestPacket : Packet {
		public int Data { get; set; }

		public TestPacket() {
			Data = 0;
		}

		public override void WriteTo(DataBuffer buffer) {
			base.WriteTo(buffer);
			buffer.Write((int)Data);
		}

		public override void ReadFrom(DataBuffer buffer) {
			base.ReadFrom(buffer);
			Data = buffer.ReadInt32();
		}
	}

	public class UserModel : IUserModel {
		public string Name { get; set; }
		public string UUID { get; set; }
		public int Id { get; set; }

		public UserModel() {
			Name = UUID = "";
			Id = -1;
		}

		public void ReadFrom(DataBuffer buffer) {
			Name = buffer.ReadString();
			UUID = buffer.ReadString();
			Id = buffer.ReadInt32();
		}

		public void WriteTo(DataBuffer buffer) {
			buffer.Write((string)Name);
			buffer.Write((string)UUID);
			buffer.Write((int)Id);
		}
	}

	public class ShardModel : IShardModel {
		public int Id { get; set; }

		public ShardModel() {
			Id = -1;
		}

		public void ReadFrom(DataBuffer buffer) {
			Id = buffer.ReadInt32();
		}

		public void WriteTo(DataBuffer buffer) {
			buffer.Write((int)Id);
		}
	}

	public class RouterModel : IRouterModel {
		public string Name { get; set; }
		public int Id { get; set; }

		public RouterModel() {
			Name = "";
			Id = -1;
		}

		public void ReadFrom(DataBuffer buffer) {
			Name = buffer.ReadString();
			Id = buffer.ReadInt32();
		}

		public void WriteTo(DataBuffer buffer) {
			buffer.Write((string)Name);
			buffer.Write((int)Id);
		}
	}

	public class Router : RouterServer<RouterModel, ShardModel, UserModel> {

	}

	public class Shard : ShardClient<RouterModel, ShardModel, UserModel> {
		public DistributedChannel<TestPacket> Channel { get; set; }
	}

	public class User : UserClient<RouterModel, ShardModel, UserModel> {
		public DistributedChannel<TestPacket> Channel { get; set; }
	}

	public class Distributed : DistributedService {
		public DistributedChannel<TestPacket> Channel { get; set; }

		protected override void InitializeDistributedService() {
			base.InitializeDistributedService();
			Channel = CreatePassthrough<TestPacket>("Test");

			if(IsShard)
			Channel.Receive = (e) => {
				Console.WriteLine(e.Data.Data);
			};
		}
	}

	public class TestDistModel : IDistributedModel {
		public int Id { get; set; }
		public string Data { get; set; }

		public void ReadFrom(DataBuffer buffer) {
			Id = buffer.ReadInt32();
			Data = buffer.ReadString();
		}

		public void WriteTo(DataBuffer buffer) {
			buffer.Write((int)Id);
			buffer.Write((string)Data);
		}
	}

	class Program {
		

		static void Main(string[] args) {
			INetContext ctx = new NetContext();
			ctx.ApplicationName = "Testbed";
			ctx.Port = 11111;
			ctx.MaxConnections = 10;
			ctx.IPAddress = "localhost";
			ctx.Services.Register<UserListService>();
			ctx.Services.Register<Distributed>();
			ctx.Services.Register<ModelTrackerService<TestDistModel>>();

			Router router = new Router();
			router.InitializeServer(ctx, new LidgrenServerProvider(), 11111, new LidgrenServerProvider(), 11112);
			router.CreatePassthrough<TestPacket>();

			router.StartShardServer();
			router.StartUserServer();

			Func<Shard> makeShard = () => {
				var sh = new Shard();
				var c = ctx.Clone();
				c.Port = 11111;

				sh.InitializeClient(c, new LidgrenClientProvider());
				sh.Channel = sh.CreateUserChannel<TestPacket>();
				sh.Channel.Receive += (e) => { Console.WriteLine("Recv" + e.Data.Data); };

				sh.StartClient();
				return sh;
			};

			Func<User> makeUser = () => {
				var us = new User();
				var c = ctx.Clone();
				c.Port = 11112;

				us.InitializeClient(c, new LidgrenClientProvider());
				us.Channel = us.CreateShardChannel<TestPacket>();
				us.Channel.Receive += (e) => { Console.WriteLine("Recv " + e.Data.Data); };

				us.StartClient();
				return us;
			};

			List<Shard> shards = new List<Shard>();
			List<User> users = new List<User>();

			for (int i = 0; i < 5; i++)
				shards.Add(makeShard());

			for (int i = 0; i < 5; i++)
				users.Add(makeUser());

			var svc = users.First().Context.Services.Get<Distributed>();
			var svc2 = router.ShardController.Context.Services.Get<ModelTrackerService<TestDistModel>>();

			for(int i = 0; i < 100; i++)
			svc2.Models.Add(new TestDistModel() {
				Data = "Hello world " + i.ToString(),
			});

			//svc.Channel.Send(new TestPacket() {
			//	Data = 10112
			//});

			var t = Environment.TickCount;
			while (true) {
				router.UpdateServer();
				foreach (var s in shards) s.UpdateClient();
				foreach (var u in users) u.UpdateClient();

				if(Environment.TickCount - t > 300)
				System.Threading.Thread.Sleep(10);
			}
		}
	}
}