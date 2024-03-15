using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using Vodovoz.Settings.Database.Cash;

namespace Vodovoz.Settings.Database
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddDatabaseSettings(this IServiceCollection services)
		{
			services.AddScoped<ISettingsController, SettingsController>();

			var settingsTypes = typeof(DependencyInjection).Assembly.GetTypes()
				.Where(t => t.IsClass
					&& t.Name.EndsWith("Settings")
					&& t.GetInterfaces().Any(i => i.Name == $"I{t.Name}"));

			foreach(var type in settingsTypes )
			{
				services.AddScoped(type.GetInterfaces().First(i => i.Name == $"I{type.Name}"), type);
			}

			return services;
		}

		public static IServiceCollection AddDatabaseSingletonSettings(this IServiceCollection services)
		{
			services.AddSingleton<ISettingsController, SettingsController>();

			var settingsTypes = typeof(DependencyInjection).Assembly.GetTypes()
				.Where(t => t.IsClass
					&& t.Name.EndsWith("Settings")
					&& t.GetInterfaces().Any(i => i.Name == $"I{t.Name}"));

			foreach(var type in settingsTypes)
			{
				services.AddSingleton(type.GetInterfaces().First(i => i.Name == $"I{type.Name}"), type);
			}

			return services;
		}

		public static IServiceCollection ConfigureSettingsOptions(this IServiceCollection services)
		{
			var settings = typeof(DependencyInjection).Assembly.GetTypes()
				.Where(t => t.IsClass && t.Name.EndsWith("Settings"))
				.ToList();

			foreach(var setting in settings)
			{
				var type = typeof(ConfigureDatabaseSettingsOptions<>).MakeGenericType(setting);

				OptionsServiceCollectionExtensions.ConfigureOptions(services, type);
			}

			return services;
		}
	}
}
