using Chip.Net.Controllers.Distributed.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chip.Net.Controllers.Distributed.Services.Regions
{
	public partial class RegionService<TRegion, TFocus> : DistributedService
		where TRegion : IRegionModel
		where TFocus : IDistributedModel {
	}
}
