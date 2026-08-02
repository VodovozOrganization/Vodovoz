using BitrixNotificationsSend.Client;
using BitrixNotificationsSend.Library.Factories;
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
			services.AddTransient<LastServiceOrdersDealsCreateService>();
			services.AddTransient<IBitrixBatchesSendService, BitrixBatchesSendService>();
			services.AddTransient<ILastServiceOrderDtoFactory, LastServiceOrderDtoFactory>();

			return services;
		}
	}
}
