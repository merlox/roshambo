using System;
using System.Collections.Generic;
using System.Text;
using Chip.Net.Data;

namespace Chip.Net.Providers.Direct
{
	public class DirectClientProvider : INetClientProvider
	{
		public EventHandler<ProviderUserEventArgs> UserConnected { get; set; }
		public EventHandler<ProviderUserEventArgs> UserDisconnected { get; set; }

		public EventHandler<ProviderDataEventArgs> DataSent { get; set; }
		public EventHandler<ProviderDataEventArgs> DataReceived { get; set; }

		public bool IsConnected { get; private set; }
		public DirectServerProvider ActiveServer { get; set; }

		public void AcceptConnection(DirectServerProvider Server) {
			IsConnected = true;
			ActiveServer = Server;
			UserConnected?.Invoke(this, new ProviderUserEventArgs());
		}

		public void RejectConnection(DirectServerProvider Server) {
			IsConnected = false;
			throw new Exception("Server not accepting new connections");
		}

		public void DisconnectFrom(DirectServerProvider Server) {
			IsConnected = false;
			UserDisconnected?.Invoke(this, new ProviderUserEventArgs());
		}

		public void ReceiveMessage(DataBuffer buffer) {
			DataBuffer b = new DataBuffer(buffer.ToBytes());
			DataReceived?.Invoke(this, new ProviderDataEventArgs(null, true, b, buffer.GetLength()));
		}

		public void Connect(INetContext context)
		{
			var server = DirectServerProvider.GetServer(context.IPAddress, context.Port);
			if (server == null) {
				long tick = Environment.TickCount;
				while (Environment.TickCount < tick + 50 && server == null) {
					server = DirectServerProvider.GetServer(context.IPAddress, context.Port);
					System.Threading.Thread.Sleep(1);
				}

				if(server == null)
					throw new Exception("Could not connect to server");
			}
			server.TryConnectClient(this);
		}

		public void Disconnect()
		{
			if(IsConnected) {
				IsConnected = false;
				ActiveServer.DisconnectClient(this);
				UserDisconnected?.Invoke(this, new ProviderUserEventArgs());
			}
		}

		public void Dispose()
		{
			if (IsConnected)
				Disconnect();
		}

		public void SendMessage(DataBuffer data)
		{
			ActiveServer.ReceiveMessage(this, data);
			DataSent?.Invoke(this, new ProviderDataEventArgs(null, false, null, data.GetLength()));
		}

		public void UpdateClient()
		{
		}
	}
}
