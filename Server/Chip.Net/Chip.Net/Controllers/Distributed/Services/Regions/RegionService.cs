using Chip.Net.Controllers.Distributed.Models;
using Chip.Net.Controllers.Distributed.Services.ModelTracking;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chip.Net.Controllers.Distributed.Services.Regions
{
	public interface IRegionResolver<TRegion, TFocus> 
		where TRegion : IRegionModel 
		where TFocus : IDistributedModel {

		IEnumerable<TRegion> Resolve(TFocus focus);
	}

	public partial class RegionService<TRegion, TFocus> : DistributedService 
		where TRegion : IRegionModel 
		where TFocus : IDistributedModel {

		public IRegionResolver<TRegion, TFocus> Resolver { get; private set; }

		public ShardChannel<FocusPacket> ShardAddFocus { get; private set; }
		public ShardChannel<FocusPacket> ShardRemoveFocus { get; private set; }
		public ShardChannel<FocusPacket> ShardUpdateFocus { get; private set; }

		public ShardChannel<RegionPacket> ShardAddRegion { get; private set; }
		public ShardChannel<RegionPacket> ShardRemoveRegion { get; private set; }
		public ShardChannel<RegionPacket> ShardUpdateRegion { get; private set; }

		public UserChannel<RegionPacket> UserAddRegion { get; private set; }
		public UserChannel<RegionPacket> UserRemoveRegion { get; private set; }
		public UserChannel<RegionPacket> UserUpdateRegion { get; private set; }

		public UserChannel<FocusPacket> UserSetFocus { get; private set; }
		public UserChannel<FocusPacket> UserUpdateFocus { get; private set; }

		public Dictionary<IUserModel, TFocus> FocusMap;

		public RegionService(IRegionResolver<TRegion, TFocus> resolver) {
			this.Resolver = resolver;
		}

		protected override void InitializeDistributedService() {
			base.InitializeDistributedService();
			if (IsUser == false) this.FocusMap = new Dictionary<IUserModel, TFocus>();

			this.ShardAddFocus = CreateShardChannel<FocusPacket>("add");
			this.ShardRemoveFocus = CreateShardChannel<FocusPacket>("remove");
			this.ShardUpdateFocus = CreateShardChannel<FocusPacket>("update");

			this.ShardAddRegion = CreateShardChannel<RegionPacket>("add");
			this.ShardRemoveRegion = CreateShardChannel<RegionPacket>("remove");
			this.ShardUpdateRegion = CreateShardChannel<RegionPacket>("update");

			this.UserAddRegion = CreateUserChannel<RegionPacket>("add");
			this.UserRemoveRegion = CreateUserChannel<RegionPacket>("remove");
			this.UserUpdateRegion = CreateUserChannel<RegionPacket>("update");

			this.UserSetFocus = CreateUserChannel<FocusPacket>("set");
			this.UserUpdateFocus = CreateUserChannel<FocusPacket>("update");
		}
	}
}
