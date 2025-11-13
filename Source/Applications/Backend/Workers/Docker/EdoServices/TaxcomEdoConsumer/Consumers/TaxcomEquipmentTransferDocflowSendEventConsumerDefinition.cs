using MassTransit;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using System;
using Edo.Contracts.Messages.Events;
using TaxcomEdoConsumer.Options;

namespace TaxcomEdoConsumer.Consumers
{
	public class TaxcomEquipmentTransferDocflowSendEventConsumerDefinition : ConsumerDefinition<TaxcomEquipmentTransferDocflowSendEventConsumer>
	{
		private readonly TaxcomEdoConsumerOptions _taxcomEdoConsumerOptions;
		public TaxcomEquipmentTransferDocflowSendEventConsumerDefinition(IOptions<TaxcomEdoConsumerOptions> taxcomEdoConsumerOptions)
		{
			_taxcomEdoConsumerOptions =
				(taxcomEdoConsumerOptions ?? throw new ArgumentNullException(nameof(taxcomEdoConsumerOptions)))
				.Value;

			Endpoint(x =>
			{
				x.Name = $"{TaxcomDocflowEquipmentTransferSendEvent.Event}.consumer_{_taxcomEdoConsumerOptions.EdoAccount}";
			});
		}
		protected override void ConfigureConsumer(
			IReceiveEndpointConfigurator endpointConfigurator,
			IConsumerConfigurator<TaxcomEquipmentTransferDocflowSendEventConsumer> consumerConfigurator)
		{
			endpointConfigurator.ConfigureConsumeTopology = false;

			if(endpointConfigurator is IRabbitMqReceiveEndpointConfigurator rmq)
			{
				rmq.Durable = true;
				rmq.AutoDelete = false;
				rmq.ExchangeType = ExchangeType.Fanout;

				rmq.Bind<TaxcomDocflowEquipmentTransferSendEvent>(c =>
				{
					c.RoutingKey = _taxcomEdoConsumerOptions.EdoAccount;
				});
			}
		}
	}
}
