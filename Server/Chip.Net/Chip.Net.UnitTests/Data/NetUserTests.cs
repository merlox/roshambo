using Chip.Net.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chip.Net.UnitTests.Data
{
	[TestClass]
	public class NetUserTests
	{
		public class TestModel {
			public string Data { get; set; }
		}

		[TestMethod]
		public void NetUser_SetLocal_StringKey_GetLocalReturnsValue() {
			NetUser user = new NetUser();
			user.SetLocal("key", "data");
			var data = user.GetLocal("key");
			Assert.IsTrue(data.Equals("data"));
		}

		[TestMethod]
		public void NetUser_SetLocal_GenericKey_GetLocalReturnsValue() {
			NetUser user = new NetUser();
			TestModel model = new TestModel();
			model.Data = "data";
			user.SetLocal<TestModel>("key", model);
			var m = user.GetLocal<TestModel>("key");
			Assert.IsTrue(m.Data.Equals(model.Data));
		}

		[TestMethod]
		public void NetUser_GetLocal_ReturnsDefault() {
			NetUser user = new NetUser();
			Assert.IsTrue(user.GetLocal<TestModel>("key") == null);
		}

		[TestMethod]
		public void NetUser_WriteTo_ReadFromIsEquivalent() {
			NetUser user = new NetUser(0, 100);
			DataBuffer buffer = new DataBuffer();
			user.WriteTo(buffer);
			buffer.Seek(0);
			NetUser result = new NetUser();
			result.ReadFrom(buffer);
			Assert.AreEqual(result.UserId, user.UserId);
		}
	}
}
