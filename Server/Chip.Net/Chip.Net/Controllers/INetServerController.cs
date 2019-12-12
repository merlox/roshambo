using Chip.Net.Data;
using Chip.Net.Providers;
using Chip.Net.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chip.Net.Controllers
{
	public interface INetServerController : IDisposable {
		PacketRouter Router { get; }

		EventHandler<NetEventArgs> NetUserConnected { get; set; }
		EventHandler<NetEventArgs> NetUserDisconnected { get; set; }
		EventHandler<NetEventArgs> PacketReceived { get; set; }
		EventHandler<NetEventArgs> PacketSent { get; set; }

		INetContext Context { get; }
		bool IsActive { get; }

		void InitializeServer(INetContext context, INetServerProvider provider);
		void StartServer();
		void StopServer();
		void UpdateServer();

		IEnumerable<NetUser> GetUsers();
	}
}
