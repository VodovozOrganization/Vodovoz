using CustomerNotifications.Application.Builders;
using CustomerNotifications.Contracts;
using CustomerOnlineOrdersUpdater.Config;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Notifications.Infrastructure;
using Osrm;
using QS.DomainModel.UoW;
using Notifications.Core;
using Vodovoz.Core.Application.Logistics;
using Vodovoz.Services.Logistics;

namespace CustomerOnlineOrdersUpdater
{
	public static class CustomerOnlineOrdersUpdaterExtensions
	{
		public static IServiceCollection AddConfig(this IServiceCollection services, IConfiguration config)
		{
			services.Configure<CustomerOnlineOrdersUpdaterOptions>(config.GetSection(CustomerOnlineOrdersUpdaterOptions.Path));
			return services;
		}
		
		public static IServiceCollection AddDependenciesGroup(this IServiceCollection services)
		{
			services
				.AddScoped(sp => sp.GetService<IUnitOfWorkFactory>().CreateWithoutRoot("Обработка онлайн заказов, ожидающих оплату"))
				.AddScoped<IRouteListService, RouteListService>()
				.AddScoped<IRouteListSpecialConditionsService, RouteListSpecialConditionsService>()
				.AddOsrm()
				.AddHostedService<CustomerOnlineOrdersUpdateWorker>()

				// Уведомления клиентов
				.AddCustomerNotificationsSettingsProvider()
				.AddScoped<IIntegrationEventBuilder<CustomerNotificationDomainEvent, CustomerNotificationIntegrationEvent>, CustomerNotificationsIntegrationEventBuilder>()
				.AddScoped<INotificationPublisher<CustomerNotificationDomainEvent>, NotificationPublisher<CustomerNotificationDomainEvent, CustomerNotificationIntegrationEvent>>()
				.AddNotificationBusNotHosted()
				;

			return services;
		}
	}
}
