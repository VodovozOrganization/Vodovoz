using Microsoft.Extensions.DependencyInjection;
using QS.Project.Core;
using System.Linq;
using System.Reflection;

namespace Vodovoz.Settings.Database
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddDatabaseSettings(this IServiceCollection services)
		{
			services.AddScoped<ISettingsController, SettingsController>();

			services.AddMappingAssemblies(Assembly.GetExecutingAssembly());

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

			services.AddMappingAssemblies(Assembly.GetExecutingAssembly());

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
	}
}
