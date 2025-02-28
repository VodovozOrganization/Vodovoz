using Edo.Common;
using Edo.Problems;
using Edo.Transport;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using TrueMark.Codes.Pool;
using TrueMark.Library;

namespace Edo.Receipt.Dispatcher
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddEdoReceiptDispatcher(this IServiceCollection services)
		{
			services
				.AddHttpClient()
				;

			services
				.AddScoped<ReceiptEdoTaskHandler>()
				.AddScoped<ResaleReceiptEdoTaskHandler>()
				.AddScoped<ForOwnNeedsReceiptEdoTaskHandler>()
				.AddScoped<TrueMarkTaskCodesValidator>()
				.AddScoped<Tag1260Checker>()
				;

			services.AddEdo();
			services.AddEdoProblemRegistation();
			services.AddCodesPool();

			services.AddEdoMassTransit(configureBus: cfg =>
			{
				cfg.AddConsumers(Assembly.GetExecutingAssembly());
			});

			return services;
		}
	}
}
