﻿using Edo.Contracts.Messages.Events;
using MassTransit;
using RabbitMQ.Client;
using Vodovoz.Core.Domain.Edo;

namespace Edo.Tender.Consumers.Definitions
{
	public class TransferCompleteConsumerDefinition : ConsumerDefinition<TransferCompleteConsumer>
	{
		public TransferCompleteConsumerDefinition()
		{
			Endpoint(x => x.Name = "edo.transfer-complete.consumer.tender");
		}

		protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator,
			IConsumerConfigurator<TransferCompleteConsumer> consumerConfigurator)
		{
			endpointConfigurator.ConfigureConsumeTopology = false;

			if(endpointConfigurator is IRabbitMqReceiveEndpointConfigurator rmq)
			{
				rmq.ExchangeType = ExchangeType.Fanout;

				rmq.Bind<TransferCompleteEvent>(x => { x.RoutingKey = TransferInitiator.Tender.ToString(); });
			}
		}
	}
}
