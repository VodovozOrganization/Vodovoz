using BitrixNotificationsSend.Client;
using BitrixNotificationsSend.Library.Options;
using BitrixNotificationsSend.Library.Services;
using BitrixNotificationsSend.Library.Services.Batches;
using Microsoft.Extensions.DependencyInjection;

namespace BitrixNotificationsSend.Library
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddBitrixNotificationsSendServices(this IServiceCollection services)
		{
			services.ConfigureOptions<ConfigureCashlessDebtsNotificationsSendOptions>();
			services.ConfigureOptions<ConfigurePlannedOrdersNotificationsSendOptions>();

			services.AddBitrixNotificationsSendClient();

			services.AddTransient<CashlessDebtsNotificationsSendService>();
			services.AddTransient<PlannedOrdersNotificationsSendService>();
			services.AddTransient<IBitrixBatchesSendService, BitrixBatchesSendService>();

			return services;
		}
	}
}
