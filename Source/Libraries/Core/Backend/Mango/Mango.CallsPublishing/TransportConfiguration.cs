using Mango.Core.Dto;
using MassTransit;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mango.CallsPublishing
{
    public static class TransportConfiguration
    {
		/// <summary>
		/// Конфигурирует общие настройки для сообщений
		/// </summary>
		public static void ConfigureMangoMessageTopology(this IRabbitMqBusFactoryConfigurator configurator, IBusRegistrationContext context)
		{
			configurator.Message<CallEvent>(x => x.SetEntityName("mango.call_event"));
		}

		/// <summary>
		/// Конфигурирует настройки отправки сообщений
		/// </summary>
		public static void ConfigureMangoProducerTopology(this IRabbitMqBusFactoryConfigurator configurator, IBusRegistrationContext context)
		{
			configurator.ConfigureMangoMessageTopology(context);

			configurator.Publish<CallEvent>(x =>
			{
				x.ExchangeType = ExchangeType.Fanout;
				x.Durable = true;
				x.AutoDelete = false;
			});
		}
	}


}
