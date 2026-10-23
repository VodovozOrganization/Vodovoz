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
			services.ConfigureOptions<ConfigureUndeliveredOrdersDealsCreateOptions>();
			services.ConfigureOptions<ConfigureChainStoreDelayDealsCreateOptions>();

			services.AddBitrixNotificationsSendClient();

			services.AddTransient<CashlessDebtsNotificationsSendService>();
			services.AddTransient<PlannedOrdersDealsCreateService>();
			services.AddTransient<LastServiceOrdersDealsCreateService>();
			services.AddTransient<IUndeliveredOrdersDealsCreateService, UndeliveredOrdersDealsCreateService>();
			services.AddTransient<IBitrixBatchesSendService, BitrixBatchesSendService>();
			services.AddTransient<IBitrixDealsStagesReadService, BitrixDealsStagesReadService>();
			services.AddTransient<ILastServiceOrderDtoFactory, LastServiceOrderDtoFactory>();
			services.AddTransient<IChainStoreDelayBitrixDealFactory, ChainStoreDelayBitrixDealFactory>();
			services.AddTransient<IChainStoreDelayDealsCreateService, ChainStoreDelayDealsCreateService>();

			return services;
		}
	}
}
