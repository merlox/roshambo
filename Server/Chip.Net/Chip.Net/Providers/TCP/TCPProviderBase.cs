using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace Chip.Net.Providers.TCP {
	public class TCPProviderBase {
		protected byte[] Compress(byte[] data) {
			return data;

			MemoryStream output = new MemoryStream();
			using (DeflateStream dstream = new DeflateStream(output, CompressionLevel.Fastest)) {
				dstream.Write(data, 0, data.Length);
			}

			return output.ToArray();
		}

		protected byte[] Decompress(byte[] data) {
			return data;

			MemoryStream input = new MemoryStream(data);
			MemoryStream output = new MemoryStream();
			using (DeflateStream dstream = new DeflateStream(input, CompressionMode.Decompress)) {
				dstream.CopyTo(output);
			}

			return output.ToArray();
		}
	}
}