using CustomerAppsApi.Library.Configs;
using MassTransit;
using MessageTransport;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using System;
using System.Net.Security;
using System.Security.Authentication;
using Vodovoz.Settings.Pacs;

namespace RabbitMQ.MailSending
{
	public static class DependencyInjections
	{
		public static IServiceCollection AddRabbitConfig(this IServiceCollection services, IConfiguration config)
		{
			services.Configure<RabbitOptions>(config.GetSection(RabbitOptions.Path));

			return services;
		}

		public static IServiceCollection AddConfig(this IServiceCollection services, IConfiguration config)
		{
			services.AddRabbitConfig(config);
			services.AddSingleton<IMessageTransportSettings>(sp =>
				{
					var configuration = sp.GetRequiredService<IConfiguration>();
					var transportSettings = new ConfigTransportSettings();
					configuration.Bind("MessageBroker", transportSettings);

					return transportSettings;
				});

			return services;
		}

		public static IBusRegistrationConfigurator ConfigureRabbitMq(
			this IBusRegistrationConfigurator busConf,
			Action<IRabbitMqBusFactoryConfigurator, IBusRegistrationContext> rabbitMqConfigurator = null,
			IMessageTransportSettings messageSettings = null)
		{
			busConf.UsingRabbitMq((context, configurator) =>
			{
				if(messageSettings is null)
				{
					messageSettings = context.GetRequiredService<IMessageTransportSettings>();
				}

				configurator.Host(
					messageSettings.Host,
					(ushort)messageSettings.Port,
					messageSettings.VirtualHost, hostConfigurator =>
					{
						hostConfigurator.Username(messageSettings.Username);
						hostConfigurator.Password(messageSettings.Password);

						if(messageSettings.UseSSL)
						{
							hostConfigurator.UseSsl(ssl =>
							{
								if(Enum.TryParse<SslPolicyErrors>(messageSettings.AllowSslPolicyErrors, out var allowedPolicyErrors))
								{
									ssl.AllowPolicyErrors(allowedPolicyErrors);
								}

								ssl.Protocol = SslProtocols.Tls12;
							});
						}
					});

				rabbitMqConfigurator?.Invoke(configurator, context);
				configurator.ConfigureEndpoints(context);
			});

			return busConf;
		}

		public static void AddSendAuthorizationCodesByEmailTopology(this IRabbitMqBusFactoryConfigurator configurator, IBusRegistrationContext context)
		{
			configurator.Publish<AuthorizationCodesSendEmailMessage>(x =>
			{
				x.ExchangeType = ExchangeType.Fanout;
				x.Durable = true;
				x.AutoDelete = false;
			});
		}

		public static void AddSendEmailMessageTopology(this IRabbitMqBusFactoryConfigurator configurator, IBusRegistrationContext context)
		{
			configurator.Publish<SendEmailMessage>(x =>
			{
				x.ExchangeType = ExchangeType.Fanout;
				x.Durable = true;
				x.AutoDelete = false;
			});
		}

		public static void AddUpdateEmailStatusTopology(this IRabbitMqBusFactoryConfigurator configurator, IBusRegistrationContext context)
		{
			configurator.Publish<UpdateStoredEmailStatusMessage>(x =>
			{
				x.ExchangeType = ExchangeType.Fanout;
				x.Durable = true;
				x.AutoDelete = false;
			});
		}
	}
}
