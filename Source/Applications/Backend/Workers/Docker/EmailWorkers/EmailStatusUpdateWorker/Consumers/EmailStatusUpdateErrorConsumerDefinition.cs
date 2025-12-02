using MassTransit;

namespace EmailStatusUpdateWorker.Consumers
{
	/// <summary>
	/// Для подключения обработчика к очереди с ошибками
	/// </summary>
	public class EmailStatusUpdateErrorConsumerDefinition : ConsumerDefinition<EmailStatusUpdateConsumer>
	{
		public EmailStatusUpdateErrorConsumerDefinition()
		{
			EndpointName = "EmailStatusUpdate_error";
		}

		protected override void ConfigureConsumer(
			IReceiveEndpointConfigurator endpointConfigurator,
			IConsumerConfigurator<EmailStatusUpdateConsumer> consumerConfigurator)
		{
			endpointConfigurator.ConfigureConsumeTopology = false;
		}
	}
}
