using Chip.Net.Controllers.Distributed.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Chip.Net.Controllers.Distributed.Services.ModelTracking
{
	public class ModelTrackerCollection<TModel> : ICollection<TModel>, IDisposable where TModel : IDistributedModel {
		public int Count => modelMap.Count;
		public bool Disposed { get; private set; }

		public struct ModelAddedEventArgs { public TModel Model { get; set; } }
		public struct ModelRemovedEventArgs { public TModel Model { get; set; } }
		public struct ModelUpdatedEventArgs {
			public TModel UpdatedModel { get; set; }
			public TModel OldModel { get; set; }
		}

		public EventHandler<ModelAddedEventArgs> ModelAddedEvent { get; set; }
		public EventHandler<ModelUpdatedEventArgs> ModelUpdatedEvent { get; set; }
		public EventHandler<ModelRemovedEventArgs> ModelRemovedEvent { get; set; }

		public TModel this[int id] {
			get {
				if (modelMap.ContainsKey(id) == false)
					return default(TModel);

				return modelMap[id];
			}
		}

		public bool IsReadOnly => throw new NotImplementedException();

		private Dictionary<int, TModel> modelMap;
		private Queue<int> availableIds;
		private int nextId;

		private int GetNextId() {
			if (availableIds.Count > 0)
				return availableIds.Dequeue();

			return nextId++;
		}

		private void RecycleId(int id) {
			if (modelMap.ContainsKey(id))
				throw new Exception("Cannot recycle ID, model not disposed.");

			availableIds.Enqueue(id);
		}

		public ModelTrackerCollection() {
			modelMap = new Dictionary<int, TModel>();
			availableIds = new Queue<int>();
			nextId = 1;
		}

		public void Add(TModel item, bool invokeEvent = true) {
			if (Disposed) throw new Exception("Collection is disposed.");

			item.Id = GetNextId();
			Add(item, item.Id);
		}

		public void Add(TModel item, int id, bool invokeEvent = true) {
			if (Disposed) throw new Exception("Collection is disposed.");

			item.Id = id;
			modelMap[item.Id] = item;

			if(invokeEvent)
				ModelAddedEvent?.Invoke(this, new ModelAddedEventArgs() {
					Model = item,
				});

			while (nextId < id)
				if (modelMap.ContainsKey(nextId) == false)
					RecycleId(nextId++);
		}

		public void Update(int itemId, TModel item, bool invokeEvent = true) {
			if (Disposed) throw new Exception("Collection is disposed.");

			if (itemId != item.Id)
				if (modelMap.ContainsKey(item.Id))
					if (modelMap[item.Id].Equals(item))
						Remove(item.Id);

			if(modelMap.ContainsKey(itemId)) {
				var old = modelMap[itemId];
				modelMap[itemId] = item;

				if(invokeEvent)
					ModelUpdatedEvent?.Invoke(this, new ModelUpdatedEventArgs() {
						OldModel = old,
						UpdatedModel = item,
					});
			} else {
				Add(item, itemId);
			}
		}

		public void Clear() {
			if (Disposed) throw new Exception("Collection is disposed.");

			if(modelMap != null)
				modelMap.Clear();

			nextId = 1;
			availableIds.Clear();
		}

		public bool Contains(TModel item) {
			if (Disposed) throw new Exception("Collection is disposed.");
			return modelMap.ContainsKey(item.Id) && modelMap[item.Id].Equals(item);
		}

		public bool Contains(int itemId) {
			if (Disposed) throw new Exception("Collection is disposed.");
			return modelMap.ContainsKey(itemId);
		}

		public void CopyTo(TModel[] array, int arrayIndex) {
			if (Disposed) throw new Exception("Collection is disposed.");
			modelMap.Values.CopyTo(array, arrayIndex);
		}

		public IEnumerator<TModel> GetEnumerator() {
			if (Disposed) throw new Exception("Collection is disposed.");
			return modelMap.Values.GetEnumerator();
		}

		public bool Remove(TModel item) {
			if (Disposed) throw new Exception("Collection is disposed.");
			if(Contains(item)) {
				return Remove(item.Id);
			}

			return false;
		}

		public bool Remove(int itemId, bool invokeEvent = true) {
			if (Disposed) throw new Exception("Collection is disposed.");

			TModel m = default(TModel);
			modelMap.TryGetValue(itemId, out m);


			var r = modelMap.Remove(itemId);
			if (r) RecycleId(itemId);

			if(m != null && invokeEvent)
				ModelRemovedEvent?.Invoke(this, new ModelRemovedEventArgs() {
					Model = m,
				});

			return r;
		}

		IEnumerator IEnumerable.GetEnumerator() {
			if (Disposed) throw new Exception("Collection is disposed.");
			return modelMap.Values.GetEnumerator();
		}

		public void Dispose() {
			if (Disposed) throw new Exception("Collection is disposed.");
			if(modelMap != null)
				modelMap.Clear();

			if(availableIds != null)
				availableIds.Clear();

			modelMap = null;
			availableIds = null;
			Disposed = true;
		}

		public void Add(TModel item) {
			this.Add(item, false);
		}
	}
}
