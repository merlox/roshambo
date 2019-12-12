using Roshambo.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roshambo.Common.Controllers.Players
{
    public class AIController : PlayerController
    {
		private static Random random = new Random();
        public AIController() {
			this.PlayerTurnStartedEvent += OnStartAITurn;
		}

		private void OnStartAITurn(object sender, PlayerController e) {

		}

		
	}
}
