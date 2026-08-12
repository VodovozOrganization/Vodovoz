using Edo.Common;
using Edo.Problems;
using Edo.Transport;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using QS.DomainModel.UoW;
using System.Reflection;
using Edo.Documents;
using Edo.Transport.Factories;

namespace Edo.Transfer.Dispatcher
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddEdoTransferDispatcherServices(this IServiceCollection services)
		{
			services.TryAddScoped<IUnitOfWork>(sp => sp.GetService<IUnitOfWorkFactory>().CreateWithoutRoot());

			services.TryAddScoped<TransferEdoHandler>();

			services
				.AddEdo()
				.AddEdoTransfer()
				.AddEdoProblemRegistration()
				.AddFaultServices()
				;

			return services;
		}

		public static IServiceCollection AddEdoTransferDispatcher(this IServiceCollection services)
		{
			services.AddEdoTransferDispatcherServices();

			services.AddEdoMassTransit(configureBus: cfg =>
			{
				cfg.AddConsumers(x => !x.ToString().Contains("Fault"), Assembly.GetExecutingAssembly());
			});

			return services;
		}

		public static IServiceCollection AddFaultServices(this IServiceCollection services)
		{
			services.TryAddScoped<FaultTransferDocumentAcceptedExceptionHandler>();
			services.TryAddScoped<IMassTransitExceptionInfoFactory, MassTransitExceptionInfoFactory>();
			
			return services;
		}
	}
}
