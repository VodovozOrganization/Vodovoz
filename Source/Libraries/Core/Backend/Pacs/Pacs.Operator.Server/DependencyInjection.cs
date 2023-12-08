using Microsoft.Extensions.DependencyInjection;
using Pacs.Server;
using Vodovoz.Settings.Pacs;

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
