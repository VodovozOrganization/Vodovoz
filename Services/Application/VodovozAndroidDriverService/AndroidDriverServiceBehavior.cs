using System;
using System.Collections.ObjectModel;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace VodovozAndroidDriverService
{
	public class AndroidDriverServiceBehavior : IServiceBehavior
	{
		private readonly AndroidDriverServiceInstanceProvider androidDriverServiceInstanceProvider;

		public AndroidDriverServiceBehavior(AndroidDriverServiceInstanceProvider androidDriverServiceInstanceProvider)
		{
			this.androidDriverServiceInstanceProvider = androidDriverServiceInstanceProvider ?? throw new ArgumentNullException(nameof(androidDriverServiceInstanceProvider));
		}

		public void AddBindingParameters(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase, Collection<ServiceEndpoint> endpoints, BindingParameterCollection bindingParameters)
		{
		}

		public void ApplyDispatchBehavior(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
		{
			foreach(ChannelDispatcherBase cdb in serviceHostBase.ChannelDispatchers) {
				ChannelDispatcher cd = cdb as ChannelDispatcher;
				if(cd != null) {
					foreach(EndpointDispatcher ed in cd.Endpoints) {
						ed.DispatchRuntime.InstanceProvider = androidDriverServiceInstanceProvider;
					}
				}
			}
		}

		public void Validate(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
		{
		}
	}
}
