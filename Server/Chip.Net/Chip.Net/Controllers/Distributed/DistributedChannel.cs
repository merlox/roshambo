using Chip.Net.Controllers.Distributed.Models;
using Chip.Net.Controllers.Distributed.Packets;
using Chip.Net.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chip.Net.Controllers.Distributed
{
    public class DistributedChannel<T> where T : Packet
    {
		public MessageChannel<T>.MessageEvent Receive { get; set; }

		private MessageChannel<PassthroughPacket<T>> Source;

		public DistributedChannel(MessageChannel<PassthroughPacket<T>> source) {
			this.Source = source;
			this.Source.Receive += OnReceive;
		}

		private void OnReceive(IncomingMessage<PassthroughPacket<T>> incoming) {
			Receive?.Invoke(new IncomingMessage<T>() {
				Data = incoming.Data.Data,
				Sender = incoming.Sender,
			});
		}

		public void Send(T data) {
			Source.Send(new OutgoingMessage<PassthroughPacket<T>>(new PassthroughPacket<T>(data)));
		}

		public void Send(T data, IShardModel shard) {
			Source.Send(new OutgoingMessage<PassthroughPacket<T>>(new PassthroughPacket<T>(data, shard)));
		}

		public void Send(T data, IEnumerable<IShardModel> shards) {
			foreach (var sh in shards)
				Send(data, sh);
		}

		public void Send(T data, IUserModel user) {
			Source.Send(new OutgoingMessage<PassthroughPacket<T>>(new PassthroughPacket<T>(data, user)));
		}

		public void Send(T data, IEnumerable<IUserModel> Users) {
			foreach (var us in Users)
				Send(data, us);
		}
	}
}
