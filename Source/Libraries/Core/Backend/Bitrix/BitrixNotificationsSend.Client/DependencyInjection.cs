using Microsoft.Extensions.DependencyInjection;
using System;
using Vodovoz.Settings.Notifications;

namespace BitrixNotificationsSend.Client
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddBitrixNotificationsSendClient(this IServiceCollection services)
		{
			services
				.AddHttpClient<IBitrixNotificationsSendClient, BitrixNotificationsSendClient>((sp, client) =>
				{
					var bitrixSettings = sp.GetRequiredService<IBitrixNotificationsSendSettings>();
					client.BaseAddress = new Uri(bitrixSettings.BitrixBaseUrl);
					client.DefaultRequestHeaders.Accept.Clear();
				})
				.SetHandlerLifetime(TimeSpan.FromMinutes(5));

			return services;
		}
	}
}
