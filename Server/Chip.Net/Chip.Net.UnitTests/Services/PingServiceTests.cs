using Chip.Net.Services.Ping;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chip.Net.UnitTests.Services
{
	[TestClass]
	public class PingServiceTests : NetServiceBaseTests<PingService>
	{
		[TestMethod]
		public void PingService_Server_GetPing_ReturnsPing() {

		}

		[TestMethod]
		public void PingService_Client_GetPing_ReturnsPing() {

		}

		[TestMethod]
		public void PingService_ClientJoinsLate_PingCalculated() {

		}
	}
}
