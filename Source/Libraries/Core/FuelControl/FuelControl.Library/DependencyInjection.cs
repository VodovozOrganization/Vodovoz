using FuelControl.Library.Converters;
using FuelControl.Library.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace FuelControl.Library
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddFuelControl(this IServiceCollection services, HostBuilderContext context) => services
			.AddScoped<IFuelManagmentAuthorizationService, GazpromAuthorizationService>()
			.AddScoped<IFuelCardsGeneralInfoService, GazpromFuelCardsGeneralInfoService>()
			.AddScoped<ITransactionConverter, TransactionConverter>()
			.AddScoped<IFuelCardConverter, FuelCardConverter>();
	}
}
