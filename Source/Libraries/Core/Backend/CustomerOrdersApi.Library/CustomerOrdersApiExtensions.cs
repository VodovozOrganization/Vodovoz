using System.Security.Authentication;
using CustomerOrdersApi.Library.Dto.Orders;
using CustomerOrdersApi.Library.Services;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using Vodovoz.Settings.Pacs;
using VodovozInfrastructure.Cryptography;

namespace CustomerOrdersApi.Library
{
	public static class CustomerOrdersApiExtensions
	{
		public static IServiceCollection AddCustomerOrdersApiLibrary(this IServiceCollection services)
		{
			services.AddLibraryDependencies();
			
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
							hostConfigurator.UseSsl(ssl => ssl.Protocol = SslProtocols.Tls12);
						}
					});
								
				configurator.Send<OnlineOrderInfoDto>(x => x.UseRoutingKeyFormatter(y => y.Message.FaultedMessage.ToString()));
				configurator.Message<OnlineOrderInfoDto>(x => x.SetEntityName("online-order-received"));
				configurator.Publish<OnlineOrderInfoDto>(x =>
				{
					x.ExchangeType = ExchangeType.Direct;
					x.Durable = true;
					x.AutoDelete = false;
					x.BindQueue(
						"online-order-received",
						"online-orders",
						conf =>
						{
							conf.ExchangeType = ExchangeType.Direct;
							conf.RoutingKey = "False";
						});
					x.BindQueue(
						"online-order-received",
						"online-orders-fault",
						conf =>
						{
							conf.ExchangeType = ExchangeType.Direct;
							conf.RoutingKey = "True";
						});
				});
								
				configurator.ConfigureEndpoints(context);
			});
			
			return busConf;
		}

		private static IServiceCollection AddLibraryDependencies(this IServiceCollection services)
		{
			services.AddScoped<ICustomerOrdersService, CustomerOrdersService>()
				.AddScoped<ISignatureManager, SignatureManager>()
				.AddScoped<IMD5HexHashFromString, MD5HexHashFromString>()
				;
			
			return services;
		}
	}
}
