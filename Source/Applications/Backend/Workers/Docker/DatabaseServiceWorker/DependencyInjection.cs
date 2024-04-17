using DatabaseServiceWorker.Options;
using FuelControl.Library.Converters;
using FuelControl.Library.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Vodovoz.EntityRepositories.Fuel;
using Vodovoz.Settings.Database.Fuel;
using Vodovoz.Settings.Fuel;

namespace DatabaseServiceWorker
{
	public static class DependencyInjection
	{
		public static IServiceCollection ConfigureClearFastDeliveryAvailabilityHistoryWorker(this IServiceCollection services, HostBuilderContext context) => services
			.Configure<ClearFastDeliveryAvailabilityHistoryOptions>(context.Configuration.GetSection(nameof(ClearFastDeliveryAvailabilityHistoryOptions)));

		public static IServiceCollection ConfigurePowerBiExportWorker(this IServiceCollection services, HostBuilderContext context) =>
			services.Configure<PowerBiExportOptions>(context.Configuration.GetSection(nameof(PowerBiExportOptions)));

		public static IServiceCollection AddFuelTransactionsControlWorker(this IServiceCollection services, HostBuilderContext context) => services
			.ConfigureFuelTransactionsControlWorker(context)
			.AddSingleton<IFuelManagmentAuthorizationService, GazpromAuthorizationService>()
			.AddSingleton<ITransactionConverter, TransactionConverter>()
			.AddSingleton<IFuelTransactionsDataService, GazpromFuelTransactionsDataService>()
			.AddSingleton<IFuelRepository, FuelRepository>()
			.AddSingleton<IFuelControlSettings, FuelControlSettings>();

		public static IServiceCollection ConfigureFuelTransactionsControlWorker(this IServiceCollection services, HostBuilderContext context) => services
			.Configure<FuelTransactionsControlOptions>(context.Configuration.GetSection(nameof(FuelTransactionsControlOptions)));
	}
}
