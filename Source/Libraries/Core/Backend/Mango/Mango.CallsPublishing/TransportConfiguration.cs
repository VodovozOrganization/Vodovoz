using Mango.Core.Dto;
using MassTransit;
using RabbitMQ.Client;

namespace Mango.CallsPublishing
{
	public static class TransportConfiguration
    {
		/// <summary>
		/// Конфигурирует настройки для сообщений Манго
		/// </summary>
		public static void AddMangoTopology(this IRabbitMqBusFactoryConfigurator configurator, IBusRegistrationContext context)
		{
			configurator.Message<MangoCallEvent>(x => x.SetEntityName("mango.event.call.publish"));
			configurator.Send<MangoCallEvent>(x => x.UseRoutingKeyFormatter(ctx => $"acdgroup-{ctx.Message.To.AcdGroup}."));
			configurator.Publish<MangoCallEvent>(x =>
			{
				x.ExchangeType = ExchangeType.Topic;
				x.Durable = true;
				x.AutoDelete = false;
			});

			configurator.Message<MangoSummaryEvent>(x => x.SetEntityName("mango.event.summary.publish"));
			configurator.Publish<MangoSummaryEvent>(x =>
			{
				x.ExchangeType = ExchangeType.Fanout;
				x.Durable = true;
				x.AutoDelete = false;
			});
		}
	}
}
