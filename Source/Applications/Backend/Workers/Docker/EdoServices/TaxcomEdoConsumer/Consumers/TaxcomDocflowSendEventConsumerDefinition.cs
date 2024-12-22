using System;
using Edo.Transport2;
using MassTransit;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using TaxcomEdoConsumer.Options;

namespace TaxcomEdoConsumer.Consumers
{
	public class TaxcomDocflowSendEventConsumerDefinition : ConsumerDefinition<TaxcomDocflowSendEventConsumer>
	{
		private readonly TaxcomEdoConsumerOptions _taxcomEdoConsumerOptions;

		public TaxcomDocflowSendEventConsumerDefinition(IOptions<TaxcomEdoConsumerOptions> taxcomEdoConsumerOptions)
		{
			_taxcomEdoConsumerOptions =
				(taxcomEdoConsumerOptions ?? throw new ArgumentNullException(nameof(taxcomEdoConsumerOptions)))
				.Value;
			
			Endpoint(x =>
			{
				x.Name = $"{_taxcomEdoConsumerOptions.EdoAccount}_{TaxcomDocflowSendEvent.Event}";
			});
		}
		
		protected override void ConfigureConsumer(
			IReceiveEndpointConfigurator endpointConfigurator,
			IConsumerConfigurator<TaxcomDocflowSendEventConsumer> consumerConfigurator)
		{
			endpointConfigurator.ConfigureConsumeTopology = false;

			if(endpointConfigurator is IRabbitMqReceiveEndpointConfigurator rmq)
			{
				rmq.Durable = true;
				rmq.AutoDelete = false;
				rmq.ExchangeType = ExchangeType.Fanout;

				rmq.Bind<TaxcomDocflowSendEvent>(c =>
				{
					c.RoutingKey = _taxcomEdoConsumerOptions.EdoAccount;
				});
			}
		}
	}
}
