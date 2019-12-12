using Chip.Net.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chip.Net.UnitTests.Data
{
	[TestClass]
    public class DataBufferTests
    {

		[TestMethod]
		public void DataBuffer_WriteReadPrimitives() {
			string str = "hello world";
			Random r = new Random();
			Int16 int16 = (Int16)r.Next(Int16.MaxValue);
			Int32 int32 = (Int32)r.Next(Int16.MaxValue);
			Int64 int64 = (Int64)r.Next(Int16.MaxValue);
			UInt16 uint16 = (UInt16)r.Next(UInt16.MaxValue);
			UInt32 uint32 = (UInt32)r.Next(UInt16.MaxValue);
			UInt64 uint64 = (UInt64)r.Next(UInt16.MaxValue);

			byte b = (byte)r.Next(byte.MaxValue);
			float f = (float)r.NextDouble() * 100f;
			double d = (double)r.NextDouble() * 100d;

			DataBuffer buff = new DataBuffer();
			buff.Write(int16);
			buff.Write(int32);
			buff.Write(int64);
			buff.Write(uint16);
			buff.Write(uint32);
			buff.Write(uint64);
			buff.Write(str);
			buff.Write(b);
			buff.Write(f);
			buff.Write(d);

			buff.Seek(0);
			Assert.AreEqual(int16, buff.ReadInt16());
			Assert.AreEqual(int32, buff.ReadInt32());
			Assert.AreEqual(int64, buff.ReadInt64());
			Assert.AreEqual(uint16, buff.ReadUInt16());
			Assert.AreEqual(uint32, buff.ReadUInt32());
			Assert.AreEqual(uint64, buff.ReadUInt64());
			Assert.AreEqual(str, buff.ReadString());
			Assert.AreEqual(b, buff.ReadByte());
			Assert.AreEqual(f, buff.ReadFloat());
			Assert.AreEqual(d, buff.ReadDouble());
		}

		[TestMethod]
		public void DataBuffer_WritePeekPrimitives() {
			string str = "hello world";
			Random r = new Random();
			Int16 int16 = (Int16)r.Next(Int16.MaxValue);
			Int32 int32 = (Int32)r.Next(Int16.MaxValue);
			Int64 int64 = (Int64)r.Next(Int16.MaxValue);
			UInt16 uint16 = (UInt16)r.Next(UInt16.MaxValue);
			UInt32 uint32 = (UInt32)r.Next(UInt16.MaxValue);
			UInt64 uint64 = (UInt64)r.Next(UInt16.MaxValue);

			byte b = (byte)r.Next(byte.MaxValue);
			float f = (float)r.NextDouble() * 100f;
			double d = (double)r.NextDouble() * 100d;

			DataBuffer buff = new DataBuffer();
			buff.Write(int16);
			buff.Write(int32);
			buff.Write(int64);
			buff.Write(uint16);
			buff.Write(uint32);
			buff.Write(uint64);
			buff.Write(str);
			buff.Write(b);
			buff.Write(f);
			buff.Write(d);

			buff.Seek(0);
			Assert.AreEqual(int16, buff.PeekInt16()); buff.ReadInt16();
			Assert.AreEqual(int32, buff.PeekInt32()); buff.ReadInt32();
			Assert.AreEqual(int64, buff.PeekInt64()); buff.ReadInt64();
			Assert.AreEqual(uint16, buff.PeekUInt16()); buff.ReadUInt16();
			Assert.AreEqual(uint32, buff.PeekUInt32()); buff.ReadUInt32();
			Assert.AreEqual(uint64, buff.PeekUInt64()); buff.ReadUInt64();
			Assert.AreEqual(str, buff.PeekString()); buff.ReadString();
			Assert.AreEqual(b, buff.PeekByte()); buff.ReadByte();
			Assert.AreEqual(f, buff.PeekFloat()); buff.ReadFloat();
			Assert.AreEqual(d, buff.PeekDouble()); buff.ReadDouble();
		}

		[TestMethod]
		public void DataBuffer_WriteReadBuffers() {
			string data = "datastring";
			DataBuffer outer = new DataBuffer();
			DataBuffer inner = new DataBuffer();

			inner.Write((string)data);
			var WritePos = inner.GetPosition();

			outer.Write((DataBuffer)inner);
			outer.Seek(0);
			var result = outer.ReadBuffer();

			result.Seek(0);
			Assert.AreEqual(result.ReadString(), data);
			Assert.AreEqual(WritePos, result.GetPosition());
		}


		[TestMethod]
		public void DataBuffer_WritePeekBuffers() {
			string data = "datastring";
			DataBuffer outer = new DataBuffer();
			DataBuffer inner = new DataBuffer();

			inner.Write((string)data);
			var WritePos = inner.GetPosition();

			outer.Write((DataBuffer)inner);
			outer.Seek(0);
			var result = outer.PeekBuffer();

			result.Seek(0);
			Assert.AreEqual(result.ReadString(), data);
			Assert.AreEqual(WritePos, result.GetPosition());
		}

		[TestMethod]
		public void DataBuffer_WriteReadByteArrays() {
			byte[] data = new byte[100];
			Random r = new Random();
			r.NextBytes(data);

			DataBuffer buff = new DataBuffer();
			buff.Write((byte[])data, 0, data.Length);

			byte[] results = new byte[data.Length];
			buff.Seek(0);
			buff.ReadBytes(results, 0, results.Length);

			for (int i = 0; i < data.Length; i++) {
				Assert.AreEqual(data[i], results[i]);
			}
		}

		[TestMethod]
		public void DataBuffer_StringToBytes_IsEqual() {
			var buff = new DataBuffer();
			string data = "data string value";
			buff.StringToBytes(data);
			buff.Seek(0);

			Assert.AreEqual(data, buff.BytesToString());
		}

		[TestMethod]
		public void DataBuffer_BufferToBytes_IsEqual() {
			var buff = new DataBuffer();
			string data = "data string value";
			buff.Write((string)data);

			var bytes = buff.ToBytes();
			var buff2 = new DataBuffer(bytes);

			Assert.AreEqual(data, buff2.ReadString());
		}
	}
}
