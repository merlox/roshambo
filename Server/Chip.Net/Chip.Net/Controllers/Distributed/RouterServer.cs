using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chip.Net.Controllers.Basic;
using Chip.Net.Controllers.Distributed.Models;
using Chip.Net.Controllers.Distributed.Packets;
using Chip.Net.Controllers.Distributed.Services;
using Chip.Net.Controllers.Distributed.Services.ModelTracking;
using Chip.Net.Data;
using Chip.Net.Providers;
using Chip.Net.Providers.Direct;

namespace Chip.Net.Controllers.Distributed
{
	public abstract class RouterServer {
		public PacketRouter Router { get; protected set; }
		public bool IsActive { get; private set; }

		public BasicServer ShardController { get; protected set; }
		public BasicServer UserController { get; protected set; }

		public INetContext ShardContext { get; protected set; }
		public INetContext UserContext { get; protected set; }

		public abstract MessageChannel<T> CreateUserChannel<T>(string key = null) where T : Packet;

		public abstract MessageChannel<T> CreateShardChannel<T>(string key = null) where T : Packet;

		public abstract void CreatePassthrough<T>(string key = null) where T : Packet;

		public abstract NetUser GetNetUserFromShard(short id);
		public abstract NetUser GetNetUserFromUser(short id);

		public abstract IShardModel GetShard(short id);
		public abstract IUserModel GetUser(short id);

		public abstract IShardModel GetShard(NetUser user);
		public abstract IUserModel GetUser(NetUser user);
	}

