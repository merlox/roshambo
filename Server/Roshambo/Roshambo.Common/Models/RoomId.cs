using Chip.Net;
using Chip.Net.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roshambo.Common.Models
{
	public class RoomId : ISerializable
	{
		public string Name { get; set; }
		public Guid GUID { get; set; }

		public void ReadFrom(DataBuffer buffer) {
			this.Name = buffer.ReadString();
			this.GUID = buffer.ReadGuid();
		}

		public void WriteTo(DataBuffer buffer) {
			buffer.Write((string)this.Name);
			buffer.Write((Guid)this.GUID);
		}
	}
}
