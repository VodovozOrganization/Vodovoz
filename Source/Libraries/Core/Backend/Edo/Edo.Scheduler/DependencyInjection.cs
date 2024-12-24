using Edo.Docflow.Consumers;
using Edo.Docflow.Consumers.Definitions;
using Edo.Scheduler.Service;
using Edo.Transport;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;

namespace Edo.Scheduler
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddEdoScheduler(this IServiceCollection services)
		{
			services
				.AddScoped<EdoTaskScheduler>()
				.AddScoped<OrderTaskScheduler>()
				.AddScoped<BillForAdvanceEdoRequestTaskScheduler>()
				.AddScoped<BillForDebtEdoRequestTaskScheduler>()
				.AddScoped<BillForPaymentEdoRequestTaskScheduler>()
				;

			services.AddEdoMassTransit(configureBus: cfg =>
			{
				cfg.AddConsumer<EdoRequestCreatedConsumer, EdoRequestCreatedConsumerDefinition>();
			});

			return services;
		}
	}
}
