using Chip.Net;
using Chip.Net.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roshambo.Common.Models
{
    public class RoundState : ISerializable
    {
		public bool IsActive { get; set; }
        public List<(UserId User, CardType Card)> Cards { get; set; }
		public UserId Winner { get; set; }

		public RoundState() {
			Cards = new List<(UserId, CardType)>();
		}

		public void WriteTo(DataBuffer buffer) {
			buffer.Write((byte)(this.Winner == null ? 0 : 1));
			if (this.Winner != null)
				this.Winner.WriteTo(buffer);

			buffer.Write((int)Cards.Count);
			foreach (var c in Cards) {
				c.User.WriteTo(buffer);
				buffer.Write((int)c.Card);
			}
		}

		public void ReadFrom(DataBuffer buffer) {
			if (buffer.ReadByte() == 1)
				this.Winner.ReadFrom(buffer);

			this.Cards = new List<(UserId User, CardType Card)>();
			var ct = buffer.ReadInt32();
			for (int i = 0; i < ct; i++) {
				UserId user = new UserId();
				user.ReadFrom(buffer);

				this.Cards.Add((user, (CardType)buffer.ReadInt32()));
			}
		}
	}
}
