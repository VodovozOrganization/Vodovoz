using MassTransit;
using Pacs.Core.Messages.Events;
using Vodovoz.Core.Domain.Pacs;
using Pacs.Core;

namespace Pacs.Operators.Client
{
	public static class TransportConfiguration
	{
		/// <summary>
		/// Конфигурирует настройки сообщений для оператора
		/// </summary>
		public static void ConfigureOperatorMessageTopology(this IRabbitMqBusFactoryConfigurator configurator, IBusRegistrationContext context)
		{
			configurator.AddPacsBaseTopology(context);
		}
	}
}
