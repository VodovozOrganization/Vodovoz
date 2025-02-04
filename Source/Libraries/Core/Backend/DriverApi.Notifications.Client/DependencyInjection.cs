using DriverApi.Notifications.Client.Clients;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Headers;
using Vodovoz.NotificationRecievers;
using Vodovoz.Settings.Logistics;

namespace DriverApi.Notifications.Client
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddDriverApiHelper(this IServiceCollection services)
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
				.AddScoped<ISmsPaymentStatusNotificationReciever, DriverApiNotificationsClient>()
				.AddScoped<IFastDeliveryOrderAddedNotificationReciever, DriverApiNotificationsClient>()
				.AddScoped<IWaitingTimeChangedNotificationReciever, DriverApiNotificationsClient>()
				.AddScoped<ICashRequestForDriverIsGivenForTakeNotificationReciever, DriverApiNotificationsClient>()
				.AddScoped<IRouteListTransferhandByHandReciever, DriverApiNotificationsClient>();

			return services;
		}
	}
}
