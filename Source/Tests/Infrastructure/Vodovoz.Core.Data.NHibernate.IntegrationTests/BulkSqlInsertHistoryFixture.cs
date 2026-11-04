using Autofac;
using MySqlConnector;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Event;
using QS.DomainModel.Tracking;
using QS.DomainModel.UoW;
using QS.HistoryLog;
using QS.HistoryLog.HibernateMapping;
using QS.Project.DB;
using QS.Project.HibernateMapping;
using QS.Project.Repositories;
using System;
using System.Reflection;
using System.Threading.Tasks;
using Vodovoz.Core.Data.NHibernate.IntegrationTests.Domain;
using Vodovoz.Testing.Database;
using Xunit;

namespace Vodovoz.Core.Data.NHibernate.IntegrationTests
{
	/// <summary>
	/// База с таблицами истории QS и настоящим трекером: те же слушатели, UoW и HibernateTracker, что в приложении.
	/// Обычный Save здесь пишет историю так же, как на проде, и служит эталоном для массовой вставки.
	/// </summary>
	public class BulkSqlInsertHistoryFixture : MariaDbFixture
	{
		private IContainer _container;
		private TrackerFactory _trackerFactory;
		private Func<int> _previousUserId;

		protected override string DatabaseName => "bulk_history";

		protected override Assembly[] MappingAssemblies => new[]
		{
			typeof(BulkParentMap).Assembly,
			typeof(ChangeSetMap).Assembly,
			typeof(UserBaseMap).Assembly
		};

		/// <summary>
		/// UoW без трекера: для него и обычный Save историю не пишет
		/// </summary>
		public IUnitOfWorkFactory UntrackedUnitOfWorkFactory { get; private set; }

		protected override void ConfigureNHibernate(Configuration configuration)
		{
			var tracker = new GlobalUowEventsTracker();
			configuration.AppendListeners(ListenerType.PostInsert, new IPostInsertEventListener[] { tracker });
			configuration.AppendListeners(ListenerType.PostUpdate, new IPostUpdateEventListener[] { tracker });
			configuration.AppendListeners(ListenerType.PostDelete, new IPostDeleteEventListener[] { tracker });
		}

		protected override IUnitOfWorkFactory CreateUnitOfWorkFactory(ISessionFactory sessionFactory)
		{
			var builder = new ContainerBuilder();
			builder.RegisterInstance<ISessionProvider>(new DefaultSessionProvider(sessionFactory));
			builder.RegisterType<SingleUowEventsTracker>().AsSelf().InstancePerDependency();
			_container = builder.Build();

			UntrackedUnitOfWorkFactory = new NotTrackedUnitOfWorkFactory(new DefaultSessionProvider(sessionFactory));

			_trackerFactory = new TrackerFactory(new MySqlConnectionStringBuilder(Database.ConnectionString));
			SingleUowEventsTracker.RegisterSingleUowListnerFactory(_trackerFactory);

			_previousUserId = UserRepository.GetCurrentUserId;
			UserRepository.GetCurrentUserId = () => TestUserId;

			return new TrackedUnitOfWorkFactory(_container);
		}

		public const int TestUserId = 1;

		/// <summary>
		/// Отключает трекер истории на время теста: UoW отслеживается, но HibernateTracker к нему не подключается.
		/// Так настроены воркеры с AddTrackedUoW без AddStaticHistoryTracker
		/// </summary>
		public void DisableHistoryTracker() => SingleUowEventsTracker.UnregisterSingleUowListnerFactory(_trackerFactory);

		public void EnableHistoryTracker() => SingleUowEventsTracker.RegisterSingleUowListnerFactory(_trackerFactory);

		public override async Task InitializeAsync()
		{
			await base.InitializeAsync();

			// В маппинге QS длина значений не задана, и схема из него дает varchar(255), а значение поля истории бывает длиннее
			Database?.Execute("ALTER TABLE history_changes MODIFY old_value TEXT NULL, MODIFY new_value TEXT NULL");
		}

		public override async Task DisposeAsync()
		{
			if(_trackerFactory != null)
			{
				SingleUowEventsTracker.UnregisterSingleUowListnerFactory(_trackerFactory);
				UserRepository.GetCurrentUserId = _previousUserId;
			}

			_container?.Dispose();

			await base.DisposeAsync();
		}
	}

	/// <summary>
	/// Тесты истории идут последовательно: журнал SQL и текущий пользователь общие.
	/// </summary>
	[CollectionDefinition(Name, DisableParallelization = true)]
	public class BulkSqlInsertHistoryCollection : ICollectionFixture<BulkSqlInsertHistoryFixture>
	{
		public const string Name = "BulkSqlInsertHistory";
	}
}
