using Microsoft.Extensions.DependencyInjection;
using Vodovoz.Infrastructure.S3;

namespace Vodovoz.Infrastructure.FileStorage
{
	public static class DependencyInjection
    {
		public static IServiceCollection AddFileStorage(this IServiceCollection services)
			=> services.AddInfrastructureS3();
    }
}
