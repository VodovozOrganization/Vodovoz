using Edo.Common;
using Edo.InformalOrderDocuments.Factories;
using Edo.InformalOrderDocuments.Handlers;
using Edo.Transport;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MySqlConnector;
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

			services.AddScoped<IInformalOrderDocumentHandlerFactory, InformalOrderDocumentHandlerFactory>();
			services.AddScoped<IInformalOrderDocumentFileDataFactory, InformalOrderDocumentFileDataFactory>();
			services.AddScoped<IInformalOrderDocumentHandler, EquipmentTransferDocumentHandler>();
			services.AddScoped<OrderDocumentEdoTaskHandler>();
			services.TryAddScoped<IPrintableDocumentSaver>(sp =>
			{
				var connectionStringBuilder = sp.GetRequiredService<MySqlConnectionStringBuilder>();
				return new PrintableDocumentSaver(connectionStringBuilder);
			});

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
