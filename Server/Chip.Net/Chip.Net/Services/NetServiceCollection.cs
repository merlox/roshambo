using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Chip.Net.Services {
	public class NetServiceCollection : IDisposable {
		public IReadOnlyList<INetService> ServiceList { get; set; }
		public IReadOnlyList<Type> ServiceTypes { get; set; }

		private Dictionary<Type, Func<INetService>> activationMap;
		private Dictionary<Type, Action<object>> configMap;
		private HashSet<Type> serviceTypeSet;
		private List<INetService> services;

		private Dictionary<Type, INetService> serviceMap;
		private Dictionary<INetService, byte> serviceToId;
		private Dictionary<byte, INetService> idToService;

		public bool IsLocked { get; private set; }

		public NetServiceCollection() {
			ServiceTypes = new List<Type>().AsReadOnly();
			activationMap = new Dictionary<Type, Func<INetService>>();
			configMap = new Dictionary<Type, Action<object>>();
			serviceTypeSet = new HashSet<Type>();
			services = new List<INetService>();
			ServiceList = services.AsReadOnly();

			serviceMap = new Dictionary<Type, INetService>();
			serviceToId = new Dictionary<INetService, byte>();
			idToService = new Dictionary<byte, INetService>();

			IsLocked = false;
		}

		public void LockServices() {
			var instances = serviceTypeSet.OrderBy(i => i.FullName).Select(i =>
			{
				var svc = activationMap[i]();
				if (configMap.ContainsKey(i))
					configMap[i].Invoke(svc);

				serviceMap.Add(i, svc);
				return svc;
			});

			services = instances.ToList();
			ServiceList = services.AsReadOnly();

			for(byte i = 0; i < services.Count; i++)
			{
				serviceToId.Add(services[i], (byte)(i + 1));
				idToService.Add((byte)(i + 1), services[i]); 
			}

			IsLocked = true;
		}

		public void SetActivationFunction(Type serviceType, Func<INetService> activator) {
			activationMap[serviceType] = activator;
		}

		public byte GetServiceId(INetService svc) {
			return serviceToId[svc];
		}

		public INetService GetServiceFromId(byte id) {
			return idToService[id];
		}

		public void InitializeServices(INetContext context)
		{
			for (int i = 0; i < services.Count; i++)
				services[i].InitializeService(context);
		}

		public void StartServices() {
			for (int i = 0; i < services.Count; i++)
				services[i].StartService();
		}

		public void UpdateServices()
		{
			for (int i = 0; i < services.Count; i++)
				services[i].UpdateService();
		}

		public void StopServices()
		{
			for (int i = 0; i < services.Count; i++)
				services[i].StopService();
		}

		public void Register<T>(T inst = null) 
            where T : class, INetService {
			if (serviceTypeSet.Contains(typeof(T)) == false) {
				serviceTypeSet.Add(typeof(T));
				activationMap[typeof(T)] = () =>  inst ?? Activator.CreateInstance<T>();
				ServiceTypes = serviceTypeSet.ToList().AsReadOnly();
			}
		}

		public void Configure<T>(Action<T> config) where T : class, INetService {
			if (configMap.ContainsKey(typeof(T)))
				throw new Exception("NetService already has configuration set.");

			configMap.Add(typeof(T), (o) => config((T)o));
			Register<T>();
		}

		public T Get<T>() where T : INetService {
			if (serviceMap.ContainsKey(typeof(T)) == false)
				return default(T);

			return (T)serviceMap[typeof(T)];
		}

		public INetService Get(Type type)
		{
			if (serviceMap.ContainsKey(type) == false)
				return null;

			return serviceMap[type];
		}

		public NetServiceCollection Clone()
		{
			NetServiceCollection c = new NetServiceCollection();

			foreach (var type in serviceTypeSet)
				c.serviceTypeSet.Add(type);

			foreach (var conf in configMap)
				c.configMap.Add(conf.Key, conf.Value);

			foreach (var actv in activationMap)
				c.activationMap.Add(actv.Key, actv.Value);

			if(IsLocked)
				c.LockServices();

			return c;
		}

		public void Dispose() {
			if (serviceTypeSet  != null) serviceTypeSet.Clear();
			if (activationMap != null) activationMap.Clear();
			if (idToService != null) idToService.Clear();
			if (serviceToId != null) serviceToId.Clear();
			if (serviceMap != null) serviceMap.Clear();
			if (configMap != null) configMap.Clear();

			if (services != null)
			{
				foreach (var svc in services)
					svc.Dispose();

				services.Clear();
			}

			activationMap = null;
			serviceTypeSet = null;
			idToService = null;
			serviceToId = null;
			serviceMap = null;
			configMap = null;
			services = null;
		}
	}
}
