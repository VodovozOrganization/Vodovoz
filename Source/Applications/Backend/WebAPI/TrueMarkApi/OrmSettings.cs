using Core.Infrastructure;
using FluentNHibernate.Cfg.Db;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MySqlConnector;
using NHibernate.Cfg;
using QS.Project.DB;
using System;
using System.Collections.Generic;
using System.Reflection;
using Vodovoz.Core.Data.NHibernate.Options;
using Vodovoz.Data.NHibernate.NhibernateExtensions;

namespace TrueMarkApi
{
	public class OrmSettings : IOrmSettings
	{
		public IDataBaseInfo DataBaseInfo { get; private set; }

		public MySQLConfiguration GetDatabaseConfiguration(IServiceProvider provider)
		{
			var connectionSettings = provider.GetRequiredService<DatabaseConnectionSettings>();
			var dbSection = connectionSettings.GetSection("DomainDB");

			var builder = new MySqlConnectionStringBuilder();
			builder.Server = dbSection.GetValue<string>("Server");
			builder.Port = dbSection.GetValue<uint>("Port");
			builder.Database = dbSection.GetValue<string>("Database");
			builder.UserID = dbSection.GetValue<string>("UserID");
			builder.Password = dbSection.GetValue<string>("Password");
			builder.SslMode = MySqlSslMode.None;

			var connectionString = builder.GetConnectionString(true);

			var dbConfig = MySQLConfiguration.Standard
				.Dialect<MySQL57SpatialExtendedDialect>()
				.ConnectionString(connectionString)
				.AdoNetBatchSize(100)
				.Driver<LoggedMySqlClientDriver>()
			;

			DataBaseInfo = new DatabaseInfo(builder.Database);

			return dbConfig;
		}
		public void ExposeConfiguration(Configuration config)
		{
		}

		public IEnumerable<Assembly> GetMappingAssemblies()
		{
			yield return typeof(Vodovoz.Core.Data.NHibernate.Mapping.Pacs.OperatorMap).Assembly;
			yield return typeof(Vodovoz.Settings.Database.SettingMap).Assembly;
		}
	}
}
