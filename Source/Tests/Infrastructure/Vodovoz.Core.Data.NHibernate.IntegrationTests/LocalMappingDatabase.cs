using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Dialect;
using QS.DomainModel.UoW;
using QS.Project.DB;
using System;
using System.Linq;
using Vodovoz.Core.Data.NHibernate.Mapping.Edo;
using Vodovoz.Core.Data.NHibernate.NhibernateExtensions;
using Vodovoz.Testing.Database;
using NhEnvironment = NHibernate.Cfg.Environment;

namespace Vodovoz.Core.Data.NHibernate.IntegrationTests
{
	/// <summary>
	/// База для smoke-теста на маппингах приложения (F-19). Схема всех сущностей не нужна:
	/// таблицы заказа и его кодов создаются вручную, без внешних ключей на остальные таблицы.
	/// </summary>
	public sealed class LocalMappingDatabase : IDisposable
	{
		private readonly ISessionFactory _sessionFactory;
		private readonly Configuration _configuration;

		public LocalMappingDatabase()
		{
			Database = TestDatabase.Create("bulk_local");

			Database.Execute(@"CREATE TABLE edo_transfer_orders (
				id INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
				date DATETIME NULL,
				seller_organization_id INT NULL,
				customer_organization_id INT NULL,
				transfer_document_id INT NULL)");
			Database.Execute(@"CREATE TABLE edo_transfer_order_codes (
				id INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
				transfer_order_id INT NULL,
				nomenclature_id INT NULL,
				quantity DECIMAL(16,3) NULL,
				individual_code_id INT NULL,
				group_code_id INT NULL)");

			_configuration = Fluently.Configure()
				.Database(MySQLConfiguration.Standard
					.Dialect<MySQL57SpatialExtendedDialect>()
					.ConnectionString(Database.ConnectionString)
					.Driver<CountingDriver>()
					.AdoNetBatchSize(100))
				.Mappings(mappings =>
				{
					mappings.FluentMappings.Conventions.Add<QS.Extensions.Observable.Collections.List.ObservableListConvention>();
					mappings.FluentMappings.AddFromAssembly(typeof(TransferOrderMap).Assembly);
					mappings.FluentMappings.AddFromAssembly(typeof(QS.Banks.HMap.BankMap).Assembly);
					mappings.FluentMappings.AddFromAssembly(typeof(QS.BusinessCommon.HMap.MeasurementUnitsMap).Assembly);
				})
				.ExposeConfiguration(x => x.SetProperty(NhEnvironment.BatchStrategy, typeof(AffectedRowsBatcherFactory).AssemblyQualifiedName))
				.BuildConfiguration();

			_sessionFactory = _configuration.BuildSessionFactory();
			UnitOfWorkFactory = new NotTrackedUnitOfWorkFactory(new DefaultSessionProvider(_sessionFactory));
		}

		public TestDatabase Database { get; }

		public IUnitOfWorkFactory UnitOfWorkFactory { get; }

		public int CreateTransferOrder()
		{
			Database.Execute("INSERT INTO edo_transfer_orders (date) VALUES ('2026-01-01')");

			using(var connection = Database.OpenConnection())
			{
				return (int)TestDatabase.Scalar(connection, System.Data.IsolationLevel.ReadCommitted, "SELECT MAX(id) FROM edo_transfer_orders");
			}
		}

		/// <summary>
		/// Создаёт организацию с версией и настройками ЭДО. Таблицы берутся из схемы маппинга приложения, только нужные для загрузки организации
		/// </summary>
		public int CreateOrganization(string jurAddress, string edoAccount)
		{
			var tables = new[] { "organizations", "organization_versions", "taxcom_edo_settings", "accounts", "banks", "bank_regions", "banks_cor_accounts" };
			var script = _configuration.GenerateSchemaCreationScript(Dialect.GetDialect(_configuration.Properties));

			foreach(var statement in script.Where(x => tables.Any(table => x.StartsWith($"create table {table} ", StringComparison.OrdinalIgnoreCase))))
			{
				Database.Execute(statement);
			}

			Database.Execute("INSERT INTO organizations (id, name) VALUES (1, 'Организация')");
			Database.Execute($"INSERT INTO organization_versions (organization_id, jur_address, start_date) VALUES (1, '{jurAddress}', '2000-01-01')");
			Database.Execute($"INSERT INTO taxcom_edo_settings (organization_id, edo_account) VALUES (1, '{edoAccount}')");

			return 1;
		}

		/// <summary>
		/// Создаёт таблицы УПД из схемы маппинга приложения
		/// </summary>
		public void CreateUpdTables()
		{
			var script = _configuration.GenerateSchemaCreationScript(Dialect.GetDialect(_configuration.Properties));

			foreach(var statement in script.Where(x => x.StartsWith("create table edo_upd_", StringComparison.OrdinalIgnoreCase)))
			{
				Database.Execute(statement);
			}

			// Схема из маппинга дает для Guid колонку binary(16), а драйвер передает его строкой
			Database.Execute("ALTER TABLE edo_upd_documents MODIFY guid CHAR(36) NULL");
		}

		public void Dispose()
		{
			_sessionFactory.Dispose();
			Database.Dispose();
		}
	}
}
