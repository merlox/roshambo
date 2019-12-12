using Chip.Net.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chip.Net.Providers
{
	public class ProviderUserEventArgs {
		public object UserKey { get; set; }
	}

	public class ProviderDataEventArgs {
		public ProviderDataEventArgs(object userKey, bool receiving, DataBuffer data, int byteCount = 0) {
			this.Receiving = receiving;
			this.ByteCount = byteCount;
			this.UserKey = userKey;
			this.Data = data;

			if (byteCount == 0)
				byteCount = (short)this.Data.GetLength();
		}

		public object UserKey { get; }
		public bool Receiving { get; }

		public int ByteCount { get; }
		public DataBuffer Data { get; }
	}
}
