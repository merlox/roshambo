using System;
using System.Collections.Generic;
using System.Text;
using Chip.Net.Data;

namespace Chip.Net.Providers.UDP
{
	public class UDPClientProvider : INetClientProvider {
		public EventHandler<ProviderUserEventArgs> UserConnected { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		public EventHandler<ProviderUserEventArgs> UserDisconnected { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

		public EventHandler<ProviderDataEventArgs> DataSent { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		public EventHandler<ProviderDataEventArgs> DataReceived { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

		public bool IsConnected => throw new NotImplementedException();

		public void Connect(INetContext context) {
			throw new NotImplementedException();
		}

		public void Disconnect() {
			throw new NotImplementedException();
		}

		public void Dispose() {
			throw new NotImplementedException();
		}

		public void SendMessage(DataBuffer data) {
			throw new NotImplementedException();
		}

		public void UpdateClient() {
			throw new NotImplementedException();
		}
	}
}
