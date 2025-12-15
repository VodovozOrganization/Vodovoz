using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using System;
using System.Linq;
using Vodovoz.Core.Data.Repositories;

namespace EmailSend.Library.Consumers
{
	public class SendEmailMessageConsumerDefinition : ConsumerDefinition<SendEmailMessageConsumer>
	{
		private readonly IServiceProvider _serviceProvider;

		public SendEmailMessageConsumerDefinition(IServiceProvider serviceProvider)
		{
			_serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
			Endpoint(x => x.Name = "email.send_message.consumer");
		}

		protected override void ConfigureConsumer(
			IReceiveEndpointConfigurator endpointConfigurator,
			IConsumerConfigurator<SendEmailMessageConsumer> consumerConfigurator)
		{
			endpointConfigurator.ConfigureConsumeTopology = false;

			endpointConfigurator.UseRateLimit(10, TimeSpan.FromMinutes(1));

			using(var scope = _serviceProvider.CreateScope())
			{
				var repository = scope.ServiceProvider.GetRequiredService<IOrganizationRepository>();
				var organizationEmails = repository.GetEmailsForMailing().ToList();

				if(endpointConfigurator is IRabbitMqReceiveEndpointConfigurator rmq)
				{
					rmq.Durable = true;
					rmq.AutoDelete = false;
					rmq.ExchangeType = ExchangeType.Fanout;

					foreach(var email in organizationEmails)
					{
						var queueName = $"email.send_message.{email.Replace("@", "_at_").Replace(".", "_")}";
						rmq.Bind(queueName, x =>
						{
							x.ExchangeType = ExchangeType.Direct;
							x.RoutingKey = email;
						});
					}
				}
			}
		}
	}
}
