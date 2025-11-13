using DatabaseServiceWorker.Options;
using DatabaseServiceWorker.PowerBiWorker.Options;
using FuelControl.Library.Converters;
using FuelControl.Library.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Vodovoz.Controllers;
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
		
		public static IServiceCollection ConfigureExportTo1cWorker(this IServiceCollection services, HostBuilderContext context) =>
			services.Configure<ExportTo1cOptions>(context.Configuration.GetSection(nameof(ExportTo1cOptions)));

		public static IServiceCollection ConfigureTechInspectWorker(this IServiceCollection services, HostBuilderContext context) =>
			services.Configure<TechInspectOptions>(context.Configuration.GetSection(nameof(TechInspectOptions)));

		public static IServiceCollection AddFuelTransactionsControlWorker(this IServiceCollection services, HostBuilderContext context) => services
			.ConfigureFuelTransactionsControlWorker(context)
			.AddScoped<IFuelControlAuthorizationService, GazpromAuthorizationService>()
			.AddScoped<ITransactionConverter, TransactionConverter>()
			.AddScoped<IFuelControlTransactionsDataService, GazpromTransactionsDataService>()
			.AddScoped<IFuelControlSettings, FuelControlSettings>()
			.AddScoped<IFuelPricesUpdateService, FuelPricesUpdateService>()
			.AddScoped<IFuelPriceVersionsController, FuelPriceVersionsController>();

		public static IServiceCollection ConfigureFuelTransactionsControlWorker(this IServiceCollection services, HostBuilderContext context) => services
			.Configure<FuelTransactionsControlOptions>(context.Configuration.GetSection(nameof(FuelTransactionsControlOptions)));
	}
}
