using FluentNHibernate.Cfg.Db;
using FluentNHibernate.Conventions;
using Microsoft.Extensions.DependencyInjection;
using QS.Extensions.Observable.Collections.List;
using QS.Project;
using QS.Project.Core;
using QS.Project.DB;
using System.Linq;
using System.Reflection;
using Vodovoz.Core.Data.NHibernate.NhibernateExtensions;
using Vodovoz.Settings.Database;
using MySqlConnectionStringBuilder = MySqlConnector.MySqlConnectionStringBuilder;

namespace Vodovoz.Core.Data.NHibernate
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddCoreDataNHibernate(this IServiceCollection services)
		{
			services.AddMappingAssemblies(Assembly.GetExecutingAssembly());

			return services;
		}

		public static IServiceCollection AddSpatialSqlConfiguration(this IServiceCollection services)
		{

			services.AddSingleton<MySQLConfiguration>((provider) =>
			{
				var connectionStringBuilder = provider.GetRequiredService<MySqlConnectionStringBuilder>();
				var dbConfig = MySQLConfiguration.Standard
					.Dialect<MySQL57SpatialExtendedDialect>()
					.ConnectionString(connectionStringBuilder.ConnectionString)
					.AdoNetBatchSize(100)
					.Driver<LoggedMySqlClientDriver>()
				;
				return dbConfig;
			});

			return services;
		}

		public static IServiceCollection AddDatabaseConnection(this IServiceCollection services)
		{
			services
				.AddCoreDataNHibernate()
				.AddDatabaseConnectionSettings()
				.AddDatabaseConnectionString()
				.AddSpatialSqlConfiguration()
				.AddNHibernateConfiguration()
				.AddNHibernateConventions()
				.AddDatabaseInfo()
				.AddDatabaseSingletonSettings()
				;

			services.AddStaticServicesConfig();

			return services;
		}

		public static IServiceCollection AddNHibernateConventions(this IServiceCollection services)
		{
			services.AddSingleton<IConvention, ObservableListConvention>();

			return services;
		}
		
		public static IServiceCollection AddCoreDataRepositories(this IServiceCollection services)
		{
			var settingsTypes = typeof(DependencyInjection).Assembly.GetTypes()
				.Where(t => t.IsClass
							&& t.Name.EndsWith("Repository")
							&& t.GetInterfaces().Any(i => i.Name == $"I{t.Name}"));

			foreach(var type in settingsTypes)
			{
				services.AddScoped(type.GetInterfaces().First(i => i.Name == $"I{type.Name}"), type);
			}
			
			return services;
		}
	}
}
