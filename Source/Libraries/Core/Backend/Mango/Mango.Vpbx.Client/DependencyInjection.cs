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
				.AddScoped<IMangoCallsService, MangoCallsService>();

			services
				.AddHttpClient<IMangoCallsService, MangoCallsService>((sp, client) =>
				{
					var mangoSettings = sp.GetRequiredService<IMangoSettings>();
					client.BaseAddress = new Uri(mangoSettings.WebhookCallsUrl);
					client.DefaultRequestHeaders.Accept.Clear();
				});

			return services;
		}
	}
}
