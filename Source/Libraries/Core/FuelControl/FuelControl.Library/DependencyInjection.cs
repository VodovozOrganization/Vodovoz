using FuelControl.Library.Converters;
using FuelControl.Library.Services;
using Microsoft.Extensions.DependencyInjection;
using Vodovoz.Settings.Fuel;

namespace FuelControl.Library
{

	public static class DependencyInjection
	{
		public static IServiceCollection AddFuelControl(this IServiceCollection services)
		{
			services.AddTransient<GazpromRequestHeadersValidationHandler>();

			services.AddHttpClient(GazpromHttpClientNames.Default, (sp, client) =>
			{
				var settings = sp.GetRequiredService<IFuelControlSettings>();
				client.BaseAddress = new System.Uri(settings.ApiBaseAddress);
			})
			.AddHttpMessageHandler<GazpromRequestHeadersValidationHandler>();

			services.AddHttpClient(GazpromHttpClientNames.WithTimeout, (sp, client) =>
			{
				var settings = sp.GetRequiredService<IFuelControlSettings>();
				client.BaseAddress = new System.Uri(settings.ApiBaseAddress);
				client.Timeout = settings.ApiRequesTimeout;
			})
			.AddHttpMessageHandler<GazpromRequestHeadersValidationHandler>();

			return services
				.AddScoped<IFuelControlAuthorizationService, GazpromAuthorizationService>()
				.AddScoped<IFuelControlFuelCardsDataService, GazpromFuelCardsDataService>()
				.AddScoped<IFuelLimitsManagementService, GazpromFuelLimitsManagementService>()
				.AddScoped<IFuelControlFuelCardProductRestrictionService, GazpromFuelCardProductRestrictionService>()
				.AddScoped<IFuelControlTransactionsDataService, GazpromTransactionsDataService>()
				.AddScoped<IFuelCardConverter, FuelCardConverter>()
				.AddScoped<IFuelLimitConverter, FuelLimitConverter>();
		}
	}
}
