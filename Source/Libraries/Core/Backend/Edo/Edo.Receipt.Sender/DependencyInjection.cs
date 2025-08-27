using Edo.Common;
using Edo.Problems;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ModulKassa;
using QS.DomainModel.UoW;

namespace Edo.Receipt.Sender
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddEdoReceiptSenderServices(this IServiceCollection services)
		{
			services.TryAddScoped<IUnitOfWork>(sp => sp.GetService<IUnitOfWorkFactory>().CreateWithoutRoot());

			services.AddModulKassa();

			services.TryAddScoped<FiscalDocumentFactory>();
			services.TryAddScoped<ReceiptSender>();

			services.AddEdo();
			services.AddEdoProblemRegistration();

			return services;
		}

		public static IServiceCollection AddEdoReceiptSender(this IServiceCollection services)
		{
			services.AddEdoReceiptSenderServices();

			return services;
		}
	}
}
