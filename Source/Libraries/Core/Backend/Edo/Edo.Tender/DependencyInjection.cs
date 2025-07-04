using System.Reflection;
using Edo.Common;
using Edo.Problems;
using Edo.Tender;
using Edo.Transport;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using QS.DomainModel.UoW;

namespace Edo.Documents
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddTenderEdoServices(this IServiceCollection services)
		{
			services.TryAddScoped<IUnitOfWork>(sp => sp.GetService<IUnitOfWorkFactory>().CreateWithoutRoot());

			services.TryAddScoped<TenderEdoTaskHandler>();

			services.AddEdo();
			services.AddEdoProblemRegistration();

			return services;
		}

		public static IServiceCollection AddTenderEdo(this IServiceCollection services)
		{
			services.AddTenderEdoServices();

			services.AddEdoMassTransit(configureBus: cfg => { cfg.AddConsumers(Assembly.GetExecutingAssembly()); });

			return services;
		}
	}
}
