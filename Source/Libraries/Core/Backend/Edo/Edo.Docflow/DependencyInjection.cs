using Edo.Docflow.Factories;
using Edo.Docflow.Handlers;
using Edo.Transport;
using Edo.Transport.Factories;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using QS.DomainModel.UoW;
using System.Reflection;
using Vodovoz.Core.Domain.Controllers;

namespace Edo.Docflow
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddEdoDocflowServices(this IServiceCollection services)
		{
			services.TryAddScoped(sp => sp.GetService<IUnitOfWorkFactory>().CreateWithoutRoot());

			services.TryAddScoped<DocflowStatusUpdateHandler>();
			services.TryAddScoped<DocflowSendHandler>();
			services.TryAddScoped<DocflowCancellationHandler>();
			services.TryAddScoped<OrderUpdInfoFactory>();
			services.TryAddScoped<TransferOrderUpdInfoFactory>();
			services.TryAddScoped<InfoForCreatingEdoEquipmentTransferFactory>();
			services.TryAddScoped<IInfoForCreatingEdoInformalOrderDocumentFactory, InfoForCreatingEdoEquipmentTransferFactory>();
			services.TryAddScoped<ICounterpartyEdoAccountEntityController, CounterpartyEdoAccountEntityController>();
			services.TryAddScoped<IInformalOrderDocumentHandlerFactory, InformalOrderDocumentHandlerFactory>();
			services.TryAddScoped<IInformalOrderDocumentHandler, EquipmentTransferDocumentHandler>();
			services.AddFaultServices();
			
			return services;
		}

		public static IServiceCollection AddEdoDocflow(this IServiceCollection services)
		{
			services.AddEdoDocflowServices();

			services.AddEdoMassTransit(

				configureRabbit: (context, rabbitCfg) =>
				{
					// Обязателен, не отключать не сделав отправку сообщений после коммита
					rabbitCfg.UseInMemoryOutbox();
				},
				configureBus: cfg =>
				{
					cfg.AddConsumers(x => !x.ToString().Contains("Fault"), Assembly.GetExecutingAssembly());
				}
			);


			services.AddEdoNotifications();

			return services;
		}

		public static IServiceCollection AddFaultServices(this IServiceCollection services)
		{
			services.TryAddScoped<FaultOrderDocumentSendExceptionHandler>();
			services.TryAddScoped<FaultTransferDocumentSendExceptionHandler>();
			services.TryAddScoped<IMassTransitExceptionInfoFactory, MassTransitExceptionInfoFactory>();
			
			return services;
		}
	}
}
