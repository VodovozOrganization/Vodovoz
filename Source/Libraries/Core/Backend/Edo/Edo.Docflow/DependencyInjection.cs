using Edo.Docflow;
using Edo.Docflow.Factories;
using Edo.Problems;
using Edo.Transport;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using QS.DomainModel.UoW;
using System.Reflection;

namespace Edo.Documents
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddEdoDocflowServices(this IServiceCollection services)
		{
			services.TryAddScoped<IUnitOfWork>(sp => sp.GetService<IUnitOfWorkFactory>().CreateWithoutRoot());

			services.TryAddScoped<DocflowHandler>();
			services.TryAddScoped<OrderUpdInfoFactory>();
			services.TryAddScoped<TransferOrderUpdInfoFactory>();
			services.AddEdoProblemRegistation();

			return services;
		}

		public static IServiceCollection AddEdoDocflow(this IServiceCollection services)
		{
			services.AddEdoDocflowServices();

			services.AddEdoMassTransit(configureBus: cfg =>
			{
				cfg.AddConsumers(Assembly.GetExecutingAssembly());
			});

			return services;
		}
	}
}
