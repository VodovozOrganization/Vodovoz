using Autofac;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Reflection;

namespace Vodovoz.Settings.Database
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddSettingsFromDatabase(this IServiceCollection services)
		{
			services.AddSingleton<ISettingsController, SettingsController>();
			Assembly.GetExecutingAssembly()
				.GetTypes()
				.Where(a => a.Name.EndsWith("Settings") && !a.IsAbstract && !a.IsInterface)
				.Select(a => new { 
					assignedType = a, 
					serviceTypes = a.GetInterfaces().ToList() 
				}).ToList()
				.ForEach(typesToRegister =>
				{
					typesToRegister.serviceTypes.ForEach(typeToRegister => services.AddSingleton(typeToRegister, typesToRegister.assignedType));
				});

			return services;
		}
	}
}
