using Chip.Net;
using Chip.Net.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roshambo.Common.Models
{
    public class UserId : ISerializable
    {
        public Guid GUID { get; set; }
		public string Name { get; set; }

		public void ReadFrom(DataBuffer buffer) {
			this.GUID = buffer.ReadGuid();
			this.Name = buffer.ReadString();
		}

		public void WriteTo(DataBuffer buffer) {
			buffer.Write((Guid)this.GUID);
			buffer.Write((string)this.Name);
		}
	}
}
