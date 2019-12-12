using Chip.Net;
using Chip.Net.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roshambo.Common.Models
{
    public class UserState : ISerializable
    {
		public UserId Id { get; set; }

		public HandState UserHand { get; set; }

		public int Stars { get; set; }

		public UserState() {
			this.UserHand = new HandState();
		}

		public void WriteTo(DataBuffer buffer) {
			Id.WriteTo(buffer);
			UserHand.WriteTo(buffer);
		}

		public void ReadFrom(DataBuffer buffer) {
			Id = new UserId();
			Id.ReadFrom(buffer);

			UserHand = new HandState();
			UserHand.ReadFrom(buffer);
		}
	}
}
