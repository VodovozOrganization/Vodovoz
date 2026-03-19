using CustomerOrdersApi.Library.Config;
using CustomerOrdersApi.Library.Converters;
using CustomerOrdersApi.Library.Dto.Orders;
using CustomerOrdersApi.Library.Factories;
using CustomerOrdersApi.Library.Services;
using CustomerOrdersApi.Library.Services.PaymentRefund;
using CustomerOrdersApi.Library.Services.PaymentRefund.HttpClients;
using CustomerOrdersApi.Library.Services.PaymentRefund.Mappers;
using FastPaymentsAPI.Library;
using FastPaymentsAPI.Library.Services;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using QS.DomainModel.UoW;
using RabbitMQ.Client;
using System;
using System.Net.Http.Headers;
using System.Net.Security;
using System.Security.Authentication;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Vodovoz.Application.Logistics;
using Vodovoz.Application.Orders.Services;
using Vodovoz.Handlers;
using Vodovoz.Services.Logistics;
using Vodovoz.Settings.Database.Nomenclature;
using Vodovoz.Settings.Nomenclature;
using Vodovoz.Settings.Pacs;
using VodovozBusiness.Services.Orders;
using VodovozInfrastructure.Cryptography;
using IOrderService = FastPaymentsAPI.Library.Services.IOrderService;

namespace CustomerOrdersApi.Library
{
	public static class CustomerOrdersApiExtensions
	{
		public static IServiceCollection AddConfig(this IServiceCollection services, IConfiguration config)
		{
			services
				.Configure<RequestsMinutesLimitsOptions>(config.GetSection(RequestsMinutesLimitsOptions.Position))
				.Configure<SignatureOptions>(config.GetSection(SignatureOptions.Path));
			
			return services;
		}
		
		public static IServiceCollection AddDependenciesGroup(this IServiceCollection services)
		{
			services.AddScoped<ICustomerOrdersService, CustomerOrdersService>()
				.AddScoped<ICustomerOrdersDiscountService, CustomerOrdersDiscountService>()
				.AddScoped<ICustomerOrderFixedPriceService, CustomerOrderFixedPriceService>()
				.AddScoped<ISignatureManager, SignatureManager>()
				.AddScoped<IMD5HexHashFromString, MD5HexHashFromString>()
				.AddScoped<ICustomerOrderFactory, CustomerOrderFactory>()
				.AddScoped<IExternalOrderStatusConverter, ExternalOrderStatusConverter>()
				.AddScoped<IOnlineOrderDiscountHandler, OnlineOrderDiscountHandler>()
				.AddScoped<IOrderTransferService, OrderTransferService>()
				.AddScoped<IOrderCancellationService, OrderCancellationService>()
				.AddScoped<IRouteListService, RouteListService>()
				.AddScoped<INomenclatureSettings, NomenclatureSettings>()
				.AddScoped<IRouteListSpecialConditionsService, RouteListSpecialConditionsService>()
				.AddScoped<IOnlineOrderService, OnlineOrderService>();

			return services;
		}

		public static IServiceCollection AddFastPaymentsDependencies(this IServiceCollection services, IConfiguration config)
		{
			services.AddHttpClient<IOrderService, OrderService>(c =>
			{
				c.BaseAddress = new Uri(config.GetSection("OrderService").GetValue<string>("ApiBase"));
				c.DefaultRequestHeaders.Add("Accept", "application/x-www-form-urlencoded");
			});

			services.AddScoped((provider) => provider.GetRequiredService<IUnitOfWorkFactory>().CreateWithoutRoot("Сервис быстрых платежей"));

			return services.AddDependencyGroup();
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

		public static IServiceCollection AddPaymentRefundServices(
			this IServiceCollection services,
			IConfiguration configuration)
		{
			services.Configure<CloudPaymentsOptions>(
				configuration.GetSection("CloudPayments"));

			services.AddSingleton<JsonSerializerOptions>(sp =>
			{
				return new JsonSerializerOptions
				{
					WriteIndented = false,
					DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
				};
			});

			services.AddHttpClient<ICloudPaymentsHttpClient, CloudPaymentsHttpClient>((sp, client) =>
			{
				var settings = sp.GetRequiredService<IOptions<CloudPaymentsOptions>>().Value;

				client.BaseAddress = new Uri(settings.ApiUrl);

				var authToken = Convert.ToBase64String(
					Encoding.ASCII.GetBytes($"{settings.PublicId}:{settings.ApiSecret}"));

				client.DefaultRequestHeaders.Authorization =
					new AuthenticationHeaderValue("Basic", authToken);

				client.Timeout = TimeSpan.FromSeconds(30);
			});


			services.AddScoped<ICloudPaymentsMapper, CloudPaymentsMapper>();
			services.AddScoped<IPaymentRefundServiceFactory, PaymentRefundServiceFactory>();
			services.AddScoped<IPaymentRefundService, CloudPaymentsRefundService>();


			services.Configure<YandexPayOptions>(
				configuration.GetSection("YandexPay"));

			services.AddHttpClient<IYandexPayHttpClient, YandexPayHttpClient>((sp, client) =>
			{
				var settings = sp.GetRequiredService<IOptions<YandexPayOptions>>().Value;

				client.BaseAddress = new Uri(settings.ApiUrl);
				client.DefaultRequestHeaders.Add("Authorization", $"Api-Key {settings.ApiKey}");
				client.DefaultRequestHeaders.Add("Accept", "application/json");

				client.Timeout = TimeSpan.FromSeconds(30);
			});

			services.AddScoped<IYandexPayMapper, YandexPayMapper>();
			services.AddScoped<IPaymentRefundService, YandexPayRefundService>();

			services.Configure<YooKassaOptions>(
				configuration.GetSection("YooKassa"));

			services.AddHttpClient<IYooKassaHttpClient, YooKassaHttpClient>((sp, client) =>
			{
				var settings = sp.GetRequiredService<IOptions<YooKassaOptions>>().Value;

				client.BaseAddress = new Uri(settings.ApiUrl);

				var authToken = Convert.ToBase64String(
					Encoding.ASCII.GetBytes($"{settings.ShopId}:{settings.SecretKey}"));

				client.DefaultRequestHeaders.Authorization =
					new AuthenticationHeaderValue("Basic", authToken);

				client.DefaultRequestHeaders.Accept.Add(
					new MediaTypeWithQualityHeaderValue("application/json"));

				client.Timeout = TimeSpan.FromSeconds(30);
			});

			services.AddScoped<IYooKassaMapper, YooKassaMapper>();
			services.AddScoped<IPaymentRefundService, YooKassaRefundService>();

			services.AddScoped<IPaymentRefundService, FastPaymentRefundService>();

			return services;
		}
	}
}
