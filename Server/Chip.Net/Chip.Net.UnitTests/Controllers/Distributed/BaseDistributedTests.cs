using Chip.Net.Controllers;
using Chip.Net.Controllers.Distributed;
using Chip.Net.Providers.Direct;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chip.Net.UnitTests.Controllers.Distributed
{
    public class BaseDistributedTests<TClient> : BaseControllerTests<INetServerController, TClient> where TClient : class, INetClientController
    {
		private static Dictionary<string, RouterServer<TestRouterModel, TestShardModel, TestUserModel>> routerMap = new Dictionary<string, RouterServer<TestRouterModel, TestShardModel, TestUserModel>>();
		private RouterServer<TestRouterModel, TestShardModel, TestUserModel> Router;

		string guid = null;
		private INetContext _cache;
		protected virtual INetContext CreateContext() {
			if (_cache != null)
				return _cache;

			if (guid == null) guid = Guid.NewGuid().ToString();

			INetContext ctx = this.Context;
			ctx.ApplicationName = guid;
			ctx.IPAddress = guid;
			ctx.Port = 0;

			_cache = ctx;
			return ctx;
		}

		protected override ServerWrapper StartNewServer() {
			var ctx = CreateContext();
			Router = new RouterServer<TestRouterModel, TestShardModel, TestUserModel>();
			Router.InitializeServer(ctx, new DirectServerProvider(), 0, new	DirectServerProvider(), 1);

			Router.StartShardServer();
			Router.StartUserServer();

			lock (routerMap) {
				routerMap.Add(Router.ShardContext.ApplicationName + "SHARD", Router);
				routerMap.Add(Router.UserContext.ApplicationName + "USER", Router);
			}

			return null;
		}

		protected override ServerWrapper NewServer() {
			return default(ServerWrapper);
		}

		protected override ClientWrapper StartNewClient() {
			var cl = NewClient();
			cl.Client.StartClient();

			return cl;
		}

		protected override ClientWrapper NewClient() {
			var cl = Activator.CreateInstance<TClient>();
			var ctx = CreateContext();
			cl.InitializeClient(ctx, new DirectClientProvider());
			var ch = cl.Router.Route<TestPacket>();

			return new BaseControllerTests<INetServerController, TClient>.ClientWrapper() {
				Channel = ch,
				Client = cl,
			};
		}

		protected override void UpdateClient(TClient client) {
			base.UpdateClient(client);
		}

		protected override void UpdateServer(INetServerController server) {
			base.UpdateServer(server);
		}
	}
}
