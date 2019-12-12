using System;
using System.Collections.Generic;
using System.Text;
using Chip.Net.Data;
using Lidgren.Network;

namespace Chip.Net.Providers.Lidgren {
	public class LidgrenServerProvider : INetServerProvider {
		public EventHandler<ProviderUserEventArgs> UserConnected { get; set; }
		public EventHandler<ProviderUserEventArgs> UserDisconnected { get; set; }


		public EventHandler<ProviderDataEventArgs> DataSent { get; set; }
		public EventHandler<ProviderDataEventArgs> DataReceived { get; set; }

		public bool AcceptIncomingConnections { get; set; }
		public bool IsActive { get; private set; }

		private NetServer server;
		private List<NetConnection> connections;
		private int maxConnections = -1;

		public void StartServer(INetContext context) {
			AcceptIncomingConnections = true;

            NetPeerConfiguration config = new NetPeerConfiguration(context.ApplicationName) {
                Port = context.Port,
                //MaximumConnections = context.MaxConnections
            };
            //maxConnections = context.MaxConnections + 2;

			connections = new List<NetConnection>();
			server = new NetServer(config);
			server.Start();
			while(server.Status == NetPeerStatus.Starting)
				System.Threading.Thread.Sleep(100);

			IsActive = true;
		}

		public void UpdateServer() {
			NetIncomingMessage inc = null;
			while (server != null && (inc = server.ReadMessage()) != null) {
				switch (inc.MessageType) {
					case NetIncomingMessageType.StatusChanged:
						Console.WriteLine(inc.SenderEndPoint.ToString() + " Status: " + inc.SenderConnection.Status.ToString());
						switch (inc.SenderConnection.Status) {
							case NetConnectionStatus.Connected:
								if((connections.Count >= maxConnections && maxConnections > 0) || AcceptIncomingConnections == false) {
									inc.SenderConnection.Disconnect("Server full");
								} else {
									connections.Add(inc.SenderConnection);
									UserConnected?.Invoke(this, new ProviderUserEventArgs() {
										UserKey = inc.SenderConnection,
									});
								}
								break;

							case NetConnectionStatus.Disconnected:
								connections.Remove(inc.SenderConnection);
								UserDisconnected?.Invoke(this, new ProviderUserEventArgs() {
									UserKey = inc.SenderConnection,
								});
								break;
						}
						break;

					case NetIncomingMessageType.Data:
						var bytes = inc.ReadBytes(inc.LengthBytes);
						DataReceived?.Invoke(this, new ProviderDataEventArgs(inc.SenderConnection, true, new DataBuffer(bytes), inc.LengthBytes));
						break;

					case NetIncomingMessageType.Error:
					case NetIncomingMessageType.ErrorMessage:
					case NetIncomingMessageType.WarningMessage:
					case NetIncomingMessageType.DebugMessage:
					case NetIncomingMessageType.VerboseDebugMessage:
						Console.WriteLine(DateTime.Now.ToShortTimeString() + "Lidgren Server" + inc.ReadString());
						break;
				}
			}
		}

		public IEnumerable<object> GetClientKeys() {
			return connections;
		}

		public void SendMessage(object recipientKey, DataBuffer data) {
			if (server == null)
				return;

            if (!(recipientKey is NetConnection client)) {
                throw new ArgumentException("Recipient key cannot be null");
            }

            var bytes = data.ToBytes();
			NetOutgoingMessage outmsg = server.CreateMessage();
			outmsg.Write(bytes);
			client.SendMessage(outmsg, NetDeliveryMethod.ReliableSequenced, 0);

			DataSent?.Invoke(this, new ProviderDataEventArgs(recipientKey, true, data, data.GetLength()));
		}

		public void DisconnectUser(object userKey) {
            if (userKey is NetConnection connection) {
                connection.Disconnect("Disconnected by server");
            }
        }

		public void StopServer() {
			if (server != null) {
				foreach(var connection in connections) {
					UserDisconnected?.Invoke(this, new ProviderUserEventArgs() {
						UserKey = connection,
					});
				}

				server.Shutdown("Server shutting down");
				IsActive = false;
				server = null;
			}
		}

		public void Dispose() {
			if (server != null)
				StopServer();
		}
	}
}
