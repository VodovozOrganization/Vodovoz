using Edo.Transport;
using MessageTransport;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using QS.DomainModel.UoW;
using ScannedTrueMarkCodesDelayedProcessing.Library.Services;
using Vodovoz.Models.TrueMark;
using VodovozBusiness.Services.TrueMark;

namespace ScannedTrueMarkCodesDelayedProcessing.Library
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddScannedTrueMarkCodesDelayedProcessing(this IServiceCollection services)
		{
			services.TryAddScoped<IUnitOfWork>(sp => sp.GetService<IUnitOfWorkFactory>().CreateWithoutRoot());

			services
				.AddMessageTransportSettings()
				.AddEdoMassTransit()
				.AddScoped<TrueMarkWaterCodeParser>()
				.AddScoped<TrueMarkCodesChecker>()
				.AddScoped<IRouteListItemTrueMarkProductCodesProcessingService, RouteListItemTrueMarkProductCodesProcessingService>()
				.AddScoped<ScannedCodesDelayedProcessingService>();

			return services;
		}

	}
}
