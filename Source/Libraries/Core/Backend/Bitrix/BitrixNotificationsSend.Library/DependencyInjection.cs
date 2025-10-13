using BitrixNotificationsSend.Library.Options;
using BitrixNotificationsSend.Library.Services;
using Microsoft.Extensions.DependencyInjection;

namespace BitrixNotificationsSend.Library
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddBitrixNotificationsSendServices(this IServiceCollection services)
		{
			services.ConfigureOptions<ConfigureCashlessDebtsNotificationsSendOptions>();

			services.AddTransient<CashlessDebtsNotificationsSendService>();

			return services;
		}
	}
}
