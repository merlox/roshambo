using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Chip.Net.Data;

namespace Chip.Net.Providers.Sockets
{
	public class SocketClientProvider : SocketProviderBase, INetClientProvider {
		public EventHandler<ProviderUserEventArgs> UserConnected { get; set; }
		public EventHandler<ProviderUserEventArgs> UserDisconnected { get; set; }

		public bool IsConnected { get; }
		public EventHandler<ProviderDataEventArgs> DataSent { get; set; }
		public EventHandler<ProviderDataEventArgs> DataReceived { get; set; }

		private object LockObject = new object();
		private Socket Connection;

		public void Connect(INetContext context) {
			IPHostEntry ipHostInfo = Dns.GetHostEntry(context.IPAddress);
			IPAddress ipAddress = ipHostInfo.AddressList[0];
			IPEndPoint localEndPoint = new IPEndPoint(ipAddress, context.Port);

			Connection = new Socket(ipAddress.AddressFamily,
				SocketType.Stream, ProtocolType.Tcp);

			//Connection.Connect(ipAddress, context.Port);
			Connection.BeginConnect(localEndPoint, OnConnect, Connection);
		}

		private void OnConnect(IAsyncResult ar) {
			var conn = ar.AsyncState as Socket;
			conn.EndConnect(ar);

			if (conn.Connected) {
				UserConnected?.Invoke(this, new ProviderUserEventArgs() { UserKey = conn });

				StateObject state = new StateObject();
				state.socket = conn;
				conn.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, OnReceive, state);
			}
		}

		private void OnReceive(IAsyncResult ar) {
			var state = ar.AsyncState as StateObject;
			var conn = state.socket;

			int bytesRead = conn.EndReceive(ar);
			conn.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, OnReceive, state);

			if (bytesRead > 0) {
				byte[] arr = new byte[bytesRead];
				Array.Copy(state.buffer, arr, bytesRead);

				DataBuffer buffer = new DataBuffer(arr);
				DataReceived?.Invoke(this, new ProviderDataEventArgs(null, true, buffer, bytesRead));
			}
		}

		public void Disconnect() {
			if(Connection != null) {
				Connection.Shutdown(SocketShutdown.Both);
				Connection.Close();
				Connection = null;

				UserDisconnected?.Invoke(this, new ProviderUserEventArgs());
			}
		}

		public void Dispose() {
			if (Connection != null)
				Disconnect();
		}

		public void SendMessage(DataBuffer data) {
			var bytes = data.ToBytes();
			Connection.BeginSend(bytes, 0, bytes.Length, 0, OnSend, Connection);
			DataSent?.Invoke(this, new ProviderDataEventArgs(null, false, null, bytes.Length));
		}

		private void OnSend(IAsyncResult ar) {
			var conn = ar.AsyncState as Socket;
			int sent = conn.EndSend(ar);
		}

		public void UpdateClient() {

		}
	}
}
