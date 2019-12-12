using Chip.Net.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chip.Net.UnitTests.Data
{
	[TestClass]
	public class DynamicPacketTests {
		public class EmptyModel {

		}

		public class TestModel {
			public int Value { get; set; }
			public string Data { get; set; }
		}

		public class TestComplexModel {
			public int Value { get; set; }
			public string Data { get; set; }
			public TestModel Model1 { get; set; }
			public TestModel Model2 { get; set; }
		}

		public class TestNestedModel {
			public TestNestedModel Inner { get; set; }
			public string Data { get; set; }
		}

		public class TestIgnorePropModel {
			public string Data { get; set; }

			[IgnoreProperty]
			public string Ignore { get; set; }
		}

		private Random random;

		[TestInitialize]
		public void Initialize() {
			random = new Random();
		}

		[TestMethod]
		public void DynamicPacket_WriteRead_EmptyModel() {
			DataBuffer buffer = new DataBuffer();
			Packet<EmptyModel> packet = new Packet<EmptyModel>();
			packet.Model = new EmptyModel();

			//this is dumb, just make sure no exception
			packet.WriteTo(buffer);
			buffer.Seek(0);

			packet = new Packet<EmptyModel>();
			packet.ReadFrom(buffer);
		}

		[TestMethod]
		public void DynamicPacket_WriteRead_SimpleModel() {
			DataBuffer buffer = new DataBuffer();
			Packet<TestModel> packet = new Packet<TestModel>();
			packet.Model.Data = Guid.NewGuid().ToString();
			packet.Model.Value = random.Next();

			packet.WriteTo(buffer);
			buffer.Seek(0);

			Packet<TestModel> result = new Packet<TestModel>();
			result.ReadFrom(buffer);

			Assert.AreEqual(result.Model.Value, packet.Model.Value);
			Assert.AreEqual(result.Model.Data, packet.Model.Data);
		}

		[TestMethod]
		public void DynamicPacket_WriteRead_ComplexModel() {
			DataBuffer buffer = new DataBuffer();
			Packet<TestComplexModel> packet = new Packet<TestComplexModel>();
			packet.Model.Data = Guid.NewGuid().ToString();
			packet.Model.Value = random.Next();
			packet.Model.Model1 = new TestModel();
			packet.Model.Model2 = new TestModel();
			packet.Model.Model1.Data = Guid.NewGuid().ToString();
			packet.Model.Model2.Data = Guid.NewGuid().ToString();
			packet.Model.Model1.Value = random.Next();
			packet.Model.Model2.Value = random.Next();

			packet.WriteTo(buffer);
			buffer.Seek(0);

			Packet<TestComplexModel> result = new Packet<TestComplexModel>();
			result.ReadFrom(buffer);
			Assert.AreEqual(result.Model.Data, packet.Model.Data);
			Assert.AreEqual(result.Model.Value, packet.Model.Value);
			Assert.AreEqual(result.Model.Model1.Data, packet.Model.Model1.Data);
			Assert.AreEqual(result.Model.Model2.Data, packet.Model.Model2.Data);
			Assert.AreEqual(result.Model.Model1.Value, packet.Model.Model1.Value);
			Assert.AreEqual(result.Model.Model2.Value, packet.Model.Model2.Value);
		}

		[TestMethod]
		public void DynamicPacket_WriteRead_NestedModel() {
			int nestCount = 100;
			DataBuffer buffer = new DataBuffer();
			TestNestedModel root = new TestNestedModel();
			var cur = root;
			for(int i = 0; i < nestCount; i++) {
				cur.Inner = new TestNestedModel();
				cur.Inner.Data = Guid.NewGuid().ToString();
				cur = cur.Inner;
			}

			Packet<TestNestedModel> packet = new Packet<TestNestedModel>();
			packet.Model = root;

			packet.WriteTo(buffer);
			buffer.Seek(0);

			Packet<TestNestedModel> result = new Packet<TestNestedModel>();
			result.ReadFrom(buffer);

			var cur2 = result.Model;
			cur = root;

			for(int i = 0; i < nestCount; i++) {
				Assert.AreEqual(cur.Data, cur2.Data);
				cur = cur.Inner;
				cur2 = cur2.Inner;
			}
		}

		[TestMethod]
		public void DynamicPacket_WriteRead_IgnoreProperty() {
			DataBuffer buffer = new DataBuffer();
			Packet<TestIgnorePropModel> packet = new Packet<TestIgnorePropModel>();
			packet.Model = new TestIgnorePropModel();
			packet.Model.Data = Guid.NewGuid().ToString();
			packet.Model.Ignore = Guid.NewGuid().ToString();

			packet.WriteTo(buffer);
			buffer.Seek(0);

			Packet<TestIgnorePropModel> result = new Packet<TestIgnorePropModel>();
			result.ReadFrom(buffer);

			Assert.IsNull(result.Model.Ignore);
			Assert.AreEqual(result.Model.Data, packet.Model.Data);
		}
	}
}
