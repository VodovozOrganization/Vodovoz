using Mango.Vpbx.Client.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using Vodovoz.Settings.Mango;

namespace Mango.Vpbx.Client
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddMangoVpbxClientServices(this IServiceCollection services)
		{
			services
				.AddScoped<IMangoWebhookCallsService, MangoWebhookCallsService>();

			services
				.AddHttpClient<IMangoWebhookCallsService, MangoWebhookCallsService>((sp, client) =>
				{
					var mangoSettings = sp.GetRequiredService<IMangoSettings>();
					client.BaseAddress = new Uri(mangoSettings.WebhookCallsUrl);
					client.DefaultRequestHeaders.Accept.Clear();
				});

			return services;
		}
	}
}
