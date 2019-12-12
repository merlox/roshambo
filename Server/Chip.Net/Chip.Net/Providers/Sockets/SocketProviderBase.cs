using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace Chip.Net.Providers.Sockets {
	public class StateObject {
		// Client  socket.  
		public Socket socket = null;
		// Size of receive buffer.  
		public const int BufferSize = 1024;
		// Receive buffer.  
		public byte[] buffer = new byte[BufferSize];
	}

	public class SocketProviderBase
    {
    }
}
