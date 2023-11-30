using Mango.Core.Dto;
using MassTransit;
using RabbitMQ.Client;

namespace Mango.CallsPublishing
{
	public static class TransportConfiguration
    {
		/// <summary>
		/// Конфигурирует общие настройки для сообщений
		/// </summary>
		public static void AddMangoBaseTopology(this IRabbitMqBusFactoryConfigurator configurator, IBusRegistrationContext context)
		{
			configurator.Message<MangoCallEvent>(x => x.SetEntityName("mango.event.call.publish"));

			configurator.Publish<MangoCallEvent>(x =>
			{
				x.ExchangeType = ExchangeType.Fanout;
				x.Durable = true;
				x.AutoDelete = false;
			});
		}

		/// <summary>
		/// Конфигурирует настройки отправки сообщений
		/// </summary>
		public static void AddMangoProducerTopology(this IRabbitMqBusFactoryConfigurator configurator, IBusRegistrationContext context)
		{
			configurator.AddMangoBaseTopology(context);
		}
	}


}
