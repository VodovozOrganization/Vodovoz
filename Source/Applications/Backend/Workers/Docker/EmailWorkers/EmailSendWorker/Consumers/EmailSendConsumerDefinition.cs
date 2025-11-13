using System;
using CustomerAppsApi.Library.Configs;
using MassTransit;
using Microsoft.Extensions.Options;

namespace EmailSendWorker.Consumers
{
	public class EmailSendConsumerDefinition : ConsumerDefinition<EmailSendConsumer>
	{
		public EmailSendConsumerDefinition(IOptions<RabbitOptions> rabbitOptions)
		{
			EndpointName =
				(rabbitOptions ?? throw new ArgumentNullException(nameof(rabbitOptions)))
					.Value
					.AuthorizationCodesQueue;
		}

		protected override void ConfigureConsumer(
			IReceiveEndpointConfigurator endpointConfigurator,
			IConsumerConfigurator<EmailSendConsumer> consumerConfigurator)
		{
			endpointConfigurator.ConfigureConsumeTopology = false;
			endpointConfigurator.UseMessageRetry(r => r.Interval(5, 2500));
		}
	}
}
