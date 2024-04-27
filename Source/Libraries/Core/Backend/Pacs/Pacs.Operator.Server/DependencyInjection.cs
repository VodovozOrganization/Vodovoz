using Microsoft.Extensions.DependencyInjection;
using Pacs.Server;

namespace Pacs.Operators.Server
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddPacsOperatorServer(this IServiceCollection services)
		{
			services.AddControllers();
			services.AddPacsOperatorServices();

			return services;
		}
	}
}
