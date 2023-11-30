using Microsoft.Extensions.DependencyInjection;
using Vodovoz.Controllers;
using Vodovoz.Core.DataService;
using Vodovoz.Domain.WageCalculation.CalculationServices.RouteList;
using Vodovoz.EntityRepositories.Goods;
using Vodovoz.EntityRepositories.Profitability;
using Vodovoz.EntityRepositories.WageCalculation;
using Vodovoz.Factories;
using Vodovoz.Parameters;
using Vodovoz.Services;
using Vodovoz.Tools.Logistic;

namespace Vodovoz
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddBusiness(this IServiceCollection services)
		{
			services.AddScoped<IRouteListAddressKeepingDocumentController, RouteListAddressKeepingDocumentController>();
			services.AddScoped<IWageParameterService, WageParameterService>();
			services.AddScoped<IDeliveryRulesParametersProvider, DeliveryRulesParametersProvider>();
			services.AddScoped<IRouteListProfitabilityController, RouteListProfitabilityController>();
			services.AddScoped<RouteGeometryCalculator>();
			services.AddScoped<IDistanceCalculator>(sp => sp.GetService<RouteGeometryCalculator>());
			services.AddScoped<IWageCalculationRepository, WageCalculationRepository>();
			services.AddScoped<IWageParametersProvider, BaseParametersProvider>();
			services.AddScoped<IRouteListProfitabilityFactory, RouteListProfitabilityFactory>();
			services.AddScoped<IProfitabilityConstantsRepository, ProfitabilityConstantsRepository>();
			services.AddScoped<IRouteListProfitabilityRepository, RouteListProfitabilityRepository>();
			services.AddScoped<INomenclatureRepository, NomenclatureRepository>();

			return services;
		}
	}
}
