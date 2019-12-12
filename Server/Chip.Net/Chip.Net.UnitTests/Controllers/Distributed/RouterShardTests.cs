using Chip.Net.Controllers;
using Chip.Net.Controllers.Distributed;
using Chip.Net.Providers.Direct;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chip.Net.UnitTests.Controllers.Distributed
{
	[TestClass]
    public class RouterShardTests : BaseDistributedTests<ShardClient<TestRouterModel, TestShardModel, TestUserModel>> {
		public RouterServer<TestRouterModel, TestShardModel, TestUserModel> Router { get; set; }

		protected override ServerWrapper NewServer() {
			Router = new RouterServer<TestRouterModel, TestShardModel, TestUserModel>();
			Router.InitializeServer(CreateContext(), new DirectServerProvider(), 0, new DirectServerProvider(), 1);
			var ch = Router.CreateShardChannel<TestPacket>();

			return new BaseControllerTests<INetServerController, ShardClient<TestRouterModel, TestShardModel, TestUserModel>>.ServerWrapper() {
				Server = Router.ShardController,
				Channel = ch,
			};
		}

		protected override ServerWrapper StartNewServer() {
			var sv = NewServer();
			Router.StartShardServer();
			return sv;
		}

		protected override void UpdateServer(INetServerController server) {
			base.UpdateServer(server);
			Router.UpdateServer();
		}

		protected override INetContext CreateContext() {
			var ctx = base.CreateContext();
			ctx.Port = 0;
			return ctx;
		}
	}
}
