using Email.Infrastructure.Factories;
using Email.Infrastructure.Generators;
using Email.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Email.Infrastructure
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddEmailInfrastructure(this IServiceCollection services)
		{
			services
				.AddScoped<IEmailMessageFactory, EmailMessageFactory>()
				.AddScoped<IDatabaseRepository, DataBaseRepositiry>()
				.AddScoped<IEmailLinkGenerator, EmailLinkGenerator>();

			return services;
		}
	}
}
