using Chip.Net.Data;
using Chip.Net.Providers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Chip.Net.UnitTests.Providers
{
    public abstract class ProviderBaseTests<TServer, TClient> : ProviderBaseTests
		where TServer : INetServerProvider
		where TClient : INetClientProvider
    {
		public TServer Server { get; set; }
		public List<TClient> Clients { get; set; }
		public int ClientCount { get; set; } = 3;

		public class TestPacket : Packet {
			public string Data { get; set; }

			public TestPacket()
			{
				Data = "";
			}

			public override void WriteTo(DataBuffer buffer)
			{
				base.WriteTo(buffer);
				buffer.Write((string)Data);
			}

			public override void ReadFrom(DataBuffer buffer)
			{
				base.ReadFrom(buffer);
				Data = buffer.ReadString();
			}
		}

		[TestInitialize]
		public void Initialize()
		{
			Server = CreateServer();

			Clients = new List<TClient>();
			for (int i = 0; i < ClientCount; i++)
				Clients.Add(CreateClient());
		}

		[TestCleanup]
		public void Cleanup()
		{
			if (Server.IsActive) Server.StopServer();
			foreach (var client in Clients)
				if (client.IsConnected)
					client.Disconnect();

			Server.Dispose();
			foreach (var client in Clients)
				client.Dispose();

			Clients.Clear();
			Server = default(TServer);
		}

		protected TestPacket CreateTestPacket(string data)
		{
			return new TestPacket()
			{
				Data = data
			};
		}

		protected TServer CreateServer()
		{
			return Activator.CreateInstance<TServer>();
		}

		protected TClient CreateClient()
		{
			return Activator.CreateInstance<TClient>();
		}

		protected INetContext CreateContext()
		{
			INetContext ctx = new NetContext();
			ctx.ApplicationName = "TestApp";
			ctx.IPAddress = "localhost";
			ctx.Port = Common.Port;
			ctx.MaxConnections = 10;
			return ctx;
		}

		[TestMethod]
		public virtual void Server_StartServer_IsActive()
		{
			var ctx = CreateContext();
			Assert.IsFalse(Server.IsActive);

			Server.StartServer(ctx);
			Assert.IsTrue(Server.IsActive);
		}

		[TestMethod]
		public virtual void Server_StopServer_IsNotActive()
		{
			var ctx = CreateContext();
			Assert.IsFalse(Server.IsActive);

			Server.StartServer(ctx);
			Wait(() => { Server.UpdateServer(); return false; }, 100);
			Server.StopServer();

			Assert.IsFalse(Server.IsActive);
		}

		[TestMethod]
		public virtual void Server_Dispose_ClientDisconnected()
		{
			var client = Clients.First();
			var ctx = CreateContext();

			Server.StartServer(ctx);
			client.Connect(ctx);

			Wait(() =>
			{
				Server.UpdateServer();
				client.UpdateClient();
				return client.IsConnected;
			}, 250);

			Assert.IsTrue(client.IsConnected);

			Server.Dispose();
			Wait(() =>
			{
				client.UpdateClient();
				return client.IsConnected == false;
			}, 250);

			Assert.IsFalse(client.IsConnected);
		}

		[TestMethod]
		public virtual void Server_DisconnectClient_ClientDisconnected() {
			var client = Clients.First();
			var ctx = CreateContext();

			Server.StartServer(ctx);
			client.Connect(ctx);

			Wait(() =>
			{
				Server.UpdateServer();
				client.UpdateClient();
				return client.IsConnected;
			}, 250);

			Assert.IsTrue(client.IsConnected);

			var clKey = Server.GetClientKeys().First();
			Server.DisconnectUser(clKey);
			Wait(() =>
			{
				Server.UpdateServer();
				client.UpdateClient();
				return client.IsConnected == false;
			}, 250);

			Assert.IsFalse(client.IsConnected);
		}

		[TestMethod]
		public virtual void Server_ClientConnected_EventsInvoked() {
			var client = Clients.First();
			var ctx = CreateContext();

			bool clientConnected = false;
			bool serverUserConnected = false;

			client.UserConnected += (s, u) => clientConnected = true;
			Server.UserConnected += (s, u) => serverUserConnected = true;

			Server.StartServer(ctx);
			client.Connect(ctx);

			Wait(() =>
			{
				Server.UpdateServer();
				client.UpdateClient();
				return client.IsConnected;
			}, 250);

			Assert.IsTrue(client.IsConnected);
			Assert.IsTrue(clientConnected);
			Assert.IsTrue(serverUserConnected);
		}

		[TestMethod]
		public virtual void Server_ClientDisconnected_EventsInvoked() {
			var client = Clients.First();
			var ctx = CreateContext();

			bool clientDisconnected = false;
			bool serverUserDisconnected = false;

			client.UserDisconnected += (s, u) => clientDisconnected = true;
			Server.UserDisconnected += (s, u) => serverUserDisconnected = true;

			Server.StartServer(ctx);
			client.Connect(ctx);

			Wait(() =>
			{
				Server.UpdateServer();
				client.UpdateClient();
				return client.IsConnected;
			}, 250);

			Assert.IsTrue(client.IsConnected);
			client.Disconnect();

			Wait(() =>
			{
				Server.UpdateServer();
				client.UpdateClient();
				return client.IsConnected == false;
			}, 250);

			Assert.IsFalse(client.IsConnected);
			Assert.IsTrue(clientDisconnected);
			Assert.IsTrue(serverUserDisconnected);
		}

		[TestMethod]
		public virtual void Server_SendToClient_ClientReceivesData() {
			var client = Clients.First();
			var ctx = CreateContext();

			Server.StartServer(ctx);
			client.Connect(ctx);

			Wait(() =>
			{
				Server.UpdateServer();
				client.UpdateClient();
				return client.IsConnected;
			}, 250);

			string data = Guid.NewGuid().ToString();
			TestPacket packet = CreateTestPacket(data);
			DataBuffer buffer = new DataBuffer();
			packet.WriteTo(buffer);

			DataBuffer inc = null;
			client.DataReceived += (s, a) => {
				inc = a.Data;
			};

			foreach(var key in Server.GetClientKeys())
				Server.SendMessage(key, buffer);

			Wait(() =>
			{
				Server.UpdateServer();
				client.UpdateClient();

				return inc != null;
			}, 250);

			Assert.IsNotNull(inc);
			TestPacket result = new TestPacket();
			result.ReadFrom(inc);

			Assert.AreEqual(result.Data, data);
		}

		[TestMethod]
		public virtual void Server_SendToOneClient_ClientReceivesData() {
			var ctx = CreateContext();

			Server.StartServer(ctx);
			foreach (var client in Clients)
				client.Connect(ctx);

			Wait(() =>
			{
				Server.UpdateServer();
				foreach (var client in Clients)
					client.UpdateClient();

				return Clients.All(c => c.IsConnected);
			}, 250);

			string data = Guid.NewGuid().ToString();
			TestPacket packet = CreateTestPacket(data);
			DataBuffer buffer = new DataBuffer();
			packet.WriteTo(buffer);


			int received = 0;
			foreach (var cl in Clients)
				cl.DataReceived += (s, a) => {
					var inc = a.Data;
					Assert.IsNotNull(inc);
					TestPacket result = new TestPacket();
					result.ReadFrom(inc);
					Assert.AreEqual(result.Data, data);
					received++;
				};

			Server.SendMessage(Server.GetClientKeys().First(), buffer);
			Wait(() =>
			{
				Server.UpdateServer();
				foreach (var client in Clients)
					client.UpdateClient();

				return received == 1;
			}, 250);

			Assert.AreEqual(received, 1);
		}

		[TestMethod]
		public virtual void Server_SendToAllClients_ClientReceivesData() {
			var ctx = CreateContext();

			Server.StartServer(ctx);
			for(int i = 0; i < Clients.Count; i++)
			{
				var client = Clients[i];
				client.Connect(ctx);
			}

			Wait(() =>
			{
				Server.UpdateServer();
				foreach (var client in Clients)
					client.UpdateClient();

				return Server.GetClientKeys().Count() == Clients.Count;
			}, 250);

			string data = Guid.NewGuid().ToString();
			TestPacket packet = CreateTestPacket(data);
			DataBuffer buffer = new DataBuffer();
			packet.WriteTo(buffer);

			int received = 0;
			foreach (var cl in Clients)
				cl.DataReceived += (s, a) => {
					var inc = a.Data;

					Assert.IsNotNull(inc);
					TestPacket result = new TestPacket();
					result.ReadFrom(inc);
					Assert.AreEqual(result.Data, data);
					received++;
				};

			foreach(var key in Server.GetClientKeys())
				Server.SendMessage(key, buffer);

			Wait(() =>
			{
				Server.UpdateServer();
				foreach (var client in Clients)
					client.UpdateClient();

				return received == Clients.Count;
			}, 250);

			Assert.AreEqual(received, Clients.Count);
		}

		[TestMethod]
		public virtual void Server_AcceptNewClientsFalse_NewClientRejected()
		{
			var client = Clients.First();
			var ctx = CreateContext();

			Server.StartServer(ctx);
			Server.AcceptIncomingConnections = false;

			try
			{
				client.Connect(ctx);
			}
			catch { }

			Wait(() =>
			{
				Server.UpdateServer();
				client.UpdateClient();
				return client.IsConnected == false;
			}, 250);

			Assert.IsFalse(client.IsConnected);
		}

		[TestMethod]
		public virtual void Server_AcceptNewClientsTrue_NewClientAccepted()
		{
			var client = Clients.First();
			var ctx = CreateContext();

			Server.StartServer(ctx);
			Server.AcceptIncomingConnections = false;

			Wait(() =>
			{
				Server.UpdateServer();
				client.UpdateClient();
				return false;
			}, 100);

			Server.AcceptIncomingConnections = true;
			client.Connect(ctx);

			Wait(() =>
			{
				Server.UpdateServer();
				client.UpdateClient();
				return client.IsConnected;
			}, 250);

			Assert.IsTrue(client.IsConnected);

		}

		[TestMethod]
		public virtual void Client_Connect_IsConnected() {
			var client = Clients.First();
			var ctx = CreateContext();

			Server.StartServer(ctx);
			client.Connect(ctx);

			Wait(() =>
			{
				Server.UpdateServer();
				client.UpdateClient();
				return client.IsConnected;
			}, 250);

			Assert.IsTrue(client.IsConnected);
		}


		[TestMethod]
		public virtual void Client_Disconnect_IsNotConnected() {
			var client = Clients.First();
			var ctx = CreateContext();

			Server.StartServer(ctx);
			client.Connect(ctx);

			Wait(() =>
			{
				Server.UpdateServer();
				client.UpdateClient();
				return client.IsConnected;
			}, 250);

			client.Disconnect();
			Wait(() =>
			{
				Server.UpdateServer();
				return client.IsConnected == false;
			}, 250);

			Assert.IsFalse(client.IsConnected);
		}

		[TestMethod]
		public virtual void Client_ConnectedToServer_EventsInvoked() {
			var client = Clients.First();
			var ctx = CreateContext();

			bool clientConnectedToServer = false;
			bool serverClientConnected = false;

			Server.UserConnected += (s, u) => serverClientConnected = true;
			client.UserConnected += (s, u) => clientConnectedToServer = true;

			Server.StartServer(ctx);
			client.Connect(ctx);

			Wait(() =>
			{
				Server.UpdateServer();
				client.UpdateClient();
				return clientConnectedToServer && serverClientConnected;
			}, 250);

			Assert.IsTrue(clientConnectedToServer);
			Assert.IsTrue(serverClientConnected);
		}

		[TestMethod]
		public virtual void Client_DisconnectedFromServer_EventsInvoked() {
			var client = Clients.First();
			var ctx = CreateContext();

			bool clientDisconnectedFromServer = false;
			bool serverClientDisconnected = false;

			Server.UserDisconnected += (s, u) => serverClientDisconnected = true;
			client.UserDisconnected += (s, u) => clientDisconnectedFromServer = true;

			Server.StartServer(ctx);
			client.Connect(ctx);

			Wait(() =>
			{
				Server.UpdateServer();
				client.UpdateClient();
				return client.IsConnected;
			}, 250);

			Server.DisconnectUser(Server.GetClientKeys().First());
			Wait(() =>
			{
				Server.UpdateServer();
				client.UpdateClient();
				return client.IsConnected == false;
			}, 250);

			Assert.IsTrue(clientDisconnectedFromServer);
			Assert.IsTrue(serverClientDisconnected);
		}

		[TestMethod]
		public virtual void Client_SendToServer_ServerReceivesData() {
			var client = Clients.First();
			var ctx = CreateContext();

			Server.StartServer(ctx);
			client.Connect(ctx);

			Wait(() =>
			{
				Server.UpdateServer();
				client.UpdateClient();
				return client.IsConnected;
			}, 250);

			string data = Guid.NewGuid().ToString();
			TestPacket packet = CreateTestPacket(data);
			DataBuffer buffer = new DataBuffer();
			packet.WriteTo(buffer);

			DataBuffer inc = null;
			Server.DataReceived += (s, a) => {
				inc = a.Data;
			};

			client.SendMessage(buffer);

			Wait(() =>
			{
				Server.UpdateServer();
				client.UpdateClient();

				return inc != null;
			}, 250);

			Assert.IsNotNull(inc);
			TestPacket result = new TestPacket();
			result.ReadFrom(inc);
			Assert.AreEqual(result.Data, data);
		}

		[TestMethod]
		public virtual void Server_SendToClient_DataSentEventInvoked() {
			var client = Clients.First();
			var ctx = CreateContext();

			Server.StartServer(ctx);
			client.Connect(ctx);

			Wait(() => {
				Server.UpdateServer();
				client.UpdateClient();
				return client.IsConnected;
			}, 250);

			string data = Guid.NewGuid().ToString();
			TestPacket packet = CreateTestPacket(data);
			DataBuffer buffer = new DataBuffer();
			packet.WriteTo(buffer);

			bool received = false;
			Server.DataSent += (s, e) => {
				received = true;
				Assert.IsTrue(e.ByteCount > 0);
				Assert.IsNotNull(e.UserKey);
			};

			Server.SendMessage(Server.GetClientKeys().First(), buffer);
			Assert.IsTrue(received);
		}

		[TestMethod]
		public virtual void Client_SendToServer_DataSentEventInvoked() {
			var client = Clients.First();
			var ctx = CreateContext();

			Server.StartServer(ctx);
			client.Connect(ctx);

			Wait(() => {
				Server.UpdateServer();
				client.UpdateClient();
				return client.IsConnected;
			}, 250);

			string data = Guid.NewGuid().ToString();
			TestPacket packet = CreateTestPacket(data);
			DataBuffer buffer = new DataBuffer();
			packet.WriteTo(buffer);

			bool received = false;
			client.DataSent += (s, e) => {
				received = true;
				Assert.IsTrue(e.ByteCount > 0);
			};

			client.SendMessage(buffer);
			Assert.IsTrue(received);
		}
	}

	public class ProviderBaseTests {
		public static void Wait(Func<bool> func, int ms = 1000) {
			Stopwatch stopwatch = new Stopwatch();

			stopwatch.Start();
			while (stopwatch.ElapsedMilliseconds <= ms && func() == false)
				System.Threading.Thread.Sleep(10);

			var m = stopwatch.ElapsedMilliseconds;
			while (stopwatch.ElapsedMilliseconds < m + 100)
				func();

			stopwatch.Stop();
		}
	}

	[TestClass]
	public class ProviderTestsTests {
		private Stopwatch stopwatch;

		[TestInitialize]
		public void Initialize() {
			stopwatch = new Stopwatch();
		}

		[TestMethod]
		public void Wait_WaitXMilliseconds_XMillisecondsElapsed() {
			for(int i = 1; i < 4; i++) {
				stopwatch.Start();
				ProviderBaseTests.Wait(() => false, i * 250);
				stopwatch.Stop();
				Assert.IsTrue(stopwatch.ElapsedMilliseconds > i * 250);
				stopwatch.Reset();
			}
		}

		[TestMethod]
		public void Wait_WaitReturnTrue_EarlyEscape() {
			stopwatch.Start();
			ProviderBaseTests.Wait(() => true, 250);
			Assert.IsTrue(stopwatch.ElapsedMilliseconds < 250);
			stopwatch.Reset();
		}
	}
}
