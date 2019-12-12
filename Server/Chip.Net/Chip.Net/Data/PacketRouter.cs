using Chip.Net.Controllers;
using Chip.Net.Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Chip.Net.Data {
    public enum Times
    {
        Once,
        Always,
    }

	public class MessageChannel {
		protected PacketRouter Parent;

		public Type SourceType { get; private set; }
		public string Key { get; private set; }
		public short Order { get; set; }

		public MessageChannel(PacketRouter parent, string key, Type type) {
			this.SourceType = type;
			this.Parent = parent;
			this.Key = key;
		}

		public virtual void Handle(IncomingMessage message) { }
	}

	public class MessageChannel<T> : MessageChannel where T : ISerializable {
		public delegate void MessageEvent(IncomingMessage<T> incoming);
		public MessageEvent Receive { get; set; }

        private List<(Func<IncomingMessage<T>, bool> matchFunc, Times repeatTimes, MessageEvent evnt)> callbacks;
        public EventHandler<OutgoingMessage<T>> MessageSendingEvent { get; set; }

		public MessageChannel(PacketRouter parent, string key)
			: base(parent, key, typeof(T)) {
            callbacks = new List<(Func<IncomingMessage<T>, bool> matchFunc, Times repeatTimes, MessageEvent evnt)>();
        }

		public override void Handle(IncomingMessage message) {
            var msg = message.AsGeneric<T>();
            msg.Route = this.Key;
            Receive?.Invoke(msg);

            if (callbacks.Count == 0)
                return;

            for(int i = callbacks.Count - 1; i >= 0; i--) {
                var evnt = callbacks[i];
                if(evnt.matchFunc == null || evnt.matchFunc(msg)) {
                    evnt.evnt.Invoke(msg);

                    if(evnt.repeatTimes == Times.Once)
                        callbacks.RemoveAt(i);
                }
            }
		}

		public void Send(T data) {
			this.Send(new OutgoingMessage<T>(data));
		}

		public void Send(NetUser recipient, T data) {
			this.Send(new OutgoingMessage<T>(data, recipient));
		}

		public void Send(IEnumerable<NetUser> recipients, T data) {
			this.Send(new OutgoingMessage<T>(data, recipients));
		}

		public void Send(OutgoingMessage<T> message) {
            message.Route = this.Key;
            this.MessageSendingEvent?.Invoke(this, message);
			Parent.Send(this, message);
        }

        public void Subscribe(
            MessageEvent eventHandler, 
            Func<IncomingMessage<T>, bool> matchFunc = null, 
            Times repeatEvent = Times.Once) {

            callbacks.Add((matchFunc, repeatEvent, eventHandler));
        }
	}

	public class PacketRouter {
		public EventHandler<ISerializable> PacketSentEvent { get; set; }
		public EventHandler<ISerializable> PacketReceivedEvent { get; set; }

		public List<MessageChannel> Nodes { get; private set; }
		private HashSet<string> Types;

		private Action<NetUser, DataBuffer> SendAction;
		private Func<object, NetUser> KeyToUserFunc;

		private PacketRouter Parent;
		private string Prefix;

		public PacketRouter(PacketRouter Parent, string prefix) {
			this.Parent = Parent;
			this.Prefix = prefix;
		}

		public PacketRouter(INetServerProvider provider, INetServerController controller) {
			Nodes = new List<MessageChannel>();
			Types = new HashSet<string>();

			var prov = provider;
			var cont = controller;

			provider.DataReceived += OnDataReceived;
			SendAction = (u, d) => {
				if (u == null) {
                    var users = cont.GetUsers().ToList();
                    foreach (var user in users)
						prov.SendMessage(user.UserKey, d);

					return;
				}

				prov.SendMessage(u.UserKey, d);
			};

			KeyToUserFunc = (k) => {
				return controller.GetUsers().First(i => i.UserKey == k);
			};
		}

		public PacketRouter(INetClientProvider provider, INetClientController controller) {
			Nodes = new List<MessageChannel>();
			Types = new HashSet<string>();

			provider.DataReceived += OnDataReceived;
			SendAction = (u, d) => provider.SendMessage(d);
			KeyToUserFunc = (k) => null;
		}

		private void OnDataReceived(object s, ProviderDataEventArgs e) {
			var buffer = e.Data;
			var index = buffer.ReadInt16();

			var channel = Resolve(index);
			var packet = (ISerializable)Activator.CreateInstance(channel.SourceType);

			packet.ReadFrom(buffer);
			NetUser sender = null;
			if (e.UserKey != null)
				sender = KeyToUserFunc(e.UserKey);

			channel.Handle(new IncomingMessage() {
				Data = packet,
				Sender = sender,
			});

			PacketReceivedEvent?.Invoke(this, packet);
		}

		public virtual MessageChannel<T> Route<T>(string key = null, bool usePrefix = true) where T : ISerializable {
			if (key == null)
				key = "";

            if (usePrefix)
                key = typeof(T).ToString() + "_" + key;

            if (Parent != null) {
				return Parent.Route<T>(Prefix + key);
			}

			var n = new MessageChannel<T>(this, key);
			if (Types.Contains(n.Key))
				throw new Exception();

			Types.Add(n.Key);
			Nodes.Add(n);
			Nodes = Nodes.OrderBy(i => i.Key).ToList();
			for (short i = 0; i < Nodes.Count; i++)
				Nodes[i].Order = i;

			return n;
		}

		public virtual short ToIndex(MessageChannel channel) {
			if (Parent != null)
				return Parent.ToIndex(channel);

			return channel.Order;
		}

		public virtual MessageChannel Resolve(short index) {
			if (Parent != null)
				return Parent.Resolve(index);

			return Nodes[index];
		}

		public void Send(MessageChannel source, OutgoingMessage message) {
			if(Parent != null) {
				Parent.Send(source, message);
				return;
			}

			if (SendAction == null)
				return;

			var index = ToIndex(source);
			DataBuffer buffer = new DataBuffer();
			buffer.Write((short)index);
			message.Data.WriteTo(buffer);

			if(message.Recipients != null && message.Recipients.Count() > 0) {
				foreach (var u in message.Recipients)
					SendAction.Invoke(u, buffer);
			} else if(message.Recipients == null) {
				SendAction(null, buffer);
			}

			PacketSentEvent?.Invoke(this, message.Data);
		}
	}
}