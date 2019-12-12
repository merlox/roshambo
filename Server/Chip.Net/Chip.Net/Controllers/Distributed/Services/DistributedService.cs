using Chip.Net.Controllers.Distributed.Models;
using Chip.Net.Controllers.Distributed.Packets;
using Chip.Net.Data;
using Chip.Net.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chip.Net.Controllers.Distributed.Services
{
	public class DistributedService<TRouter, TShard, TUser> : DistributedService
		where TRouter : IRouterModel
		where TShard : IShardModel
		where TUser : IUserModel {
		new public RouterServer<TRouter, TShard, TUser> RouterController { get; set; }
		new public ShardClient<TRouter, TShard, TUser> ShardController { get; set; }
		new public UserClient<TRouter, TShard, TUser> UserController { get; set; }
	}

	public class DistributedService : IDistributedService {
		public bool IsShard { get; set; }
		public bool IsUser { get; set; }
		public bool IsRouter { get; set; }

		public RouterServer RouterController { get; set; }
		public ShardClient ShardController { get; set; }
		public UserClient UserController { get; set; }

		public PacketRouter Router { get => throw new Exception("Use server/client router"); }
		public INetServerController Server { get; set; }
		public INetClientController Client { get; set; }

		public bool IsServer { get; set; }
		public bool IsClient { get; set; }

		public bool Initialized { get; private set; }
		public bool Disposed { get; private set; }

		protected virtual void GlobalInitialize(INetContext context) { }

		protected virtual void InitializeDistributedService() { }

		protected virtual void InitializeContext(INetContext context) { }

		public void InitializeService(INetContext context) {
			InitializeContext(context);
			if (Initialized == false) {
				GlobalInitialize(context);
				Initialized = true;
			}
		}

		public enum RouteType {
			RouterShard,
			RouterUser,
			Passthrough,
		}

		public class ShardChannel<T> : DistributedChannel<T, IShardModel> where T : Packet {
			public ShardChannel(MessageChannel<PassthroughPacket<T>> peerChannel) : base(peerChannel) { }

			public ShardChannel(MessageChannel<T> destinationRawChannel, MessageChannel<PassthroughPacket<T>> destinationChannel) : base(destinationRawChannel, destinationChannel) { }

			public ShardChannel(Func<short, NetUser> netUserResolver, MessageChannel<T> destinationRawChannel, MessageChannel<PassthroughPacket<T>> destinationChannel, MessageChannel<PassthroughPacket<T>> peerChannel) : base(netUserResolver, destinationRawChannel, destinationChannel, peerChannel) { }
		}

		public class UserChannel<T> : DistributedChannel<T, IUserModel> where T : Packet {
			public UserChannel(MessageChannel<PassthroughPacket<T>> peerChannel) : base(peerChannel) { }

			public UserChannel(MessageChannel<T> destinationRawChannel, MessageChannel<PassthroughPacket<T>> destinationChannel) : base(destinationRawChannel, destinationChannel) { }

			public UserChannel(Func<short, NetUser> netUserResolver, MessageChannel<T> destinationRawChannel, MessageChannel<PassthroughPacket<T>> destinationChannel, MessageChannel<PassthroughPacket<T>> peerChannel) : base(netUserResolver, destinationRawChannel, destinationChannel, peerChannel) { }
		}

		public class RouterChannel<T> where T : Packet {
			public MessageChannel<T> Source { get; set; }
			public MessageChannel<T> Shard { get; set; }
			public MessageChannel<T> User { get; set; }

			public RouterChannel(MessageChannel<T> source) {
				this.Source = source;
			}

			public RouterChannel(MessageChannel<T> shard, MessageChannel<T> user) {
				this.Shard = shard;
				this.User = user;
			}

			public void Send(T data) {
				if (Source != null) Source.Send(data);
				if (Shard != null) Shard.Send(data);
				if (User != null) User.Send(data);
			}
		}

		public class DistributedChannel<T, TModel> where T : Packet where TModel : IDistributedModel {
			private MessageChannel<T> destinationRawChannel;
			private MessageChannel<PassthroughPacket<T>> destinationChannel;
			private MessageChannel<PassthroughPacket<T>> peerChannel;

			public EventHandler<T> Receive { get; set; }
			public Func<short, NetUser> Resolver { get; set; }

			public void Send(T data) {
				if (destinationRawChannel != null) {
					if (peerChannel != null) {
						destinationRawChannel.Send(new OutgoingMessage<T>(data));
					} else {
						destinationRawChannel.Send(new OutgoingMessage<T>(data));
					}
				} else if (peerChannel != null) {
					peerChannel.Send(new OutgoingMessage<PassthroughPacket<T>>(new PassthroughPacket<T>(data)));
				}
			}

			public void Send(T data, TModel recipient) {
				this.Send(data, recipient.Id);
			}

			public void Send(T data, IEnumerable<TModel> recipients) {
				foreach (var recipient in recipients)
					this.Send(data, recipient);
			}

			private void Send(T data, int recipient) {
				if (destinationRawChannel != null) {
					if (peerChannel != null) {
						if(recipient <= 0) {
							destinationRawChannel.Send(data);
						} else {
							destinationRawChannel.Send(Resolver((short)recipient), data);
						}
					} else {
						destinationChannel.Send(new OutgoingMessage<PassthroughPacket<T>>(new PassthroughPacket<T>(data, recipient)));
					}
				} else if (peerChannel != null) {
					peerChannel.Send(new OutgoingMessage<PassthroughPacket<T>>(new PassthroughPacket<T>(data, recipient)));
				}
			}

			public DistributedChannel(MessageChannel<PassthroughPacket<T>> peerChannel) {
				this.peerChannel = peerChannel;
			}

			public DistributedChannel(MessageChannel<T> destinationRawChannel, MessageChannel<PassthroughPacket<T>> destinationChannel) {
				this.destinationRawChannel = destinationRawChannel;
				this.destinationChannel = destinationChannel;

				this.destinationRawChannel.Receive += (e) => {
					Receive?.Invoke(this, e.Data);
				};

				this.destinationChannel.Receive += (e) => {
					Receive?.Invoke(this, e.Data.Data);
				};
			}

			public DistributedChannel(Func<short, NetUser> netUserResolver, MessageChannel<T> destinationRawChannel, MessageChannel<PassthroughPacket<T>> destinationChannel, MessageChannel<PassthroughPacket<T>> peerChannel) {
				this.Resolver = netUserResolver;
				this.destinationRawChannel = destinationRawChannel;
				this.destinationChannel = destinationChannel;
				this.peerChannel = peerChannel;

				this.destinationRawChannel.Receive += (e) => {
					Send(e.Data);
				};

				this.destinationChannel.Receive += (e) => {
					Send(e.Data.Data, e.Data.RecipientId);
				};

				this.peerChannel.Receive += (e) => {
					Send(e.Data.Data, e.Data.RecipientId);
				};
			}
		}

		public ShardChannel<T> CreateShardChannel<T>(string key = null) where T : Packet {
			if (key == null)
				key = "";

			key = "[ShardChannel]" + key;

			if (IsUser) {
				var userChannel = UserController.Router.Route<PassthroughPacket<T>>(key);
				return new ShardChannel<T>(
					userChannel);
			}

			if (IsShard) {
				var shardRawChannel = ShardController.Router.Route<T>(key);
				var shardChannel = ShardController.Router.Route<PassthroughPacket<T>>(key);

				return new ShardChannel<T>(
					shardRawChannel,
					shardChannel);
			}

			if (IsRouter) {
				var shardRawChannel = RouterController.ShardController.Router.Route<T>(key);
				var shardChannel = RouterController.ShardController.Router.Route<PassthroughPacket<T>>(key);
				var userChannel = RouterController.UserController.Router.Route<PassthroughPacket<T>>(key);

				return new ShardChannel<T>((id) => RouterController.GetNetUserFromShard(id),
					shardRawChannel,
					shardChannel,
					userChannel);
			}

			return null;
		}

		public UserChannel<T> CreateUserChannel<T>(string key = null) where T : Packet {
			if (key == null)
				key = "";

			key = "[UserChannel]" + key;

			if (IsUser) {
				var userRawChannel = UserController.Router.Route<T>(key);
				var userChannel = UserController.Router.Route<PassthroughPacket<T>>(key);

				return new UserChannel<T>(
					userRawChannel,
					userChannel);
			}

			if (IsShard) {
				var shardChannel = ShardController.Router.Route<PassthroughPacket<T>>(key);
				return new UserChannel<T>(
					shardChannel);
			}

			if (IsRouter) {
				var userRawChannel = RouterController.UserController.Router.Route<T>(key);
				var userChannel = RouterController.UserController.Router.Route<PassthroughPacket<T>>(key);
				var shardChannel = RouterController.ShardController.Router.Route<PassthroughPacket<T>>(key);

				return new UserChannel<T>((id) => RouterController.GetNetUserFromUser(id),
					userRawChannel,
					userChannel,
					shardChannel);
			}

			return null;
		}

		public RouterChannel<T> CreateRouterChannel<T>(string key = null) where T : Packet {
			if(IsRouter) {
				var sh = ShardController.Router.Route<T>(key);
				var us = UserController.Router.Route<T>(key);

				return new RouterChannel<T>(sh, us);
			}

			if (IsShard) {
				return new RouterChannel<T>(ShardController.Router.Route<T>(key));
			}

			if(IsUser) {
				return new RouterChannel<T>(UserController.Router.Route<T>(key));
			}

			return null;
		}

		public DistributedChannel<T> CreatePassthrough<T>(string key = null) where T : Packet {
			if(IsClient) {
				return new DistributedChannel<T>(Client.Router.Route<PassthroughPacket<T>>(key));
			}

			if(IsServer) {
				RouterController.CreatePassthrough<T>(key);
			}

			return null;
		}

		public virtual void InitializeRouter() {
			IsServer = true;
			IsRouter = true;
			IsClient = false;
			IsShard = false;
			IsUser = false;

			InitializeDistributedService();
		}

		public virtual void InitializeShard() {
			IsServer = false;
			IsRouter = false;
			IsClient = true;
			IsShard = true;
			IsUser = false;

			InitializeDistributedService();
		}

		public virtual void InitializeUser() {
			IsServer = false;
			IsRouter = false;
			IsClient = true;
			IsShard = false;
			IsUser = true;

			InitializeDistributedService();
		}

		public virtual void StartService() { }

		public virtual void StopService() { }

		public virtual void UpdateService() {
			if (IsRouter) UpdateRouter();
			if (IsShard) UpdateShard();
			if (IsUser) UpdateUser();
		}

		protected virtual void UpdateRouter() { }
		protected virtual void UpdateShard() { }
		protected virtual void UpdateUser() { }

		public void Dispose() {
			Disposed = true;
		}
	}
}
