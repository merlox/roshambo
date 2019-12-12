using Chip.Net.Controllers;
using Chip.Net.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Chip.Net.Services.Ping {
	public class PingService : NetService {
		public int PingSendDelay { get; set; } = 1000;

		private readonly string TimerKey = "pingtimer";
		private readonly string LatencyKey = "pinglatency";

		private HashSet<NetUser> connectedUsers;

		private MessageChannel<P_Ping> ChPing;

		public override void InitializeService(INetContext context) {
			base.InitializeService(context);
			connectedUsers = new HashSet<NetUser>();
			context.Packets.Register<P_Ping>();

			ChPing = Router.Route<P_Ping>();
		}

		public override void StartService() {
			base.StartService();
			connectedUsers.Clear();

			if(IsServer) {
				Server.NetUserConnected += OnUserConnected;
				Server.NetUserDisconnected += OnUserDisconnected;
			}
		}

		private void OnPingReceived(IncomingMessage<P_Ping> obj) {
			if (IsServer) {
				var timer = GetTimer(obj.Sender);
				obj.Sender.SetLocal<double>(LatencyKey, timer.Elapsed.TotalSeconds);
				timer.Stop();
				timer.Reset();

				var user = obj.Sender;
				ScheduleEvent(new TimeSpan(0, 0, 0, 0, PingSendDelay), () => {
					P_Ping ping = new P_Ping();
					ChPing.Send(new OutgoingMessage<P_Ping>(ping, user));
					timer.Start();
				});
			}

			if (IsClient) {
				ChPing.Send(new OutgoingMessage<P_Ping>(obj.Data));
			}
		}

		private void OnUserDisconnected(object sender, NetEventArgs args) {
			connectedUsers.Remove(args.User);
		}

		public double GetPing(NetUser user) {
			return user.GetLocal<double>(LatencyKey);
		}

		private void OnUserConnected(object sender, NetEventArgs args) {
			var timer = GetTimer(args.User);
			P_Ping ping = new P_Ping();
			ChPing.Send(new OutgoingMessage<P_Ping>(ping, args.User));
			timer.Start();

			connectedUsers.Add(args.User);
		}

		private Stopwatch GetTimer(NetUser user) {
			if (user.GetLocal<Stopwatch>(TimerKey) == null)
				user.SetLocal<Stopwatch>(TimerKey, new Stopwatch());

			return user.GetLocal<Stopwatch>(TimerKey);
		}

		public override void StopService() {
			base.StopService();
		}
	}
}