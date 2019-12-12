using System;
using System.Collections.Generic;
using System.Text;
using Chip.Net.Data;
using Chip.Net.Providers;
using Chip.Net.Services;

namespace Chip.Net.Controllers.Basic
{
	public class BasicClient : INetClientController {
		#region INetClient
		public EventHandler<NetEventArgs> OnConnected { get; set; }
		public EventHandler<NetEventArgs> OnDisconnected { get; set; }
		public EventHandler<NetEventArgs> OnPacketReceived { get; set; }
		public EventHandler<NetEventArgs> OnPacketSent { get; set; }

		public PacketRouter Router { get; protected set; }
		public INetContext Context { get; protected set; }

		public bool IsInitialized { get; private set; }
		public bool IsConnected { get; protected set; }
		public bool IsServer { get; set; } = false;
		public bool IsClient { get; set; } = true;

		private INetClientProvider provider;
		private bool disposed;

		public virtual void InitializeClient(INetContext context, INetClientProvider provider) {
			if (IsInitialized)
				throw new Exception("Server already initialized");

			IsInitialized = true;
			this.provider = provider;
			this.IsConnected = false;
			this.Router = new PacketRouter(provider, this);
			this.Context = context;
			disposed = false;

			context.LockContext(client: this);
		}

		public virtual void StartClient() {
			provider.UserConnected += (s, i) => { IsConnected = true; OnConnected?.Invoke(this, new NetEventArgs()); };
			provider.UserDisconnected += (s, i) => { IsConnected = false; OnDisconnected?.Invoke(this, new NetEventArgs()); };

			Context.Services.StartServices();
			provider.Connect(Context);
		}

		public virtual void StopClient() {
			Context.Services.StopServices();
			provider.Disconnect();
			IsConnected = false;
		}

		public virtual void UpdateClient() {
			Context.Services.UpdateServices();
			provider.UpdateClient();
		}

		public virtual void Dispose() {
			if (IsConnected)
				StopClient();

			if(disposed == false) {
				disposed = true;
				Context.Services.Dispose();
			}
		}
		#endregion
	}
}
