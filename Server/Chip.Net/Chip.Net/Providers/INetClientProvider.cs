using Chip.Net.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chip.Net.Providers {
	public interface INetClientProvider : IDisposable {
		EventHandler<ProviderUserEventArgs> UserConnected { get; set; }
		EventHandler<ProviderUserEventArgs> UserDisconnected { get; set; }

		EventHandler<ProviderDataEventArgs> DataSent { get; set; }
		EventHandler<ProviderDataEventArgs> DataReceived { get; set; }

		bool IsConnected { get; }

		void Connect(INetContext context);
		void Disconnect();
		void UpdateClient();
		void SendMessage(DataBuffer data);
	}
}