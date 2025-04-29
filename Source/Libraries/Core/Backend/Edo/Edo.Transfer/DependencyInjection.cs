using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using QS.DomainModel.UoW;

namespace Edo.Transfer
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddEdoTransfer(this IServiceCollection services)
		{
			services.TryAddScoped<IUnitOfWork>(sp => sp.GetService<IUnitOfWorkFactory>().CreateWithoutRoot());

			services.TryAddScoped<TransferDispatcher>();
			services.TryAddScoped<TransferTaskRepository>();

			return services;
		}
	}
}
