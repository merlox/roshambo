using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Net;
using Chip.Net.Data;
using System.Linq;

namespace Chip.Net.Providers.Sockets {
    public class SocketServerProvider : SocketProviderBase, INetServerProvider {
		public EventHandler<ProviderUserEventArgs> UserConnected { get; set; }
		public EventHandler<ProviderUserEventArgs> UserDisconnected { get; set; }

		public EventHandler<ProviderDataEventArgs> DataSent { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		public EventHandler<ProviderDataEventArgs> DataReceived { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

		public bool IsActive { get; }
		public bool AcceptIncomingConnections { get; set; }

		private Socket Listener;

		private List<Socket> Clients;
		private object LockObject = new object();

		public void DisconnectUser(object userKey) {
			var client = userKey as Socket;
			client.Shutdown(SocketShutdown.Both);
			client.Close();

			Clients.Remove(client);
			UserDisconnected?.Invoke(this, new ProviderUserEventArgs() {
				UserKey = client,
			});
		}

		public void Dispose() {
			if (Listener != null)
				StopServer();
		}

		public IEnumerable<object> GetClientKeys() {
			return Clients.AsReadOnly();
		}

		public void SendMessage(object recipientKey, DataBuffer data) {
			var client = recipientKey as Socket;
			var dataArr = data.ToBytes();
			client.BeginSend(dataArr, 0, dataArr.Length, 0, OnSend, client);
			DataSent?.Invoke(this, new ProviderDataEventArgs(client, false, data, dataArr.Length));
		}

		private void OnSend(IAsyncResult ar) {

		}

		public void StartServer(INetContext context) {
			IPHostEntry ipHostInfo = Dns.GetHostEntry("localhost");
			IPAddress ipAddress = ipHostInfo.AddressList[0];
			IPEndPoint localEndPoint = new IPEndPoint(ipAddress, context.Port);

			Clients = new List<Socket>();
			Listener = new Socket(ipAddress.AddressFamily,
				SocketType.Stream, ProtocolType.Tcp);

			Listener.Bind(localEndPoint);
			Listener.Listen(context.MaxConnections > 0 ? context.MaxConnections : 10);
			Listener.BeginAccept(OnSocketBeginAccept, Listener);
		}

		private void OnSocketBeginAccept(IAsyncResult ar) {
			var server = ar.AsyncState as Socket;
			var client = server.EndAccept(ar);
			Listener.BeginAccept(OnSocketBeginAccept, Listener);

			Clients.Add(client);
			UserConnected?.Invoke(this, new ProviderUserEventArgs() {
				UserKey = client,
			});

			StateObject state = new StateObject();
			state.socket = client;
			client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, OnSocketRead, state);
		}

		private void OnSocketRead(IAsyncResult ar) {
			var state = ar.AsyncState as StateObject;
			var client = state.socket;

			int bytesRead = client.EndReceive(ar);
			client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, OnSocketRead, state);

			if(bytesRead > 0) {
				byte[] arr = new byte[bytesRead];
				Array.Copy(state.buffer, arr, bytesRead);

				DataBuffer buffer = new DataBuffer(arr);
				DataReceived?.Invoke(this, new ProviderDataEventArgs(client, true, buffer, bytesRead));
			}
		}

		public void StopServer() {
			if (Listener != null) {
				Listener.Shutdown(SocketShutdown.Both);
				Listener.Close();
			}

			Listener = null;
		}

		public void UpdateServer() {

		}
	}
}
