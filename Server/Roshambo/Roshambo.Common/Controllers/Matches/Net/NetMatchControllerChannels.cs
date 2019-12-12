using Chip.Net.Data;
using Roshambo.Common.Controllers.Matches.Net.Models;
using Roshambo.Common.Controllers.Matches.Net.Models.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roshambo.Common.Controllers.Matches.Net
{
    public class NetMatchControllerChannels
    {
		public MessageChannel<MatchEndedEventModel> MatchEndedEventChannel { get; private set; }
		public MessageChannel<MatchStartedEventModel> MatchStartedEventChannel { get; private set; }

		public MessageChannel<RoundCreatedEventModel> RoundCreatedEventChannel { get; private set; }
		public MessageChannel<RoundEndedEventModel> RoundEndedEventChannel { get; private set; }

		public MessageChannel<UserJoinedEventModel> UserJoinedEventChannel { get; private set; }
		public MessageChannel<UserLeftEventModel> UserLeftEventChannel { get; private set; }

		public MessageChannel<StartMatchRequest> StartMatchRequestChannel { get; private set; }
		public MessageChannel<EndMatchRequest> EndMatchRequestChannel { get; private set; }

		public MessageChannel<PlayCardRequest> PlayCardRequestChannel { get; private set; }
		public MessageChannel<PlayCardResponse> PlayCardResponseChannel { get; private set; }

		public NetMatchControllerChannels(PacketRouter router) {
			this.MatchEndedEventChannel = router.Route<MatchEndedEventModel>();
			this.MatchStartedEventChannel = router.Route<MatchStartedEventModel>();

			this.RoundCreatedEventChannel = router.Route<RoundCreatedEventModel>();
			this.RoundEndedEventChannel = router.Route<RoundEndedEventModel>();

			this.UserJoinedEventChannel = router.Route<UserJoinedEventModel>();
			this.UserLeftEventChannel = router.Route<UserLeftEventModel>();

			this.StartMatchRequestChannel = router.Route<StartMatchRequest>();
			this.EndMatchRequestChannel = router.Route<EndMatchRequest>();

			this.PlayCardRequestChannel = router.Route<PlayCardRequest>();
			this.PlayCardResponseChannel = router.Route<PlayCardResponse>();
		}
	}
}
