using DatabaseServiceWorker.Options;
using DatabaseServiceWorker.PowerBiWorker.Options;
using FuelControl.Library.Converters;
using FuelControl.Library.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Vodovoz.Controllers;
using Vodovoz.Settings.Database.Fuel;
using Vodovoz.Settings.Fuel;
using FuelControl.Library;
using QS.Project.Core;
using VodovozBusiness.Services.Orders;
using Vodovoz.Core.Application.Orders.Services;
using Vodovoz.Data.NHibernate.NhibernateExtensions;

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

		public static IServiceCollection ConfigureExportTo1cApiWorker(this IServiceCollection services, HostBuilderContext context) =>
			services.Configure<ExportTo1cApiOptions>(context.Configuration.GetSection(nameof(ExportTo1cApiOptions)));

		public static IServiceCollection ConfigureTechInspectWorker(this IServiceCollection services, HostBuilderContext context) =>
			services.Configure<TechInspectOptions>(context.Configuration.GetSection(nameof(TechInspectOptions)));

		public static IServiceCollection AddFuelTransactionsControlWorker(this IServiceCollection services, HostBuilderContext context) => services
			.ConfigureFuelTransactionsControlWorker(context)
			.AddScoped<IFuelControlAuthorizationService, GazpromAuthorizationService>()
			.AddScoped<ITransactionConverter, TransactionConverter>()
			.AddScoped<IFuelControlSettings, FuelControlSettings>()
			.AddScoped<IFuelPricesUpdateService, FuelPricesUpdateService>()
			.AddScoped<IFuelPriceVersionsController, FuelPriceVersionsController>()
			.AddFuelControl();

		public static IServiceCollection ConfigureFuelTransactionsControlWorker(this IServiceCollection services, HostBuilderContext context) => services
			.Configure<FuelTransactionsControlOptions>(context.Configuration.GetSection(nameof(FuelTransactionsControlOptions)));

		public static IServiceCollection AddClosingDeliveriesWorker(this IServiceCollection services, HostBuilderContext context) => services
			.Configure<ClosingDeliveriesOptions>(context.Configuration.GetSection(nameof(ClosingDeliveriesOptions)))
			.AddScoped<IClosingDeliveriesService, ClosingDeliveriesService>()
			.AddDatabaseConfigurationExposer(config => config.LinqToHqlGeneratorsRegistry<LinqToHqlGeneratorsRegistry>());
	}
}
