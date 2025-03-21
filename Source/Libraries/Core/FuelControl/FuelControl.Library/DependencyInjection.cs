using FuelControl.Library.Converters;
using FuelControl.Library.Services;
using Microsoft.Extensions.DependencyInjection;

namespace FuelControl.Library
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddFuelControl(this IServiceCollection services) => services
			.AddScoped<IFuelControlAuthorizationService, GazpromAuthorizationService>()
			.AddScoped<IFuelControlFuelCardsDataService, GazpromFuelCardsDataService>()
			.AddScoped<IFuelLimitsManagementService, GazpromFuelLimitsManagementService>()
			.AddScoped<IFuelControlFuelCardProductRestrictionService, GazpromFuelCardProductRestrictionService>()
			.AddScoped<IFuelCardConverter, FuelCardConverter>()
			.AddScoped<IFuelLimitConverter, FuelLimitConverter>();
	}
}
