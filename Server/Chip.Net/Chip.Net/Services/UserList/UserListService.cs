using Chip.Net.Data;
using Chip.Net.Services.RFC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Chip.Net.Services.UserList {
	public delegate void UserListEvent(NetUser User);

	public class UserListService : RFCService {
		public NetUser LocalUser { get; set; }
		public IReadOnlyList<NetUser> UserList { get; private set; }

		public UserListEvent OnLocalUserSet { get; set; }

		private List<NetUser> userList;
		private Action<NetUser> ClSetUser;
		private Action<NetUser> ClAddUser;
		private Action<NetUser> ClRemoveUser;
		private Action<List<NetUser>> ClSetUserList;

		public override void InitializeService(INetContext context) {
			base.InitializeService(context);
			userList = new List<NetUser>();
			UserList = userList.AsReadOnly();

			if(IsServer) {
				Server.NetUserConnected += onUserConnected;
				Server.NetUserDisconnected += onUserDisconnected;
			}

			ClSetUser = ClientAction<NetUser>(_clSetUser);
			ClAddUser = ClientAction<NetUser>(_clAddUser);
			ClRemoveUser = ClientAction<NetUser>(_clRemoveUser);
			ClSetUserList = ClientAction<List<NetUser>>(_clSetUserList);
		}

		public NetUser Get(int userId) {
			return userList.FirstOrDefault(i => i.UserId == userId);
		}

		private void _clSetUser(NetUser obj) {
			this.LocalUser = obj;
			OnLocalUserSet?.Invoke(obj);
		}

		private void _clAddUser(NetUser obj) {
			this.userList.Add(obj);
		}

		private void _clRemoveUser(NetUser obj) {
			this.userList = userList
				.Where(i => i.UserId != obj.UserId)
				.ToList();
		}

		private void _clSetUserList(List<NetUser> obj) {
			this.userList = new List<NetUser>(obj);
		}

		private void onUserDisconnected(object sender, NetEventArgs args) {
			userList.Remove(args.User);
		}

		private void onUserConnected(object sender, NetEventArgs args) {
			userList.Add(args.User);

			Broadcast(() => {
				ClSetUserList(userList);
			});

			ClSetUser(args.User);
		}
	}
}
