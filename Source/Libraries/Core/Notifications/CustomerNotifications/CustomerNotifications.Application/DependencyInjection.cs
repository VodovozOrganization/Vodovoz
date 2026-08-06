using CustomerNotifications.Application.Builders;
using CustomerNotifications.Application.Providers;
using CustomerNotifications.Application.SmsNotificationCreators;
using CustomerNotifications.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Notifications.Infrastructure;
using TransactionalOutbox.Abstractions;
using VodovozBusiness.Services.Logistics;

public static class DependencyInjection
{
	public static IServiceCollection AddCustomerNotificationsSettingsProvider(
		this IServiceCollection services)
	{
		services.AddSingleton<ICustomerNotificationsSettingsProvider, CustomerNotificationsSettingsProvider>();

		services.AddScoped<IOutboxSettingsProvider<CustomerNotificationDomainEvent>>(
			sp => sp.GetRequiredService<ICustomerNotificationsSettingsProvider>());

		return services;
	}

	/// <summary>
	/// Регистрирует публикацию уведомлений клиентам с резервной отправкой смс уведомлений,
	/// если событие не может быть отправлено по причине отсутствия внешнего пользователя у клиента
	/// </summary>
	public static IServiceCollection AddCustomerNotificationsWithSmsFallback(
		this IServiceCollection services)
	{
		services.AddCustomerNotificationsSettingsProvider();

		services.AddScoped<IDriverContactNumberService, DriverContactNumberService>();
		services.AddScoped<ISmsNotificationSendingPolicy, SmsNotificationSendingPolicy>();

		services.AddScoped<ISmsNotificationCreator<CustomerNotificationDomainEvent>, CourierOnTheWaySmsNotificationCreator>();

		services
			.AddScoped<IIntegrationEventBuilder<CustomerNotificationDomainEvent, CustomerNotificationIntegrationEvent>,
				CustomerNotificationsIntegrationEventBuilder>();

		return services
			.AddMappingOutboxNotificationWithSmsFallbackPublisher<CustomerNotificationDomainEvent, CustomerNotificationIntegrationEvent>();
	}
}
