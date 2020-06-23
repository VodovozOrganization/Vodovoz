using System;
using System.Collections.ObjectModel;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace VodovozEmailService
{
	public class EmailServiceBehavior : IServiceBehavior
	{
		private readonly EmailInstanceProvider emailInstanceProvider;

		public EmailServiceBehavior(EmailInstanceProvider emailInstanceProvider)
		{
			this.emailInstanceProvider = emailInstanceProvider ?? throw new ArgumentNullException(nameof(emailInstanceProvider));
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
						ed.DispatchRuntime.InstanceProvider = emailInstanceProvider;
					}
				}
			}
		}

		public void Validate(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
		{
		}
	}
}
