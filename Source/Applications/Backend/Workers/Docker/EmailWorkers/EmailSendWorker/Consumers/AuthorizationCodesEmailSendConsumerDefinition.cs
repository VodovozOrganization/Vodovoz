using MassTransit;

namespace EmailSendWorker.Consumers
{
	public class AuthorizationCodesEmailSendConsumerDefinition : ConsumerDefinition<AuthorizationCodesEmailSendConsumer>
	{
		protected override void ConfigureConsumer(
			IReceiveEndpointConfigurator endpointConfigurator,
			IConsumerConfigurator<AuthorizationCodesEmailSendConsumer> consumerConfigurator)
		{
			endpointConfigurator.UseMessageRetry(r => r.Interval(5, 2500));
		}
	}
}
