using MassTransit;
using RabbitMQ.Client;

namespace EmailSchedulerWorker.Consumers
{
	/// <summary>
	/// Конфигурация потребителя событий для обработки писем клиентов
	/// </summary>
	public class ProcessClientEmailConsumerDefinition : ConsumerDefinition<ProcessClientEmailConsumer>
	{
		public ProcessClientEmailConsumerDefinition()
		{
			EndpointName = "process-client-email-event-queue.email-scheduler";
		}

		protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator,
			IConsumerConfigurator<ProcessClientEmailConsumer> consumerConfigurator)
		{
			endpointConfigurator.ConfigureConsumeTopology = false;

			if(endpointConfigurator is IRabbitMqReceiveEndpointConfigurator rmq)
			{
				rmq.ExchangeType = ExchangeType.Fanout;
				rmq.Durable = true;
				rmq.PrefetchCount = 1;
				rmq.UseRateLimit(10, TimeSpan.FromMinutes(1));
				rmq.Bind<ProcessClientEmailEvent>();
			}
		}
	}
}
