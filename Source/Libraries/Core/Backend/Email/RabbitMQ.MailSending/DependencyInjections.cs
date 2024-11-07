﻿using System;
using System.Net.Security;
using System.Security.Authentication;
using CustomerAppsApi.Library.Configs;
using Mailjet.Api.Abstractions;
using MassTransit;
using MessageTransport;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
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
			services.AddRabbitConfig(config)
				.AddSingleton<IMessageTransportSettings>(sp =>
				{
					var configuration = sp.GetRequiredService<IConfiguration>();
					var transportSettings = new ConfigTransportSettings();
					configuration.Bind("MessageBroker", transportSettings);

					return transportSettings;
				});
			
			return services;
		}
		
		public static IBusRegistrationConfigurator ConfigureRabbitMq(this IBusRegistrationConfigurator busConf)
		{
			busConf.UsingRabbitMq((context, configurator) =>
			{
				var messageSettings = context.GetRequiredService<IMessageTransportSettings>();
				var rabbitOptions = context.GetRequiredService<IOptions<RabbitOptions>>().Value;

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
				
				configurator.Message<SendEmailMessage>(x => x.SetEntityName(rabbitOptions.AuthorizationCodesExchange));
				configurator.Publish<SendEmailMessage>(x =>
				{
					x.ExchangeType = "fanout";
					x.Durable = true;
					x.AutoDelete = false;
					x.BindQueue(
						rabbitOptions.AuthorizationCodesExchange,
						rabbitOptions.AuthorizationCodesQueue,
						conf =>
						{
							conf.ExchangeType = "fanout";
						});
				});

				configurator.Publish<EmailMessage>(x => x.Exclude = true);
				
				configurator.ConfigureEndpoints(context);
			});
			
			return busConf;
		}
	}
}
