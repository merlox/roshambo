using Chip.Net.Services.UserList;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chip.Net.UnitTests.Services
{
	[TestClass]
	public class UserListServiceTests : NetServiceBaseTests<UserListService>
	{
		[TestMethod]
		public void UserListService_EmptyServer_UserListEmpty() {

		}

		[TestMethod]
		public void UserListService_ClientJoins_ServerUserListContainsNewUser() {

		}

		[TestMethod]
		public void UserListService_ClientJoins_ClientUserListContainsNewUser() {

		}

		[TestMethod]
		public void UserListService_ManyClientsJoin_ServerUserListContainsNewUser() {

		}

		[TestMethod]
		public void UserListService_ManyClientsJoin_ClientUserListContainsNewUser() {

		}
	}
}
