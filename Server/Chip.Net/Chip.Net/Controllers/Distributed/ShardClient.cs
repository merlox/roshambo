using System;
using System.Collections.Generic;
using System.Text;
using Chip.Net.Controllers.Basic;
using Chip.Net.Controllers.Distributed.Models;
using Chip.Net.Controllers.Distributed.Packets;
using Chip.Net.Controllers.Distributed.Services;
using Chip.Net.Controllers.Distributed.Services.ModelTracking;
using Chip.Net.Data;
using Chip.Net.Providers;

namespace Chip.Net.Controllers.Distributed
{
	public class ShardClient : BasicClient {

	}

	public class ShardClient<TRouter, TShard, TUser> : ShardClient, INetClientController
		where TRouter : IRouterModel
		where TShard : IShardModel
		where TUser : IUserModel {

		public TShard Model { get; private set; }
		private MessageChannel<SetShardModelPacket<TShard>> ShardSetModel;

		public override void InitializeClient(INetContext context, INetClientProvider provider) {
			context.Packets.Register<SetShardModelPacket<TShard>>();
			context.Packets.Register<SetUserModelPacket<TUser>>();
			context.Packets.Register<SendToShardPacket>();
			context.Packets.Register<SendToUserPacket>();

			base.InitializeClient(context, provider);
			ShardSetModel = CreateRouterChannel<SetShardModelPacket<TShard>>();
			ShardSetModel.Receive += SetShardModel;

			foreach (var svc in Context.Services.ServiceList)
				if (typeof(IDistributedService).IsAssignableFrom(svc.GetType())) {
					(svc as IDistributedService).IsClient = true;
					(svc as IDistributedService).IsShard = true;
					(svc as IDistributedService).ShardController = this;
					(svc as IDistributedService).InitializeShard();
				}
		}

		private void SetShardModel(IncomingMessage<SetShardModelPacket<TShard>> obj) {
			Model = obj.Data.Model;
		}

		public MessageChannel<T> CreateRouterChannel<T>(string key = null) where T : Packet {
			return Router.Route<T>(key);
		}

		public DistributedChannel<T> CreateUserChannel<T>(string key = null) where T : Packet {
			return new DistributedChannel<T>(Router.Route<PassthroughPacket<T>>(key));
		}
	}
}
