using System;
using System.Collections.ObjectModel;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace VodovozSmsPaymentService
{
	internal class SmsPaymentServiceBehavior : IServiceBehavior
	{
		private readonly SmsPaymentServiceInstanceProvider smsPaymentServiceInstanceProvider;

		public SmsPaymentServiceBehavior(SmsPaymentServiceInstanceProvider smsPaymentServiceInstanceProvider)
		{
			this.smsPaymentServiceInstanceProvider = smsPaymentServiceInstanceProvider ?? throw new ArgumentNullException(nameof(smsPaymentServiceInstanceProvider));
		}

		public void AddBindingParameters(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase, Collection<ServiceEndpoint> endpoints, BindingParameterCollection bindingParameters)
		{
		}

		public void ApplyDispatchBehavior(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
		{
			foreach(ChannelDispatcherBase cdb in serviceHostBase.ChannelDispatchers) {
				if(cdb is ChannelDispatcher cd) {
					foreach(EndpointDispatcher ed in cd.Endpoints) {
						ed.DispatchRuntime.InstanceProvider = smsPaymentServiceInstanceProvider;
					}
				}
			}
		}

		public void Validate(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
		{
		}
	}
}