using BitrixApi.Library.Services;
using Microsoft.Extensions.DependencyInjection;

namespace BitrixApi.Library
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddBitrixApiServices(this IServiceCollection services)
		{
			services
				.AddScoped<IEmailAttachmentsCreateService, EmailAttachmentsCreateService>();

			return services;
		}
	}
}
