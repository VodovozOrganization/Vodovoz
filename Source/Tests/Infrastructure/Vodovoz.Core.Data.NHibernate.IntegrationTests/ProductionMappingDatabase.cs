using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using NHibernate;
using QS.DomainModel.UoW;
using QS.Project.DB;
using System;
using Vodovoz.Core.Data.NHibernate.Mapping.Edo;
using Vodovoz.Core.Data.NHibernate.NhibernateExtensions;
using Vodovoz.Testing.Database;
using NhEnvironment = NHibernate.Cfg.Environment;

namespace Vodovoz.Core.Data.NHibernate.IntegrationTests
{
	/// <summary>
	/// База для smoke-теста на продовых маппингах (F-19). Схема всех сущностей не нужна:
	/// таблицы заказа и его кодов создаются вручную, без внешних ключей на остальные таблицы.
	/// </summary>
	public sealed class ProductionMappingDatabase : IDisposable
	{
		private readonly ISessionFactory _sessionFactory;

		public ProductionMappingDatabase()
		{
			Database = TestDatabase.Create("bulk_production");

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

			var configuration = Fluently.Configure()
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

			_sessionFactory = configuration.BuildSessionFactory();
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

		public void Dispose()
		{
			_sessionFactory.Dispose();
			Database.Dispose();
		}
	}
}
