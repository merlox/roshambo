using Roshambo.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roshambo.Common.Controllers.Players
{
    public class PlayerController
    {
        public EventHandler<PlayerController> PlayerTurnStartedEvent { get; set; }
		public EventHandler<PlayerController> PlayerCardSelectedEvent { get; set; }
        public EventHandler<PlayerController> PlayerTurnEndedEvent { get; set; }

		public void SelectCard(CardType cardType) {

		}
    }
}
