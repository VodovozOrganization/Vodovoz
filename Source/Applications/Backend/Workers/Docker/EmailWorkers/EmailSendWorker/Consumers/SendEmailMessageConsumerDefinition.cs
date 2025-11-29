using MassTransit;

namespace EmailSendWorker.Consumers
{
	public class SendEmailMessageConsumerDefinition : ConsumerDefinition<SendEmailMessageConsumer>
	{
		public SendEmailMessageConsumerDefinition()
		{
			Endpoint(x => x.Name = "email.send_message.consumer");
		}

		protected override void ConfigureConsumer(
			IReceiveEndpointConfigurator endpointConfigurator,
			IConsumerConfigurator<SendEmailMessageConsumer> consumerConfigurator)
		{
			endpointConfigurator.UseMessageRetry(r => r.Interval(5, 2500));
		}
	}
}
