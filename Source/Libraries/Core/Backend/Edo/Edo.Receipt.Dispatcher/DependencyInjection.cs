using Edo.Common;
using Edo.Problems;
using Edo.Transport;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Edo.Receipt.Dispatcher
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddEdoReceiptDispatcher(this IServiceCollection services)
		{
			services
				.AddScoped<ReceiptEdoTaskHandler>()
				.AddScoped<ResaleReceiptEdoTaskHandler>()
				.AddScoped<ForOwnNeedsReceiptEdoTaskHandler>()
				;

			services.AddEdo();
			services.AddEdoProblemRegistation();

			services.AddEdoMassTransit(configureBus: cfg =>
			{
				cfg.AddConsumers(Assembly.GetExecutingAssembly());
			});

			return services;
		}
	}
}
