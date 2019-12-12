using Chip.Net.Data;
using Roshambo.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chip.Net.Data
{
    public static class NetUserExtensions
    {
		public static void SetUserId(this NetUser user, UserId id) {
			user.SetLocal<UserId>("userid", id);
		}

		public static UserId GetUserId(this NetUser user) {
			return user.GetLocal<UserId>("userid");
		}

		public static void SetUserState(this NetUser user, UserState id) {
			user.SetLocal<UserState>("userstate", id);
		}

		public static UserState GetUserState(this NetUser user) {
			return user.GetLocal<UserState>("userstate");
		}
	}
}
