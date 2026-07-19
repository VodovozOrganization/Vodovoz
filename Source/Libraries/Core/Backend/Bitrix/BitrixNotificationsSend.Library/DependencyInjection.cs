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
			services.ConfigureOptions<ConfigurePlannedOrdersDealsCreateOptions>();
			services.ConfigureOptions<ConfigureLastServiceOrdersDealsCreateOptions>();

			services.AddBitrixNotificationsSendClient();

			services.AddTransient<CashlessDebtsNotificationsSendService>();
			services.AddTransient<PlannedOrdersDealsCreateService>();
			services.AddTransient<IBitrixBatchesSendService, BitrixBatchesSendService>();

			return services;
		}
	}
}
