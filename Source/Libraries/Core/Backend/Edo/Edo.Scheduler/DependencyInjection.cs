using Edo.Scheduler.Service;
using Edo.Transport;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using QS.DomainModel.UoW;
using System.Reflection;

namespace Edo.Scheduler
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddEdoSchedulerServices(this IServiceCollection services)
		{
			services.TryAddScoped<IUnitOfWork>(sp => sp.GetService<IUnitOfWorkFactory>().CreateWithoutRoot());

			services.TryAddScoped<EdoTaskScheduler>();
			services.TryAddScoped<OrderTaskScheduler>();
			services.TryAddScoped<BillForAdvanceEdoRequestTaskScheduler>();
			services.TryAddScoped<BillForDebtEdoRequestTaskScheduler>();
			services.TryAddScoped<BillForPaymentEdoRequestTaskScheduler>();

			return services;
		}

		public static IServiceCollection AddEdoScheduler(this IServiceCollection services)
		{
			services.AddEdoSchedulerServices();

			services.AddEdoMassTransit(configureBus: cfg =>
			{
				cfg.AddConsumers(Assembly.GetExecutingAssembly());
			});

			return services;
		}
	}
}
