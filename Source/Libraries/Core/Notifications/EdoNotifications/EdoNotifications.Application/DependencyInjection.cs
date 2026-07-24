
using EdoNotifications.Application.Providers;
using EdoNotifications.Contracts;
using Microsoft.Extensions.DependencyInjection;
using TransactionalOutbox.Abstractions;

public static class DependencyInjection
{
	public static IServiceCollection AddEdoNotificationsSettingsProvider(
		this IServiceCollection services)
	{
		services.AddSingleton<IEdoNotificationsSettingsProvider, EdoNotificationsSettingsProvider>();

		services.AddScoped<IOutboxSettingsProvider<EdoNotificationMessage>>(
			sp => sp.GetRequiredService<IEdoNotificationsSettingsProvider>());

		return services;
	}
}
