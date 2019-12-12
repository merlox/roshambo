using Chip.Net.Data;
using Chip.Net.Providers;
using Chip.Net.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chip.Net.Controllers
{
    public interface INetClientController : IDisposable {
		PacketRouter Router { get; }

		EventHandler<NetEventArgs> OnConnected { get; set; }
		EventHandler<NetEventArgs> OnDisconnected { get; set; }
		EventHandler<NetEventArgs> OnPacketReceived { get; set; }
		EventHandler<NetEventArgs> OnPacketSent { get; set; }

		INetContext Context { get; }
		bool IsConnected { get; }

		void InitializeClient(INetContext context, INetClientProvider provider);
		void StartClient();
		void StopClient();
		void UpdateClient();
    }
}
