using Edo.Transport;
using MessageTransport;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using QS.DomainModel.UoW;
using ScannedTrueMarkCodesDelayedProcessing.Library.Services;
using TrueMark.Codes.Pool;
using Vodovoz.Models.TrueMark;
using VodovozBusiness.Models.TrueMark;
using VodovozBusiness.Services.TrueMark;
using MassTransit;
using System;

namespace ScannedTrueMarkCodesDelayedProcessing.Library
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddScannedTrueMarkCodesDelayedProcessing(
			this IServiceCollection services,
			Action<IBusRegistrationConfigurator> configureBus)
		{
			services.TryAddScoped<IUnitOfWork>(sp => sp.GetService<IUnitOfWorkFactory>().CreateWithoutRoot());
			services.AddScoped<TrueMarkCodesPoolFactory>();

			services
				.AddMessageTransportSettings()
				.AddEdoMassTransit(configureBus: configureBus)
				.AddCodesPool()
				.AddScoped<TrueMarkWaterCodeParser>()
				.AddScoped<ITrueMarkCodesChecker, TrueMarkCodesChecker>()
				.AddScoped<IRouteListItemTrueMarkProductCodesProcessingService, RouteListItemTrueMarkProductCodesProcessingService>()
				.AddScoped<ITrueMarkCodesPoolCleanupService, TrueMarkCodesPoolCleanupService>()
				.AddScoped<ScannedCodesDelayedProcessingService>();

			return services;
		}

	}
}
