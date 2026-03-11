using System;
using Mango.Business.Interfaces;
using Mango.Business.Models;
using Mango.Business.Services;
using Mango.Contracts.V1.Options;
using Mango.Infrastructure.Clients;
using Mango.Infrastructure.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Mango.Infrastructure.DependencyInjection
{
	public static class ServiceCollectionExtensions
	{
		public static IServiceCollection AddMangoServices(
			this IServiceCollection services,
			IConfiguration configuration)
		{
			services.Configure<MangoOptions>(
				configuration.GetSection(MangoOptions.SectionName));

			services.Configure<SyncOptions>(
				configuration.GetSection(SyncOptions.SectionName));

			services.Configure<DatabaseOptions>(
				configuration.GetSection(DatabaseOptions.SectionName));

			services.AddSingleton<ICallStatisticParser, CallStatisticParser>();

			services.AddHttpClient<IMangoApiClient, MangoApiClient>((provider, client) =>
			{
				var options = configuration
					.GetSection(MangoOptions.SectionName)
					.Get<MangoOptions>() ?? new MangoOptions();

				client.BaseAddress = new Uri(options.BaseUrl);
				client.Timeout = TimeSpan.FromSeconds(60);
			});

			services.AddScoped<ICallStatisticService, CallStatisticService>();
			
			services.AddScoped<ICallStatisticRepository, CallStatisticRepository>();
			services.AddScoped<ISyncStateRepository, SyncStateRepository>();

			return services;
		}
	}
}
