using Chip.Net.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chip.Net.UnitTests.Data
{
	[TestClass]
    public class DynamicSerializerTests {
		public enum TestEnum {
			One = 1,
			Two = 10,
			Three = 11,
		}

		public interface ITestInterface {
			string Data { get; set; }
		}

		public class TestImplemented : ITestInterface {
			public string Data { get; set; }
		}

		public struct TestStruct {
			public string Data { get; set; }
		}

		public class TestSerializedPacket : SerializedPacket {
			public string Data { get; set; }
		}

		public class TestModelWithIgnore {
			public string Data { get; set; }

			[IgnoreProperty]
			public string Ignored { get; set; }
		}

		public class TestModelWithConstructor {
			public string Data { get; set; }
			public TestModelWithConstructor(string Data) {
				this.Data = Data;
			}
		}

		public class TestModelISerializable : ISerializable {
			public UInt16 UShort { get; set; }
			public UInt32 UInt { get; set; }
			public UInt64 ULong { get; set; }

			public void ReadFrom(DataBuffer buffer) {
				UShort = buffer.ReadUInt16();
				UInt = buffer.ReadUInt32();
				ULong = buffer.ReadUInt64();
			}

			public void WriteTo(DataBuffer buffer) {
				buffer.Write((UInt16)UShort);
				buffer.Write((UInt32)UInt);
				buffer.Write((UInt64)ULong);
			}
		}

		public class TestModelDynamic : Serializable {
			public string Data { get; set; }
			public float Float { get; set; }
			public double Double { get; set; }
		}

		public class TestModel {
			public string Data { get; set; }
			public byte ByteValue { get; set; }
			public short ShortValue { get; set; }
			public int IntValue { get; set; }
			public long LongValue { get; set; }
		}

		public class TestLargeModel {
			public string B1 { get; set; }
			public string B2 { get; set; }
			public string B3 { get; set; }
			public string B4 { get; set; }
			public string B5 { get; set; }
			public string B6 { get; set; }
			public string B7 { get; set; }
			public string B8 { get; set; }
			public string B9 { get; set; }
			public string B10 { get; set; }
		}

		public class TestSimpleModel {
			public string Data { get; set; }
		}

		public class TestNestedModel {
			public TestNestedModel Inner { get; set; }
			public string Data { get; set; }
		}


		[TestMethod]
		public void DynamicSerializer_WriteReadTestModel() {
			Random r = new Random();
			TestModel m = new TestModel();

			m.Data = "Hello world";
			m.ByteValue = (byte)r.Next();
			m.ShortValue = (short)r.Next();
			m.IntValue = (int)r.Next();
			m.LongValue = (long)r.Next();

			DataBuffer b = new DataBuffer();
			DynamicSerializer.Instance.Write(b, typeof(TestModel), m);

			b.Seek(0);
			var mresult = DynamicSerializer.Instance.Read<TestModel>(b);

			Assert.IsNotNull(mresult);
			Assert.IsNotNull(mresult.Data);
			Assert.IsFalse(mresult.Data == "");
			Assert.AreEqual(m.Data, mresult.Data);
			Assert.AreEqual(m.ByteValue, mresult.ByteValue);
			Assert.AreEqual(m.ShortValue, mresult.ShortValue);
			Assert.AreEqual(m.IntValue, mresult.IntValue);
			Assert.AreEqual(m.LongValue, mresult.LongValue);
		}

		[TestMethod]
		public void DynamicSerializer_WriteReadTestStruct() {
			Random r = new Random();
			TestStruct m = new TestStruct();

			m.Data = "Hello world";

			DataBuffer b = new DataBuffer();
			DynamicSerializer.Instance.Write(b, typeof(TestStruct), m);

			b.Seek(0);
			var mresult = DynamicSerializer.Instance.Read<TestStruct>(b);

			Assert.IsNotNull(mresult);
			Assert.IsNotNull(mresult.Data);
			Assert.IsFalse(mresult.Data == "");
			Assert.AreEqual(m.Data, mresult.Data);
		}

		[TestMethod]
		public void DynamicSerializer_WriteReadNestedModel() {
			TestNestedModel m = new TestNestedModel();
			m.Inner = new TestNestedModel();
			m.Data = "Root";
			m.Inner.Data = "Inner";

			DataBuffer b = new DataBuffer();
			DynamicSerializer.Instance.Write<TestNestedModel>(b, m);

			b.Seek(0);
			var mresult = DynamicSerializer.Instance.Read<TestNestedModel>(b);

			Assert.IsNotNull(mresult);
			Assert.IsNotNull(mresult.Inner);
			Assert.IsNotNull(mresult.Data);
			Assert.IsNotNull(mresult.Inner.Data);
			Assert.AreEqual(m.Data, mresult.Data);
			Assert.AreEqual(m.Inner.Data, mresult.Inner.Data);
		}

		[TestMethod]
		public void DynamicSerializer_WriteReadListOfPrimitives() {
			List<byte> data = new List<byte>() {
				1, 2, 4, 8, 16, 32
			};

			DataBuffer b = new DataBuffer();
			DynamicSerializer.Instance.Write<List<byte>>(b, data);

			b.Seek(0);
			var result = DynamicSerializer.Instance.Read<List<byte>>(b);

			Assert.IsNotNull(result);
			Assert.IsTrue(result.Count == data.Count);
			for (int i = 0; i < data.Count; i++)
				Assert.AreEqual(data[i], result[i]);
		}

		[TestMethod]
		public void DynamicSerializer_WriteReadListOfModels() {
			List<TestSimpleModel> data = new List<TestSimpleModel>() {
				new TestSimpleModel() {
					Data = "d1"
				},
				new TestSimpleModel() {
					Data = "d2"
				},
				new TestSimpleModel() {
					Data = "d3"
				},
				new TestSimpleModel() {
					Data = "d4"
				},
			};


			DataBuffer b = new DataBuffer();
			DynamicSerializer.Instance.Write<List<TestSimpleModel>>(b, data);

			b.Seek(0);
			var result = DynamicSerializer.Instance.Read<List<TestSimpleModel>>(b);

			Assert.IsNotNull(result);
			Assert.AreEqual(result.Count, data.Count);
			for (int i = 0; i < result.Count; i++)
				Assert.AreEqual(result[i].Data, data[i].Data);
		}

		[TestMethod]
		public void DynamicSerializer_WriteReadArrayOfPrimitives() {
			byte[] data = new byte[] {
				1, 2, 4, 8, 16, 32
			};

			DataBuffer b = new DataBuffer();
			DynamicSerializer.Instance.Write<byte[]>(b, data);

			b.Seek(0);
			var result = DynamicSerializer.Instance.Read<byte[]>(b);

			Assert.IsNotNull(result);
			Assert.IsTrue(result.Length == data.Length);
			for (int i = 0; i < data.Length; i++)
				Assert.AreEqual(data[i], result[i]);
		}

		[TestMethod]
		public void DynamicSerializer_WriteReadArrayOfModels() {
			TestSimpleModel[] data = new TestSimpleModel[] {
				new TestSimpleModel() {
					Data = "d1"
				},
				new TestSimpleModel() {
					Data = "d2"
				},
				new TestSimpleModel() {
					Data = "d3"
				},
				new TestSimpleModel() {
					Data = "d4"
				},
			};

			DataBuffer b = new DataBuffer();
			DynamicSerializer.Instance.Write<TestSimpleModel[]>(b, data);

			b.Seek(0);
			var result = DynamicSerializer.Instance.Read<TestSimpleModel[]>(b);

			Assert.IsNotNull(result);
			Assert.AreEqual(result.Length, data.Length);
			for (int i = 0; i < result.Length; i++)
				Assert.AreEqual(result[i].Data, data[i].Data);
		}

		[TestMethod]
		public void DynamicSerializer_WriteReadLargeModel() {
			TestLargeModel m = new TestLargeModel();
			m.B1 = "1";
			m.B2 = "2";
			m.B3 = "3";
			m.B4 = "4";
			m.B5 = "5";
			m.B6 = "6";
			m.B7 = "7";
			m.B8 = "8";
			m.B9 = "9";
			m.B10 = "10";

			DataBuffer b = new DataBuffer();
			DynamicSerializer.Instance.Write<TestLargeModel>(b, m);

			b.Seek(0);
			var mresult = DynamicSerializer.Instance.Read<TestLargeModel>(b);

			Assert.IsNotNull(mresult);
			Assert.AreEqual(m.B1, mresult.B1);
			Assert.AreEqual(m.B2, mresult.B2);
			Assert.AreEqual(m.B3, mresult.B3);
			Assert.AreEqual(m.B4, mresult.B4);
			Assert.AreEqual(m.B5, mresult.B5);
			Assert.AreEqual(m.B6, mresult.B6);
			Assert.AreEqual(m.B7, mresult.B7);
			Assert.AreEqual(m.B8, mresult.B8);
			Assert.AreEqual(m.B9, mresult.B9);
			Assert.AreEqual(m.B10, mresult.B10);
		}

		[TestMethod]
		public void DynamicSerializer_WriteReadISerializable() {
			Random r = new Random();
			TestModelISerializable m = new TestModelISerializable();
			m.UInt = (UInt32)r.Next();
			m.UShort = (UInt16)r.Next();
			m.ULong = (UInt64)r.Next();

			DataBuffer b = new DataBuffer();
			DynamicSerializer.Instance.Write<TestModelISerializable>(b, m);

			b.Seek(0);
			var result = DynamicSerializer.Instance.Read<TestModelISerializable>(b);

			Assert.IsNotNull(result);
			Assert.AreEqual(m.UInt, result.UInt);
			Assert.AreEqual(m.UShort, result.UShort);
			Assert.AreEqual(m.ULong, result.ULong);
		}

		[TestMethod]
		public void DynamicSerializer_WriteReadSerializable() {
			Random r = new Random();
			TestModelDynamic m = new TestModelDynamic();

			m.Data = "Data";
			m.Double = r.NextDouble();
			m.Float = (float)r.NextDouble();

			DataBuffer b = new DataBuffer();
			DynamicSerializer.Instance.Write<TestModelDynamic>(b, m);

			b.Seek(0);
			var result = DynamicSerializer.Instance.Read<TestModelDynamic>(b);

			Assert.IsNotNull(result);
			Assert.AreEqual(m.Data, result.Data);
			Assert.AreEqual(m.Double, result.Double);
			Assert.AreEqual(m.Float, result.Float);
		}

		[TestMethod]
		public void DynamicSerializerTests_TestModelISerializable_WriteRead() {
			Random r = new Random();
			TestModelISerializable m = new TestModelISerializable();
			m.UInt = (UInt32)r.Next();
			m.UShort = (UInt16)r.Next();
			m.ULong = (UInt64)r.Next();

			DataBuffer b = new DataBuffer();
			m.WriteTo(b);

			b.Seek(0);
			TestModelISerializable result = new TestModelISerializable();
			result.ReadFrom(b);

			Assert.IsNotNull(result);
			Assert.AreEqual(m.UInt, result.UInt);
			Assert.AreEqual(m.UShort, result.UShort);
			Assert.AreEqual(m.ULong, result.ULong);
		}

		[TestMethod]
		public void DynamicSerializer_WriteReadEnum() {
			TestEnum e1 = TestEnum.One;
			TestEnum e2 = TestEnum.Two;
			TestEnum e3 = TestEnum.Three;

			DataBuffer b = new DataBuffer();
			DynamicSerializer.Instance.Write<TestEnum>(b, e1);
			DynamicSerializer.Instance.Write<TestEnum>(b, e2);
			DynamicSerializer.Instance.Write<TestEnum>(b, e3);

			b.Seek(0);
			var r1 = DynamicSerializer.Instance.Read<TestEnum>(b);
			var r2 = DynamicSerializer.Instance.Read<TestEnum>(b);
			var r3 = DynamicSerializer.Instance.Read<TestEnum>(b);

			Assert.AreEqual(e1, r1);
			Assert.AreEqual(e2, r2);
			Assert.AreEqual(e3, r3);
		}

		[TestMethod]
		public void DynamicSerializer_AddReaderWriter_CanReadWrite() {
			DynamicSerializer s = new DynamicSerializer();
			Assert.IsFalse(s.CanReadWrite(typeof(TestModelWithConstructor)));

			s.AddReaderWriter<TestModelWithConstructor>(
				new DataWriter(s, typeof(TestModelWithConstructor)),
				new DataReader(s, typeof(TestModelWithConstructor), () => new TestModelWithConstructor("")));

			Assert.IsTrue(s.CanReadWrite(typeof(TestModelWithConstructor)));

			TestModelWithConstructor m = new TestModelWithConstructor("Hello world");

			DataBuffer b = new DataBuffer();
			s.Write<TestModelWithConstructor>(b, m);

			b.Seek(0);
			var result = s.Read<TestModelWithConstructor>(b);

			Assert.IsNotNull(result);
			Assert.IsNotNull(result.Data);
			Assert.AreEqual(result.Data, m.Data);
		}

		[TestMethod]
		public void DynamicSerializer_WriteReadWithIgnore_PropertyIgnored() {
			TestModelWithIgnore m = new TestModelWithIgnore();
			m.Data = "Data";
			m.Ignored = "Ignored";

			DataBuffer b = new DataBuffer();
			DynamicSerializer.Instance.Write<TestModelWithIgnore>(b, m);

			b.Seek(0);
			var result = DynamicSerializer.Instance.Read<TestModelWithIgnore>(b);

			Assert.IsNotNull(result);
			Assert.IsNull(result.Ignored);
			Assert.AreEqual(result.Data, m.Data);
			Assert.AreNotEqual(result.Ignored, m.Ignored);
		}

		[TestMethod]
		public void SerializedPacket_WriteRead_IgnoresProperties() {
			TestSerializedPacket p = new TestSerializedPacket();
			DataBuffer b = new DataBuffer();

			p.Data = "Data";
			p.WriteTo(b);

			b.Seek(0);
			TestSerializedPacket result = new TestSerializedPacket();
			result.ReadFrom(b);

			Assert.AreEqual(p.Data, result.Data);
			Assert.IsTrue(DynamicSerializer.Instance.GetSerializableProperties(typeof(TestSerializedPacket)).Length == 1);
		}

		[TestMethod]
		public void DynamicSerializer_WriteReadCoveredType() {
			DynamicSerializer s = new DynamicSerializer();

			s.AddReaderWriter(typeof(ITestInterface),
				new DataWriter(typeof(ITestInterface), WriteTestInterface),
				new DataReader(typeof(ITestInterface), ReadTestInterface), true);

			Random r = new Random();
			ITestInterface m = new TestImplemented();

			m.Data = "Hello world";

			DataBuffer b = new DataBuffer();
			s.Write(b, typeof(TestImplemented), m);

			b.Seek(0);
			var mresult = s.Read<ITestInterface>(b);

			Assert.IsNotNull(mresult);
			Assert.IsNotNull(mresult.Data);
			Assert.IsFalse(mresult.Data == "");
			Assert.AreEqual(m.Data, mresult.Data);
		}

		private object ReadTestInterface(DataBuffer arg1, object arg2) {
			var inst = new TestImplemented();
			inst.Data = arg1.ReadString();
			return inst;
		}

		private void WriteTestInterface(DataBuffer arg1, object arg2) {
			arg1.Write((string)(arg2 as ITestInterface).Data);
		}
	}
}
