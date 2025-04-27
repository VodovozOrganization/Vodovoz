using Edo.Transport;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using QS.DomainModel.UoW;
using System.Reflection;
using Edo.Common;
using TrueMark.Codes.Pool;

namespace Edo.CodesSaver
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddCodesSaverServices(this IServiceCollection services)
		{
			services.TryAddScoped<IUnitOfWork>(x => x.GetService<IUnitOfWorkFactory>().CreateWithoutRoot());

			services.TryAddScoped<SaveCodesEventHandler>();
			services.TryAddScoped<ISaveCodesService, SaveCodesService>();
			services.AddCodesPool();

			return services;
		}

		public static IServiceCollection AddCodesSaver(this IServiceCollection services)
		{
			services.AddCodesSaverServices();

			services.AddEdoMassTransit(configureBus: cfg =>
			{
				cfg.AddConsumers(Assembly.GetExecutingAssembly());
			});

			return services;
		}
	}
}
