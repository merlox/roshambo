using Roshambo.Common.Controllers.Rounds;
using Roshambo.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roshambo.Common.Controllers.Matches
{
    public interface IMatchController
    {
		EventHandler<IMatchController> MatchStartedEvent { get; set; }
		EventHandler<(IMatchController Match, IRoundController Round)> RoundCreatedEvent { get; set; }
		EventHandler<(IMatchController Match, PlayerState User)> UserJoinedEvent { get; set; }
		EventHandler<(IMatchController Match, PlayerState User)> UserLeftEvent { get; set; }
		EventHandler<(IMatchController Match, PlayerState Winner)> MatchEndedEvent { get; set; }

		IRoundController CurrentRound { get; }
		MatchOptions Options { get; }
		MatchState State { get; }
		MatchId Id { get; }
	}
}
