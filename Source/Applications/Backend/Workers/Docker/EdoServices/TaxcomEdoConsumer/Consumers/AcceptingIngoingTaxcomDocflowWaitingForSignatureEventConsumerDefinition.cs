using MassTransit;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using System;
using Edo.Contracts.Messages.Events;
using TaxcomEdoConsumer.Options;

namespace TaxcomEdoConsumer.Consumers
{
	public class AcceptingIngoingTaxcomDocflowWaitingForSignatureEventConsumerDefinition
		: ConsumerDefinition<AcceptingIngoingTaxcomDocflowWaitingForSignatureEventConsumer>
	{
		private readonly TaxcomEdoConsumerOptions _taxcomEdoConsumerOptions;

		public AcceptingIngoingTaxcomDocflowWaitingForSignatureEventConsumerDefinition(
			IOptions<TaxcomEdoConsumerOptions> taxcomEdoConsumerOptions)
		{
			_taxcomEdoConsumerOptions =
				(taxcomEdoConsumerOptions ?? throw new ArgumentNullException(nameof(taxcomEdoConsumerOptions)))
				.Value;
			
			Endpoint(x =>
			{
				x.Name = $"{AcceptingIngoingTaxcomDocflowWaitingForSignatureEvent.Event}.consumer_{_taxcomEdoConsumerOptions.EdoAccount}";
			});
		}
		
		protected override void ConfigureConsumer(
			IReceiveEndpointConfigurator endpointConfigurator,
			IConsumerConfigurator<AcceptingIngoingTaxcomDocflowWaitingForSignatureEventConsumer> consumerConfigurator)
		{
			endpointConfigurator.ConfigureConsumeTopology = false;
			
			if(endpointConfigurator is IRabbitMqReceiveEndpointConfigurator rmq)
			{
				rmq.Durable = true;
				rmq.AutoDelete = false;
				rmq.ExchangeType = ExchangeType.Fanout;

				rmq.Bind<AcceptingIngoingTaxcomDocflowWaitingForSignatureEvent>(c =>
				{
					c.RoutingKey = _taxcomEdoConsumerOptions.EdoAccount;
				});
			}
		}
	}
}
