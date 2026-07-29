using Mango.Core.Sign;
using Mango.Vpbx.Client.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using Vodovoz.Settings.Mango;

namespace Mango.Vpbx.Client
{
	public static class DependencyInjection
	{
		/// <summary>
		/// Добавляет сервисы клиента Mango Vpbx (virtual Private Branch Exchange виртуальная частная автоматическая телефонная станция) в контейнер зависимостей.
		/// </summary>
		/// <param name="services">Сервисы</param>
		/// <returns>Сервисы</returns>
		public static IServiceCollection AddMangoVpbxClientServices(this IServiceCollection services)
		{
			services.TryAddSingleton<ISignGenerator, SignGenerator>();

			services
				.AddHttpClient<IMangoWebhookCallsService, MangoWebhookCallsService>((sp, client) =>
				{
					var mangoSettings = sp.GetRequiredService<IMangoSettings>();
					client.BaseAddress = new Uri(mangoSettings.WebhookCallsUrl);
					client.DefaultRequestHeaders.Accept.Clear();
				});

			services
				.AddHttpClient<IMangoVpbxEmployeesService, MangoVpbxEmployeesService>((sp, client) =>
				{
					var mangoSettings = sp.GetRequiredService<IMangoSettings>();
					client.BaseAddress = new Uri(mangoSettings.VpbxApiUrl);
					client.DefaultRequestHeaders.Accept.Clear();
				});

			return services;
		}
	}
}
