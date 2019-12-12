using Chip.Net.Data;
using Chip.Net.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Chip.Net.UnitTests
{
	[TestClass]
	public class NetContextTests
	{
		public class Svc1 : NetService { }
		public class Svc2 : NetService { }
		public class Svc3 : NetService { }
		public class Svc4 : NetService { }

		public class Pck1 : Packet { }
		public class Pck2 : Packet { }
		public class Pck3 : Packet { }
		public class Pck4 : Packet { }

		public class TestData { public string Data { get; set; } }

		private INetContext CreateContext()
		{
			INetContext ctx = new NetContext();
			ctx.ApplicationName = Guid.NewGuid().ToString();
			ctx.IPAddress = "localhost";
			ctx.MaxConnections = 10;
			ctx.Port = 1111;
			ctx.Serializer.AddReaderWriter<TestData>(
				new DataWriter(ctx.Serializer, typeof(TestData)), 
				new DataReader(ctx.Serializer, typeof(TestData)));

			ctx.Services.Register<Svc1>();
			ctx.Services.Register<Svc2>();
			ctx.Services.Register<Svc3>();
			ctx.Services.Register<Svc4>();

			ctx.Packets.Register<Pck4>();
			ctx.Packets.Register<Pck3>();
			ctx.Packets.Register<Pck2>();
			ctx.Packets.Register<Pck1>();

			ctx.LockContext();
			return ctx;
		}

		[TestMethod]
		public void NetContext_Clone_ServicesCopied()
		{
			var context = CreateContext();
			var clone = context.Clone();

			Assert.IsTrue(clone.Services.ServiceList.Count == context.Services.ServiceList.Count);
			foreach(var svc in context.Services.ServiceList)
			{
				var match = clone.Services.ServiceList.FirstOrDefault(i => i.GetType() == svc.GetType());
				Assert.IsNotNull(match);
				Assert.AreEqual(
					context.Services.GetServiceId(svc), 
					clone.Services.GetServiceId(match));
			}
		}

		[TestMethod]
		public void NetContext_Clone_PacketsCopied()
		{
			var context = CreateContext();
			var clone = context.Clone();

			Assert.AreEqual(context.Packets.GetID<Pck1>(), clone.Packets.GetID<Pck1>());
			Assert.AreEqual(context.Packets.GetID<Pck2>(), clone.Packets.GetID<Pck2>());
			Assert.AreEqual(context.Packets.GetID<Pck3>(), clone.Packets.GetID<Pck3>());
			Assert.AreEqual(context.Packets.GetID<Pck4>(), clone.Packets.GetID<Pck4>());
		}

		[TestMethod]
		public void NetContext_Clone_ConfigurationCopied()
		{
			var context = CreateContext();
			var clone = context.Clone();

			Assert.AreEqual(context.ApplicationName, clone.ApplicationName);
			Assert.AreEqual(context.MaxConnections, clone.MaxConnections);
			Assert.AreEqual(context.IPAddress, clone.IPAddress);
			Assert.AreEqual(context.Port, clone.Port);
		}

		[TestMethod]
		public void NetContext_Clone_SerializerCopied()
		{
			var context = CreateContext();
			var clone = context.Clone();

			var wkeys = context.Serializer.Writers.Keys;
			var rkeys = context.Serializer.Readers.Keys;

			foreach (var wkey in wkeys)
				Assert.IsTrue(clone.Serializer.Writers.ContainsKey(wkey));
			foreach (var rkey in rkeys)
				Assert.IsTrue(clone.Serializer.Readers.ContainsKey(rkey));
		}
	}
}
 