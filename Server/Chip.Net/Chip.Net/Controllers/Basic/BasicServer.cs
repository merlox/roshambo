using System;
using System.Collections.Generic;
using System.Text;
using Chip.Net.Data;
using Chip.Net.Providers;
using Chip.Net.Services;

namespace Chip.Net.Controllers.Basic
{
	public class BasicServer : INetServerController {
		#region INetServer
		public EventHandler<NetEventArgs> NetUserConnected { get; set; }
		public EventHandler<NetEventArgs> NetUserDisconnected { get; set; }
		public EventHandler<NetEventArgs> PacketReceived { get; set; }
		public EventHandler<NetEventArgs> PacketSent { get; set; }

		public INetContext Context { get; protected set; }
		public PacketRouter Router { get; protected set; }

		public bool IsInitialized { get; private set; }
		public bool IsActive { get; protected set; }
		public bool IsServer { get; set; } = true;
		public bool IsClient { get; set; } = false;

		private INetServerProvider provider;
		private Dictionary<object, NetUser> userMap;
		private List<NetUser> userList;
		private int nextUserId;
		private bool disposed;

		public virtual void InitializeServer(INetContext context, INetServerProvider provider) {
			if (IsInitialized)
				throw new Exception("Server already initialized");

			IsInitialized = true;
			this.provider = provider;
			this.Router = new PacketRouter(provider, this);
			this.Context = context;
			this.IsActive = false;

			nextUserId = 0;
			disposed = false;
			userMap = new Dictionary<object, NetUser>();
			userList = new List<NetUser>();

			Context.LockContext(server: this);
		}

		public virtual void StartServer() {
			userMap.Clear();
			userList.Clear();

			provider.UserConnected += OnProviderUserConnected;
			provider.UserDisconnected += OnProviderUserDisconnected;

			provider.StartServer(Context);
			Context.Services.StartServices();
			IsActive = true;
		}

		protected virtual void OnProviderUserConnected(object sender, ProviderUserEventArgs args) {
			var user = new NetUser(args.UserKey, nextUserId++);
			userMap[user.UserKey] = user;
			userList.Add(user);

			NetUserConnected?.Invoke(this, new NetEventArgs() {
				User = user,
			});
		}

		protected virtual void OnProviderUserDisconnected(object sender, ProviderUserEventArgs args) {
			var user = userMap[args.UserKey];
			NetUserDisconnected?.Invoke(this, new NetEventArgs() {
				User = user,
			});

			userMap.Remove(user.UserKey);
			userList.Remove(user);
		}

		public virtual void UpdateServer() {
			if (IsActive == false)
				return;

			Context.Services.UpdateServices();
			provider.UpdateServer();
		}

		public virtual void StopServer() {
			if (IsActive == false)
				return;

			Context.Services.StopServices();
			provider.StopServer();
			IsActive = false;
		}

		public virtual void Dispose() {
			if (IsActive) {
				StopServer();
			}

			if(disposed == false) {
				disposed = true;
				Context.Services.Dispose();
			}
		}

		public virtual IEnumerable<NetUser> GetUsers() {
			return userList.AsReadOnly();
		}
		#endregion
	}
}
