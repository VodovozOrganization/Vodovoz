using MassTransit;
using Pacs.Core.Messages.Events;

namespace Pacs.Operator.Client
{
	public static class TransportConfiguration
	{
		/// <summary>
		/// Конфигурирует настройки сообщений для оператора
		/// </summary>
		public static void ConfigureOperatorMessageTopology(this IRabbitMqBusFactoryConfigurator configurator, IBusRegistrationContext context)
		{
			configurator.Message<OperatorStateEvent>(x => x.SetEntityName("pacs.operator_state_event"));
		}
	}
}
