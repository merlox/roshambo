using Chip.Net.Controllers;
using Chip.Net.Data;
using Chip.Net.Providers.Direct;
using Chip.Net.Providers.TCP;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Chip.Net.UnitTests.Controllers {
	public abstract class BaseControllerTests<TServer, TClient>
		where TServer : class, INetServerController
		where TClient : class, INetClientController {

		public class ServerWrapper {
			public INetServerController Server { get; set; }
			public MessageChannel<TestPacket> Channel { get; set; }
		}

		public class ClientWrapper {
			public INetClientController Client { get; set; }
			public MessageChannel<TestPacket> Channel { get; set; }
		}

		private int _port = 0;
		private int Port {
			get {
				if (_port == 0)
					_port = Common.Port;

				return _port;
			}
		}

		private INetContext _context;
		public INetContext Context {
			get {
				_context = new NetContext();
				_context.IPAddress = "127.0.0.1";
				_context.Port = Port;
				_context.Services.Register<TestNetService>();
				_context.Packets.Register<TestPacket>();
				return _context;
			}
		}

		protected virtual ServerWrapper StartNewServer() {
			var sv = NewServer();
			sv.Server.StartServer();
			return sv;
		}

		protected virtual ServerWrapper NewServer() {
			var sv = Activator.CreateInstance<TServer>();
			sv.InitializeServer(Context, new DirectServerProvider());
			var ch = sv.Router.Route<TestPacket>();

			return new ServerWrapper() {
				Server = sv,
				Channel = ch,
			};
		}

		protected virtual ClientWrapper StartNewClient() {
			var cl = NewClient();
			cl.Client.StartClient();
			return cl;
		}

		protected virtual ClientWrapper NewClient() {
			var cl = Activator.CreateInstance<TClient>();
			cl.InitializeClient(Context, new DirectClientProvider());
			var ch = cl.Router.Route<TestPacket>();

			return new ClientWrapper() {
				Client = cl,
				Channel = ch,
			};
		}

		protected virtual void Wait(Func<bool> func, int ticks = 1000) {
			var tick = Environment.TickCount;
			while (Environment.TickCount - tick < ticks && func() == false)
				System.Threading.Thread.Sleep(10);
		}

		protected virtual void UpdateServer(TServer server) {
			server.UpdateServer();
		}

		protected virtual void UpdateClient(TClient client) {
			client.UpdateClient();
		}

		[TestMethod]
		public virtual void Server_StartServer_RouterNotNull() {
			var sv = StartNewServer();
			Assert.IsNotNull(sv.Server.Router);
		}

		[TestMethod]
		public virtual void Client_StartClient_RouterNotNull() {
			var sv = StartNewServer();
			var cl = StartNewClient();
			Assert.IsNotNull(cl.Client.Router);
		}

		[TestMethod]
		public virtual void Server_StartServer_IsActive() {
			var sv = StartNewServer();
			Assert.IsTrue(sv.Server.IsActive);
		}

		[TestMethod]
		public virtual void Client_StartClient_IsConnected() {
			var sv = StartNewServer();
			var cl = StartNewClient();

			Wait(() => {
				UpdateServer(sv.Server as TServer);
				UpdateClient(cl.Client as TClient);
				return cl.Client.IsConnected;
			});

			Assert.IsTrue(cl.Client.IsConnected);
		}

		[TestMethod]
		public virtual void Server_ClientConnects_EventInvoked() {
			var sv = StartNewServer();
			bool eventInvoked = false;
			NetUser user = null;
			sv.Server.NetUserConnected += (s, i) => { user = i.User; eventInvoked = true; };


			var cl = StartNewClient();
			Wait(() => {
				UpdateServer(sv.Server as TServer);
				UpdateClient(cl.Client as TClient);
				return cl.Client.IsConnected;
			});

			Wait(() => {
				UpdateServer(sv.Server as TServer);
				UpdateClient(cl.Client as TClient);
				return eventInvoked;
			});

			Assert.IsNotNull(user);
			Assert.IsTrue(cl.Client.IsConnected);
			Assert.IsTrue(eventInvoked);
		}

		[TestMethod]
		public virtual void Server_ClientDisconnects_EventInvoked() {
			var sv = StartNewServer();
			bool eventInvoked = false;
			NetUser user1 = null;
			NetUser user2 = null;
			sv.Server.NetUserConnected += (s, i) => { user1 = i.User; };
			sv.Server.NetUserDisconnected += (s, i) => { user2 = i.User; eventInvoked = true; };

			var cl = StartNewClient();
			Wait(() => {
				UpdateServer(sv.Server as TServer);
				UpdateClient(cl.Client as TClient);
				return cl.Client.IsConnected;
			});

			cl.Client.StopClient();

			Wait(() => {
				UpdateServer(sv.Server as TServer);
				UpdateClient(cl.Client as TClient);
				return eventInvoked;
			});

			Assert.IsNotNull(user1);
			Assert.AreEqual(user1, user2);
			Assert.IsTrue(cl.Client.IsConnected == false);
			Assert.IsTrue(eventInvoked);
		}

		[TestMethod]
		public virtual void Client_ClientConnects_EventInvoked() {
			var sv = StartNewServer();
			var cl = NewClient();
			bool eventInvoked = false;
			cl.Client.OnConnected += (s, i) => { eventInvoked = true; };
			cl.Client.StartClient();

			Wait(() => {
				UpdateServer(sv.Server as TServer);
				UpdateClient(cl.Client as TClient);
				return cl.Client.IsConnected;
			});

			Wait(() => {
				UpdateServer(sv.Server as TServer);
				UpdateClient(cl.Client as TClient);
				return eventInvoked;
			});

			Assert.IsTrue(cl.Client.IsConnected);
			Assert.IsTrue(eventInvoked);
		}

		[TestMethod]
		public virtual void Client_ClientDisconnects_EventInvoked() {
			var sv = StartNewServer();
			var cl = NewClient();
			bool eventInvoked = false;
			cl.Client.OnDisconnected += (s, i) => { eventInvoked = true; };
			cl.Client.StartClient();

			Wait(() => {
				UpdateServer(sv.Server as TServer);
				UpdateClient(cl.Client as TClient);
				return cl.Client.IsConnected;
			});

			cl.Client.StopClient();

			Wait(() => {
				UpdateServer(sv.Server as TServer);
				UpdateClient(cl.Client as TClient);
				return eventInvoked;
			});

			Assert.IsTrue(cl.Client.IsConnected == false);
			Assert.IsTrue(eventInvoked);
		}

		[TestMethod]
		public virtual void Server_ClientConnected_HasUser() {
			var sv = StartNewServer();
			NetUser user = null;
			sv.Server.NetUserConnected += (s, i) => {
				user = i.User;
			};

			var cl = StartNewClient();

			Wait(() => {
				UpdateServer(sv.Server as TServer);
				UpdateClient(cl.Client as TClient);
				return cl.Client.IsConnected;
			});

			Assert.IsNotNull(user);
			Assert.IsTrue(sv.Server.GetUsers().Count() == 1);
			Assert.AreEqual(sv.Server.GetUsers().First(), user);
		}

		[TestMethod]
		public virtual void Server_InitializeServer_HasContext() {
			var sv = StartNewServer();
			Assert.IsNotNull(sv.Server.Context);
		}

		[TestMethod]
		public virtual void Client_InitializeClient_HasContext() {
			var cl = NewClient();
			Assert.IsNotNull(cl.Client.Context);
		}

		[TestMethod]
		public virtual void Server_InitializeServer_ContextLocked() {
			var sv = StartNewServer();
			Assert.IsTrue(sv.Server.Context.IsLocked);
		}

		[TestMethod]
		public virtual void Server_StartServer_ServicesStarted() {
			var sv = StartNewServer();
			Assert.IsTrue(sv.Server.Context.Services.Get<TestNetService>().Started);
		}

		[TestMethod]
		public virtual void Server_UpdateServer_ServicesUpdated() {
			var sv = StartNewServer();
			UpdateServer(sv.Server as TServer);
			Assert.IsTrue(sv.Server.Context.Services.Get<TestNetService>().Updated);

		}

		[TestMethod]
		public virtual void Server_StopServer_ServicesStopped() {
			var sv = StartNewServer();
			UpdateServer(sv.Server as TServer);
			sv.Server.StopServer();
			Assert.IsTrue(sv.Server.Context.Services.Get<TestNetService>().Stopped);
		}

		[TestMethod]
		public virtual void Server_DisposeServer_ServicesDisposed() {
			var sv = StartNewServer();
			var svc = sv.Server.Context.Services.Get<TestNetService>();
			UpdateServer(sv.Server as TServer);
			sv.Server.StopServer();
			sv.Server.Dispose();
			Assert.IsTrue(svc.Disposed);
		}

		[TestMethod]
		public virtual void Client_InitializeClient_ContextLocked() {
			var cl = NewClient();
			Assert.IsTrue(cl.Client.Context.IsLocked);
		}

		[TestMethod]
		public virtual void Server_InitializeServer_ServicesInitialized() {
			var sv = StartNewServer();
			var cl = NewClient();
			Assert.IsTrue(sv.Server.Context.Services.Get<TestNetService>().Initialized);
		}

		[TestMethod]
		public virtual void Client_InitializeClient_ServicesInitialized() {
			var sv = StartNewServer();
			var cl = NewClient();
			Assert.IsTrue(cl.Client.Context.Services.Get<TestNetService>().Initialized);
		}

		[TestMethod]
		public virtual void Client_StartClient_ServicesStarted() {
			var sv = StartNewServer();
			var cl = StartNewClient();
			Assert.IsTrue(cl.Client.Context.Services.Get<TestNetService>().Started);
		}

		[TestMethod]
		public virtual void Client_UpdateClient_ServicesUpdated() {
			var sv = StartNewServer();
			var cl = StartNewClient();
			UpdateClient(cl.Client as TClient);
			Assert.IsTrue(cl.Client.Context.Services.Get<TestNetService>().Updated);
		}

		[TestMethod]
		public virtual void Client_StopClient_ServicesStopped() {
			var sv = StartNewServer();
			var cl = StartNewClient();
			UpdateClient(cl.Client as TClient);
			cl.Client.StopClient();
			Assert.IsTrue(cl.Client.Context.Services.Get<TestNetService>().Stopped);
		}

		[TestMethod]
		public virtual void Client_DisposeClient_ServicesDisposed() {
			var sv = StartNewServer();
			var cl = StartNewClient();
			var svc = cl.Client.Context.Services.Get<TestNetService>();
			UpdateClient(cl.Client as TClient);
			cl.Client.StopClient();
			cl.Client.Dispose();
			Assert.IsTrue(svc.Disposed);
		}

		[TestMethod]
		public virtual void Server_SendPacket_ClientReceivesPacket() {
			var sv = StartNewServer();
			NetUser user = null;
			sv.Server.NetUserConnected += (s, i) => {
				user = i.User;
			};

			var cl = StartNewClient();
			bool received = false;

			cl.Channel.Receive += (e) => {
				received = true;
			};

			Wait(() => {
				UpdateServer(sv.Server as TServer);
				UpdateClient(cl.Client as TClient);
				return cl.Client.IsConnected;
			});

			Assert.IsNotNull(user);
			sv.Channel.Send(new OutgoingMessage<TestPacket>(new TestPacket()));

			Wait(() => {
				UpdateServer(sv.Server as TServer);
				UpdateClient(cl.Client as TClient);
				return received;
			});

			Assert.IsTrue(received);
		}

		[TestMethod]
		public virtual void Server_SendPacketToUser_ClientReceivesPacket() {
			var sv = StartNewServer();
			NetUser user = null;
			sv.Server.NetUserConnected += (s, i) => {
				user = i.User;
			};

			var cl = StartNewClient();
			bool received = false;

			cl.Channel.Receive += (e) => {
				received = true;
			};

			Wait(() => {
				UpdateServer(sv.Server as TServer);
				UpdateClient(cl.Client as TClient);
				return cl.Client.IsConnected;
			});

			Assert.IsNotNull(user);
			sv.Channel.Send(new OutgoingMessage<TestPacket>(new TestPacket(), user));

			Wait(() => {
				UpdateServer(sv.Server as TServer);
				UpdateClient(cl.Client as TClient);
				return received;
			});

			Assert.IsTrue(received);
		}

		[TestMethod]
		public virtual void Client_SendPacket_ServerReceivesPacket() {
			var sv = StartNewServer();
			var cl = StartNewClient();
			bool received = false;

			sv.Channel.Receive += (e) => {
				received = true;
			};

			Wait(() => {
				UpdateServer(sv.Server as TServer);
				UpdateClient(cl.Client as TClient);
				return cl.Client.IsConnected;
			});

			cl.Channel.Send(new OutgoingMessage<TestPacket>(new TestPacket()));

			Wait(() => {
				UpdateServer(sv.Server as TServer);
				UpdateClient(cl.Client as TClient);
				return received;
			});

			Assert.IsTrue(received);
		}

		[TestMethod]
		public virtual void Client_SendPacket_PacketHasRecipient() {
			var sv = StartNewServer();
			NetUser user = null;
			sv.Server.NetUserConnected += (s, i) => {
				user = i.User;
			};

			var cl = StartNewClient();
			bool received = false;
			NetUser user2 = null;

			sv.Channel.Receive += (e) => {
				received = true;
				user2 = e.Sender;

			};

			Wait(() => {
				UpdateServer(sv.Server as TServer);
				UpdateClient(cl.Client as TClient);
				return cl.Client.IsConnected;
			});

			cl.Channel.Send(new OutgoingMessage<TestPacket>(new TestPacket()));

			Wait(() => {
				UpdateServer(sv.Server as TServer);
				UpdateClient(cl.Client as TClient);
				return received;
			});

			Assert.IsTrue(received);
			Assert.IsNotNull(user);
			Assert.AreEqual(user, user2);
		}

		[TestMethod]
		public virtual void Server_ServiceSendPacket_ClientServiceReceivesPacket() {
			var sv = StartNewServer();
			var cl = StartNewClient();
			string data = "Hello world" + Port;

			Wait(() => {
				UpdateServer(sv.Server as TServer);
				UpdateClient(cl.Client as TClient);
				return cl.Client.IsConnected;
			});

			var cl_svc = cl.Client.Context.Services.Get<TestNetService>();
			var sv_svc = sv.Server.Context.Services.Get<TestNetService>();
			sv_svc.Channel.Send(new OutgoingMessage<TestPacket>(new TestPacket() {
				data = data,
			}));

			Wait(() => {
				UpdateServer(sv.Server as TServer);
				UpdateClient(cl.Client as TClient);
				return cl_svc.Received;
			});

			Assert.IsTrue(cl_svc.Received);
			Assert.AreEqual(cl_svc.ReceivedData, data);
		}

		[TestMethod]
		public virtual void Server_ServiceSendPacketToUser_ClientServiceReceivesPacket() {
			var sv = StartNewServer();
			var cl = StartNewClient();
			string data = "Hello world" + Port;

			Wait(() => {
				UpdateServer(sv.Server as TServer);
				UpdateClient(cl.Client as TClient);
				return cl.Client.IsConnected;
			});

			var cl_svc = cl.Client.Context.Services.Get<TestNetService>();
			var sv_svc = sv.Server.Context.Services.Get<TestNetService>();
			sv_svc.Channel.Send(new OutgoingMessage<TestPacket>(new TestPacket() {
				data = data,
			}, sv.Server.GetUsers().First()));

			Wait(() => {
				UpdateServer(sv.Server as TServer);
				UpdateClient(cl.Client as TClient);
				return cl_svc.Received;
			});

			Assert.IsTrue(cl_svc.Received);
			Assert.AreEqual(cl_svc.ReceivedData, data);
		}

		[TestMethod]
		public virtual void Client_ServiceSendsPacket_ServerServiceReceivesPacket() {
			var sv = StartNewServer();
			var cl = StartNewClient();
			string data = "Hello world" + Port;

			Wait(() => {
				UpdateServer(sv.Server as TServer);
				UpdateClient(cl.Client as TClient);
				return cl.Client.IsConnected;
			});

			var cl_svc = cl.Client.Context.Services.Get<TestNetService>();
			var sv_svc = sv.Server.Context.Services.Get<TestNetService>();
			cl_svc.Channel.Send(new OutgoingMessage<TestPacket>(new TestPacket() {
				data = data,
			}));

			Wait(() => {
				UpdateServer(sv.Server as TServer);
				UpdateClient(cl.Client as TClient);
				return sv_svc.Received;
			});

			Assert.IsTrue(sv_svc.Received);
			Assert.AreEqual(sv_svc.ReceivedData, data);
		}

		[TestMethod]
		public virtual void Server_InitializeServer_ServiceIsServerTrue() {
			var sv = StartNewServer();
			Assert.IsTrue(sv.Server.Context.Services.Get<TestNetService>().IsServer);
			Assert.IsFalse(sv.Server.Context.Services.Get<TestNetService>().IsClient);
		}

		[TestMethod]
		public virtual void Client_InitializeClient_ServiceIsServerFalse() {
			var cl = NewClient();
			Assert.IsTrue(cl.Client.Context.Services.Get<TestNetService>().IsClient);
			Assert.IsFalse(cl.Client.Context.Services.Get<TestNetService>().IsServer);
		}
	}
}
