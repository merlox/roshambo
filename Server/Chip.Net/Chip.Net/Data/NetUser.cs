using System;
using System.Collections.Generic;
using System.Text;

namespace Chip.Net.Data
{
	public class NetUser : ISerializable
	{
		[IgnoreProperty]
		public object UserKey { get; private set; }

		public int UserId { get; private set; }

		private Dictionary<string, object> localDataMap;

		public NetUser() {
			this.UserId = -1;
			this.UserKey = null;
			localDataMap = new Dictionary<string, object>();
		}

		public NetUser(object userKey, int userId)
		{
			this.UserId = userId;
			this.UserKey = userKey;
			localDataMap = new Dictionary<string, object>();
		}

		public override int GetHashCode()
		{
			return UserKey.GetHashCode();
		}

		public void SetLocal(string key, object data)
		{
			localDataMap[key] = data;
		}

		public object GetLocal(string key)
		{
			if (localDataMap.ContainsKey(key))
				return localDataMap[key];

			return null;
		}

		public void SetLocal<T>(string key, T data)
		{
			SetLocal(key, (object)data);
		}

		public T GetLocal<T>(string key)
		{
			var ret = GetLocal(key);
			if (ret == null)
				return default(T);

			return (T)ret;
		}

		public void WriteTo(DataBuffer buffer) {
			buffer.Write((short)UserId);
		}

		public void ReadFrom(DataBuffer buffer) {
			UserId = buffer.ReadInt16();
		}

		public override bool Equals(object obj) {
			var other = obj as NetUser;
			if(other != null) {
				return other.UserId == UserId;
			}

			return base.Equals(obj);
		}

		public override string ToString() {
			return "User [" + UserId + "]";
		}
	}
}