	public class RouterServer<TRouter, TShard, TUser> : RouterServer
		where TRouter : IRouterModel
		where TShard : IShardModel
		where TUser : IUserModel {

		public EventHandler<TRouter> RouterConfiguredEvent { get; set; }
		public EventHandler<TShard> ShardDisconnectedEvent { get; set; }
		public EventHandler<TShard> ShardConnectedEvent { get; set; }
		public EventHandler<TUser> UserDisconnectedEvent { get; set; }
		public EventHandler<TUser> UserConnectedEvent { get; set; }

		public EventHandler<ShardDataEventArgs> ShardDataReceivedEvent { get; set; }
		public EventHandler<UserDataEventArgs> UserDataReceivedEvent { get; set; }
		public EventHandler<ShardDataEventArgs> ShardDataSentEvent { get; set; }
		public EventHandler<UserDataEventArgs> UserDataSentEvent { get; set; }

		public struct ShardDataEventArgs {
			public TShard Shard { get; set; }
			public Packet Data { get; set; }
		}

		public struct UserDataEventArgs {
			public TUser User { get; set; }
			public Packet Data { get; set; }
		}


		public ModelTrackerCollection<TShard> Shards { get; private set; }
		public ModelTrackerCollection<TUser> Users { get; private set; }

		public TRouter Model { get; private set; }

		private INetContext Context;
		private Dictionary<TShard, NetUser> shardToNetUser;
		private Dictionary<TUser, NetUser> userToNetUser;

		private readonly string UK_Shard = "Router-ShardConnection";
		private readonly string UK_User = "Router-UserConnection";

		private MessageChannel<SetShardModelPacket<TShard>> ShardSetModel;
		private MessageChannel<SetUserModelPacket<TUser>> UserSetModel;

		public void InitializeServer(INetContext context, 
			INetServerProvider shardProvider, int shardPort, 
			INetServerProvider userProvider, int userPort) {
			Context = context;

			shardToNetUser = new Dictionary<TShard, NetUser>();
			userToNetUser = new Dictionary<TUser, NetUser>();

			var services = new List<IDistributedService>();
			foreach (var type in context.Services.ServiceTypes)
				if (typeof(IDistributedService).IsAssignableFrom(type)) {
					var instance = Activator.CreateInstance(type) as IDistributedService;
					context.Services.SetActivationFunction(type, () => instance);
					services.Add(instance);
				}

			var shardContext = context.Clone();
			var userContext = context.Clone();

			shardContext.Port = shardPort;
			userContext.Port = userPort;

			shardContext.Port = shardPort;
			userContext.Port = userPort;

			ShardController = shardContext.CreateServer<BasicServer>(shardProvider);
			ShardController.NetUserConnected += OnShardConnected;
			ShardController.NetUserDisconnected += OnShardDisconnected;
			ShardController.PacketReceived += OnShardDataReceived;
			ShardContext = ShardController.Context;

			UserController = userContext.CreateServer<BasicServer>(userProvider);
			UserController.NetUserConnected += OnUserConnected;
			UserController.NetUserDisconnected += OnUserDisconnected;
			UserController.PacketReceived += OnUserDataReceived;
			UserContext = UserController.Context;

			ShardSetModel = CreateShardChannel<SetShardModelPacket<TShard>>();
			UserSetModel = CreateUserChannel<SetUserModelPacket<TUser>>();

			Shards = new ModelTrackerCollection<TShard>();
			Users = new ModelTrackerCollection<TUser>();

			this.Model = Activator.CreateInstance<TRouter>();
			this.Model.Id = 0;
			this.Model.Name = context.ApplicationName + " Router";
			RouterConfiguredEvent?.Invoke(this, Model);

			foreach (var service in services) {
				service.RouterController = this;
				service.InitializeRouter();
			}
		}

		#region Event Handling
		private void OnShardConnected(object sender, NetEventArgs e) {
			var shard = Activator.CreateInstance<TShard>();
			Shards.Add(shard);

			SetShard(e.User, shard);
			ShardConnectedEvent?.Invoke(this, shard);

			ShardSetModel.Send(e.User, new SetShardModelPacket<TShard>() {
				Model = shard
			});
		}

		private void OnShardDisconnected(object sender, NetEventArgs e) {
			var shard = GetShardModel(e.User);
			Shards.Remove(shard);

			ShardDisconnectedEvent?.Invoke(this, shard);
		}

		private void OnShardDataReceived(object sender, NetEventArgs e) {
			var shard = GetShardModel(e.User);

			ShardDataReceivedEvent?.Invoke(this, new ShardDataEventArgs()
			{
				Data = e.Packet,
				Shard = shard
			});
		}

		private void OnUserConnected(object sender, NetEventArgs e) {
			var user = Activator.CreateInstance<TUser>();
			Users.Add(user);

			SetUserModel(e.User, user);
			UserConnectedEvent?.Invoke(this, user);

			UserSetModel.Send(e.User, new SetUserModelPacket<TUser>() {
				Model = user,
			});
		}

		private void OnUserDisconnected(object sender, NetEventArgs e) {
			var user = GetUserModel(e.User);
			Users.Remove(user);

			UserDisconnectedEvent?.Invoke(this, user);
		}

		private void OnUserDataReceived(object sender, NetEventArgs e) {
			var user = GetUserModel(e.User);

			UserDataReceivedEvent?.Invoke(this, new UserDataEventArgs()
			{
				Data = e.Packet,
				User = user,
			});
		}
		#endregion

		public override MessageChannel<T> CreateUserChannel<T>(string key = null) {
			return UserController.Router.Route<T>(key);
		}

		public override MessageChannel<T> CreateShardChannel<T>(string key = null) {
			return ShardController.Router.Route<T>(key);
		}

		public override void CreatePassthrough<T>(string key = null) {
			var user = CreateUserChannel<PassthroughPacket<T>>(key);
			var shard = CreateShardChannel<PassthroughPacket<T>>(key);

			user.Receive += (e) => {
				if(e.Data.RecipientId == 0) {
					foreach (var sh in Shards)
						shard.Send(new OutgoingMessage<PassthroughPacket<T>>(e.Data, GetNetUser(sh)));
				} else {
					var s = Shards[e.Data.RecipientId];
					shard.Send(new OutgoingMessage<PassthroughPacket<T>>(e.Data, GetNetUser(s)));
				}
			};

			shard.Receive += (e) => {
				if (e.Data.RecipientId == 0) {
					foreach (var us in Users)
						user.Send(new OutgoingMessage<PassthroughPacket<T>>(e.Data, GetNetUser(us)));
				} else {
					var u = Users[e.Data.RecipientId];
					user.Send(new OutgoingMessage<PassthroughPacket<T>>(e.Data, GetNetUser(u)));
				}
			};
		}

		public void StartShardServer() {
			ShardController.StartServer();
		}

		public void StartUserServer() {
			UserController.StartServer();
		}

		public void UpdateServer()
		{
			if (ShardController != null && ShardController.IsActive) ShardController.UpdateServer();
			if (UserController != null && UserController.IsActive) UserController.UpdateServer();
		}

		private void SetShard(NetUser user, TShard shardModel)
		{
			shardToNetUser.Add(shardModel, user);
			user.SetLocal<TShard>(UK_Shard, shardModel);
		}

		public TShard GetShardModel(NetUser user) 
		{
			return user.GetLocal<TShard>(UK_Shard);
		}

		private void SetUserModel(NetUser user, TUser userModel)
		{
			userToNetUser.Add(userModel, user);
			user.SetLocal<TUser>(UK_User, userModel);
		}

		public TUser GetUserModel(NetUser user)
		{
			return user.GetLocal<TUser>(UK_User);
		}

		public NetUser GetNetUser(TShard shard) {
			if (shardToNetUser.ContainsKey(shard) == false)
				return null;

			return shardToNetUser[shard];
		}

		public NetUser GetNetUser(TUser user) {
			if (userToNetUser.ContainsKey(user) == false)
				return null;

			return userToNetUser[user];
		}

		public override NetUser GetNetUserFromShard(short id) {
			return shardToNetUser[Shards[id]];
		}

		public override NetUser GetNetUserFromUser(short id) {
			return userToNetUser[Users[id]];
		}

		public void Shutdown()
		{
			if (ShardController.IsActive) ShardController.Dispose();
			if (UserController.IsActive) UserController.Dispose();

			ShardController = null;
			UserController = null;
		}

		public void Dispose() {
			Shutdown();
		}

		public override IShardModel GetShard(short id) {
			return Shards[id];
		}

		public override IUserModel GetUser(short id) {
			return Users[id];
		}

		public override IShardModel GetShard(NetUser user) {
			return GetShardModel(user);
		}

		public override IUserModel GetUser(NetUser user) {
			return GetUserModel(user);
		}
	}
}
