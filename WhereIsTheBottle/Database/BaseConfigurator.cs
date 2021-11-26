using System;
using System.Reflection;
using System.Security;
using FluentNHibernate.Cfg.Db;
using NHibernate.AdoNet;
using NLog;
using QS.Attachments.Domain;
using QS.Banks.Domain;
using QS.Project.DB;
using QS.Project.HibernateMapping;
using QS.Utilities.Text;
using Vodovoz.HibernateMapping;
using Vodovoz.NhibernateExtensions;
using Vodovoz.Tools;

namespace WhereIsTheBottle.Database
{
	public class BaseConfigurator : IBaseConnector
	{
		private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
		private readonly IOrmConfig _ormConfig;

		public BaseConfigurator(IOrmConfig ormConfig)
		{
			_ormConfig = ormConfig ?? throw new ArgumentNullException(nameof(ormConfig));
		}

		public void Connect(string server, string databaseName, string user, SecureString password)
		{
			var connectionString =
				$"server={server};" +
				"port=3306;" +
				$"database={databaseName};" +
				$"user id={user};" +
				$"password={password?.ToPlainString()};" +
				"sslmode=None;" +
				"ConnectionTimeout=120";

			Connect(connectionString);
		}

		public void Connect(string connectionString)
		{
			_logger.Info("Настройка параметров базы...");

			var dbConfig = MySQLConfiguration.Standard
				.Dialect<MySQL57SpatialExtendedDialect>()
				.ConnectionString(connectionString)
				.AdoNetBatchSize(100)
				.Driver<LoggedMySqlClientDriver>();

			_ormConfig.ConfigureOrm(
				dbConfig,
				new[]
				{
					Assembly.GetAssembly(typeof(OrganizationMap)),
					Assembly.GetAssembly(typeof(UserBaseMap)),
					Assembly.GetAssembly(typeof(Bank)),
					Assembly.GetAssembly(typeof(Attachment))
				},
				cnf => cnf.DataBaseIntegration(dbi =>
				{
					dbi.BatchSize = 100;
					dbi.Batcher<MySqlClientBatchingBatcherFactory>();
				})
			);
		}

		public bool TryConnect(string server, string databaseName, string user, SecureString password)
		{
			var connectionString =
				$"server={server};" +
				"port=3306;" +
				$"database={databaseName};" +
				$"user id={user};" +
				$"password={password.ToPlainString()};" +
				"sslmode=None;" +
				"ConnectionTimeout=120";

			return TryConnect(connectionString);
		}

		public bool TryConnect(string connectionString)
		{
			try
			{
				Connect(connectionString);
			}
			catch(Exception ex)
			{
				_logger.Error(ex, "Ошибка при настройке базы");
				return false;
			}
			return true;
		}
	}
}
