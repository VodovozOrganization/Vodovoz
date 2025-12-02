using MassTransit;

namespace EmailSend.Library.Consumers
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
		}
	}
}
