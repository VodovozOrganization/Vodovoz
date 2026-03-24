using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Security;
using System.Security.Authentication;
using CustomerNotifications.Contracts;
using CustomerNotifications.Contracts.Messages;
using CustomerNotifications.Contracts.Providers;
using CustomerNotifications.Publisher.Bus;
using CustomerNotifications.Publisher.Cache;
using CustomerNotifications.Publisher.Services;
using CustomerNotifications.Transport.Settings;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using QS.DomainModel.UoW;
using RabbitMQ.Client;
using StackExchange.Redis;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Core.Domain.Orders.OrderEnums;

namespace CustomerNotifications.Publisher.Configuration
{
	public static class CustomerNotificationPublisherExtensions
	{
		private static IServiceCollection AddGarnetRedisConnection(this IServiceCollection services, IConfiguration configuration)
		{
			services.Configure<GarnetConnection>(
				configuration.GetSection(nameof(GarnetConnection)));

			services.AddSingleton<IConnectionMultiplexer>(sp =>
			{
				var garnetConnection = sp.GetRequiredService<IOptions<GarnetConnection>>();
				return ConnectionMultiplexer.Connect(garnetConnection.Value.ConnectionString);
			});

			return services;
		}
		
		private static IServiceCollection AddOnlineOrderNotificationSettingsProvider(this IServiceCollection services)
		{
			services.AddSingleton<IOnlineOrderNotificationSettingsProvider>(sp =>
			{
				var uowFactory = sp.GetRequiredService<IUnitOfWorkFactory>();

				using(var uow = uowFactory.CreateWithoutRoot())
				{
					var settings = new ReadOnlyDictionary<CustomerNotificationEventType, OnlineOrderNotificationSetting>(
						uow.GetAll<OnlineOrderNotificationSetting>()
							.ToDictionary(s => s.CustomerNotificationEventType));

					return new OnlineOrderNotificationSettingsProvider(settings);
				}
			});

			return services;
		}
		
		/// <summary>
		/// Регистрирует издателя уведомлений, работающего через отдельную шину MassTransit (multibus)
		/// </summary>
		public static IServiceCollection AddMultibusCustomerNotificationsPublisher(
			this IServiceCollection services,
			IConfiguration configuration)
		{
			services.Configure<CustomerNotificationTransportSettings>(
				configuration.GetSection(nameof(CustomerNotificationTransportSettings)));

			services
				.AddGarnetRedisConnection(configuration)
				.AddScoped<ICustomerNotificationCache, RedisCustomerNotificationCache>()
				.AddOnlineOrderNotificationSettingsProvider()
				;

			services.AddMassTransit<ICustomerNotificationBus>(x =>
			{
				x.AddScoped<ICustomerNotificationPublisher, CustomerNotificationPublisherMultibusAdapter>();

				x.UsingRabbitMq((context, cfg) =>
				{
					var settings = context.GetRequiredService<IOptions<CustomerNotificationTransportSettings>>().Value;

					cfg.Host(settings.Host, (ushort)settings.Port, settings.VirtualHost, h =>
					{
						h.Username(settings.Username);
						h.Password(settings.Password);

						if(settings.UseSSL)
						{
							h.UseSsl(ssl =>
							{
								if(Enum.TryParse<SslPolicyErrors>(settings.AllowSslPolicyErrors, out var allowed))
								{
									ssl.AllowPolicyErrors(allowed);
								}

								ssl.Protocol = SslProtocols.Tls12;
							});
						}
					});

					cfg.AddCustomerNotificationPublisherTopology(context);
				});
			});

			return services;
		}

		/// <summary>
		/// Регистрирует издателя уведомлений для десктопного приложения.
		/// </summary>
		public static IServiceCollection AddDesktopCustomerNotificationsPublisher(this IServiceCollection services)
		{
			services
				.AddScoped<ICustomerNotificationCache, RedisCustomerNotificationCache>()
				.AddScoped<ICustomerNotificationPublisher, CustomerNotificationPublisherDesktopAdapter>()
				.AddOnlineOrderNotificationSettingsProvider();

			return services;
		}
		
		/// <summary>
		/// Настраивает топологию RabbitMQ для публикации уведомлений.
		/// </summary>
		public static void AddCustomerNotificationPublisherTopology(
			this IRabbitMqBusFactoryConfigurator configurator,
			IBusRegistrationContext context)
		{
			configurator.Message<CustomerNotificationMessage>(x =>
				x.SetEntityName(CustomerNotificationsConstants.QueueName));

			configurator.Publish<CustomerNotificationMessage>(x =>
			{
				x.ExchangeType = ExchangeType.Fanout;
				x.Durable = true;
				x.AutoDelete = false;
			});
		}
	}
}
