using System;
using System.Collections.Generic;
using System.Text;
using Chip.Net.Data;

namespace Chip.Net.Providers.UDP
{
	public class UDPServerProvider : INetServerProvider {
		public EventHandler<ProviderUserEventArgs> UserConnected { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		public EventHandler<ProviderUserEventArgs> UserDisconnected { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

		public EventHandler<ProviderDataEventArgs> DataSent { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		public EventHandler<ProviderDataEventArgs> DataReceived { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

		public bool AcceptIncomingConnections { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		public bool IsActive => throw new NotImplementedException();

		public void DisconnectUser(object userKey) {
			throw new NotImplementedException();
		}

		public void Dispose() {
			throw new NotImplementedException();
		}

		public IEnumerable<object> GetClientKeys() {
			throw new NotImplementedException();
		}

		public void SendMessage(DataBuffer data, object excludeKey = null) {
			throw new NotImplementedException();
		}

		public void SendMessage(object recipientKey, DataBuffer data) {
			throw new NotImplementedException();
		}

		public void StartServer(INetContext context) {
			throw new NotImplementedException();
		}

		public void StopServer() {
			throw new NotImplementedException();
		}

		public void UpdateServer() {
			throw new NotImplementedException();
		}
	}
}
