using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using System;
using Edo.Transport;
using ModulKassa;

namespace Edo.Receipt.Callback
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddReceiptCallback(
			this IServiceCollection services,
			Action<IBusRegistrationConfigurator> configureBus = null)
		{
			services.AddControllers();

			services.AddModulKassa();

			services.AddEdoMassTransit(configureBus: configureBus);

			return services;
		}
	}
}
