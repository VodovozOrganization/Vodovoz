using Edo.Common;
using Edo.Problems;
using Edo.Transport;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using QS.DomainModel.UoW;
using System.Reflection;

namespace Edo.Admin
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddEdoAdminServices(this IServiceCollection services)
		{
			services.TryAddScoped<IUnitOfWork>(sp => sp.GetService<IUnitOfWorkFactory>().CreateWithoutRoot());

			services.TryAddScoped<EdoCancellationService>();
			services.TryAddScoped<IEdoCancellationValidator, EdoCancellationValidator>();

			services.AddEdo();
			services.AddEdoProblemRegistration();

			return services;
		}

		public static IServiceCollection AddEdoAdmin(this IServiceCollection services)
		{
			services.AddEdoAdminServices();

			services.AddEdoMassTransit(
				configureRabbit: (context, rabbitCfg) =>
				{
					// Обязателен, не отключать не сделав отправку сообщений после коммита
					rabbitCfg.UseInMemoryOutbox();
				},
				configureBus: cfg =>
				{
					cfg.AddConsumers(Assembly.GetExecutingAssembly());
				}
			);

			return services;
		}
	}
}
