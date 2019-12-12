using System;
using System.Collections.Generic;
using System.Text;

namespace Chip.Net.Providers.Direct
{
	public class ConnectionMap
	{
		public static Dictionary<string, DirectServerProvider> Servers { get; private set; }

		public static void AddServer(string key, DirectServerProvider server)
		{
			if (Servers.ContainsKey(key))
				throw new Exception("Server already exists with the given key.");

			Servers[key] = server;
		}

		public static void RemoveServer(string key)
		{
			if (Servers.ContainsKey(key) == false)
				throw new Exception("Server does not exist with the given key.");

			Servers.Remove(key);
		}

		public static string GetKey(string ip, int port)
		{
			return ip + port.ToString();
		}
	}
}
