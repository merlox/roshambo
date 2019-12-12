using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using Chip.Net.Data;

namespace Chip.Net.Providers.TCP {
	public class TCPClientProvider : TCPProviderBase, INetClientProvider {
		public EventHandler<ProviderUserEventArgs> UserConnected { get; set; }
		public EventHandler<ProviderUserEventArgs> UserDisconnected { get; set; }
		public EventHandler<ProviderDataEventArgs> DataSent { get; set; }
		public EventHandler<ProviderDataEventArgs> DataReceived { get; set; }

		public bool IsConnected { get; private set; }

		private TcpClient client;
		private byte[] message;

		public void Connect(INetContext context) {
			if(IsConnected) {
				Disconnect();
			}

			message = new byte[1024 * 512];

			client = new TcpClient {
				NoDelay = false
			};

			client.SendBufferSize = client.ReceiveBufferSize = 1024 * 256;
			client.Connect(context.IPAddress, context.Port);

			if (client.Connected) {
				UserConnected?.Invoke(this, new ProviderUserEventArgs());
				IsConnected = true;
			}
		} 

		public void UpdateClient() {
			//If we haven't started the client, do nothing.
			if (client == null || client.Client == null)
				return;

			//If we haven't connected or are waiting to connect, do nothing.
			if (client.Connected == false)
			{
				if (UserDisconnected != null)
					UserDisconnected(this, new ProviderUserEventArgs());

				return;
			}

			//Attempt to read any incoming data from the server.
			int bytesRead = 0;
			int totalBytesRead = 0;
			var messageStream = client.GetStream();
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
					byte[] d = new byte[size];
					buff.ReadBytes(d, 0, d.Length);

					d = Decompress(d);
					if(d.Length == 1 && d[0] == 0) {
						Disconnect();
						break;
					}

					DataBuffer msgBuffer = new DataBuffer(d);
					msgBuffer.Seek(0);

					//Delegate that other classes can attach to, mainly the network handler
					DataReceived?.Invoke(this, new ProviderDataEventArgs(null, true, msgBuffer, bytesRead));
				}
			}
		}

		public void Disconnect() {
			Dispose();
		}

		public void SendMessage(DataBuffer buffer) {
			var msg = buffer.ToBytes();
			msg = Compress(msg);

			client.GetStream().Write(BitConverter.GetBytes((Int16)msg.Length), 0, 2);
			client.GetStream().Write(msg, 0, msg.Length);
			client.GetStream().Flush();

			DataSent?.Invoke(this, new ProviderDataEventArgs(null, false, null, msg.Length));
		}

		public void Dispose() {
			if (client != null) {
				byte[] arr = new byte[1] { 0 };
				try {
					SendMessage(new DataBuffer(arr));
				} catch { }

				client.Close();

				UserDisconnected?.Invoke(this, new ProviderUserEventArgs());
			}

			IsConnected = false;
			client = null;
		}
	}
}
