using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Chip.Net.Data;
using Chip.Net.Services.NetTime;

namespace Chip.Net.Services.SharedData
{
    public class SharedDataService : NetService
    {
		private static readonly string Key_Service = "shareddatasvc";
		private static readonly string Key_UserMap = "shareddatadata";
		private static readonly string Key_UserDateMap = "shareddatadates";
		private static readonly string Key_UserId = "shareddataid";

		private int nextId = 0;
		private DataBuffer buffer;

		private NetTimeService netTimeService;

		public bool NoDelay { get; set; } = true;

		public override void InitializeService(INetContext context) {
			base.InitializeService(context);
			buffer = new DataBuffer();

			context.Packets.Register<P_SetData>();
			context.Packets.Register<P_AddUser>();
			context.Packets.Register<P_RemoveUser>();

			netTimeService = context.Services.Get<NetTimeService>();
		}

		public override void StartService() {
			base.StartService();
			if (IsServer) {
				Server.NetUserConnected += OnUserConnected;
				Server.NetUserDisconnected += OnUserDisconnected;
			}
		}

		private void OnUserConnected(object sender, NetEventArgs args) {
			var user = args.User;
			user.SetLocal<SharedDataService>(Key_Service, this);
			user.SetLocal<Dictionary<int, byte[]>>(Key_UserMap, new Dictionary<int, byte[]>());
			user.SetLocal<Dictionary<int, double>>(Key_UserDateMap, new Dictionary<int, double>());
			user.SetLocal<int>(Key_UserId, nextId++);
		}

		private void OnUserDisconnected(object sender, NetEventArgs args) {
			var user = args.User;
			user.SetLocal<SharedDataService>(Key_Service, null);
			user.SetLocal<Dictionary<int, byte[]>>(Key_UserMap, null);
			user.SetLocal<Dictionary<int, double>>(Key_UserDateMap, null);
			user.SetLocal<int>(Key_UserId, 0);
		}

		private int GetUserId(NetUser user) {
			return user.GetLocal<int>(Key_UserId);
		}

		private Dictionary<int, byte[]> GetUserMap(NetUser user) {
			return user.GetLocal<Dictionary<int, byte[]>>(Key_UserMap);
		}

		private Dictionary<int, double> GetUserTimeMap(NetUser user) {
			return user.GetLocal<Dictionary<int, double>>(Key_UserDateMap);
		}

		public void SetShared(NetUser user, int key, ISerializable data) {
			buffer.Seek(0);
			P_SetData msg = null;
			if (IsServer) {
				data.WriteTo(buffer);

				msg = new P_SetData();
				msg.UserId = GetUserId(user);
				msg.Key = key;
				msg.Data = buffer.ToBytes();
			}

			if(IsClient) {

			}
		}

		public void SetShared<T>(NetUser user, int key, T data) where T : ISerializable {
			SetShared(user, key, (ISerializable)data);
		}

		public T GetShared<T>(NetUser user, int key) where T : ISerializable {
			return default(T);
		}

		private void Set(NetUser user, int key, double time, ISerializable data) {
			buffer.Seek(0);
			data.WriteTo(buffer);
			var bytes = buffer.ToBytes();

			var map = GetUserMap(user);
			var times = GetUserTimeMap(user);

			if(time > times[key]) {
				times[key] = time;
				map[key] = bytes;
			}
		}

		private byte[] Get(NetUser user, int key) {
			var map = GetUserMap(user);
			var times = GetUserTimeMap(user);

			if (map.ContainsKey(key) == false)
				return null;

			return map[key];
		}
	}

	public static class SharedDataExtensions {
		public static void SetShared(this NetUser user, int key, ISerializable data) {

		}
	}
}
