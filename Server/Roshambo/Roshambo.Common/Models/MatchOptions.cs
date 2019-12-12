using Chip.Net;
using Chip.Net.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roshambo.Common.Models
{
	public enum MatchStyle
	{
		Rounds,
		AllCards,
	}

	public class MatchOptions : ISerializable
    {
		public MatchStyle Style { get; set; }
		public int Rounds { get; set; }
        public string Name { get; set; }
		public float TimerLength { get; set; }

		public void ReadFrom(DataBuffer buffer) {
			this.Style = (MatchStyle)buffer.ReadByte();
			this.Rounds = buffer.ReadInt32();
			this.Name = buffer.ReadString();
			this.TimerLength = buffer.ReadFloat();
		}

		public void WriteTo(DataBuffer buffer) {
			buffer.Write((byte)Style);
			buffer.Write((int)Rounds);
			buffer.Write((string)Name);
			buffer.Write((float)TimerLength);
		}
	}
}
