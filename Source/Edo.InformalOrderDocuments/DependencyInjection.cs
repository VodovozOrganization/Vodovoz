using Edo.Common;
using Edo.Transport;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using QS.DomainModel.UoW;
using System.Reflection;

namespace Edo.InformalOrderDocuments
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddInformalOrderDocumentEdoServices(this IServiceCollection services)
		{
			services.TryAddScoped(sp => sp.GetService<IUnitOfWorkFactory>().CreateWithoutRoot());

			services.AddEdo();

			return services;
		}

		public static IServiceCollection AddInformalOrderDocumentEdo(this IServiceCollection services)
		{
			services.AddInformalOrderDocumentEdoServices();

			services.AddEdoMassTransit(configureBus: cfg => { cfg.AddConsumers(Assembly.GetExecutingAssembly()); });

			return services;
		}
	}
}
