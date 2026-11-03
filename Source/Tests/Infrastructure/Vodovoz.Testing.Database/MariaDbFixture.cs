using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Tool.hbm2ddl;
using QS.DomainModel.UoW;
using QS.Extensions.Observable.Collections.List;
using QS.Project.DB;
using System.Reflection;
using System.Threading.Tasks;
using Vodovoz.Core.Data.NHibernate.NhibernateExtensions;
using Xunit;
using NhEnvironment = NHibernate.Cfg.Environment;

namespace Vodovoz.Testing.Database
{
	/// <summary>
	/// База со схемой из маппингов и фабрикой настоящих UoW с настройками как на проде.
	/// Схема строится один раз на коллекцию тестов.
	/// </summary>
	public abstract class MariaDbFixture : IAsyncLifetime
	{
		protected abstract string DatabaseName { get; }

		protected abstract Assembly[] MappingAssemblies { get; }

		public TestDatabase Database { get; private set; }

		public Configuration Configuration { get; private set; }

		public ISessionFactory SessionFactory { get; private set; }

		public IUnitOfWorkFactory UnitOfWorkFactory { get; private set; }

		public Task InitializeAsync()
		{
			if(!IsServerExpected)
			{
				return Task.CompletedTask;
			}

			Database = TestDatabase.Create(DatabaseName);
			Configuration = BuildConfiguration();

			using(var connection = Database.OpenConnection())
			{
				new SchemaExport(Configuration).Execute(false, true, false, connection, null);
			}

			SessionFactory = Configuration.BuildSessionFactory();
			UnitOfWorkFactory = new NotTrackedUnitOfWorkFactory(new DefaultSessionProvider(SessionFactory));

			return Task.CompletedTask;
		}

		public Task DisposeAsync()
		{
			SessionFactory?.Dispose();
			Database?.Dispose();

			return Task.CompletedTask;
		}

		private static bool IsServerExpected => MariaDbServer.IsCi || MariaDbServer.UnavailableReason == null;

		private Configuration BuildConfiguration()
		{
			var sqlConfiguration = MySQLConfiguration.Standard
				.Dialect<MySQL57SpatialExtendedDialect>()
				.ConnectionString(Database.ConnectionString)
				.Driver<CountingDriver>()
				.AdoNetBatchSize(100);

			return Fluently.Configure()
				.Database(sqlConfiguration)
				.Mappings(mappings =>
				{
					mappings.FluentMappings.Conventions.Add<ObservableListConvention>();

					foreach(var assembly in MappingAssemblies)
					{
						mappings.FluentMappings.AddFromAssembly(assembly);
					}
				})
				.ExposeConfiguration(configuration =>
					configuration.SetProperty(NhEnvironment.BatchStrategy, typeof(AffectedRowsBatcherFactory).AssemblyQualifiedName))
				.BuildConfiguration();
		}
	}

	/// <summary>
	/// Тест с БД: пропускается локально, если тестовый сервер запустить нельзя. В CI пропуска нет, тест падает.
	/// </summary>
	public sealed class DatabaseFactAttribute : FactAttribute
	{
		public DatabaseFactAttribute()
		{
			if(!MariaDbServer.IsCi)
			{
				Skip = MariaDbServer.UnavailableReason;
			}
		}
	}

	/// <summary>
	/// Параметризованный тест с БД: пропускается локально, если тестовый сервер запустить нельзя. В CI пропуска нет.
	/// </summary>
	public sealed class DatabaseTheoryAttribute : TheoryAttribute
	{
		public DatabaseTheoryAttribute()
		{
			if(!MariaDbServer.IsCi)
			{
				Skip = MariaDbServer.UnavailableReason;
			}
		}
	}
}
