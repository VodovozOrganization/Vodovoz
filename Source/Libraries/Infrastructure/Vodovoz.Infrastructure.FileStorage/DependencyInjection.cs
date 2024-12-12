using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using Vodovoz.Infrastructure.S3;

namespace Vodovoz.Infrastructure.FileStorage
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddFileStorage(this IServiceCollection services)
			=> services
				.AddInfrastructureS3()
				.AddFileStorageServices();

		public static IServiceCollection AddFileStorageServices(this IServiceCollection services)
		{
			var fileStorageServiceTypes = typeof(DependencyInjection).Assembly.GetTypes()
				.Where(t => t.Name.EndsWith("FileStorageService")
					&& !t.IsAbstract);

			foreach(var fileStorageService in fileStorageServiceTypes)
			{
				var fileStorageServiceInterface = fileStorageService.GetInterfaces().FirstOrDefault(i => i.Name == $"I{fileStorageService.Name}");

				if(fileStorageServiceInterface != null)
				{
					services.AddScoped(fileStorageServiceInterface, fileStorageService);
				}
			}

			return services;
		}
	}
}
