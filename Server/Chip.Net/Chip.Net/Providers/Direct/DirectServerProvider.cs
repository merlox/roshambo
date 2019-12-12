using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using Chip.Net.Data;

namespace Chip.Net.Providers.Direct
{
	public class DirectServerProvider : INetServerProvider
	{
		public static Dictionary<string, DirectServerProvider> Servers { get; private set; } 
			= new Dictionary<string, DirectServerProvider>();

		public static DirectServerProvider GetServer(string ip, int port) {
			lock (Servers) {
				DirectServerProvider server = null;
				Servers.TryGetValue(ip + ":" + port, out server);

				return server;
			}
		}

		public List<DirectClientProvider> Clients { get; private set; }

		public EventHandler<ProviderUserEventArgs> UserConnected { get; set; }
		public EventHandler<ProviderUserEventArgs> UserDisconnected { get; set; }

		public bool IsActive { get; private set; }

		public bool AcceptIncomingConnections { get; set; }
		public EventHandler<ProviderDataEventArgs> DataSent { get; set; }
		public EventHandler<ProviderDataEventArgs> DataReceived { get; set; }

		public void StartServer(INetContext context)
		{
			var key = context.IPAddress + ":" + context.Port;
			lock (Servers) {
				if (Servers.ContainsKey(key))
					throw new Exception("Server already exists with that IP:Port endpoint");

				Servers.Add(key, this);
			}

			Clients = new List<DirectClientProvider>();
			AcceptIncomingConnections = true;
			IsActive = true;
		}

		public void TryConnectClient(DirectClientProvider client) {
			if(AcceptIncomingConnections) {
				Clients.Add(client);

				UserConnected?.Invoke(this, new ProviderUserEventArgs() {
					UserKey = client,
				});

				client.AcceptConnection(this);
			} else {
				client.RejectConnection(this);
			}
		}

		public void DisconnectClient(DirectClientProvider client) {
			Clients.Remove(client);
			UserDisconnected?.Invoke(this, new ProviderUserEventArgs() {
				UserKey = client,
			});
		}

		public void ReceiveMessage(DirectClientProvider client, DataBuffer data) {
			DataBuffer d = new DataBuffer(data.ToBytes());
			DataReceived?.Invoke(this, new ProviderDataEventArgs(client, true, d, d.GetLength()));
		}

		public IEnumerable<object> GetClientKeys()
		{
			return Clients;
		}

		public void SendMessage(object recipientKey, DataBuffer data)
		{
			(recipientKey as DirectClientProvider).ReceiveMessage(data);
			DataSent?.Invoke(this, new ProviderDataEventArgs(recipientKey, false, null, data.GetLength()));
		}

		public void UpdateServer()
		{
		}

		public void StopServer()
		{
			IsActive = false;

			if (Clients != null) {
				foreach (var client in Clients)
					client.DisconnectFrom(this);

				Clients.Clear();
			}

			Clients = null;
		}

		public void DisconnectUser(object userKey)
		{
			Clients.Remove(userKey as DirectClientProvider);
			(userKey as DirectClientProvider).DisconnectFrom(this);

			UserDisconnected?.Invoke(this, new ProviderUserEventArgs() {
				UserKey = userKey,
			});
		}

		public void Dispose()
		{
			if (IsActive)
				StopServer();
		}
	}
}
