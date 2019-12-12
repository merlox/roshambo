using Chip.Net;
using Chip.Net.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roshambo.Common.Models
{
    public class MatchId : ISerializable
    {
		public string Name { get; set; }
        public Guid GUID { get; set; }

		public MatchId() {
			this.GUID = Guid.Empty;
		}

		public static MatchId GenerateNew(string name) {
			return new MatchId() {
				GUID = Guid.NewGuid(),
				Name = name,
			};
		}

		public void WriteTo(DataBuffer buffer) {
			buffer.Write((Guid)this.GUID);
			buffer.Write((string)this.Name);
		}

		public void ReadFrom(DataBuffer buffer) {
			this.GUID = buffer.ReadGuid();
			this.Name = buffer.ReadString();
		}
	}
}
