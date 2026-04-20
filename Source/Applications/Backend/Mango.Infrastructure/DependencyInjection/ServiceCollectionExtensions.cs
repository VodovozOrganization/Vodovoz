using System;
using System.Text.Json;
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
		public static IServiceCollection AddMangoStatisticsServices(
			this IServiceCollection services,
			IConfiguration configuration)
		{
			services.Configure<MangoOptions>(
				configuration.GetSection(MangoOptions.SectionName));

			services.Configure<SyncOptions>(
				configuration.GetSection(SyncOptions.SectionName));

			services.Configure<DatabaseOptions>(
				configuration.GetSection(DatabaseOptions.SectionName));
			
			services.Configure<MangoGroupOptions>(
				configuration.GetSection(MangoGroupOptions.SectionName));
			
			services.Configure<JsonSerializerOptions>(
				configuration.GetSection("JsonOptions"));

			services.AddSingleton<ICallStatisticParser, CallStatisticParser>();

			services.AddHttpClient<IMangoApiClient, MangoApiClient>((_, client) =>
			{
				client.Timeout = TimeSpan.FromSeconds(120);
			});

			services.AddScoped<ICallStatisticService, CallStatisticService>();
			
			services.AddScoped<ICallStatisticRepository, CallStatisticRepository>();
			services.AddScoped<ISyncStateRepository, SyncStateRepository>();
			
			services.AddSingleton<IMangoReferenceDataBuilder, MangoReferenceDataBuilder>();

			return services;
		}
	}
}
