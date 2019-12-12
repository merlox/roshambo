using Chip.Net.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Chip.Net.Services.NetTime {
	public class NetTimeService : NetService {
		private Stopwatch serverTimer;
		private Stopwatch clientTimer;
		private Stopwatch pingTimer;

		private double netTime;
		private double offset;

		public double NetTime {
			get {
				return netTime;
			}
		}

		MessageChannel<P_SetNetTime> ChSetNetTime;
		MessageChannel<P_GetNetTime> ChGetNetTime;

		public override void InitializeService(INetContext context) {
			base.InitializeService(context);
			clientTimer = new Stopwatch();
			serverTimer = new Stopwatch();
			pingTimer = new Stopwatch();

			if(IsClient)
				Client.OnConnected += OnConnected;

			context.Packets.Register<P_GetNetTime>();
			context.Packets.Register<P_SetNetTime>();

			ChSetNetTime = Router.Route<P_SetNetTime>();
			ChGetNetTime = Router.Route<P_GetNetTime>();

            ChSetNetTime.Receive += OnSetNetTime;
            ChGetNetTime.Receive += OnGetNetTime;
		}

        private void OnSetNetTime(IncomingMessage<P_SetNetTime> obj) {
			var svNetTime = obj.Data.NetTime;
			var diff = pingTimer.Elapsed.TotalSeconds;
			svNetTime += diff / 2d;

			offset = svNetTime - clientTimer.Elapsed.TotalSeconds;

			ScheduleEvent(250, () => {
				pingTimer.Reset();
				pingTimer.Start();
				P_GetNetTime msg = new P_GetNetTime();
				ChGetNetTime.Send(new OutgoingMessage<P_GetNetTime>(msg));
			});
		}

		private void OnGetNetTime(IncomingMessage<P_GetNetTime> obj) {
			P_SetNetTime reply = new P_SetNetTime();
			reply.NetTime = GetNetTime();

			ChSetNetTime.Send(new OutgoingMessage<P_SetNetTime>(reply, obj.Sender));
		}

		public override void StartService() {
			base.StartService();
			if (IsServer) {
				serverTimer.Reset();
				serverTimer.Start();
			}

			if(IsClient) {
				clientTimer.Reset();
				clientTimer.Start();
			}
		}

		private void OnConnected(object sender, NetEventArgs args) {
			P_GetNetTime msg = new P_GetNetTime();
			ChGetNetTime.Send(new OutgoingMessage<P_GetNetTime>(msg));

			pingTimer.Reset();
			pingTimer.Start();
		}

		public override void UpdateService() {
			base.UpdateService();

			if(IsServer)
				netTime = serverTimer.Elapsed.TotalSeconds;

			if (IsClient)
				netTime = clientTimer.Elapsed.TotalSeconds + offset;
		}

		public override void StopService() {
			base.StopService();
		}

		public double GetNetTime() {
			return netTime;
		}
	}
}
