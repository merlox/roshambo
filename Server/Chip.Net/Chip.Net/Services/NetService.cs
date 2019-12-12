using System;
using System.Collections.Generic;
using System.Text;
using Chip.Net.Controllers;
using Chip.Net.Data;

namespace Chip.Net.Services
{
	public class NetService : INetService {
		public PacketRouter Router { get; private set; }

		public bool IsServer { get; set; } = false;
		public bool IsClient { get; set; } = false;

		public INetServerController Server { get; set; }
		public INetClientController Client { get; set; }

		protected INetContext Context { get; private set; }

		private object LockObject = new object();
		private List<Tuple<DateTime, Action>> scheduledEvents;

		public virtual void InitializeService(INetContext context) {
			Context = context;
			scheduledEvents = new List<Tuple<DateTime, Action>>();

			if (IsClient) Router = new PacketRouter(Client.Router, GetRouterPrefix());
			if (IsServer) Router = new PacketRouter(Server.Router, GetRouterPrefix());
		}

		public virtual void StartService() {
			scheduledEvents.Clear();
		}

		public virtual void UpdateService() {
			for(int i = scheduledEvents.Count - 1; i >= 0; i--) {
				var time = scheduledEvents[i].Item1;
				var ev = scheduledEvents[i].Item2;

				if(DateTime.Now > time) {
					ev.Invoke();
					scheduledEvents.RemoveAt(i);
				}
			}
		}

		public virtual void StopService() { }

		public virtual void Dispose() {
			if (scheduledEvents != null) scheduledEvents.Clear();
			scheduledEvents = null;
		}

		public void ScheduleEvent(TimeSpan time, Action action) {
			var endt = DateTime.Now.Add(time);
			scheduledEvents.Add(new Tuple<DateTime, Action>(endt, action));
		}

		public void ScheduleEvent(int milliseconds, Action action) {
			var endt = DateTime.Now.Add(new TimeSpan(0, 0, 0, 0, milliseconds));
			scheduledEvents.Add(new Tuple<DateTime, Action>(endt, action));
		}

        protected virtual string GetRouterPrefix() {
            return GetType().Name + "___";
        }
	}
}
