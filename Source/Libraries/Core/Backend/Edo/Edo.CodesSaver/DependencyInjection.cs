using Edo.Transport;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
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

			services.AddEdoMassTransit(configureBus: cfg =>
			{
				cfg.AddConsumers(Assembly.GetExecutingAssembly());
				//cfg.AddConsumer<SaveCodesTaskCreatedEventConsumer, SaveCodesTaskCreatedEventConsumerDefinition>();
			});

			return services;
		}
	}
}
