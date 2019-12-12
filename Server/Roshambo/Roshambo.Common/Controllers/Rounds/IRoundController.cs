using Roshambo.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roshambo.Common.Controllers.Rounds
{
    public interface IRoundController
    {
		EventHandler<IRoundController> RoundEndedEvent { get; set; }
		EventHandler<IRoundController> RoundDrawEvent { get; set; }
		EventHandler<(IRoundController Round, UserId User)> PlayerWinRoundEvent { get; set; }
		EventHandler<(IRoundController Round, UserId User)> PlayerLoseRoundEvent { get; set; }

		RoundState State { get; }

		bool PlayCard(UserId user, CardType card);
	}
}
