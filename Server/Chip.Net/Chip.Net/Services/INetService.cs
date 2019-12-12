using Chip.Net.Controllers;
using Chip.Net.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chip.Net.Services {
	public interface INetService : IDisposable {
		INetServerController Server { get; set; }
		INetClientController Client { get; set; }

		bool IsServer { get; set; }
		bool IsClient { get; set; }

		void InitializeService(INetContext context);
		void StartService();
		void StopService();
		void UpdateService();
	}
}