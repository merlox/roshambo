using Chip.Net.Controllers.Distributed;
using Chip.Net.Controllers.Distributed.Services;
using Chip.Net.Data;
using Chip.Net.Providers.Direct;
using Chip.Net.Providers.TCP;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Chip.Net.UnitTests.Controllers.Distributed.Services
{
	[TestClass]
	public class DistributedServiceInitializationTests {
		public RouterServer<TestRouterModel, TestShardModel, TestUserModel> Router { get; set; }
		public ShardClient<TestRouterModel, TestShardModel, TestUserModel> Shard { get; set; }
		public UserClient<TestRouterModel, TestShardModel, TestUserModel> User { get; set; }

		public class TestService : DistributedService {

		}


		[TestInitialize]
		public void Initialize() {
			Router = new RouterServer<TestRouterModel, TestShardModel, TestUserModel>();
			Shard = new ShardClient<TestRouterModel, TestShardModel, TestUserModel>();
			User = new UserClient<TestRouterModel, TestShardModel, TestUserModel>();
		}

		[TestCleanup]
		public void Cleanup()
		{
			if (Router.IsActive) Router.Shutdown();
			if (Shard.IsConnected) Shard.StopClient();
			if (User.IsConnected) User.StopClient();

			try
			{
				Router.Dispose();
				Shard.Dispose();
				User.Dispose();
			} catch { }
		}

		private INetContext GetContext() {
			INetContext ctx = new NetContext();
			ctx.Services.Register<TestService>();
			return ctx;
		}

		#region Router Server

		[TestMethod]
		public void DistributedService_RouterServer_Initialize_IsRouterIsTrue() {
			Router.InitializeServer(GetContext(), new DirectServerProvider(), 0, new DirectServerProvider(), 1);
			Assert.IsTrue(Router.ShardContext.Services.Get<TestService>().IsRouter);
			Assert.IsTrue(Router.UserContext.Services.Get<TestService>().IsRouter);
		}

		[TestMethod]
		public void DistributedService_RouterServer_Initialize_IsShardIsFalse() {
			Router.InitializeServer(GetContext(), new DirectServerProvider(), 0, new DirectServerProvider(), 1);
			Assert.IsFalse(Router.ShardContext.Services.Get<TestService>().IsShard);
			Assert.IsFalse(Router.UserContext.Services.Get<TestService>().IsShard);
		}

		[TestMethod]
		public void DistributedService_RouterServer_Initialize_IsUserIsFalse() {
			Router.InitializeServer(GetContext(), new DirectServerProvider(), 0, new DirectServerProvider(), 1);
			Assert.IsFalse(Router.ShardContext.Services.Get<TestService>().IsUser);
			Assert.IsFalse(Router.UserContext.Services.Get<TestService>().IsUser);
		}

		[TestMethod]
		public void DistributedService_RouterServer_Initialize_IsServerIsTrue() {
			Router.InitializeServer(GetContext(), new DirectServerProvider(), 0, new DirectServerProvider(), 1);
			Assert.IsTrue(Router.ShardContext.Services.Get<TestService>().IsServer);
			Assert.IsTrue(Router.UserContext.Services.Get<TestService>().IsServer);
		}

		[TestMethod]
		public void DistributedService_RouterServer_Initialize_IsClientIsFalse() {
			Router.InitializeServer(GetContext(), new DirectServerProvider(), 0, new DirectServerProvider(), 1);
			Assert.IsFalse(Router.ShardContext.Services.Get<TestService>().IsClient);
			Assert.IsFalse(Router.UserContext.Services.Get<TestService>().IsClient);
		}

		[TestMethod]
		public void DistributedService_RouterServer_Initialize_ConfiguredEventInvoked() {
			bool configured = false;
			Router.RouterConfiguredEvent += (s, e) => { configured = true; };
			Router.InitializeServer(GetContext(), new DirectServerProvider(), 0, new DirectServerProvider(), 1);

			Assert.IsTrue(configured);
		}

		#endregion

		#region Shard Client

		[TestMethod]
		public void DistributedService_ShardClient_Initialize_IsRouterIsFalse() {
			Shard.InitializeClient(GetContext(), new DirectClientProvider());
			Assert.IsFalse(Shard.Context.Services.Get<TestService>().IsRouter);
		}

		[TestMethod]
		public void DistributedService_ShardClient_Initialize_IsShardIsTrue() {
			Shard.InitializeClient(GetContext(), new DirectClientProvider());
			Assert.IsTrue(Shard.Context.Services.Get<TestService>().IsShard);
		}

		[TestMethod]
		public void DistributedService_ShardClient_Initialize_IsUserIsFalse() {
			Shard.InitializeClient(GetContext(), new DirectClientProvider());
			Assert.IsFalse(Shard.Context.Services.Get<TestService>().IsUser);
		}

		[TestMethod]
		public void DistributedService_ShardClient_Initialize_IsServerIsFalse() {
			Shard.InitializeClient(GetContext(), new DirectClientProvider());
			Assert.IsFalse(Shard.Context.Services.Get<TestService>().IsServer);
		}

		[TestMethod]
		public void DistributedService_ShardClient_Initialize_IsClientIsTrue() {
			Shard.InitializeClient(GetContext(), new DirectClientProvider());
			Assert.IsTrue(Shard.Context.Services.Get<TestService>().IsClient);
		}

		#endregion

		#region User Client

		[TestMethod]
		public void DistributedService_UserClient_Initialize_IsRouterIsFalse() {
			User.InitializeClient(GetContext(), new DirectClientProvider());
			Assert.IsFalse(User.Context.Services.Get<TestService>().IsRouter);
		}

		[TestMethod]
		public void DistributedService_UserClient_Initialize_IsShardIsFalse() {
			User.InitializeClient(GetContext(), new DirectClientProvider());
			Assert.IsFalse(User.Context.Services.Get<TestService>().IsShard);
		}

		[TestMethod]
		public void DistributedService_UserClient_Initialize_IsUserIsTrue() {
			User.InitializeClient(GetContext(), new DirectClientProvider());
			Assert.IsTrue(User.Context.Services.Get<TestService>().IsUser);
		}

		[TestMethod]
		public void DistributedService_UserClient_Initialize_IsServerIsFalse() {
			User.InitializeClient(GetContext(), new DirectClientProvider());
			Assert.IsFalse(User.Context.Services.Get<TestService>().IsServer);
		}

		[TestMethod]
		public void DistributedService_UserClient_Initialize_IsClientIsTrue() {
			User.InitializeClient(GetContext(), new DirectClientProvider());
			Assert.IsTrue(User.Context.Services.Get<TestService>().IsClient);
		}

		#endregion
	}

	[TestClass]
	public class DistributedServiceIntegrationTests {
		public class TestPacket : Packet {
			public string Data { get; set; }

			public TestPacket() {
				Data = "";
			}

			public override void WriteTo(DataBuffer buffer) {
				buffer.Write((string)Data);
			}

			public override void ReadFrom(DataBuffer buffer) {
				base.ReadFrom(buffer);
				Data = buffer.ReadString();
			}
		}

		public class TestDistributedService : DistributedService {
			public ShardChannel<TestPacket> ShardChannel { get; set; }

			protected override void InitializeDistributedService() {
				base.InitializeDistributedService();

				ShardChannel = CreateShardChannel<TestPacket>();
			}
		}

		public RouterServer<TestRouterModel, TestShardModel, TestUserModel> Router { get; set; }
		public List<ShardClient<TestRouterModel, TestShardModel, TestUserModel>> Shards { get; set; }
		public List<UserClient<TestRouterModel, TestShardModel, TestUserModel>> Users { get; set; }

		public int ShardCount { get; set; } = 5;
		public int UserCount { get; set; } = 5;

		public INetContext GetContext(string Name, int Port) {
			INetContext ctx = new NetContext();
			ctx.IPAddress = Name;
			ctx.Port = Port;

			ctx.Services.Register<TestDistributedService>();
			ctx.Packets.Register<TestPacket>();

			return ctx;
		}

		protected virtual void Update() {
			Router.UpdateServer();
			foreach (var u in Users)
				if (u.IsConnected)
					u.UpdateClient();
			foreach (var s in Shards)
				if (s.IsConnected)
					s.UpdateClient();
		}

		[TestInitialize]
		public void Initialize() {
			Router = new RouterServer<TestRouterModel, TestShardModel, TestUserModel>();
			Shards = new List<ShardClient<TestRouterModel, TestShardModel, TestUserModel>>();
			Users = new List<UserClient<TestRouterModel, TestShardModel, TestUserModel>>();

			for(int i = 0; i < ShardCount; i++) {
				Shards.Add(new ShardClient<TestRouterModel, TestShardModel, TestUserModel>());
			}

			for(int i = 0; i < UserCount; i++) {
				Users.Add(new UserClient<TestRouterModel, TestShardModel, TestUserModel>());
			}

			string name = Guid.NewGuid().ToString();
			Router.InitializeServer(GetContext(name, 0), new DirectServerProvider(), 0, new DirectServerProvider(), 1);
			foreach (var shard in Shards) {
				shard.InitializeClient(GetContext(name, 0), new DirectClientProvider());
			}
			foreach (var user in Users) {
				user.InitializeClient(GetContext(name, 1), new DirectClientProvider());
			}

			Router.StartShardServer();
			Router.StartUserServer();
			foreach (var shard in Shards) {
				shard.StartClient();
			}
			foreach (var user in Users) {
				user.StartClient();
			}
		}

		[TestCleanup]
		public void Cleanup()
		{
			if (Router.IsActive) Router.Shutdown();
			foreach (var shard in Shards) if (shard.IsConnected) shard.StopClient();
			foreach (var user in Users) if (user.IsConnected) user.StopClient();

			Router.Dispose();
			foreach (var shard in Shards) shard.Dispose();
			foreach (var user in Users) user.Dispose();

			Shards.Clear();
			Users.Clear();
		}

		private string GetTestString() {
			return Guid.NewGuid().ToString();
		}

		private TestPacket CreatePacket(string data) {
			return new TestPacket() {
				Data = data,
			};
		}

		private void WaitUntil(Func<bool> func, long ms = 1000, long runoff = 0) {
			Stopwatch sw = new Stopwatch();
			sw.Start();

			while(sw.ElapsedMilliseconds < ms + runoff) {
				if (runoff == 0 && func())
					break;

				if (sw.ElapsedMilliseconds < ms && func()) {
					ms = sw.ElapsedMilliseconds;
				}

				System.Threading.Thread.Sleep(10);
			}

			sw.Stop();
			sw = null;
		}

		[TestMethod]
		public void DistributedService_WaitUntil_WaitsOneSecond() {
			Stopwatch sw = new Stopwatch();

			sw.Start();
			WaitUntil(() => false, 1000);
			sw.Stop();

			Assert.IsTrue(sw.Elapsed.TotalSeconds > 0.95);
			Assert.IsTrue(sw.Elapsed.TotalSeconds < 1.05);
		}

		[TestMethod]
		public void DistributedService_WaitUntil_FuncBreak() {
			Stopwatch sw = new Stopwatch();

			sw.Start();
			WaitUntil(() => sw.Elapsed.TotalSeconds >= 0.5f, 1000);
			sw.Stop();

			Assert.IsTrue(sw.Elapsed.TotalSeconds > 0.45);
			Assert.IsTrue(sw.Elapsed.TotalSeconds < 0.55);
		}

		[TestMethod]
		public void DistributedService_WaitUntil_WaitsOneSecond_Runoff() {
			Stopwatch sw = new Stopwatch();

			sw.Start();
			WaitUntil(() => false, 1000, 1000);
			sw.Stop();

			Assert.IsTrue(sw.Elapsed.TotalSeconds > 1.95);
			Assert.IsTrue(sw.Elapsed.TotalSeconds < 2.05);
		}

		[TestMethod]
		public void DistributedService_WaitUntil_FuncBreak_Runoff() {
			Stopwatch sw = new Stopwatch();

			sw.Start();
			WaitUntil(() => {
				Update();
				return sw.Elapsed.TotalSeconds >= 0.5f;
			}, 1000, 1000);
			sw.Stop();

			Assert.IsTrue(sw.Elapsed.TotalSeconds > 1.45);
			Assert.IsTrue(sw.Elapsed.TotalSeconds < 1.55);
		}

		#region Router Server

		[TestMethod]
		public void DistributedService_RouterServer_SendToUser_PacketReceived() {

		}

		[TestMethod]
		public void DistributedService_RouterServer_SendToAllUsers_PacketsReceived() {

		}

		[TestMethod]
		public void DistributedService_RouterServer_SendToAllUsers_ExcludingOne_PacketsReceived() {

		}

		[TestMethod]
		public void DistributedService_RouterServer_SendToAllUsers_ExcludingMany_PacketsReceived() {
			
		}

		[TestMethod]
		public void DistributedService_RouterServer_SendToShard_PacketReceived() {
			var routerShardSvc = Router.ShardController.Context.Services.Get<TestDistributedService>();
			var routerUserSvc = Router.UserController.Context.Services.Get<TestDistributedService>();
			var user = Users.First();
			var shard = Shards.First();
			var usvc = user.Context.Services.Get<TestDistributedService>();
			var ssvc = shard.Context.Services.Get<TestDistributedService>();
			var pck = new TestPacket();

			bool recv = false;
			ssvc.ShardChannel.Receive += (s, e) => {
				recv = true;
			};

			usvc.ShardChannel.Send(pck, shard.Model);
			Assert.IsTrue(recv);
		}

		[TestMethod]
		public void DistributedService_RouterServer_SendToAllShards_PacketsReceived() {
			var routerShardSvc = Router.ShardController.Context.Services.Get<TestDistributedService>();
			var routerUserSvc = Router.UserController.Context.Services.Get<TestDistributedService>();
			var user = Users.First();
			var shard = Shards.First();
			var usvc = user.Context.Services.Get<TestDistributedService>();
			var ssvc = shard.Context.Services.Get<TestDistributedService>();
			var pck = new TestPacket();

			int recv = 0;
			foreach(var sh in Shards) {
				sh.Context.Services.Get<TestDistributedService>().ShardChannel.Receive += (s, e) => {
					recv++;
				};
			}

			usvc.ShardChannel.Send(pck);
			Assert.IsTrue(recv == Shards.Count);
		}

		[TestMethod]
		public void DistributedService_RouterServer_SendToAllShards_ExcludingOne_PacketsReceived() {
			var routerShardSvc = Router.ShardController.Context.Services.Get<TestDistributedService>();
			var routerUserSvc = Router.UserController.Context.Services.Get<TestDistributedService>();
			var user = Users.First();
			var shard = Shards.First();
			var usvc = user.Context.Services.Get<TestDistributedService>();
			var ssvc = shard.Context.Services.Get<TestDistributedService>();
			var pck = new TestPacket();

			int recv = 0;
			foreach (var sh in Shards) {
				sh.Context.Services.Get<TestDistributedService>().ShardChannel.Receive += (s, e) => {
					recv++;
				};
			}

			Router.UpdateServer();
			foreach (var sh in Shards) sh.UpdateClient();
			foreach (var us in Users) us.UpdateClient();

			usvc.ShardChannel.Send(pck, Shards.Select(i => i.Model).Skip(1));
			Assert.AreEqual(recv, Shards.Count - 1);
		}

		[TestMethod]
		public void DistributedService_RouterServer_SendToAllShards_ExcludingMany_PacketsReceived() {
			var routerShardSvc = Router.ShardController.Context.Services.Get<TestDistributedService>();
			var routerUserSvc = Router.UserController.Context.Services.Get<TestDistributedService>();
			var user = Users.First();
			var shard = Shards.First();
			var usvc = user.Context.Services.Get<TestDistributedService>();
			var ssvc = shard.Context.Services.Get<TestDistributedService>();
			var pck = new TestPacket();

			int recv = 0;
			foreach (var sh in Shards) {
				sh.Context.Services.Get<TestDistributedService>().ShardChannel.Receive += (s, e) => {
					recv++;
				};
			}

			Router.UpdateServer();
			foreach (var sh in Shards) sh.UpdateClient();
			foreach (var us in Users) us.UpdateClient();

			usvc.ShardChannel.Send(pck, Shards.Select(i => i.Model).Skip(3));
			Assert.AreEqual(recv, Shards.Count - 3);
		}

		[TestMethod]
		public void DistributedService_RouterServer_UserConnected_EventInvoked() {

		}

		[TestMethod]
		public void DistributedService_RouterServer_UserDisconnected_EventInvoked() {

		}

		[TestMethod]
		public void DistributedService_RouterServer_UserConnected_UserAddedToUserList() {

		}

		[TestMethod]
		public void DistributedService_RouterServer_UserDisconnected_UserRemovedFromUserList() {

		}

		[TestMethod]
		public void DistributedService_RouterServer_ShardConnected_EventInvoked() {

		}

		[TestMethod]
		public void DistributedService_RouterServer_ShardDisconnected_EventInvoked() {

		}

		[TestMethod]
		public void DistributedService_RouterServer_ShardConnected_ShardAddedToShardList() {

		}

		[TestMethod]
		public void DistributedService_RouterServer_ShardDisconnected_ShardRemovedFromShardList() {

		}

		#endregion

		#region Shard Client

		[TestMethod]
		public void DistributedService_ShardClient_SendToRouter_PacketReceived() {
			var data = GetTestString();
			var packet = CreatePacket(data);
			var shard = Shards.First();
		}

		[TestMethod]
		public void DistributedService_ShardClient_SendToUser_PacketReceived() {
			var data = GetTestString();
			var packet = CreatePacket(data);

		}

		[TestMethod]
		public void DistributedService_ShardClient_SendToAllUsers_PacketsReceived() {
			var data = GetTestString();
			var packet = CreatePacket(data);

		}

		[TestMethod]
		public void DistributedService_ShardClient_SendToAllUsers_ExcludingOne_PacketsReceived() {
			var data = GetTestString();
			var packet = CreatePacket(data);

		}

		[TestMethod]
		public void DistributedService_ShardClient_SendToAllUsers_ExcludingMany_PacketsReceived() {
			var data = GetTestString();
			var packet = CreatePacket(data);

		}

		[TestMethod]
		public void DistributedService_ShardClient_SendToShard_PacketReceived() {
			var shardSvc1 = Shards.First().Context.Services.Get<TestDistributedService>();
			var shardSvc2 = Shards.Skip(1).First().Context.Services.Get<TestDistributedService>();
			var pck = new TestPacket();

			bool recv = false;
			shardSvc2.ShardChannel.Receive += (s, e) => {
				recv = true;
			};

			shardSvc1.ShardChannel.Send(pck, Shards.Skip(1).First().Model);
			Assert.IsTrue(recv);
		}

		[TestMethod]
		public void DistributedService_ShardClient_SendToAllShards_PacketsReceived() {
			var shardSvc1 = Shards.First().Context.Services.Get<TestDistributedService>();
			var shardSvc2 = Shards.Skip(1).First().Context.Services.Get<TestDistributedService>();
			var pck = new TestPacket();

			int recv = 0;
			foreach (var sh in Shards)
				sh.Context.Services.Get<TestDistributedService>().ShardChannel.Receive += (s, e) => {
					recv += 1;
				};

			shardSvc1.ShardChannel.Send(pck);
			Assert.AreEqual(Shards.Count, recv);
		}

		[TestMethod]
		public void DistributedService_ShardClient_SendToAllShards_ExcludingOne_PacketsReceived() {
			var shardSvc1 = Shards.First().Context.Services.Get<TestDistributedService>();
			var shardSvc2 = Shards.Skip(1).First().Context.Services.Get<TestDistributedService>();
			var pck = new TestPacket();

			int recv = 0;
			foreach (var sh in Shards)
				sh.Context.Services.Get<TestDistributedService>().ShardChannel.Receive += (s, e) => {
					recv += 1;
				};

			shardSvc1.ShardChannel.Send(pck, Shards.Select(i => i.Model).Skip(1));
			Assert.AreEqual(Shards.Count - 1, recv);
		}

		[TestMethod]
		public void DistributedService_ShardClient_SendToAllShards_ExcludingMany_PacketsReceived() {
			var shardSvc1 = Shards.First().Context.Services.Get<TestDistributedService>();
			var shardSvc2 = Shards.Skip(1).First().Context.Services.Get<TestDistributedService>();
			var pck = new TestPacket();

			int recv = 0;
			foreach (var sh in Shards)
				sh.Context.Services.Get<TestDistributedService>().ShardChannel.Receive += (s, e) => {
					recv += 1;
				};

			shardSvc1.ShardChannel.Send(pck, Shards.Select(i => i.Model).Skip(3));
			Assert.AreEqual(Shards.Count - 3, recv);
		}

		[TestMethod]
		public void DistributedService_ShardClient_ConnectedToRouter_ConfiguredEventInvoked() {
			
		}

		[TestMethod]
		public void DistributedService_ShardClient_UserAssigned_EventInvoked() {

		}

		[TestMethod]
		public void DistributedService_ShardClient_UserUnassigned_EventInvoked() {

		}

		[TestMethod]
		public void DistributedService_ShardClient_ConnectedToRouter_ConnectedEventInvoked() {

		}

		[TestMethod]
		public void DistributedService_ShardClient_DisconnectedFromRouter_DisconnectedEventInvoked() {

		}

		[TestMethod]
		public void DistributedService_ShardClient_UserAssigned_UserAddedToUserList() {

		}

		[TestMethod]
		public void DistributedService_ShardClient_UserUnassigned_UserRemovedFromUserList() {

		}

		#endregion

		#region User Client

		[TestMethod]
		public void DistributedService_UserClient_SendToRouter_ReceivesPacket() {

		}

		[TestMethod]
		public void DistributedService_UserClient_SendToShard_ReceivesPacket() {
			var userSvc = Users.First().Context.Services.Get<TestDistributedService>();
			var shardSvc = Shards.First().Context.Services.Get<TestDistributedService>();
			var pck = new TestPacket();

			bool recv = false;
			shardSvc.ShardChannel.Receive += (s, e) => {
				recv = true;
			};

			userSvc.ShardChannel.Send(pck, Shards.First().Model);
			Assert.IsTrue(recv);
		}

		[TestMethod]
		public void DistributedService_UserClient_SendToAllShards_ReceivesPackets() {
			var userSvc = Users.First().Context.Services.Get<TestDistributedService>();
			var pck = new TestPacket();

			int recv = 0;
			foreach (var sh in Shards)
				sh.Context.Services.Get<TestDistributedService>().ShardChannel.Receive += (s, e) => {
					recv += 1;
				};

			userSvc.ShardChannel.Send(pck);
			Assert.AreEqual(Shards.Count, recv);
		}

		[TestMethod]
		public void DistributedService_UserClient_SendToAllShards_ExcludingOne_ReceivesPackets() {
			var userSvc = Users.First().Context.Services.Get<TestDistributedService>();
			var pck = new TestPacket();

			int recv = 0;
			foreach (var sh in Shards)
				sh.Context.Services.Get<TestDistributedService>().ShardChannel.Receive += (s, e) => {
					recv += 1;
				};

			userSvc.ShardChannel.Send(pck, Shards.Select(i => i.Model).Skip(1));
			Assert.AreEqual(Shards.Count - 1, recv);
		}

		[TestMethod]
		public void DistributedService_UserClient_SendToAllShards_ExcludingMany_ReceivesPackets() {
			var userSvc = Users.First().Context.Services.Get<TestDistributedService>();
			var pck = new TestPacket();

			int recv = 0;
			foreach (var sh in Shards)
				sh.Context.Services.Get<TestDistributedService>().ShardChannel.Receive += (s, e) => {
					recv += 1;
				};

			userSvc.ShardChannel.Send(pck, Shards.Select(i => i.Model).Skip(3));
			Assert.AreEqual(Shards.Count - 3, recv);
		}

		[TestMethod]
		public void DistributedService_UserClient_ConnectedToRouter_UserConfiguredEventInvoked() {

		}

		[TestMethod]
		public void DistributedService_UserClient_ConnectedToRouter_ConnectedEventInvoked() {

		}

		[TestMethod]
		public void DistributedService_UserClient_DisconnectedFromRouter_DisconnectedEventInvoked() {

		}

		[TestMethod]
		public void DistributedService_UserClient_RouterAssignUserToShard_AssignedToShardEventInvoked() {

		}

		[TestMethod]
		public void DistributedService_UserClient_RouterUnassignUserFromShard_UnassignedToShardEventInvoked() {

		}

		[TestMethod]
		public void DistributedService_UserClient_ConnectedToRouter_ActiveRouterSet() {

		}

		[TestMethod]
		public void DistributedService_UserClient_DisconnectedToRouter_ActiveRouterNull() {

		}
		#endregion
	}
}
