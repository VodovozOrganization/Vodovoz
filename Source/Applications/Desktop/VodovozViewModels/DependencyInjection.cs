using EdoService.Library;
using GeoCoderApi.Client.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Vodovoz.Application.BankStatements;
using Vodovoz.Presentation.ViewModels;
using Vodovoz.Presentation.ViewModels.Factories;
using Vodovoz.ViewModels.Factories;
using Vodovoz.ViewModels.Options;
using Vodovoz.ViewModels.Services.RouteOptimization;

namespace Vodovoz.ViewModels
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddVodovozViewModels(this IServiceCollection services) =>
			services.AddScoped<IDialogSettingsFactory, DialogSettingsFactory>()
				.AddScoped<BankStatementHandler>()
				.AddScoped<BankStatementParser>()
				.AddScoped<IRouteOptimizer, RouteOptimizer>()
				.AddGeoCoderClient()
				.AddPresentationViewModels()
				.AddEdoServicesLibrary()
				.ConfigureOptions<ConfigureGeoCoderApiOptions>();
	}
}
