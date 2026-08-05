using Edo.Admin;
using Edo.Common;
using Edo.Documents.Services;
using Edo.Problems;
using Edo.Transport;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using QS.DomainModel.UoW;
using System.Reflection;
using Edo.Transport.Factories;
using TrueMark.Codes.Pool;

namespace Edo.Documents
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddEdoDocumentsServices(this IServiceCollection services)
		{
			services.TryAddScoped<IUnitOfWork>(sp => sp.GetService<IUnitOfWorkFactory>().CreateWithoutRoot());

			services.TryAddScoped<IUpdDocumentBuilder, UpdDocumentBuilder>();

			services.TryAddScoped<DocumentEdoTaskHandler>();
			services.TryAddScoped<ForOwnNeedDocumentEdoTaskHandler>();
			services.TryAddScoped<ForResaleDocumentEdoTaskHandler>();
			services.TryAddScoped<WithdrawalEdoRequestHandler>();

			services.AddEdo()
				.AddCodesPool()
				.AddEdoProblemRegistration()
				.AddEdoAdminServices()
				.AddFaultServices();
			
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
		
		public static IServiceCollection AddFaultServices(this IServiceCollection services)
		{
			services
				.AddScoped<FaultTransferCompleteExceptionHandler>()
				.AddScoped<FaultDocumentTaskCreatedExceptionHandler>()
				.AddScoped<FaultOrderDocumentSentExceptionHandler>()
				.AddScoped<FaultOrderDocumentAcceptedExceptionHandler>()
				;
			
			services.TryAddScoped<IMassTransitExceptionInfoFactory, MassTransitExceptionInfoFactory>();

			return services;
		}
	}
}
