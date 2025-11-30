using EmailSend.Library.Consumers;
using EmailSend.Library.Factories;
using EmailSend.Library.Handlers;
using EmailSend.Library.Services;
using Mailganer.Api.Client;
using MassTransit;
using MessageTransport;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.EmailSending.Masstransit;
using RabbitMQ.MailSending;

namespace EmailSend.Library
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddEmailSendLibrary(this IServiceCollection services, IConfiguration configuration)
		{
			services
				.AddHttpClient()
				.AddMailganerApiClient();

			services
				.AddTransient<IEmailSendService, EmailSendService>()
				.AddTransient<IEmailMessageFactory, EmailMessageFactory>()
				.AddTransient<SendEmailMessageHandler>();

			services
				.AddMassTransit(busConf =>
				{
					var transportSettings = new ConfigTransportSettings();
					configuration.Bind("MessageBroker", transportSettings);

					busConf.AddConsumer<AuthorizationCodesEmailSendConsumer, AuthorizationCodesEmailSendConsumerDefinition>();
					busConf.AddConsumer<SendEmailMessageConsumer, SendEmailMessageConsumerDefinition>();
					busConf.ConfigureRabbitMq((rabbitMq, context) =>
					{
						rabbitMq.AddSendEmailMessageTopology(context);
						rabbitMq.AddSendAuthorizationCodesByEmailTopology(context);
						rabbitMq.AddUpdateEmailStatusTopology(context);
					},
					transportSettings);
				})
				.AddMassTransit<IEmailSendBus>(busConf =>
				{
					busConf.AddConsumer<SendEmailMessageConsumer, SendEmailMessageConsumerDefinition>();
					busConf.ConfigureRabbitMq((rabbitMq, context) =>
					{
						rabbitMq.AddSendEmailMessageTopology(context);
					});
				});

			return services;
		}
	}
}
