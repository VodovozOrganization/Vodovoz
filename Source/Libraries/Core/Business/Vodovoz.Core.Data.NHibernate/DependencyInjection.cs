using FluentNHibernate.Cfg.Db;
using Microsoft.Extensions.DependencyInjection;
using QS.Project.Core;
using QS.Project.DB;
using Vodovoz.Core.Data.NHibernate.NhibernateExtensions;
using MySqlConnectionStringBuilder = MySqlConnector.MySqlConnectionStringBuilder;

namespace Vodovoz.Core.Data.NHibernate
{
	public static class DependencyInjection
	{
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
				.AddDatabaseConnectionSettings()
				.AddDatabaseConnectionString()
				.AddSpatialSqlConfiguration()
				.AddNHibernateConfiguration()
				.AddDatabaseInfo()
				;
			return services;
		}
	}
}
