using System;
using System.Collections.ObjectModel;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace VodovozDeliveryRulesService
{
	public class DeliveryRulesServiceBehavior : IServiceBehavior
	{
		private readonly DeliveryRulesInstanceProvider deliveryRulesInstanceProvider;

		public DeliveryRulesServiceBehavior(DeliveryRulesInstanceProvider deliveryRulesInstanceProvider)
		{
			this.deliveryRulesInstanceProvider = deliveryRulesInstanceProvider ?? throw new ArgumentNullException(nameof(deliveryRulesInstanceProvider));
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
						ed.DispatchRuntime.InstanceProvider = deliveryRulesInstanceProvider;
					}
				}
			}
		}

		public void Validate(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
		{
		}
	}
}
