using CustomerNotifications.Application.Providers;
using CustomerNotifications.Contracts;
using Microsoft.Extensions.DependencyInjection;
using TransactionalOutbox.Abstractions;

public static class DependencyInjection
{
	public static IServiceCollection AddCustomerNotificationsSettingsProvider(
		this IServiceCollection services)
	{
		services.AddSingleton<ICustomerNotificationsSettingsProvider, CustomerNotificationsSettingsProvider>();

		services.AddScoped<IOutBoxSettingsProvider<CustomerNotificationDomainEvent>>(
			sp => sp.GetRequiredService<ICustomerNotificationsSettingsProvider>());

		return services;
	}
}
