using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Chip.Net.Data
{
    public class PacketRegistry
    {
		private List<Type> tempTypeList;
		private Dictionary<Type, int> typeToId;
		private Dictionary<int, Type> idToType;

		public bool IsLocked { get; private set; }

		public PacketRegistry() {
			IsLocked = false;
			tempTypeList = new List<Type>();
		}

		public void Register<T>() where T : Packet {
			if (IsLocked)
				throw new Exception("Packet registry has been locked");

			if(tempTypeList.Contains(typeof(T)) == false)
				tempTypeList.Add(typeof(T));
		}

		public void LockPackets() {
			if (IsLocked)
				throw new Exception("Packet registry can only be locked once");

			typeToId = new Dictionary<Type, int>();
			idToType = new Dictionary<int, Type>();

			tempTypeList = tempTypeList.OrderBy(i => i.Name).ToList();
			for(int i = 0; i < tempTypeList.Count; i++) {
				typeToId.Add(tempTypeList[i], i);
				idToType.Add(i, tempTypeList[i]);
			}

			tempTypeList = null;
			IsLocked = true;
		}

		public int GetID<T>() where T : Packet {
			return typeToId[typeof(T)];
		}

		public int GetID(Type type) {
			return typeToId[type];
		}

		public Type GetType(int id) {
			return idToType[id];
		}

		public Packet CreateFromId(int id) {
			return (Packet)Activator.CreateInstance(GetType(id));
		}

		public PacketRegistry Clone()
		{
			PacketRegistry r = new PacketRegistry();
			if(IsLocked)
			{
				foreach (var kvp in typeToId)
					r.tempTypeList.Add(kvp.Key);
			}
			else
			{
				foreach (var t in tempTypeList)
					r.tempTypeList.Add(t);
			}

			if (IsLocked)
				r.LockPackets();

			return r;
		}
    }
}
