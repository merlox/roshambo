using Chip.Net.Controllers.Distributed.Models;
using Chip.Net.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chip.Net.UnitTests.Controllers
{
    public class TestUserModel : IUserModel {
		public int Id { get; set; }
		public string Name { get; set; }
		public string UUID { get; set; }

		public TestUserModel() {
			Name = "";
			UUID = "";
		}

		public void ReadFrom(DataBuffer buffer) {
			Id = buffer.ReadInt32();
			Name = buffer.ReadString();
			UUID = buffer.ReadString();
		}

		public void WriteTo(DataBuffer buffer) {
			buffer.Write((int)Id);
			buffer.Write((string)Name);
			buffer.Write((string)UUID);
		}
	}

	public class TestShardModel : IShardModel {
		public int Id { get; set; }

		public void ReadFrom(DataBuffer buffer) {
			Id = buffer.ReadInt32();
		}

		public void WriteTo(DataBuffer buffer) {
			buffer.Write((int)Id);
		}
	}

	public class TestRouterModel : IRouterModel {
		public string Name { get; set; }
		public int Id { get; set; }

		public void ReadFrom(DataBuffer buffer) {
			Name = buffer.ReadString();
			Id = buffer.ReadInt32();
		}

		public void WriteTo(DataBuffer buffer) {
			buffer.Write((string)Name);
			buffer.Write((int)Id);
		}
	}
}
