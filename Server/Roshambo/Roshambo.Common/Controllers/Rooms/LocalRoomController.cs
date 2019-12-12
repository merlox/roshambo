using Roshambo.Common.Controllers.Matches;
using Roshambo.Common.Controllers.Players;
using Roshambo.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roshambo.Common.Controllers.Rooms
{
    public class LocalRoomController : IRoomController
	{
		public EventHandler<IRoomController> RoomStartedEvent { get; set; }
		public EventHandler<IRoomController> RoomEndedEvent { get; set; }
		public EventHandler<(IRoomController Room, UserState User)> UserJoinedEvent { get; set; }
		public EventHandler<(IRoomController Room, UserState User)> UserLeftEvent { get; set; }
		public EventHandler<(IRoomController Room, IMatchController Match)> MatchCreatedEvent { get; set; }

		public HandState GlobalHand { get; private set; }
		public RoomState State { get; private set; }
		public List<LocalMatchController> MatchList { get; private set; }

		public string Name => this.State.Id.Name;

		public LocalRoomController(RoomId id) {
			this.State = new RoomState();
			this.State.CurrentState = RoomState.State.New;
			this.MatchList = new List<LocalMatchController>();
			this.GlobalHand = new HandState();
			this.State.Id = id;
		}

		public void AddExistingPlayer(UserState state) {
			if (this.State.CurrentState != RoomState.State.New)
				return;

			if (this.State.Users.Any(i => i.Id.GUID == state.Id.GUID))
				return;

			this.State.Users.Add(state);
			this.UserJoinedEvent?.Invoke(this, (this, state));
		}

		public UserState AddPlayer(UserId user) {
			if (this.State.CurrentState != RoomState.State.New)
				return null;

			if (this.State.Users.Any(i => i.Id.GUID == user.GUID))
				return null;

			var userState = new UserState();
			userState.UserHand.RockCount = 9;
			userState.UserHand.PaperCount = 9;
			userState.UserHand.ScissorsCount = 9;
			userState.Stars = 3;
			userState.Id = user;

			this.State.Users.Add(userState);
			this.UserJoinedEvent?.Invoke(this, (this, userState));
			return userState;
		}

		public void StartRoom() {
			if (this.State.CurrentState != RoomState.State.New)
				return;

			if (this.State.Users.Count <= 1)
				return;

			this.State.CurrentState = RoomState.State.Active;
			this.RoomStartedEvent?.Invoke(this, this);
		}

		public void CloseRoom() {
			if (this.State.CurrentState == RoomState.State.Ended)
				return;

			this.State.CurrentState = RoomState.State.Ended;
			foreach (var match in MatchList)
				match.EndMatch();
		}

		private int matchCt = 0;
		public void CreateMatch(MatchOptions options) {
			var match = new LocalMatchController(options);
			this.MatchList.Add(match);
			this.MatchCreatedEvent?.Invoke(this, (this, match));
		}

		public List<IMatchController> GetMatches() {
			return this.MatchList.Cast<IMatchController>().ToList();
		}

		public void UpdateMatches() {
			foreach (var match in MatchList)
				match.UpdateMatch();
		}
	}
}
