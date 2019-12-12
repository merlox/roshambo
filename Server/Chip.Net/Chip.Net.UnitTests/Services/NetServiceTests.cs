using Chip.Net.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Chip.Net.UnitTests.Services {
	[TestClass]
	public class NetServiceTests {
		[TestMethod]
		public void NetServiceCollection_RegisterService_HasService() {
			NetServiceCollection c = new NetServiceCollection();
			c.Register<TestNetService>();
			c.LockServices();

			var result = c.Get<TestNetService>();

			Assert.IsNotNull(result);
		}

		[TestMethod]
		public void NetServiceCollection_RegisterServiceWithConfigure_ServiceConfigured() {
			NetServiceCollection c = new NetServiceCollection();
			c.Register<TestNetService>();
			c.Configure<TestNetService>((o) => { o.Configured = true; });
			c.LockServices();

			var svc = c.Get<TestNetService>();

			Assert.IsNotNull(svc);
			Assert.IsTrue(svc.Configured);
		}

		[TestMethod]
		public void NetServiceCollection_UpdateServices_ServiceUpdated() {
			NetServiceCollection c = new NetServiceCollection();
			c.Register<TestNetService>();
			c.LockServices();
			c.InitializeServices(new NetContext());
			c.UpdateServices();

			Assert.IsTrue(c.Get<TestNetService>().Updated == true);
		}

		[TestMethod]
		public void NetServiceCollection_GetServices_HasServices() {
			NetServiceCollection c = new NetServiceCollection();
			c.Register<TestNetService>();
			c.LockServices();

			Assert.IsTrue(c.ServiceList.Count() == 1);
			Assert.IsTrue(c.ServiceList.First() == c.Get<TestNetService>());
		}

		[TestMethod]
		public void NetServiceCollection_Initialized_ServiceInitialized() {
			NetServiceCollection c = new NetServiceCollection();
			c.Register<TestNetService>();
			c.LockServices();
			c.InitializeServices(new NetContext());

			Assert.IsTrue(c.Get<TestNetService>().Initialized == true);
		}

		[TestMethod]
		public void NetServiceCollection_Disposed_ServiceDisposed() {
			NetServiceCollection c = new NetServiceCollection();
			c.Register<TestNetService>();
			c.LockServices();
			var svc = c.Get<TestNetService>();
			c.Dispose();

			Assert.IsTrue(svc.Disposed == true);
		}

		[TestMethod]
		public void NetServiceCollection_SetActivationFunc_ActivationFuncInvoked() {
			NetServiceCollection c = new NetServiceCollection();
			c.Register<TestNetService>();

			bool invoked = false;
			c.SetActivationFunction(typeof(TestNetService), () => {
				invoked = true;
				return new TestNetService();
			});

			c.LockServices();
			Assert.IsTrue(invoked);
		}

		[TestMethod]
		public void NetService_ScheduleEvent_EventInvoked() {
			NetService svc = new NetService();
			bool invoked = false;

			svc.InitializeService(new NetContext());
			svc.StartService();
			svc.ScheduleEvent(new TimeSpan(0, 0, 0, 0, 100), () => { invoked = true; });

			int tick = Environment.TickCount;
			while(Environment.TickCount - tick < 1000 && invoked == false) {
				svc.UpdateService();
			}

			Assert.IsTrue(invoked);
		}
	}
}
