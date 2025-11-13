using Edo.Problems;
using Edo.Transfer.Sender;
using Edo.Transport;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using QS.DomainModel.UoW;
using System.Reflection;

namespace Edo.Transfer.Dispatcher
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddEdoTransferSenderServices(this IServiceCollection services)
		{
			services.TryAddScoped<IUnitOfWork>(sp => sp.GetService<IUnitOfWorkFactory>().CreateWithoutRoot());

			services.AddEdoTransfer();
			services.AddEdoProblemRegistration();

			services.TryAddScoped<TransferSender>();
			services.TryAddScoped<TransferSendPreparer>();

			return services;
		}

		public static IServiceCollection AddEdoTransferSender(this IServiceCollection services)
		{
			services.AddEdoTransferSenderServices();

			services.AddEdoMassTransit(configureBus: cfg =>
			{
				cfg.AddConsumers(Assembly.GetExecutingAssembly());
			});

			return services;
		}
	}
}
