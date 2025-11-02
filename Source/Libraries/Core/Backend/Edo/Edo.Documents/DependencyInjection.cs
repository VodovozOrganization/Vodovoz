using Edo.Common;
using Edo.Problems;
using Edo.Transport;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using QS.DomainModel.UoW;
using System.Reflection;
using TrueMark.Codes.Pool;

namespace Edo.Documents
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddEdoDocumentsServices(this IServiceCollection services)
		{
			services.TryAddScoped<IUnitOfWork>(sp => sp.GetService<IUnitOfWorkFactory>().CreateWithoutRoot());

			services.TryAddScoped<DocumentEdoTaskHandler>();
			services.TryAddScoped<ForOwnNeedDocumentEdoTaskHandler>();
			services.TryAddScoped<ForResaleDocumentEdoTaskHandler>();

			services.AddEdo();
			services.AddCodesPool();
			services.AddEdoProblemRegistration();

			return services;
		}

		public static IServiceCollection AddEdoDocuments(this IServiceCollection services)
		{
			services.AddEdoDocumentsServices();

			services.AddEdoMassTransit(configureBus: cfg =>
			{
				cfg.AddConsumers(Assembly.GetExecutingAssembly());
			});

			return services;
		}
	}
}
