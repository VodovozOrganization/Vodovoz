using CustomerNotifications.Consumer.Consumers;
using CustomerNotifications.Consumer.Defenitions;
using CustomerNotifications.Consumer.Options;
using CustomerNotifications.Consumer.Services;
using CustomerNotifications.Contracts.Providers;
using CustomerNotifications.Transport.Settings;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NLog.Extensions.Logging;
using QS.Banks.Domain;
using QS.DomainModel.UoW;
using QS.Project.Core;
using QS.Project.Domain;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Security;
using System.Security.Authentication;
using System.Text.Json;
using Vodovoz.Core.Data.NHibernate;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Core.Domain.Orders.OrderEnums;
using AssemblyFinder = Vodovoz.Data.NHibernate.AssemblyFinder;

namespace CustomerNotifications.Consumer.Configuration
{
	/// <summary>
	/// Методы расширения для регистрации консюмера уведомлений в DI.
	/// </summary>
	public static class CustomerNotificationConsumerExtensions
	{
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
		/// Регистрирует консюмер уведомлений и настраивает подключение к RabbitMQ.
		/// </summary>
		/// <param name="services">Коллекция сервисов приложения.</param>
		/// <param name="configuration">Конфигурация приложения.</param>
		/// <returns>Коллекция сервисов с добавленной конфигурацией консюмера.</returns>
		private static IServiceCollection AddCustomerNotificationConsumer(
			this IServiceCollection services,
			IConfiguration configuration)
		{
			services.Configure<CustomerNotificationTransportSettings>(
				configuration.GetSection(nameof(CustomerNotificationTransportSettings)));

			services.AddMassTransit(x =>
			{
				x.AddConsumer<CustomerNotificationConsumer, CustomerNotificationConsumerDefinition>();

				x.UsingRabbitMq((context, cfg) =>
				{
					var settings = context
						.GetRequiredService<IOptions<CustomerNotificationTransportSettings>>()
						.Value;

					cfg.Host(settings.Host,
						(ushort)settings.Port,
						settings.VirtualHost,
						h =>
						{
							h.Username(settings.Username);
							h.Password(settings.Password);

							if(settings.UseSSL)
							{
								h.UseSsl(ssl =>
								{
									if(Enum.TryParse<SslPolicyErrors>(settings.AllowSslPolicyErrors, out var allowedPolicyErrors))
									{
										ssl.AllowPolicyErrors(allowedPolicyErrors);
									}

									ssl.Protocol = SslProtocols.Tls12;
								});
							}
						});

					cfg.ConfigureEndpoints(context);
				});
			});

			return services;
		}


		public static IServiceCollection AddCustomerNotificationsConsumer(
			this IServiceCollection services,
			HostBuilderContext hostContext)
		{
			services
			.AddMappingAssemblies(
				typeof(AssemblyFinder).Assembly,
				typeof(Bank).Assembly,
				typeof(TypeOfEntity).Assembly
		   )
		   .AddDatabaseConnection()
		   .AddCore()
		   .AddNotTrackedUoW()

		   	.AddCustomerNotificationConsumer(hostContext.Configuration)
			.AddOnlineOrderNotificationSettingsProvider()

			.AddLogging(logging =>
			{
				logging.ClearProviders();
				logging.AddNLog();
				logging.AddConfiguration(hostContext.Configuration.GetSection("NLog"));
			})
			.Configure<NotifierOptions>(hostContext.Configuration.GetSection(NotifierOptions.Path))
			.AddSingleton(_ => new JsonSerializerOptions
			{
				PropertyNamingPolicy = JsonNamingPolicy.CamelCase
			})

			.AddHttpClient<IOnlineOrdersStatusUpdatedNotificationService, OnlineOrdersStatusUpdatedNotificationService>((provider, client) =>
			{
				var timeout = provider.GetRequiredService<IOptionsMonitor<NotifierOptions>>().CurrentValue.SendingTimeoutInSeconds;
				client.Timeout = TimeSpan.FromSeconds(timeout);
			});

			Vodovoz.Data.NHibernate.DependencyInjection.AddStaticScopeForEntity(services);

			return services;
		}
	}
}
