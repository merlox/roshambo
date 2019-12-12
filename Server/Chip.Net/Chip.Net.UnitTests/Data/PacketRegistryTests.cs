using Chip.Net.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chip.Net.UnitTests.Data
{
	[TestClass]
	public class PacketRegistryTests {
		public class TestPacket : SerializedPacket {
			public string Data { get; set; }
		}

		public class TestPacket2 : SerializedPacket {
			public string Data { get; set; }
		}

		public class TestPacket3 : SerializedPacket {
			public string Data { get; set; }
		}

		private PacketRegistry registry;

		[TestInitialize]
		public void Initialize() {
			registry = new PacketRegistry();
		}

		[TestMethod]
		public void PacketRegistry_RegisterPacket_GetID() {
			registry.Register<TestPacket>();
			registry.LockPackets();

			Assert.IsTrue(registry.GetID<TestPacket>() >= 0);
			Assert.IsTrue(registry.GetID(typeof(TestPacket)) >= 0);
		}

		[TestMethod]
		public void PacketRegistry_RegisterPacket_CreateFromID() {
			registry.Register<TestPacket>();
			registry.LockPackets();

			var id = registry.GetID<TestPacket>();
			var result = registry.CreateFromId(id);

			Assert.IsNotNull(result);
			Assert.IsTrue(result.GetType() == typeof(TestPacket));
		}

		[TestMethod]
		public void PacketRegistry_EmptyRegistry_GetID_ExceptionThrown() {
			registry.LockPackets();

			bool exception = false;
			try {
				registry.GetID(typeof(TestPacket));
			} catch {
				exception = true;
			}

			Assert.IsTrue(exception);
		}

		[TestMethod]
		public void PacketRegistry_EmptyRegistry_CreateFromId_ExceptionThrown() {
			registry.LockPackets();

			bool exception = false;
			try {
				registry.CreateFromId(0);
			} catch {
				exception = true;
			}

			Assert.IsTrue(exception);
		}

		[TestMethod]
		public void PacketRegistry_RegisterMany_UniqueIDs() {
			registry.Register<TestPacket>();
			registry.Register<TestPacket2>();
			registry.Register<TestPacket3>();
			registry.LockPackets();

			Assert.AreNotEqual(registry.GetID<TestPacket>(), registry.GetID<TestPacket2>());
			Assert.AreNotEqual(registry.GetID<TestPacket>(), registry.GetID<TestPacket3>());
			Assert.AreNotEqual(registry.GetID<TestPacket2>(), registry.GetID<TestPacket3>());
		}

		[TestMethod]
		public void PacketRegistry_RegisterMany_OrderConsistent() {
			registry.Register<TestPacket>();
			registry.Register<TestPacket2>();
			registry.Register<TestPacket3>();
			registry.LockPackets();

			PacketRegistry compare = new PacketRegistry();
			compare.Register<TestPacket3>();
			compare.Register<TestPacket2>();
			compare.Register<TestPacket>();
			compare.LockPackets();

			Assert.AreEqual(registry.GetID<TestPacket>(), compare.GetID<TestPacket>());
			Assert.AreEqual(registry.GetID<TestPacket2>(), compare.GetID<TestPacket2>());
			Assert.AreEqual(registry.GetID<TestPacket3>(), compare.GetID<TestPacket3>());
		}

		[TestMethod]
		public void PacketRegistry_LockRegistry_IsLocked() {
			registry.Register<TestPacket>();
			registry.LockPackets();

			Assert.IsTrue(registry.IsLocked);
		}

		[TestMethod]
		public void PacketRegistry_Locked_RegisterPacket_ExceptionThrown() {
			registry.LockPackets();

			bool exception = false;
			try {
				registry.Register<TestPacket>();
			} catch {
				exception = true;
			}

			Assert.IsTrue(exception);
		}
	}
}
