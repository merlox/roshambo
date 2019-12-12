using Chip.Net.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chip.Net.UnitTests.Data
{
	[TestClass]
	public class SerializedPacketTests
	{
		public class EmptyModel : SerializedPacket {

		}

		public class TestModel : SerializedPacket {
			public int Value { get; set; }
			public string Data { get; set; }
		}

		public class TestComplexModel : SerializedPacket {
			public int Value { get; set; }
			public string Data { get; set; }
			public TestModel Model1 { get; set; }
			public TestModel Model2 { get; set; }
		}

		public class TestNestedModel : SerializedPacket {
			public TestNestedModel Inner { get; set; }
			public string Data { get; set; }
		}

		public class TestIgnorePropModel : SerializedPacket {
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
		public void SerializedPacket_WriteRead_EmptyPacket() {
			DataBuffer buffer = new DataBuffer();
			EmptyModel model = new EmptyModel();

			model.WriteTo(buffer);
			buffer.Seek(0);

			model = new EmptyModel();
			model.ReadFrom(buffer);
		}

		[TestMethod]
		public void SerializedPacket_WriteRead_SimplePacket() {
			DataBuffer buffer = new DataBuffer();
			TestModel model = new TestModel();
			model.Data = Guid.NewGuid().ToString();
			model.Value = random.Next();

			model.WriteTo(buffer);
			buffer.Seek(0);

			TestModel result = new TestModel();
			result.ReadFrom(buffer);

			Assert.AreEqual(result.Data, model.Data);
			Assert.AreEqual(result.Value, model.Value);
		}

		[TestMethod]
		public void SerializedPacket_WriteRead_ComplexPacket() {
			DataBuffer buffer = new DataBuffer();
			TestComplexModel model = new TestComplexModel();
			model.Data = Guid.NewGuid().ToString();
			model.Value = random.Next();
			model.Model1 = new TestModel();
			model.Model2 = new TestModel();
			model.Model1.Data = Guid.NewGuid().ToString();
			model.Model2.Data = Guid.NewGuid().ToString();
			model.Model1.Value = random.Next();
			model.Model2.Value = random.Next();

			model.WriteTo(buffer);
			buffer.Seek(0);

			TestComplexModel result = new TestComplexModel();
			result.ReadFrom(buffer);

			Assert.AreEqual(result.Data, model.Data);
			Assert.AreEqual(result.Value, model.Value);
			Assert.AreEqual(result.Model1.Data, model.Model1.Data);
			Assert.AreEqual(result.Model2.Data, model.Model2.Data);
			Assert.AreEqual(result.Model1.Value, model.Model1.Value);
			Assert.AreEqual(result.Model2.Value, model.Model2.Value);
		}

		[TestMethod]
		public void SerializedPacket_WriteRead_NestedPacket() {
			int nestCount = 100;
			DataBuffer buffer = new DataBuffer();
			TestNestedModel root = new TestNestedModel();
			var cur = root;
			for (int i = 0; i < nestCount; i++) {
				cur.Inner = new TestNestedModel();
				cur.Inner.Data = Guid.NewGuid().ToString();
				cur = cur.Inner;
			}

			root.WriteTo(buffer);
			buffer.Seek(0);

			TestNestedModel result = new TestNestedModel();
			result.ReadFrom(buffer);

			var cur2 = result;
			cur = root;

			for (int i = 0; i < nestCount; i++) {
				Assert.AreEqual(cur.Data, cur2.Data);
				cur = cur.Inner;
				cur2 = cur2.Inner;
			}

		}

		[TestMethod]
		public void SerializedPacket_WriteRead_IgnoreProperty() {
			DataBuffer buffer = new DataBuffer();
			TestIgnorePropModel model = new TestIgnorePropModel();
			model = new TestIgnorePropModel();
			model.Data = Guid.NewGuid().ToString();
			model.Ignore = Guid.NewGuid().ToString();

			model.WriteTo(buffer);
			buffer.Seek(0);

			TestIgnorePropModel result = new TestIgnorePropModel();
			result.ReadFrom(buffer);

			Assert.IsNull(result.Ignore);
			Assert.AreEqual(result.Data, model.Data);
		}
	}
}
