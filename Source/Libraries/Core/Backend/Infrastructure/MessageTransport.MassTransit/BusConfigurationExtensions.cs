using System;
using System.Net.Security;
using System.Security.Authentication;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MessageTransport;

namespace MessageTransport.MassTransit
{
	public static class BusConfigurationExtensions
	{
		/// <summary>
		/// Настраивает подключение bus'а к именованному RabbitMQ vhost'у по имени секции
		/// конфигурации. Регистрирует и настройки, и сам bus одним вызовом.
		/// Используйте для сервисов с одним bus'ом (воркеры-издатели, воркеры-consumer'ы).
		/// </summary>
		public static IBusRegistrationConfigurator ConfigureRabbitMq(
			this IBusRegistrationConfigurator busConf,
			IServiceCollection services,
			IConfiguration configuration,
			string sectionName)
		{
			services.AddOptions<ConfigTransportSettings>(sectionName)
				.Configure<IConfiguration>((settings, config) => config.Bind(sectionName, settings));

			busConf.UsingRabbitMq((context, configurator) =>
			{
				var settings = context.GetRequiredService<IOptionsMonitor<ConfigTransportSettings>>().Get(sectionName);

				ConfigureHost(configurator, settings);
				configurator.ConfigureEndpoints(context);
			});

			return busConf;
		}

		/// <summary>
		/// Настраивает подключение bus'а к именованному RabbitMQ vhost'у по имени секции
		/// конфигурации. Регистрирует и настройки, и сам bus для multi-bus сценария — когда в одном процессе несколько
		/// независимых bus'ов, каждый со своим маркерным интерфейсом (TBus : IBus).
		/// </summary>
		public static IBusRegistrationConfigurator<TBus> ConfigureRabbitMq<TBus>(
			this IBusRegistrationConfigurator<TBus> busConf,
			IServiceCollection services,
			IConfiguration configuration,
			string sectionName)
			where TBus : class, IBus
		{
			services.AddOptions<ConfigTransportSettings>(sectionName)
				.Configure<IConfiguration>((settings, config) => config.Bind(sectionName, settings));

			busConf.UsingRabbitMq((context, configurator) =>
			{
				var settings = context.GetRequiredService<IOptionsMonitor<ConfigTransportSettings>>().Get(sectionName);

				ConfigureHost(configurator, settings);
				configurator.ConfigureEndpoints(context);
			});

			return busConf;
		}

		private static void ConfigureHost(IRabbitMqBusFactoryConfigurator configurator, ConfigTransportSettings settings)
		{
			configurator.Host(settings.Host, (ushort)settings.Port, settings.VirtualHost, hostConfigurator =>
			{
				hostConfigurator.Username(settings.Username);
				hostConfigurator.Password(settings.Password);

				if(settings.UseSSL)
				{
					hostConfigurator.UseSsl(ssl =>
					{
						if(Enum.TryParse<SslPolicyErrors>(settings.AllowSslPolicyErrors, out var allowedPolicyErrors))
						{
							ssl.AllowPolicyErrors(allowedPolicyErrors);
						}

						ssl.Protocol = SslProtocols.Tls12;
					});
				}
			});
		}
	}
}
