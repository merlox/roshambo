using Chip.Net.Controllers.Distributed.Models;
using Chip.Net.Controllers.Distributed.Services.ModelTracking;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chip.Net.Controllers.Distributed.Services.Regions
{
	public partial class RegionService<TRegion, TFocus> : DistributedService
		where TRegion : IRegionModel
		where TFocus : IDistributedModel {
		
		public override void InitializeRouter() {
			base.InitializeRouter();

			this.ShardUpdateFocus.Receive += OnShardUpdateFocus;
			this.ShardUpdateRegion.Receive += OnShardUpdateRegion;

			this.UserSetFocus.Receive += OnUserSetFocus;
			this.UserUpdateFocus.Receive += OnUserUpdateFocus;
		}

		private void OnUserUpdateFocus(object sender, FocusPacket e) {
			throw new NotImplementedException();
		}

		private void OnUserSetFocus(object sender, FocusPacket e) {
			throw new NotImplementedException();
		}

		private void OnShardUpdateRegion(object sender, RegionPacket e) {
			throw new NotImplementedException();
		}

		private void OnShardUpdateFocus(object sender, FocusPacket e) {
			throw new NotImplementedException();
		}

		protected override void UpdateRouter() {
			base.UpdateRouter();
		}

		private void Router_OnFocusAdded(object sender, ModelTrackerCollection<TFocus>.ModelAddedEventArgs e) {
			throw new NotImplementedException();
		}

		private void Router_OnFocusUpdated(object sender, ModelTrackerCollection<TFocus>.ModelUpdatedEventArgs e) {
			throw new NotImplementedException();
		}

		private void Router_OnFocusRemoved(object sender, ModelTrackerCollection<TFocus>.ModelRemovedEventArgs e) {
			throw new NotImplementedException();
		}
	}
}
