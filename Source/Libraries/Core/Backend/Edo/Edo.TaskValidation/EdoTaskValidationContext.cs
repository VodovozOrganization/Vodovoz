using System;
using System.Collections.Generic;

namespace Edo.TaskValidation
{
	public class EdoTaskValidationContext : IServiceProvider
	{
		private readonly Dictionary<Type, object> _services = new Dictionary<Type, object>();
		private IServiceProvider _serviceProvider;

		public void AddService<T>(T service)
		{
			if(_services.ContainsKey(typeof(T)))
			{
				_services[typeof(T)] = service;
			}
			else
			{
				_services.Add(typeof(T), service);
			}
		}

		internal void AddServiceProvider(IServiceProvider serviceProvider)
		{
			_serviceProvider = serviceProvider;
		}

		public object GetService(Type serviceType)
		{
			if(_services.ContainsKey(serviceType))
			{
				return _services[serviceType];
			}
			else
			{
				return _serviceProvider.GetService(serviceType);
			}
		}
	}
}
