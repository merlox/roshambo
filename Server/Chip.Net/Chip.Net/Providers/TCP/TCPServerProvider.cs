using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Chip.Net.Data;

namespace Chip.Net.Providers.TCP
{
	public class TCPServerProvider : TCPProviderBase, INetServerProvider {
		public EventHandler<ProviderUserEventArgs> UserConnected { get; set; }
		public EventHandler<ProviderUserEventArgs> UserDisconnected { get; set; }

		public EventHandler<ProviderDataEventArgs> DataSent { get; set; }
		public EventHandler<ProviderDataEventArgs> DataReceived { get; set; }

		public bool IsActive { get; private set; }
		public bool AcceptIncomingConnections
		{
			get
			{
				return clientListener != null;
			}

			set
			{
				if(value && clientListener == null)
				{
					clientListener = new TcpListener(IPAddress.Any, port);
					clientListener.Start(maxConnections);
				}
				else
				{
					if (clientListener != null) {
						clientListener.Stop();
						clientListener = null;
					}
				}
			}
		}

		private HashSet<TcpClient> connected;
		private List<TcpClient> clientList;
		private TcpListener clientListener;

		private int maxConnections;
		private byte[] message;
		private int port;

		public void StartServer(INetContext context) {
			this.maxConnections = context.MaxConnections;
			this.port = context.Port;

			clientListener = new TcpListener(IPAddress.Any, context.Port);
			clientListener.Start(context.MaxConnections);

			//clientList = new List<ClientInfo>();
			message = new byte[1024 * 512];
			clientList = new List<TcpClient>();
			connected = new HashSet<TcpClient>();

			IsActive = true;
		}

		public void UpdateServer() {
			if (IsActive == false)
				return;

			//If any clients are waiting to connect
			if (clientListener != null && clientListener.Pending()) {
				TcpClient newClient = clientListener.AcceptTcpClient();
				newClient.SendBufferSize = newClient.ReceiveBufferSize = 1024 * 256;
				newClient.NoDelay = false;

				if (AcceptIncomingConnections)
				{
					connected.Add(newClient);
					clientList.Add(newClient);

					//Delegate
					UserConnected?.Invoke(this, new ProviderUserEventArgs()
					{
						UserKey = newClient,
					});
				}
			}

			if (clientList == null)
				return;

			//Update each client, listening for any incoming data.
			for (int i = clientList.Count - 1; i >= 0; i--) {
				TcpClient tcpClient = clientList[i];

				//If the client is no longer connected, fire off a disconnection event delegate
				if (!tcpClient.Connected) {
					DisconnectUser(tcpClient);
					continue;
				}

				//Attempt to read any incoming data from this client
				int bytesRead = 0;
				int totalBytesRead = 0;
				NetworkStream messageStream = tcpClient.GetStream();
				MemoryStream memStream = new MemoryStream();

				if (messageStream.DataAvailable) {
					var buff = new DataBuffer();
					while (messageStream.DataAvailable) {
						bytesRead = messageStream.Read(message, 0, message.Length);
						byte[] data = new byte[bytesRead];
						Array.Copy(message, data, bytesRead);
						buff.Write((byte[])data, 0, data.Length);
						totalBytesRead += bytesRead;
					}

					buff.Seek(0);
					while (buff.GetPosition() < buff.GetLength()) {
						var size = buff.ReadInt16();
                        if (size == 0)
                            break;

						byte[] d = new byte[size];
						buff.ReadBytes(d, 0, d.Length);

						d = Decompress(d);
						if (d.Length == 1 && d[0] == 0) {
							DisconnectUser(tcpClient);
							break;
						}

						DataBuffer msgBuffer = new DataBuffer(d);
						msgBuffer.Seek(0);

						//Delegate that other classes can attach to, mainly the network handler
						DataReceived?.Invoke(this, new ProviderDataEventArgs(tcpClient, true, msgBuffer, msgBuffer.GetLength()));
					}
				}
			}
		}

		public void StopServer() {
			Dispose();
		}

		public IEnumerable<object> GetClientKeys() {
			return clientList.AsReadOnly();
		}

		public void DisconnectUser(object userKey) {
			var tcpClient = userKey as TcpClient;
			if(tcpClient != null && connected.Contains(tcpClient)) {
				connected.Remove(tcpClient);
				byte[] arr = new byte[1] { 0 };
				try {
					SendMessage(tcpClient, new DataBuffer(arr));
				} catch { }

				UserDisconnected?.Invoke(this, new ProviderUserEventArgs() {
					UserKey = tcpClient,
				});

				tcpClient.Close();
				clientList.Remove(tcpClient);
			}
		}

		public void SendMessage(object recipientKey, DataBuffer buffer) {
			var msg = Compress(buffer.ToBytes());

			try {
				var tcpClient = recipientKey as TcpClient;
				tcpClient.GetStream().Write(BitConverter.GetBytes((Int16)msg.Length), 0, 2);
				tcpClient.GetStream().Write(msg, 0, msg.Length);
				tcpClient.GetStream().Flush();

				DataSent?.Invoke(this, new ProviderDataEventArgs(recipientKey, false, null, msg.Length));
			} catch(Exception ex) {
				if(connected.Contains(recipientKey as TcpClient))
					DisconnectUser(recipientKey);
			}
		}

		public void Dispose() {
			if (clientList != null)
			{
				for (int i = clientList.Count - 1; i >= 0; i--)
				{
					DisconnectUser(clientList[i]);
				}

				clientList.Clear();
			}

			if (clientListener != null)
			{
				clientListener.Stop();
				clientListener = null;
			}

			clientList = null;
			connected = null;
			message = null;
			IsActive = false;
		}
	}
}
