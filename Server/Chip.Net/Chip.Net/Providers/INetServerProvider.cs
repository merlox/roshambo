using Chip.Net.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chip.Net.Providers {
	public interface INetServerProvider : IDisposable {
		EventHandler<ProviderUserEventArgs> UserConnected { get; set; }
		EventHandler<ProviderUserEventArgs> UserDisconnected { get; set; }

		EventHandler<ProviderDataEventArgs> DataSent { get; set; }
		EventHandler<ProviderDataEventArgs> DataReceived { get; set; }

		bool IsActive { get; }
		bool AcceptIncomingConnections { get; set; }

		void StartServer(INetContext context);
		void StopServer();
		void UpdateServer();

		IEnumerable<object> GetClientKeys();
		void DisconnectUser(object userKey);
		void SendMessage(object recipientKey, DataBuffer data);
	}
}
