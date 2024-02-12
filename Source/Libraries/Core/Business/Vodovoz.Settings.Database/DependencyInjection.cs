using Microsoft.Extensions.DependencyInjection;
using System.Linq;

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
	}
}
