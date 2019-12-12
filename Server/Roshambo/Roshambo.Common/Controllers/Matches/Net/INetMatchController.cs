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
    public interface INetMatchController : IMatchController
    {
		void HandleMatchStartedEvent(IncomingMessage<MatchStartedEventModel> message);
		void HandleMatchEndedEvent(IncomingMessage<MatchEndedEventModel> message);
		void HandleUserJoinedEvent(IncomingMessage<UserJoinedEventModel> message);
		void HandleUserLeftEvent(IncomingMessage<UserLeftEventModel> message);
		void HandleRoundCreatedEvent(IncomingMessage<RoundCreatedEventModel> message);
		void HandleRoundEndedEvent(IncomingMessage<RoundEndedEventModel> message);

		void HandleStartMatchRequest(IncomingMessage<StartMatchRequest> message);
		void HandleEndMatchRequest(IncomingMessage<EndMatchRequest> message);
		void HandlePlayCardRequest(IncomingMessage<PlayCardRequest> message);
		void HandlePlayCardResponse(IncomingMessage<PlayCardResponse> message);

	}
}
