using MassTransit;

namespace EmailSendWorker.Consumers
{
	public class AuthorizationCodesEmailSendConsumerDefinition : ConsumerDefinition<AuthorizationCodesEmailSendConsumer>
	{
		public AuthorizationCodesEmailSendConsumerDefinition()
		{
			Endpoint(x => x.Name = "email.send_authorization_codes_message.consumer");
		}

		protected override void ConfigureConsumer(
			IReceiveEndpointConfigurator endpointConfigurator,
			IConsumerConfigurator<AuthorizationCodesEmailSendConsumer> consumerConfigurator)
		{
			endpointConfigurator.UseMessageRetry(r => r.Interval(5, 2500));
		}
	}
}
