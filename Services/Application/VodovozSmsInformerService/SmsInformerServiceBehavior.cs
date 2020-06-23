using System;
using System.Collections.ObjectModel;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace VodovozSmsInformerService
{
	public class SmsInformerServiceBehavior : IServiceBehavior
	{
		private readonly SmsInformerInstanceProvider smsInformerInstanceProvider;

		public SmsInformerServiceBehavior(SmsInformerInstanceProvider smsInformerInstanceProvider)
		{
			this.smsInformerInstanceProvider = smsInformerInstanceProvider ?? throw new ArgumentNullException(nameof(smsInformerInstanceProvider));
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
						ed.DispatchRuntime.InstanceProvider = smsInformerInstanceProvider;
					}
				}
			}
		}

		public void Validate(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
		{
		}
	}
}
