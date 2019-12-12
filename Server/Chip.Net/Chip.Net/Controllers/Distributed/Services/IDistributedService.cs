using Chip.Net.Controllers.Distributed.Models;
using Chip.Net.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chip.Net.Controllers.Distributed.Services {
	public interface IDistributedService : INetService {
		bool IsShard { get; set; }
		bool IsUser { get; set; }
		bool IsRouter { get; set; }

		RouterServer RouterController { get; set; }
		ShardClient ShardController { get; set; }
		UserClient UserController { get; set; }

		void InitializeShard();
		void InitializeRouter();
		void InitializeUser();
	}
}
