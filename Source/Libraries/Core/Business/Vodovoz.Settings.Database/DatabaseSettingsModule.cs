using Autofac;
using System.Linq;

namespace Vodovoz.Settings.Database
{
	public class DatabaseSettingsModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<SettingsController>()
				.AsSelf()
				.AsImplementedInterfaces()
				.InstancePerLifetimeScope();

			builder.RegisterAssemblyTypes(typeof(DatabaseSettingsModule).Assembly)
				.Where(t => t.Name.EndsWith("Settings"))
				.AsSelf()
				.AsImplementedInterfaces()
				.InstancePerLifetimeScope();
		}
	}
}
