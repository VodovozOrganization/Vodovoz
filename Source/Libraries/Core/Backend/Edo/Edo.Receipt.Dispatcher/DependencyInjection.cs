using Edo.Common;
using Edo.Problems;
using Edo.Transport;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using QS.DomainModel.UoW;
using System.Reflection;
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

			services.AddEdo();
			services.AddEdoProblemRegistration();
			services.AddCodesPool();

			return services;
		}

		public static IServiceCollection AddEdoReceiptDispatcher(this IServiceCollection services)
		{
			services.AddEdoReceiptDispatcherServices();

			services.AddEdoMassTransit(configureBus: cfg =>
			{
				cfg.AddConsumers(Assembly.GetExecutingAssembly());
			});

			return services;
		}
	}
}
