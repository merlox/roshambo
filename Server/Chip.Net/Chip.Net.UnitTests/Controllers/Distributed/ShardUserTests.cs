using Chip.Net.Controllers.Distributed;
using Chip.Net.Providers.Direct;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Chip.Net.UnitTests.Controllers.Distributed
{
	[TestClass]
    public class ShardUserTests
    {
		private class RouterContainer {
			public RouterServer<TestRouterModel, TestShardModel, TestUserModel> Router { get; set; }
		}

		private class ShardContainer {
			public ShardClient<TestRouterModel, TestShardModel, TestUserModel> Shard { get; set; }
			public DistributedChannel<TestPacket> Passthrough { get; set; }
		}

		private class UserContainer {
			public UserClient<TestRouterModel, TestShardModel, TestUserModel> User { get; set; }
			public DistributedChannel<TestPacket> Passthrough { get; set; }
		}

		private RouterContainer Router;
		private List<ShardContainer> Shards;
		private List<UserContainer> Users;

		private List<ShardContainer> ShardsReceived;
		private List<UserContainer> UsersReceived;

		[TestInitialize]
		public void Initialize() {
			INetContext ctx = new NetContext();
			ctx.IPAddress = Guid.NewGuid().ToString();
			ctx.Port = 0;

			var router = new RouterServer<TestRouterModel, TestShardModel, TestUserModel>();
			router.InitializeServer(ctx, new DirectServerProvider(), 0, new DirectServerProvider(), 1);
			router.CreatePassthrough<TestPacket>();

			router.StartShardServer();
			router.StartUserServer();

			this.Router = new RouterContainer() {
				Router = router,
			};

			Shards = new List<ShardContainer>();
			Users = new List<UserContainer>();

			ShardsReceived = new List<ShardContainer>();
			UsersReceived = new List<UserContainer>();

			for(int i = 0; i < 5; i++) {
				var shard = new ShardClient<TestRouterModel, TestShardModel, TestUserModel>();
				var user = new UserClient<TestRouterModel, TestShardModel, TestUserModel>();

				var shctx = ctx.Clone();
				var usctx = ctx.Clone();
				shctx.Port = 0;
				usctx.Port = 1;

				shard.InitializeClient(shctx, new DirectClientProvider());
				user.InitializeClient(usctx, new DirectClientProvider());

				var shch = shard.CreateUserChannel<TestPacket>();
				var usch = user.CreateShardChannel<TestPacket>();

				var shcont = new ShardContainer() {
					Shard = shard,
					Passthrough = shch,
				};

				var uscont = new UserContainer() {
					User = user,
					Passthrough = usch,
				};

				shch.Receive += (e) => ShardsReceived.Add(shcont);
				usch.Receive += (e) => UsersReceived.Add(uscont);
				shard.StartClient();
				user.StartClient();

				Shards.Add(shcont);
				Users.Add(uscont);
			}

			var tick = Environment.TickCount;
			bool run = true;
			while(run) {
				int ct = 0;
				Router.Router.UpdateServer();
				foreach(var sh in Shards) {
					sh.Shard.UpdateClient();
					if (sh.Shard.Model != null)
						ct++;
				}

				foreach(var us in Users) {
					us.User.UpdateClient();
					if (us.User.Model != null)
						ct++;
				}

				if (ct == Users.Count + Shards.Count)
					run = false;

				Assert.IsFalse(Environment.TickCount - tick > 200);
			}
		}

		[TestMethod]
		public void Shard_SendToOneUser_UserReceivedMessage() {
			var shard = Shards.First();
			var user = Users.First();
			var pck = new TestPacket();

			shard.Passthrough.Send(pck, user.User.Model);
			Assert.IsTrue(UsersReceived.Contains(user));
			Assert.IsTrue(UsersReceived.Count == 1);
		}

		[TestMethod]
		public void Shard_SendToManyUsers_UsersReceivedMessages() {
			var shard = Shards.First();
			var userModels = Users.Select(i => i.User.Model);
			var pck = new TestPacket();

			shard.Passthrough.Send(pck, userModels);
			Assert.IsTrue(UsersReceived.Count == Users.Count);
			foreach (var user in Users)
				Assert.IsTrue(UsersReceived.Contains(user));
		}

		[TestMethod]
		public void User_SendToOneShard_ShardReceivedMessage() {
			var shard = Shards.First();
			var user = Users.First();
			var pck = new TestPacket();

			user.Passthrough.Send(pck, shard.Shard.Model);
			Assert.IsTrue(ShardsReceived.Contains(shard));
			Assert.IsTrue(ShardsReceived.Count == 1);
		}

		[TestMethod]
		public void User_SendToManyShards_ShardsReceivedMessages() {
			var user = Users.First();
			var shardModels = Shards.Select(i => i.Shard.Model);
			var pck = new TestPacket();

			user.Passthrough.Send(pck, shardModels);
			Assert.IsTrue(ShardsReceived.Count == Shards.Count);
			foreach (var shard in Shards)
				Assert.IsTrue(ShardsReceived.Contains(shard));
		}

		[TestMethod]
		public void Shard_SendToOneShard_UserReceivedMessage() {
			
		}

		[TestMethod]
		public void Shard_SendToManyShards_UsersReceivedMessages() {
			
		}

		[TestMethod]
		public void User_SendToOneUser_ShardReceivedMessage() {
			
		}

		[TestMethod]
		public void User_SendToManyUsers_ShardsReceivedMessages() {
			
		}
	}
}
