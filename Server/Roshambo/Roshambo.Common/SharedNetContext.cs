using Chip.Net;
using Roshambo.Common.Controllers.Matches.Net;
using Roshambo.Common.Controllers.RoomList.Net;
using Roshambo.Common.Controllers.Rooms.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roshambo.Common
{
    public class SharedNetContext
    {
        public static NetContext CreateContext(string ipAddress, int port) {
			var ctx = new NetContext();
			ctx.IPAddress = ipAddress;
			ctx.Port = port;

			ctx.Services.Register<NetRoomListControllerService>();
			ctx.Services.Register<NetRoomControllerService>();
			ctx.Services.Register<NetMatchControllerService>();
			//ctx.Services.Register<NetRoundControllerService>();

			ctx.ApplicationName = "Roshambo";
			return ctx;
		}
    }
}
