using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Chip.Net.Data
{
    public class DynamicSerializer {
		public static DynamicSerializer Instance { get; private set; } = new DynamicSerializer();

		public Dictionary<Type, DataWriter> Writers { get; private set; } = new Dictionary<Type, DataWriter>() {
			{ typeof(Byte), new DataWriter(typeof(Byte), (b, v) => b.Write((Byte)v)) },
			{ typeof(Int16), new DataWriter(typeof(Int16), (b, v) => b.Write((Int16)v)) },
			{ typeof(Int32), new DataWriter(typeof(Int32), (b, v) => b.Write((Int32)v)) },
			{ typeof(Int64), new DataWriter(typeof(Int64), (b, v) => b.Write((Int64)v)) },
			{ typeof(UInt16), new DataWriter(typeof(UInt16), (b, v) => b.Write((UInt16)v)) },
			{ typeof(UInt32), new DataWriter(typeof(UInt32), (b, v) => b.Write((UInt32)v)) },
			{ typeof(UInt64), new DataWriter(typeof(UInt64), (b, v) => b.Write((UInt64)v)) },
			{ typeof(float), new DataWriter(typeof(float), (b, v) => b.Write((float)v)) },
			{ typeof(double), new DataWriter(typeof(double), (b, v) => b.Write((double)v)) },
			{ typeof(string), new DataWriter(typeof(string), (b, v) => b.Write((string)v)) },
			{ typeof(byte[]), new DataWriter(typeof(byte[]), (b, v) => b.Write((byte[])v)) },
			{ typeof(DataBuffer), new DataWriter(typeof(DataBuffer), (b, v) => b.Write((DataBuffer)v)) },
		};

		public Dictionary<Type, DataReader> Readers { get; private set; } = new Dictionary<Type, DataReader>() {
			{ typeof(Byte), new DataReader(typeof(Byte), (b, v) => b.ReadByte()) },
			{ typeof(Int16), new DataReader(typeof(Int16), (b, v) => b.ReadInt16()) },
			{ typeof(Int32), new DataReader(typeof(Int32), (b, v) => b.ReadInt32()) },
			{ typeof(Int64), new DataReader(typeof(Int64), (b, v) => b.ReadInt64()) },
			{ typeof(UInt16), new DataReader(typeof(UInt16), (b, v) => b.ReadUInt16()) },
			{ typeof(UInt32), new DataReader(typeof(UInt32), (b, v) => b.ReadUInt32()) },
			{ typeof(UInt64), new DataReader(typeof(UInt64), (b, v) => b.ReadUInt64()) },
			{ typeof(float), new DataReader(typeof(float), (b, v) => b.ReadFloat()) },
			{ typeof(double), new DataReader(typeof(double), (b, v) => b.ReadDouble()) },
			{ typeof(string), new DataReader(typeof(string), (b, v) => b.ReadString()) },
			{ typeof(byte[]), new DataReader(typeof(byte[]), (b, v) => b.ReadByteArray()) },
			{ typeof(DataBuffer), new DataReader(typeof(DataBuffer), (b, v) => b.ReadBuffer()) },
		};

		private object LockObject = new object();
		private List<Type> CoveredTypes = new List<Type>();
		private Dictionary<Type, Type> CoveredTypeMap = new Dictionary<Type, Type>();

		public void Write(DataBuffer buffer, Type type, object instance) {
			if (CanReadWrite(type) == false)
				throw new Exception("Invalid model: Can not read/write");

			DataWriter writer = null;
			lock (LockObject) {
				if (Writers.ContainsKey(type) == false) {
					if (CoveredTypeMap.ContainsKey(type)) {
						writer = Writers[CoveredTypeMap[type]];
					} else {
						Writers.Add(type, new DataWriter(this, type));
					}
				}
			}

			if (writer == null)
				writer = Writers[type];

			writer.Write(buffer, instance);
		}

		public void Write<T>(DataBuffer buffer, T instance) {
			Write(buffer, typeof(T), instance);
		}

		public object Read(DataBuffer buffer, Type type, object existing = null) {
			if (CanReadWrite(type) == false)
				throw new Exception("Invalid model: Can not read/write");

			DataReader reader = null;
			lock (LockObject) {
				if (Readers.ContainsKey(type) == false) {
					if (CoveredTypeMap.ContainsKey(type)) {
						reader = Readers[CoveredTypeMap[type]];
					} else {
						Readers.Add(type, new DataReader(this, type));
					}
				}
			}

			if(reader == null)
				reader = Readers[type];

			var model = reader.Read(buffer, existing);
			return model;
		}

		public T Read<T>(DataBuffer buffer) {
			return (T)Read(buffer, typeof(T));
		}

		public void AddReaderWriter(Type type, DataWriter writer, DataReader reader, bool covered = false) {
			lock (LockObject) {
				Writers.Add(type, writer);
				Readers.Add(type, reader);
			}

			if (covered) {
				var collisions = CoveredTypes.Where(i => i.IsAssignableFrom(type) || type.IsAssignableFrom(i));
				if (collisions.Any())
					throw new Exception(string.Format("Types {0} and {1} cannot implement one another", collisions.First().Name, type.Name));

				CoveredTypes.Add(type);
			}
		}

		public void AddReaderWriter<T>(DataWriter writer, DataReader reader, bool covered = false) {
			AddReaderWriter(typeof(T), writer, reader, covered);
		}

		public bool CanReadWrite(Type type) {
			if (Writers.ContainsKey(type) && Readers.ContainsKey(type))
				return true;

			if (CoveredTypeMap.ContainsKey(type))
				return true;

			var t = CoveredTypes.Where(i => i.IsAssignableFrom(type));
			if (t.Any()) {
				CoveredTypeMap[type] = t.First();
				return true;
			}

			bool hasConstructor = type.GetConstructors().Any(i => i.GetParameters().Count() == 0);

			return (hasConstructor || type.IsValueType) ||
				typeof(IEnumerable).IsAssignableFrom(type) ||
				typeof(ICollection).IsAssignableFrom(type) ||
				typeof(ISerializable).IsAssignableFrom(type) ||
				type.IsEnum;
		}

		public PropertyInfo[] GetSerializableProperties(Type type) {
			return type.GetProperties()
				.Where(i => CanReadWrite(i.PropertyType))
				.Where(i => i.CustomAttributes.Any(o => o.AttributeType == typeof(IgnoreProperty)) == false)
				.ToArray();
		}

		public DynamicSerializer Clone()
		{
			DynamicSerializer d = new DynamicSerializer();
			foreach (var w in Writers)
				if (d.Writers.ContainsKey(w.Key) == false)
					d.Writers.Add(w.Key, w.Value);

			foreach (var r in Readers)
				if (d.Readers.ContainsKey(r.Key) == false)
					d.Readers.Add(r.Key, r.Value);

			return d;
		}
	}

	public class DataWriter {
		private Action<DataBuffer, object> WriteAction;
		private PropertyInfo[] Properties;
		private DynamicSerializer Parent;

		private Type ListType;

		public DataWriter(DynamicSerializer parent, Type type) {
			this.Parent = parent;
			if (type.IsEnum) {
				WriteAction = WriteEnum;
				return;
			}

			if (typeof(IEnumerable).IsAssignableFrom(type)) {
				WriteAction = WriteEnumerable;

				if (type.GenericTypeArguments.Any())
					ListType = type.GenericTypeArguments.First();
				else
					ListType = type.GetElementType();

				return;
			}

			Properties = Parent.GetSerializableProperties(type);
			WriteAction = WritePropertyValues;
		}

		private void WriteEnum(DataBuffer buffer, object data) {
			var underlying = data.GetType().GetEnumUnderlyingType();
			byte underlyingValue = (byte)((IConvertible)data).ToType(typeof(byte), null);
			buffer.Write((byte)underlyingValue);
		}

		private void WriteEnumerable(DataBuffer buffer, object inst) {
			IEnumerable<object> list = (inst as IEnumerable).OfType<object>();

			short count = (short)list.Count();
			buffer.Write((short)count);

			foreach(var obj in list) {
				if (obj.GetType() != ListType)
					throw new Exception("Type mismatch");

				Parent.Write(buffer, ListType, obj);
			}
		}

		private void WritePropertyValues(DataBuffer buff, object inst) {
			object[] values = new object[Properties.Length];
			Type[] types = new Type[values.Length];
			bool[] isNull = new bool[values.Length];
			for (int i = 0; i < values.Length; i++) {
				values[i] = Properties[i].GetValue(inst);
				types[i] = Properties[i].PropertyType;
				isNull[i] = values[i] == null;
			}

			var fold = DataHelpers.Fold(isNull);
			for (int i = 0; i < fold.Length; i++)
				buff.Write((byte)fold[i]);

			for (int i = 0; i < values.Length; i++)
				if (values[i] != null)
					Parent.Write(buff, types[i], values[i]);
		}

		public DataWriter(Type type, Action<DataBuffer, object> writer) {
			this.WriteAction = writer;
		}

		public void Write(DataBuffer buffer, object instance) {
			WriteAction.Invoke(buffer, instance);
		}
	}

	public class DataReader {
		private Func<DataBuffer, object, object> ReadFunction;
		private Func<object> ActivationFunction;
		private PropertyInfo[] Properties;
		private DynamicSerializer Parent;
		private Type ModelType;
		private Type ListType;

		public DataReader(DynamicSerializer parent, Type type, Func<object> activator = null) {
			this.Parent = parent;
			ModelType = type;
			if (type.IsEnum) {
				ReadFunction = ReadEnum;
				return;
			}

			if (typeof(IEnumerable).IsAssignableFrom(type)) {

				if (type.GenericTypeArguments.Any()) {
					ReadFunction = ReadList;
					ListType = type.GenericTypeArguments.First();
				} else {
					ReadFunction = ReadArray;
					ListType = type.GetElementType();
				}

				return;
			}

			Properties = Parent.GetSerializableProperties(type);
			ReadFunction = ReadPropertyValues;
			ActivationFunction = activator;
		}

		public DataReader(Type type, Func<DataBuffer, object, object> reader) {
			this.ReadFunction = reader;
		}

		private object ReadEnum(DataBuffer buffer, object arg2) {
			return Enum.ToObject(ModelType, buffer.ReadByte());
		}

		private object ReadList(DataBuffer buffer, object inst) {
			IList ret = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(ListType));

			var count = buffer.ReadInt16();
			for(int i = 0; i < count; i++) {
				ret.Add(Parent.Read(buffer, ListType));
			}

			return ret;
		}

		private object ReadArray(DataBuffer buffer, object inst) {
			var count = buffer.ReadInt16();
			Array arr = (Array)Activator.CreateInstance(ListType.MakeArrayType(), new object[] { (int)count });

			for(int i = 0; i < count; i++) {
				arr.SetValue(Parent.Read(buffer, ListType), i);
			}

			return arr;
		}

		private object ReadPropertyValues(DataBuffer buff, object inst) {
			if (inst == null) {
				if (ActivationFunction == null)
					inst = Activator.CreateInstance(ModelType);
				else
					inst = ActivationFunction.Invoke();
			}

			Type[] types = new Type[Properties.Length];
			for (int i = 0; i < Properties.Length; i++) {
				types[i] = Properties[i].PropertyType;
			}

			var foldBytes = (Properties.Length + 7) / 8;
			byte[] fold = new byte[foldBytes];
			for (int i = 0; i < fold.Length; i++)
				fold[i] = buff.ReadByte();

			var isNull = DataHelpers.Unfold(fold);
			for (int i = 0; i < Properties.Length; i++)
				if (isNull[i] == false)
					Properties[i].SetValue(inst, Parent.Read(buff, types[i], Properties[i].GetValue(inst)));

			return inst;
		}

		public object Read(DataBuffer buffer, object existing = null) {
			return ReadFunction(buffer, existing);
		}
	}

	public class DataHelpers {
		public static byte[] Fold(bool[] flags) {
			int byteCount = (flags.Length + 7) / 8;
			byte[] ret = new byte[byteCount];

			byte mask = 1;
			byte index = 0;
			for (int i = 0; i < flags.Length; i++) {
				if (flags[i])
					ret[index] = (byte)(ret[index] | mask);

				mask = (byte)(mask * 2);
				if (mask == 0) {
					mask = 1;
					index++;
				}
			}

			return ret;
		}

		public static bool[] Unfold(byte[] bytes) {
			bool[] ret = new bool[bytes.Length * 8];

			byte mask = 1;
			byte index = 0;
			for (int i = 0; i < ret.Length; i++) {
				if ((bytes[index] & mask) == mask) {
					ret[i] = true;
				}

				mask = (byte)(mask * 2);
				if (mask == 0) {
					mask = 1;
					index++;
				}
			}

			return ret;
		}
	}

	public class IgnoreProperty : Attribute { }
}
