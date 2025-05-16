using MessageTransport;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using QS.DomainModel.UoW;
using ScannedTrueMarkCodesDelayedProcessing.Library.Workers;

namespace ScannedTrueMarkCodesDelayedProcessing.Library
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddScannedTrueMarkCodesDelayedProcessing(this IServiceCollection services)
		{
			services
				.AddMessageTransportSettings()
				.TryAddScoped<IUnitOfWork>(sp => sp.GetService<IUnitOfWorkFactory>().CreateWithoutRoot());

			services
				.AddHostedService<ScannedCodesDelayedProcessingWorker>();

			return services;
		}

	}
}
