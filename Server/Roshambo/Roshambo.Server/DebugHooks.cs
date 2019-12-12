using Roshambo.Common.Controllers.Matches;
using Roshambo.Common.Controllers.RoomList;
using Roshambo.Common.Controllers.RoomList.Net;
using Roshambo.Common.Controllers.Rooms;
using Roshambo.Common.Controllers.Rounds;
using Roshambo.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roshambo.Server
{
    public class DebugHooks
    {
		private string prefix;
		private Action<string> writeFunc;

		public DebugHooks(string prefix, IRoomListController controller, Action<string> write) {
			this.prefix = prefix;
			this.writeFunc = write;
			controller.RoomCreatedEvent += OnRoomCreated;
			controller.RoomListUpdatedEvent += OnRoomListUpdated;
			if (controller is NetServerRoomListController sv) {
				sv.Server.Router.PacketSentEvent += (s, p) => Log("send: " + p.GetType().Name + " " + p.ToString());
				sv.Server.Router.PacketReceivedEvent += (s, p) => Log("recv: " + p.GetType().Name + " " + p.ToString());
			}

			if (controller is NetClientRoomListController cl) {
				cl.Client.Router.PacketSentEvent += (s, p) => Log("send: " + p.GetType().Name + " " + p.ToString());
				cl.Client.Router.PacketReceivedEvent += (s, p) => Log("recv: " + p.GetType().Name + " " + p.ToString());
			}
		}

		#region Room List Debug Logs
		private void OnRoomListUpdated(object sender, IRoomListController e) {
			Log("{0}: Room list updated", e.ToString());
		}

		private void OnRoomCreated(object sender, IRoomController e) {
			Log("{0}: Room created", e.ToString());
			e.MatchCreatedEvent += OnMatchCreated;
			e.RoomEndedEvent += OnRoomEnded;
			e.RoomStartedEvent += OnRoomStarted;
			e.UserJoinedEvent += OnUserJoinedRoom;
			e.UserLeftEvent += OnUserLeftRoom;
		}
		#endregion

		#region Room Debug Logs
		private void OnMatchCreated(object sender, (IRoomController Room, IMatchController Match) e) {
			Log("{0} {1}: Match Created", e.Room.ToString(), e.Match.ToString());
			e.Match.MatchStartedEvent += OnMatchStarted;
			e.Match.MatchEndedEvent += OnMatchEnded;
			e.Match.RoundCreatedEvent += OnRoundCreated;
			e.Match.UserJoinedEvent += OnUserJoinedMatch;
			e.Match.UserLeftEvent += OnUserLeftMatch;
		}

		private void OnRoomStarted(object sender, IRoomController e) {
			Log("{0}: Room Started", e.ToString());
		}

		private void OnRoomEnded(object sender, IRoomController e) {
			Log("{0}: Room ended", e.ToString());
		}

		private void OnUserJoinedRoom(object sender, (IRoomController Room, UserState User) e) {
			Log("{0} {1}: User joined room", e.Room.ToString(), e.User.ToString());
		}

		private void OnUserLeftRoom(object sender, (IRoomController Room, UserState User) e) {
			Log("{0} {1}: User left room", e.Room.ToString(), e.User.ToString());
		}
		#endregion



		#region Match Debug Logs
		private void OnMatchStarted(object sender, IMatchController e) {
			Log("{0}: Match Started", e.ToString());
		}

		private void OnMatchEnded(object sender, (IMatchController Match, PlayerState Winner) e) {
			Log("{0}: Match Ended, Result: {1}", e.Match.ToString(), e.Winner == null ? "draw" : e.Winner.ToString());
		}

		private void OnRoundCreated(object sender, (IMatchController Match, IRoundController Round) e) {
			Log("{0} {1}: Round created", e.Match.ToString(), e.Round.ToString());

			e.Round.PlayerLoseRoundEvent += OnPlayerLoseRound;
			e.Round.PlayerWinRoundEvent += OnPlayerWinRound;
			e.Round.RoundDrawEvent += OnRoundDrawEvent;
			e.Round.RoundEndedEvent += OnRoundEndedEvent;
		}

		private void OnUserJoinedMatch(object sender, (IMatchController Match, PlayerState User) e) {
			Log("{0} {1}: User joined match", e.Match.ToString(), e.User.ToString());
		}

		private void OnUserLeftMatch(object sender, (IMatchController Match, PlayerState User) e) {
			Log("{0} {1}: User left match", e.Match.ToString(), e.User.ToString());
		}
		#endregion



		#region Round Debug Logs
		private void OnPlayerLoseRound(object sender, (IRoundController Round, UserId User) e) {
			Log("{0} {1}: Player loss", e.Round.ToString(), e.User.ToString());
		}

		private void OnPlayerWinRound(object sender, (IRoundController Round, UserId User) e) {
			Log("{0} {1}: Player victory", e.Round.ToString(), e.User.ToString());
		}

		private void OnRoundDrawEvent(object sender, IRoundController e) {
			Log("{0}: Round draw", e.ToString());
		}

		private void OnRoundEndedEvent(object sender, IRoundController e) {
			Log("{0}: Round ended", e.ToString());
		}
		#endregion

		private void Log(string msg) {
			writeFunc?.Invoke(DateTime.Now.ToString("hh:mm:ss.ff") +  " [" + prefix + "] " + msg);
		}

		private void Log(string msg, params object[] args) {
			Log(string.Format(msg, args));
		}
	}
}
