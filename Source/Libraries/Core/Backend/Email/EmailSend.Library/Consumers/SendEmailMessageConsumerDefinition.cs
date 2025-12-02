using MassTransit;

namespace EmailSend.Library.Consumers
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
		}
	}
}
