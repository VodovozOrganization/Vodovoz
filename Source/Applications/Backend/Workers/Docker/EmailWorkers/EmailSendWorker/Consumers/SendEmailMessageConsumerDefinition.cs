using MassTransit;

namespace EmailSendWorker.Consumers
{
	public class SendEmailMessageConsumerDefinition : ConsumerDefinition<SendEmailMessageConsumer>
	{
		protected override void ConfigureConsumer(
			IReceiveEndpointConfigurator endpointConfigurator,
			IConsumerConfigurator<SendEmailMessageConsumer> consumerConfigurator)
		{
			endpointConfigurator.UseMessageRetry(r => r.Interval(5, 2500));
		}
	}
}
