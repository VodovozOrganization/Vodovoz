using DatabaseServiceWorker.Options;
using FuelControl.Library.Converters;
using FuelControl.Library.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Vodovoz.EntityRepositories.Fuel;

namespace DatabaseServiceWorker
{
	public static class DependencyInjection
	{
		public static IServiceCollection ConfigureClearFastDeliveryAvailabilityHistoryWorker(this IServiceCollection services, HostBuilderContext context) => services
			.Configure<ClearFastDeliveryAvailabilityHistoryOptions>(context.Configuration.GetSection(nameof(ClearFastDeliveryAvailabilityHistoryOptions)));

		public static IServiceCollection AddFuelTransactionsControlWorker(this IServiceCollection services, HostBuilderContext context) => services
			.ConfigureFuelTransactionsControlWorker(context)
			.AddSingleton<IFuelManagmentAuthorizationService, GazpromAuthorizationService>()
			.AddSingleton<TransactionConverter>()
			.AddSingleton<IFuelTransactionsDataService, GazpromFuelTransactionsDataService>()
			.AddSingleton<IFuelRepository, FuelRepository>();

		public static IServiceCollection ConfigureFuelTransactionsControlWorker(this IServiceCollection services, HostBuilderContext context) => services
			.Configure<FuelTransactionsControlOptions>(context.Configuration.GetSection(nameof(FuelTransactionsControlOptions)));
	}
}
