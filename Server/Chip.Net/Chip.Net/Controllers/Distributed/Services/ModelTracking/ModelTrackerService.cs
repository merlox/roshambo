using Chip.Net.Controllers.Distributed.Models;
using Chip.Net.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Chip.Net.Controllers.Distributed.Services.ModelTracking
{
    public partial class ModelTrackerService<TModel> : DistributedService where TModel : IDistributedModel
    {
		public ModelTrackerCollection<TModel> Models { get; set; }
		private Func<ValidationContext, IUserModel, bool> ShardValidate;
		private Func<ValidationContext, IUserModel, bool> UserValidate;

		private ShardChannel<AddModel> ShardAddModel;
		private ShardChannel<RemoveModel> ShardRemoveModel;
		private ShardChannel<UpdateModel> ShardUpdateModel;
		private ShardChannel<UpdateSet> ShardUpdateSet;

		private UserChannel<AddModel> UserAddModel;
		private UserChannel<RemoveModel> UserRemoveModel;
		private UserChannel<UpdateModel> UserUpdateModel;
		private UserChannel<UpdateSet> UserUpdateSet;

		private bool SendMessages;

		protected override void InitializeDistributedService() {
			base.InitializeDistributedService();
			SendMessages = true;

			ShardAddModel = CreateShardChannel<AddModel>();
			ShardRemoveModel = CreateShardChannel<RemoveModel>();
			ShardUpdateModel = CreateShardChannel<UpdateModel>();
			ShardUpdateSet = CreateShardChannel<UpdateSet>();

			ShardAddModel.Receive += addModel;
			ShardRemoveModel.Receive += removeModel;
			ShardUpdateModel.Receive += updateModel;
			ShardUpdateSet.Receive += updateSet;

			UserAddModel = CreateUserChannel<AddModel>();
			UserRemoveModel = CreateUserChannel<RemoveModel>();
			UserUpdateModel = CreateUserChannel<UpdateModel>();
			UserUpdateSet = CreateUserChannel<UpdateSet>();

			UserAddModel.Receive += addModel;
			UserRemoveModel.Receive += removeModel;
			UserUpdateModel.Receive += updateModel;
			UserUpdateSet.Receive += updateSet;

			Models = new ModelTrackerCollection<TModel>();
			Models.ModelAddedEvent += OnModelAdded;
			Models.ModelRemovedEvent += OnModelRemoved;
			Models.ModelUpdatedEvent += OnModelUpdated;

			if(IsRouter) {
				RouterController.ShardController.NetUserConnected += OnShardConnected;
				RouterController.UserController.NetUserConnected += OnUserConnected;
			}
		}

		private void OnShardConnected(object sender, NetEventArgs e) {
			ShardUpdateSet.Send(new UpdateSet() {
				Models = this.Models.ToList(),
				ModelCount = this.Models.Count,
			}, RouterController.GetShard(e.User));
		}

		private void OnUserConnected(object sender, NetEventArgs e) {
			UserUpdateSet.Send(new UpdateSet() {
				Models = this.Models.ToList(),
				ModelCount = this.Models.Count,
			}, RouterController.GetUser(e.User));
		}

		public struct ValidationContext {
			public TModel Model { get; set; }
			public NetUser User { get; set; }
			public ModelTrackerPacket Source { get; set; }
		}

		public void SetShardValidation(Func<ValidationContext, IUserModel, bool> func) {
			this.ShardValidate = func;
		}

		public void SetUserValidation(Func<ValidationContext, IUserModel, bool> func) {
			this.UserValidate = func;
		}

		private void addModel(object sender, AddModel e) {
			SendMessages = false;
			e.Model.Id = e.Id;
			Models.Add(e.Model, e.Id, false);
			SendMessages = true;
		}

		private void removeModel(object sender, RemoveModel e) {
			SendMessages = false;
			Models.Remove(e.Id, false);
			SendMessages = true;
		}

		private void updateModel(object sender, UpdateModel e) {
			SendMessages = false;
			Models.Update(e.Id, e.Model, false);
			SendMessages = true;
		}

		private void updateSet(object sender, UpdateSet e) {
			SendMessages = false;
			foreach(var m in e.Models) {
				updateModel(sender, new UpdateModel() { Id = m.Id, Model = m });
			}
			SendMessages = true;
		}

		private void OnModelAdded(object sender, ModelTrackerCollection<TModel>.ModelAddedEventArgs e) {
			if (SendMessages == false) return;

			AddModel m = new AddModel();
			m.Id = e.Model.Id;
			m.Model = e.Model;

			ShardAddModel.Send(m);
			UserAddModel.Send(m);
		}

		private void OnModelRemoved(object sender, ModelTrackerCollection<TModel>.ModelRemovedEventArgs e) {
			if (SendMessages == false) return;

			RemoveModel m = new RemoveModel();
			m.Id = e.Model.Id;
			m.Model = e.Model;

			ShardRemoveModel.Send(m);
			UserRemoveModel.Send(m);
		}

		private void OnModelUpdated(object sender, ModelTrackerCollection<TModel>.ModelUpdatedEventArgs e) {
			if (SendMessages == false) return;

			UpdateModel m = new UpdateModel();
			m.Id = e.UpdatedModel.Id;
			m.Model = e.UpdatedModel;

			ShardUpdateModel.Send(m);
			UserUpdateModel.Send(m);
		}
	}
}
