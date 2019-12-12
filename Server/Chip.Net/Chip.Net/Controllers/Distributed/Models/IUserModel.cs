using System;
using System.Collections.Generic;
using System.Text;

namespace Chip.Net.Controllers.Distributed.Models
{
    public interface IUserModel : IDistributedModel {
		string Name { get; set; }
		string UUID { get; set; }
	}
}
