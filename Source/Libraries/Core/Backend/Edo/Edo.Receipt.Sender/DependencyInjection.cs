using Edo.Common;
using Edo.Problems;
using Edo.Transport;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using ModulKassa;

namespace Edo.Receipt.Sender
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddEdoReceiptSender(this IServiceCollection services)
		{
			services.AddModulKassa();

			services
				.AddScoped<FiscalDocumentFactory>()
				.AddScoped<ReceiptSender>()
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
