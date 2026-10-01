using CloudPaymentsApi.Client;
using CustomerOrdersApi.Library.Config;
using CustomerOrdersApi.Library.Converters;
using FastPaymentsApi.Client;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Security;
using System.Security.Authentication;
using System.Text.Json;
using System.Text.Json.Serialization;
using Vodovoz.Settings.Pacs;
using VodovozInfrastructure.Cryptography;
using YandexPayApi.Client;
using YooKassaApi.Client;

namespace CustomerOrdersApi.Library
{
	public static class CustomerOrdersApiExtensions
	{
		public static IServiceCollection AddConfig(this IServiceCollection services, IConfiguration config)
		{
			services.Configure<RequestsMinutesLimitsOptions>(config.GetSection(RequestsMinutesLimitsOptions.Position));
			services.Configure<SignatureOptions>(config.GetSection(SignatureOptions.Path));

			return services;
		}

		public static IServiceCollection AddCommonDependencies(this IServiceCollection services)
		{
			services
				.AddScoped<ISignatureManager, SignatureManager>()
				.AddScoped<IMD5HexHashFromString, MD5HexHashFromString>()
				.AddScoped<IExternalOrderStatusConverter, ExternalOrderStatusConverter>();

			return services;
		}

		public static IBusRegistrationConfigurator ConfigureRabbitMq(this IBusRegistrationConfigurator busConf)
		{
			busConf.UsingRabbitMq((context, configurator) =>
			{
				var messageSettings = context.GetRequiredService<IMessageTransportSettings>();

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

				Default.Extensions.VersionExtensions.AddTopologyV3(configurator);
				V4.Extensions.VersionExtensions.AddTopologyV4(configurator);
				V5.Extensions.VersionExtensions.AddTopologyV5(configurator);
				V6.Extensions.VersionExtensions.AddTopologyV6(configurator);
				V7.Extensions.VersionExtensions.AddTopologyV7(configurator);
				V8.Extensions.VersionExtensions.AddTopologyV8(configurator);

				configurator.ConfigureEndpoints(context);
			});
			
			return busConf;
		}

		public static IServiceCollection AddPaymentApiClients(
			this IServiceCollection services,
			IConfiguration configuration)
		{
			services.AddSingleton<JsonSerializerOptions>(sp =>
			{
				return new JsonSerializerOptions
				{
					WriteIndented = false,
					DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
				};
			});

			services
				.AddFastPaymentsApiClient(configuration)
				.AddCloudPaymentsApiClient(configuration)
				.AddYandexPayApiClient(configuration)
				.AddYooKassaApiClient(configuration);

			return services;
		}
	}
}
