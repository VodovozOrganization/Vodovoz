﻿using Edo.Contracts.Messages.Events;
using MassTransit;
using RabbitMQ.Client;

namespace Edo.Receipt.Dispatcher.Consumers.Definitions
{
	public class TransferCompleteConsumerDefinition : ConsumerDefinition<TransferCompleteConsumer>
	{
		public TransferCompleteConsumerDefinition()
		{
			Endpoint(x => x.Name = "edo.transfer-complete.consumer.receipt-dispatcher");
		}

		protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator,
			IConsumerConfigurator<TransferCompleteConsumer> consumerConfigurator)
		{
			endpointConfigurator.ConfigureConsumeTopology = false;

			if(endpointConfigurator is IRabbitMqReceiveEndpointConfigurator rmq)
			{
				rmq.AutoDelete = true;
				rmq.ExchangeType = ExchangeType.Fanout;

				rmq.Bind<TransferCompleteEvent>();
			}
		}
	}
}
