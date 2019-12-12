using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Chip.Net.Data
{
	public class DataBuffer : ISerializable {
		private MemoryStream ms;
		private BinaryWriter writer;
		private BinaryReader reader;

		private long _length;
		private long length {
			get { return _length; }
			set { if (_length < value) _length = value; }
		}

		public DataBuffer() {
			ms = new MemoryStream();

			writer = new BinaryWriter(ms);
			reader = new BinaryReader(ms);
			length = 0;
		}

		public DataBuffer(byte[] message) {
			ms = new MemoryStream();
			for (int i = 0; i < message.Length; i++)
				ms.WriteByte(message[i]);

			ms.Seek(0, SeekOrigin.Begin);

			writer = new BinaryWriter(ms);
			reader = new BinaryReader(ms);
			length = message.Length;
		}

		/// <summary>
		/// Sets the offset to the given position.
		/// </summary>
		/// <param name="pos"></param>
		public void Seek(int pos) {
			ms.Seek(pos, SeekOrigin.Begin);
		}

		#region Writing
		/// <summary>
		/// Writes a string to the buffer.
		/// </summary>
		/// <param name="source"></param>
		public void Write(string source) {
			writer.Write(source);
			length = ms.Position;
		}

		/// <summary>
		/// Writes an array of bytes to the buffer.
		/// </summary>
		/// <param name="bytes"></param>
		/// <param name="offset"></param>
		/// <param name="count"></param>
		public void Write(byte[] bytes, int offset, int count) {
			ms.Write(bytes, offset, count);
			length = ms.Position;
		}

		/// <summary>
		/// Writes the contents of another databuffer to the buffer.
		/// </summary>
		/// <param name="v"></param>
		public void Write(DataBuffer v) {
			var size = v.GetPosition();
			v.Seek(0);

			Write((int)size);
			for (int i = 0; i < size; i++)
				Write(v.ReadByte());

			length = ms.Position;
		}

		/// <summary>
		/// Writes a 16-bit integer (short) to the buffer.
		/// </summary>
		/// <param name="source"></param>
		public void Write(Int16 source) {
			writer.Write(source);

			length = ms.Position;
		}

		/// <summary>
		/// Writes an unsigned 16-bit integer (ushort) to the buffer.
		/// </summary>
		/// <param name="source"></param>
		public void Write(UInt16 source) {
			writer.Write(source);

			length = ms.Position;
		}

		/// <summary>
		/// Writes a 32-bit integer to the buffer.
		/// </summary>
		/// <param name="source"></param>
		public void Write(Int32 source) {
			writer.Write(source);

			length = ms.Position;
		}

		/// <summary>
		/// Writes an unsigned 32-bit integer to the buffer.
		/// </summary>
		/// <param name="source"></param>
		public void Write(UInt32 source) {
			writer.Write(source);

			length = ms.Position;
		}

		/// <summary>
		/// Writes a 64-bit integer to the buffer.
		/// </summary>
		/// <param name="source"></param>
		public void Write(Int64 source) {
			writer.Write(source);

			length = ms.Position;
		}

		/// <summary>
		/// Writes an unsigned 64-bit integer to the buffer.
		/// </summary>
		/// <param name="source"></param>
		public void Write(UInt64 source) {
			writer.Write(source);

			length = ms.Position;
		}

		/// <summary>
		/// Writes a float to the buffer.
		/// </summary>
		/// <param name="source"></param>
		public void Write(float source) {
			writer.Write(source);

			length = ms.Position;
		}

		/// <summary>
		/// Writes a double to the buffer.
		/// </summary>
		/// <param name="source"></param>
		public void Write(double source) {
			writer.Write(source);

			length = ms.Position;
		}

		/// <summary>
		/// Writes a byte to the buffer.
		/// </summary>
		/// <param name="source"></param>
		public void Write(byte source) {
			ms.WriteByte(source);

			length = ms.Position;
		}

		/// <summary>
		/// Writes an array of bytes
		/// </summary>
		/// <param name="source"></param>
		public void Write(byte[] source) {
			Write((ushort)source.Length);
			for (int i = 0; i < source.Length; i++)
				Write((byte)source[i]);
		}

        /// <summary>
        /// Writes a guid to the buffer.
        /// </summary>
        /// <param name="source"></param>
        public void Write(Guid source) {
            for (int i = 0; i < 16; i++)
                this.Write((byte)source.ToByteArray()[i]);
        }
		#endregion

		#region Reading
		/// <summary>
		/// Reads an array of bytes from the buffer.
		/// </summary>
		/// <param name="bytesOut"></param>
		/// <param name="offset"></param>
		/// <param name="count"></param>
		/// <returns></returns>
		public int ReadBytes(byte[] bytesOut, int offset, int count) {
			var b = reader.ReadBytes(count);
			b.CopyTo(bytesOut, offset);
			return b.Length;
		}

		/// <summary>
		/// Reads the contents of another databuffer from the buffer.
		/// </summary>
		/// <returns></returns>
		public DataBuffer ReadBuffer() {
			int len = ReadInt32();
			byte[] ret = new byte[len];

			for (int i = 0; i < len; i++)
				ret[i] = ReadByte();

			return new DataBuffer(ret);
		}

		/// <summary>
		/// Reads a byte from the buffer.
		/// </summary>
		/// <returns></returns>
		public byte ReadByte() {
			return reader.ReadByte();
		}

		/// <summary>
		/// Reads an array of bytes
		/// </summary>
		/// <returns></returns>
		public byte[] ReadByteArray() {
			var length = ReadUInt16();
			byte[] ret = new byte[length];
			for (int i = 0; i < length; i++)
				ret[i] = ReadByte();

			return ret;
		}

		/// <summary>
		/// Reads a short from the buffer.
		/// </summary>
		/// <returns></returns>
		public Int16 ReadInt16() {
			return reader.ReadInt16();
		}

		/// <summary>
		/// Reads an int from the buffer.
		/// </summary>
		/// <returns></returns>
		public Int32 ReadInt32() {
			return reader.ReadInt32();
		}

		/// <summary>
		/// Reads a long from the buffer.
		/// </summary>
		/// <returns></returns>
		public Int64 ReadInt64() {
			return reader.ReadInt64();
		}

		/// <summary>
		/// Reads a ushort from the buffer.
		/// </summary>
		/// <returns></returns>
		public UInt16 ReadUInt16() {
			return reader.ReadUInt16();
		}

		/// <summary>
		/// Reads a uint from the buffer.
		/// </summary>
		/// <returns></returns>
		public UInt32 ReadUInt32() {
			return reader.ReadUInt32();
		}

		/// <summary>
		/// Reads a ulong from the buffer.
		/// </summary>
		/// <returns></returns>
		public UInt64 ReadUInt64() {
			return reader.ReadUInt64();
		}

		/// <summary>
		/// Reads a float from the buffer.
		/// </summary>
		/// <returns></returns>
		public float ReadFloat() {
			return reader.ReadSingle();
		}

		/// <summary>
		/// Reads a double from the buffer.
		/// </summary>
		/// <returns></returns>
		public double ReadDouble() {
			return reader.ReadDouble();
		}

		/// <summary>
		/// Reads a string from the buffer.
		/// </summary>
		/// <returns></returns>
		public string ReadString() {
			return reader.ReadString();
		}

        /// <summary>
        /// Reads a guid from the buffer.
        /// </summary>
        /// <returns></returns>
        public Guid ReadGuid() {
            byte[] bytes = new byte[16];
            for (int i = 0; i < bytes.Length; i++)
                bytes[i] = this.ReadByte();

            return new Guid(bytes);
        }
		#endregion

		#region Peeking
		/// <summary>
		/// Peeks the contents of another databuffer from the buffer.
		/// </summary>
		/// <returns></returns>
		public DataBuffer PeekBuffer() {
			var pos = this.GetPosition();
			var buff = this.ReadBuffer();
			Seek(pos);
			return buff;
		}

		/// <summary>
		/// Peeks the next byte of the buffer.
		/// </summary>
		/// <returns></returns>
		public byte PeekByte() {
			var pos = ms.Position;
			var ret = reader.ReadByte();
			ms.Position = pos;

			return ret;
		}

		/// <summary>
		/// Peeks the next sbyte of the buffer.
		/// </summary>
		/// <returns></returns>
		public sbyte PeekSByte() {
			var pos = ms.Position;
			var ret = reader.ReadSByte();
			ms.Position = pos;

			return ret;
		}

		/// <summary>
		/// Peeks the next short of the buffer.
		/// </summary>
		/// <returns></returns>
		public short PeekInt16() {
			var pos = ms.Position;
			var ret = reader.ReadInt16();
			ms.Position = pos;

			return ret;
		}

		/// <summary>
		/// Peeks the next int of the buffer.
		/// </summary>
		/// <returns></returns>
		public int PeekInt32() {
			var pos = ms.Position;
			var ret = reader.ReadInt32();
			ms.Position = pos;

			return ret;
		}

		/// <summary>
		/// Peeks the next long of the buffer.
		/// </summary>
		/// <returns></returns>
		public long PeekInt64() {
			var pos = ms.Position;
			var ret = reader.ReadInt64();
			ms.Position = pos;

			return ret;
		}

		/// <summary>
		/// Peeks the next short of the buffer.
		/// </summary>
		/// <returns></returns>
		public ushort PeekUInt16() {
			var pos = ms.Position;
			var ret = reader.ReadUInt16();
			ms.Position = pos;

			return ret;
		}

		/// <summary>
		/// Peeks the next int of the buffer.
		/// </summary>
		/// <returns></returns>
		public uint PeekUInt32() {
			var pos = ms.Position;
			var ret = reader.ReadUInt32();
			ms.Position = pos;

			return ret;
		}

		/// <summary>
		/// Peeks the next long of the buffer.
		/// </summary>
		/// <returns></returns>
		public ulong PeekUInt64() {
			var pos = ms.Position;
			var ret = reader.ReadUInt64();
			ms.Position = pos;

			return ret;
		}

		/// <summary>
		/// Peeks the next float of the buffer.
		/// </summary>
		/// <returns></returns>
		public float PeekFloat() {
			var pos = ms.Position;
			var ret = reader.ReadSingle();
			ms.Position = pos;

			return ret;
		}

		/// <summary>
		/// Peeks the next double of the buffer.
		/// </summary>
		/// <returns></returns>
		public double PeekDouble() {
			var pos = ms.Position;
			var ret = reader.ReadDouble();
			ms.Position = pos;

			return ret;
		}

		/// <summary>
		/// Peeks the next string of the buffer.
		/// </summary>
		/// <returns></returns>
		public string PeekString() {
			var pos = ms.Position;
			var ret = reader.ReadString();
			ms.Position = pos;

			return ret;
		}
		#endregion

		/// <summary>
		/// Returns the bytes array.
		/// </summary>
		/// <returns></returns>
		public byte[] ToBytes() {
			return ms.ToArray();
		}

		/// <summary>
		/// Converts the contents of this buffer to a string.
		/// </summary>
		/// <returns></returns>
		public string BytesToString() {
			string ret = "";
			var bytes = ToBytes();
			for (int i = 0; i < bytes.Length; i++)
				ret += (char)bytes[i];

			return ret;
		}

		/// <summary>
		/// Reads the contents of a string into the buffer.
		/// </summary>
		/// <param name="str"></param>
		public void StringToBytes(string str) {
			for (int i = 0; i < str.Length; i++) {
				ms.WriteByte((byte)str[i]);
			}
		}

		/// <summary>
		/// Returns the length of the buffer.
		/// </summary>
		/// <returns></returns>
		public int GetLength() {
			return (int)length;
		}

		/// <summary>
		/// Returns the current position of the buffer.
		/// </summary>
		/// <returns></returns>
		public int GetPosition() {
			return (int)ms.Position;
		}

		/// <summary>
		/// Writes the buffer's data to a file.
		/// </summary>
		/// <param name="path"></param>
		public void WriteToFile(string path) {
			var file = File.OpenWrite(path);
			ms.WriteTo(file);
			file.Close();
		}

		/// <summary>
		/// Reads the contents of a file to the buffer.
		/// </summary>
		/// <param name="path"></param>
		public void ReadFromFile(string path) {
			var file = File.OpenRead(path);
			file.CopyTo(ms);
			file.Close();
		}

		public void Clear() {
			ms = new MemoryStream();
			LinkedList<int> list = new LinkedList<int>();

			writer = new BinaryWriter(ms);
			reader = new BinaryReader(ms);
			length = 0;
		}

		public void WriteTo(DataBuffer buffer) {
			var bytes = ToBytes();
			buffer.Write((byte[])bytes);
		}

		public void ReadFrom(DataBuffer buffer) {
			var message = buffer.ReadByteArray();
			ms = new MemoryStream();
			for (int i = 0; i < message.Length; i++)
				ms.WriteByte(message[i]);

			ms.Seek(0, SeekOrigin.Begin);

			writer = new BinaryWriter(ms);
			reader = new BinaryReader(ms);
			length = message.Length;
		}
	}
}
