using Chip.Net;
using Chip.Net.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roshambo.Common.Models
{
    public class PlayerState : ISerializable
    {
        public UserId Id { get; set; }

		public HandState PlayerHand { get; set; }

		public int Stars { get; set; }

		public PlayerState() {
			PlayerHand = new HandState();
		}

		public void WriteTo(DataBuffer buffer) {
			this.Id.WriteTo(buffer);
			this.PlayerHand.WriteTo(buffer);
			buffer.Write((int)this.Stars);
		}

		public void ReadFrom(DataBuffer buffer) {
			this.Id = new UserId();
			this.Id.ReadFrom(buffer);

			this.PlayerHand = new HandState();
			this.PlayerHand.ReadFrom(buffer);

			this.Stars = buffer.ReadInt32();
		}
	}
}
