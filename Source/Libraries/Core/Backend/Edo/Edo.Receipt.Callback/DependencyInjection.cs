using Microsoft.Extensions.DependencyInjection;
using Edo.Transport;
using ModulKassa;

namespace Edo.Receipt.Callback
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddReceiptCallback(this IServiceCollection services)
		{
			services.AddControllers();

			services.AddModulKassa();

			services.AddEdoMassTransit();

			return services;
		}
	}
}
