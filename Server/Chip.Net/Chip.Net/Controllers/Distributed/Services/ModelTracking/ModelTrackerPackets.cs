using Chip.Net.Controllers.Distributed.Models;
using Chip.Net.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chip.Net.Controllers.Distributed.Services.ModelTracking
{
	public partial class ModelTrackerService<TModel> : DistributedService where TModel : IDistributedModel {
		public class ModelTrackerPacket : Packet {
			public int Id { get; set; }
			public TModel Model { get; set; }

			public ModelTrackerPacket() {
				Model = Activator.CreateInstance<TModel>();
				Id = -1;
			}

			public override void WriteTo(DataBuffer buffer) {
				base.WriteTo(buffer);
				buffer.Write((int)Id);
				Model.WriteTo(buffer);
			}

			public override void ReadFrom(DataBuffer buffer) {
				base.ReadFrom(buffer);
				Id = buffer.ReadInt32();
				Model.ReadFrom(buffer);
			}
		}

		public class AddModel : ModelTrackerPacket { }

		public class RemoveModel : ModelTrackerPacket { }

		public class UpdateModel : ModelTrackerPacket { }

		public class UpdateSet : Packet {
			public List<TModel> Models { get; set; }
			public int ModelCount { get; set; }

			public override void WriteTo(DataBuffer buffer) {
				base.WriteTo(buffer);
				buffer.Write((int)ModelCount);
				foreach (var m in Models)
					m.WriteTo(buffer);
			}

			public override void ReadFrom(DataBuffer buffer) {
				base.ReadFrom(buffer);
				ModelCount = buffer.ReadInt32();
				Models = new List<TModel>();
				for(int i = 0; i < ModelCount; i++) {
					var m = Activator.CreateInstance<TModel>();
					m.ReadFrom(buffer);
					Models.Add(m);
				}
			}
		}
	}
}
