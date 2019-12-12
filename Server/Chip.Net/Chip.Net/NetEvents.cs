using Chip.Net.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chip.Net
{
	public class NetEventArgs
	{
		public NetUser User { get; set; }
		public Packet Packet { get; set; }
	}
}
