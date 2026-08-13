using Edo.Admin;
using Edo.Common;
using Edo.Problems;
using Edo.Transport;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ModulKassa;
using QS.DomainModel.UoW;
using System.Reflection;
using Edo.Transport.Factories;

namespace Edo.Receipt.Sender
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddEdoReceiptSenderServices(this IServiceCollection services)
		{
			services.TryAddScoped<IUnitOfWork>(sp => sp.GetService<IUnitOfWorkFactory>().CreateWithoutRoot());

			services.AddModulKassa();

			services.TryAddScoped<FiscalDocumentFactory>();
			services.TryAddScoped<ReceiptSendingFailedNotificationService>();
			services.TryAddScoped<ReceiptSender>();

			services
	.AddEdo()
	.AddEdoProblemRegistration()
	.AddEdoAdminServices()
	.AddEdoNotifications()
	.AddFaultServices()
	;

			return services;
		}

		public static IServiceCollection AddEdoReceiptSender(this IServiceCollection services)
		{
			services.AddEdoReceiptSenderServices();

			services.AddEdoMassTransit(configureBus: cfg =>
			{
				cfg.AddConsumers(x => !x.ToString().Contains("Fault"), Assembly.GetExecutingAssembly());
			});

			return services;
		}

		public static IServiceCollection AddFaultServices(this IServiceCollection services)
		{
			services.TryAddScoped<FaultReceiptReadyToSendExceptionHandler>();
			services.TryAddScoped<IMassTransitExceptionInfoFactory, MassTransitExceptionInfoFactory>();

			return services;
		}
	}
}
