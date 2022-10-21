using System;
using System.Collections.ObjectModel;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace VodovozSalesReceiptsService
{
	public class SalesReceiptsServiceBehavior : IServiceBehavior
	{
		private readonly SalesReceiptsInstanceProvider salesReceiptsInstanceProvider;

		public SalesReceiptsServiceBehavior(SalesReceiptsInstanceProvider salesReceiptsInstanceProvider)
		{
			this.salesReceiptsInstanceProvider = salesReceiptsInstanceProvider ?? throw new ArgumentNullException(nameof(salesReceiptsInstanceProvider));
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
						ed.DispatchRuntime.InstanceProvider = salesReceiptsInstanceProvider;
					}
				}
			}
		}

		public void Validate(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
		{
		}
	}
}
