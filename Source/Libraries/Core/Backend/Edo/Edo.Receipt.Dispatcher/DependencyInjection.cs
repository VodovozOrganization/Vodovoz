using Edo.Common;
using Edo.Problems;
using Edo.Transport;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using QS.DomainModel.UoW;
using System.Reflection;
using Edo.Admin;
using Edo.Transport.Factories;
using TrueMark.Codes.Pool;
using TrueMark.Library;

namespace Edo.Receipt.Dispatcher
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddEdoReceiptDispatcherServices(this IServiceCollection services)
		{
			services.TryAddScoped<IUnitOfWork>(sp => sp.GetService<IUnitOfWorkFactory>().CreateWithoutRoot());

			services.AddHttpClient();

			services.TryAddScoped<ReceiptEdoTaskHandler>();
			services.TryAddScoped<ResaleReceiptEdoTaskHandler>();
			services.TryAddScoped<ForOwnNeedsReceiptEdoTaskHandler>();
			services.TryAddScoped<Tag1260Checker>();
			services.TryAddScoped<ISaveCodesService, SaveCodesService>();

			services
				.AddEdo()
				.AddEdoProblemRegistration()
				.AddCodesPool()
				.AddEdoAdminServices()
				.AddFaultServices()
				;

			return services;
		}

		public static IServiceCollection AddEdoReceiptDispatcher(this IServiceCollection services)
		{
			services.AddEdoReceiptDispatcherServices();

			services.AddEdoMassTransit(configureBus: cfg =>
			{
				cfg.AddConsumers(x => !x.ToString().Contains("Fault"), Assembly.GetExecutingAssembly());
			});

			return services;
		}

		public static IServiceCollection AddFaultServices(this IServiceCollection services)
		{
			services.TryAddScoped<FaultReceiptTaskCreatedExceptionHandler>();
			services.TryAddScoped<IMassTransitExceptionInfoFactory, MassTransitExceptionInfoFactory>();

			return services;
		}
	}
}
