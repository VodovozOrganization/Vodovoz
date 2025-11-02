using DriverApi.Notifications.Client.Clients;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Headers;
using Vodovoz.NotificationSenders;
using Vodovoz.Settings.Logistics;
using VodovozBusiness.NotificationSenders;

namespace DriverApi.Notifications.Client
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddDriverApiNotificationsSenders(this IServiceCollection services)
		{
			services
				.AddHttpClient(nameof(DriverApiNotificationsClient), (serviceProvider, client) =>
				{
					var databaseSettings = serviceProvider.GetRequiredService<IDriverApiSettings>();

					client.BaseAddress = databaseSettings.ApiBase;
					client.DefaultRequestHeaders.Accept.Clear();
					client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
				});

			services
				.AddScoped<ISmsPaymentStatusNotificationSender, DriverApiNotificationsClient>()
				.AddScoped<IFastDeliveryOrderAddedNotificationSender, DriverApiNotificationsClient>()
				.AddScoped<IWaitingTimeChangedNotificationSender, DriverApiNotificationsClient>()
				.AddScoped<ICashRequestForDriverIsGivenForTakeNotificationSender, DriverApiNotificationsClient>()
				.AddScoped<IRouteListChangesNotificationSender, DriverApiNotificationsClient>();

			return services;
		}
	}
}
