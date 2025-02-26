using Edo.Transport;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using QS.DomainModel.UoW;
using System.Reflection;
using TrueMark.Codes.Pool;

namespace Edo.CodesSaver
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddCodesSaver(this IServiceCollection services)
		{
			services
				.AddScoped<SaveCodesEventHandler>()
				.AddCodesPool()
				;

			services.TryAddScoped<IUnitOfWork>(x => x.GetService<IUnitOfWorkFactory>().CreateWithoutRoot());

			services.AddEdoMassTransit(configureBus: cfg =>
			{
				cfg.AddConsumers(Assembly.GetExecutingAssembly());
				//cfg.AddConsumer<SaveCodesTaskCreatedEventConsumer, SaveCodesTaskCreatedEventConsumerDefinition>();
			});

			return services;
		}
	}
}
