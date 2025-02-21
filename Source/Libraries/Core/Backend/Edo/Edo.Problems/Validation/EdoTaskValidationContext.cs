using System;
using System.Collections.Generic;

namespace Edo.Problems.Validation
{
	public class EdoTaskValidationContext : IServiceProvider
	{
		private readonly Dictionary<Type, object> _services = new Dictionary<Type, object>();
		private IServiceProvider _serviceProvider;

		public void AddService(object service)
		{
			var type = service.GetType();
			if(_services.ContainsKey(type))
			{
				_services[type] = service;
			}
			else
			{
				_services.Add(type, service);
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
