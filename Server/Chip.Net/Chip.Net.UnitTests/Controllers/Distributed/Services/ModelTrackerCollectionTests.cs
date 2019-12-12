using Chip.Net.Controllers.Distributed.Models;
using Chip.Net.Controllers.Distributed.Services.ModelTracking;
using Chip.Net.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Chip.Net.UnitTests.Controllers.Distributed.Services
{
	public class TestModel : IDistributedModel {
		public int Id { get; set; }
		public string Data { get; set; }

		public TestModel() {
			Data = Guid.NewGuid().ToString();
		}

		public void ReadFrom(DataBuffer buffer) {
			Id = buffer.ReadInt32();
			Data = buffer.ReadString();
		}

		public void WriteTo(DataBuffer buffer) {
			buffer.Write((int)Id);
			buffer.Write((string)Data);
		}

		public override bool Equals(object obj) {
			if(obj is TestModel other) {
				return other.Id == Id && other.Data == Data;
			}

			return false;
		}

		public override int GetHashCode() {
			return base.GetHashCode();
		}
	}

	[TestClass]
	public class ModelTrackerCollectionTests {
		public ModelTrackerCollection<TestModel> Empty { get; set; }
		public ModelTrackerCollection<TestModel> Existing { get; set; }
		public ModelTrackerCollection<TestModel> Disposed { get; set; }

		public int ExistingCount { get; set; } = 1000;
		public int ExistingRemoveCount { get; set; } = 100;

		[TestInitialize]
		public void Initialize() {
			Empty = new ModelTrackerCollection<TestModel>();
			Existing = new ModelTrackerCollection<TestModel>();
			Disposed = new ModelTrackerCollection<TestModel>();
			Disposed.Dispose();

			for (int i = 0; i < ExistingCount; i++) {
				Existing.Add(new TestModel());
			}

			Random r = new Random();
			for (int i = 0; i < ExistingRemoveCount; i++) {
				int index = r.Next(ExistingCount - i);
				Existing.Remove(index);
			}
		}

		[TestMethod]
		public void ModelTrackerCollection_NewCollection_AddModel_EventInvoked() {
			bool added = false;
			var m = new TestModel();
			Empty.ModelAddedEvent += (s, e) => {
				Assert.AreEqual(e.Model.Id, m.Id);
				Assert.AreEqual(e.Model.Data, m.Data);
				added = true;
			};

			Empty.Add(m);
			Assert.IsTrue(added);
		}

		[TestMethod]
		public void ModelTrackerCollection_ExistingCollection_UpdateModel_EventInvoked() {
			var m = Existing.Skip(1).First();
			var oldData = m.Data;
			var oldId = m.Id;

			m = new TestModel();
			m.Id = oldId;
			m.Data = Guid.NewGuid().ToString();

			bool updated = false;
			Existing.ModelUpdatedEvent += (s, e) => {
				Assert.AreEqual(e.UpdatedModel.Data, m.Data);
				Assert.AreEqual(e.UpdatedModel.Id, m.Id);
				Assert.AreEqual(e.OldModel.Data, oldData);
				Assert.AreEqual(e.OldModel.Id, oldId);
				updated = true;
			};

			Existing.Update(m.Id, m);
			Assert.IsTrue(updated);
		}

		[TestMethod]
		public void ModelTrackerCollection_ExistingCollection_RemoveModel_EventInvoked() {
			var m = Existing.Skip(1).First();
			bool removed = false;
			Existing.ModelRemovedEvent += (s, e) => {

				Assert.AreEqual(m.Data, e.Model.Data);
				Assert.AreEqual(m.Id, e.Model.Id);
				removed = true;
			};

			Existing.Remove(m.Id);
			Assert.IsTrue(removed);
		}

		[TestMethod]
		public void ModelTrackerCollection_NewCollection_AddModel_CountIncreased() {
			var m = new TestModel();
			int c = Empty.Count;
			Empty.Add(m);

			Assert.IsTrue(Empty.Count > c);
		}

		[TestMethod]
		public void ModelTrackerCollection_NewCollection_AddModel_IdAssigned() {
			var m = new TestModel();
			var m2 = new TestModel();
			m.Id = 10000000;

			Empty.Add(m);
			Empty.Add(m2);
			Assert.IsTrue(m.Id != 10000000);
			Assert.IsTrue(m2.Id != 10000000);
			Assert.IsTrue(m.Id != m2.Id);
		}

		[TestMethod]
		public void ModelTrackerCollection_NewCollection_AddModel_ContainsModel_ReturnsTrue() {
			var m = new TestModel();

			Assert.IsFalse(Empty.Contains(m));
			Empty.Add(m);
			Assert.IsTrue(Empty.Contains(m));
		}

		[TestMethod]
		public void ModelTrackerCollection_NewCollection_AddModel_IndexReturnsModel() {
			var m = new TestModel();

			Empty.Add(m);
			Assert.IsNotNull(Empty[m.Id]);
			Assert.AreEqual(m, Empty[m.Id]);
		}

		[TestMethod]
		public void ModelTrackerCollection_NewCollection_RemoveModel_RemoveReturnsFalse() {
			Assert.IsFalse(Empty.Remove(0));
		}

		[TestMethod]
		public void ModelTrackerCollection_NewCollection_AddRemoveModel_RemoveReturnsTrue() {
			var m = new TestModel();

			Empty.Add(m);
			Assert.IsTrue(Empty.Remove(m));
		}

		[TestMethod]
		public void ModelTrackerCollection_NewCollection_AddRmoveModel_CountUnchanged() {
			var m = new TestModel();
			var c = Empty.Count;

			Empty.Add(m);
			Empty.Remove(m);
			Assert.AreEqual(Empty.Count, c);
		}

		[TestMethod]
		public void ModelTrackerCollection_NewCollection_AddRemoveModel_ContainsModel_ReturnsFalse() {
			var m = new TestModel();

			Empty.Add(m);
			Empty.Remove(m);
			Assert.IsFalse(Empty.Contains(m));
		}

		[TestMethod]
		public void ModelTrackerCollection_NewCollection_AddRemoveModel_IdRecycled() {
			var m = new TestModel();
			var m2 = new TestModel();

			Empty.Add(m);
			var id = m.Id;
			Empty.Remove(m);

			Empty.Add(m2);
			Assert.IsTrue(id == m2.Id);
		}

		[TestMethod]
		public void ModelTrackerCollection_NewCollection_CopyToArray_ArrayContainsNothing() {
			var m = new TestModel();

			TestModel[] arr = new TestModel[0];
			Empty.CopyTo(arr, 0);
		}

		[TestMethod]
		public void ModelTrackerCollection_NewCollection_AddModel_ClearCollection_CollectionEmpty() {
			var m = new TestModel();

			Empty.Add(m);
			Empty.Clear();

			Assert.IsTrue(Empty.Count == 0);
		}

		[TestMethod]
		public void ModelTrackerCollection_NewCollection_DisposeCollection_CollectionDisposed() {
			var m = new TestModel();

			Empty.Add(m);
			Empty.Dispose();

			Assert.IsTrue(Empty.Disposed);
		}

		[TestMethod]
		public void ModelTrackerCollection_NewCollection_GetEnumerator_ReturnsEnumerator() {
			Assert.IsNotNull(Empty.GetEnumerator());
		}

		[TestMethod]
		public void ModelTrackerCollection_NewCollection_GetFromIndex_ReturnsNull() {
			Assert.IsNull(Empty[0]);
		}

		[TestMethod]
		public void ModelTrackerCollection_ExistingCollection_UpdateModel_ModelUpdated() {
			var ex = Existing.First();
			ex.Data = "new data";
			Existing.Update(ex.Id, ex);

		}

		[TestMethod]
		public void ModelTrackerCollection_ExistingCollection_AddModel_CountIncreased() {
			var m = new TestModel();
			int c = Existing.Count;
			Existing.Add(m);

			Assert.IsTrue(Existing.Count > c);
		}

		[TestMethod]
		public void ModelTrackerCollection_ExistingCollection_AddModel_IdAssigned() {
			var m = new TestModel();
			var m2 = new TestModel();
			m.Id = 1000000;

			Existing.Add(m);
			Existing.Add(m2);
			Assert.IsTrue(m.Id != 1000000);
			Assert.IsTrue(m2.Id != 1000000);
			Assert.IsTrue(m.Id != m2.Id);
		}

		[TestMethod]
		public void ModelTrackerCollection_ExistingCollection_AddModel_ContainsModel_ReturnsTrue() {
			var m = new TestModel();

			Assert.IsFalse(Existing.Contains(m));
			Existing.Add(m);
			Assert.IsTrue(Existing.Contains(m));
		}

		[TestMethod]
		public void ModelTrackerCollection_ExistingCollection_AddModel_IndexReturnsModel() {
			var m = new TestModel();

			Existing.Add(m);
			Assert.IsNotNull(Existing[m.Id]);
			Assert.AreEqual(m, Existing[m.Id]);
		}

		[TestMethod]
		public void ModelTrackerCollection_ExistingCollection_AddRemoveModel_RemoveReturnsTrue() {
			var m = new TestModel();

			Existing.Add(m);
			Assert.IsTrue(Existing.Remove(m));
		}

		[TestMethod]
		public void ModelTrackerCollection_ExistingCollection_AddRmoveModel_CountUnchanged() {
			var m = new TestModel();
			var c = Existing.Count;

			Existing.Add(m);
			Existing.Remove(m);
			Assert.AreEqual(Existing.Count, c);
		}

		[TestMethod]
		public void ModelTrackerCollection_ExistingCollection_AddRemoveModel_ContainsModel_ReturnsFalse() {
			var m = new TestModel();

			Existing.Add(m);
			Existing.Remove(m);
			Assert.IsFalse(Existing.Contains(m));
		}

		[TestMethod]
		public void ModelTrackerCollection_ExistingCollection_CopyToArray_ArrayContainsModels() {
			TestModel[] arr = new TestModel[Existing.Count];
			Existing.CopyTo(arr, 0);

			foreach (var m in Existing)
				Assert.IsTrue(arr.Contains(m));
		}

		[TestMethod]
		public void ModelTrackerCollection_ExistingCollection_ClearCollection_CollectionEmpty() {
			Existing.Clear();

			Assert.IsTrue(Existing.Count == 0);
		}

		[TestMethod]
		public void ModelTrackerCollection_ExistingCollection_DisposeCollection_CollectionDisposed() {
			Existing.Dispose();

			Assert.IsTrue(Existing.Disposed == true);
		}

		[TestMethod]
		public void ModelTrackerCollection_ExistingCollection_GetEnumerator_ReturnsEnumerator() {
			Assert.IsNotNull(Existing.GetEnumerator());
		}

		[TestMethod]
		public void ModelTrackerCollection_DisposedCollection_AddModel_ThrowsException() {
			bool exc = false;
			try {
				Disposed.Add(new TestModel());
			} catch {
				exc = true;
			}

			Assert.IsTrue(exc);
		}

		[TestMethod]
		public void ModelTrackerCollection_DisposedCollection_RemoveModel_ThrowsException() {
			bool exc = false;
			try {
				Disposed.Remove(0);
			} catch {
				exc = true;
			}

			Assert.IsTrue(exc);
		}

		[TestMethod]
		public void ModelTrackerCollection_DisposedCollection_Contains_ThrowsException() {
			bool exc = true;
			try {
				Disposed.Contains(0);
				exc = false;
			} catch { }

			try {
				Disposed.Contains(new TestModel());
				exc = false;
			} catch { }

			Assert.IsTrue(exc);
		}

		[TestMethod]
		public void ModelTrackerCollection_DisposedCollection_Clear_ThrowsException() {
			bool exc = false;
			try {
				Disposed.Clear();
			} catch {
				exc = true;
			}

			Assert.IsTrue(exc);
		}

		[TestMethod]
		public void ModelTrackerCollection_DisposedCollection_CopyTo_ThrowsException() {
			bool exc = false;
			try {
				Disposed.CopyTo(new TestModel[0], 0);
			} catch {
				exc = true;
			}

			Assert.IsTrue(exc);
		}

		[TestMethod]
		public void ModelTrackerCollection_DisposedCollection_GetEnumerator_ThrowsException() {
			bool exc = false;
			try {
				Disposed.GetEnumerator();
			} catch {
				exc = true;
			}

			Assert.IsTrue(exc);
		}

		[TestMethod]
		public void ModelTrackerCollection_DisposedCollection_GetCount_ThrowsException() {
			bool exc = false;
			try {
				int ct = Disposed.Count;
			} catch {
				exc = true;
			}

			Assert.IsTrue(exc);
		}

		[TestMethod]
		public void ModelTrackerCollection_DisposedCollection_Dispose_ThrowsException() {
			bool exc = false;
			try {
				Disposed.Dispose();
			} catch {
				exc = true;
			}

			Assert.IsTrue(exc);
		}
	}
}
