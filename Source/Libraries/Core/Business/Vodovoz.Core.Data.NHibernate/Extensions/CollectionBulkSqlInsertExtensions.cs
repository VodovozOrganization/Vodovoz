using NHibernate;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Vodovoz.Core.Data.NHibernate.Extensions
{
	/// <summary>
	/// Массовая вставка новых дочерних сущностей в one-to-many коллекцию родителя многострочными INSERT
	/// с загрузкой вставленных строк в сессию и в коллекцию
	/// </summary>
	/// <remarks>
	/// Единственный разрешенный сценарий: родитель находится в сессии переданного UoW, коллекция смаплена как one-to-many
	/// bag или set без индекса и без Where, дочерняя сущность хранится в одной таблице, не версионируется,
	/// ее Id смаплен как GeneratedBy.Native() (GeneratedBy.Identity() во время работы от него неотличим и тоже допускается).
	/// Все остальное заканчивается исключением. Пустой список новых элементов: метод ничего не делает.
	/// <para>Порядок работы: проверки → открытие транзакции UoW → Flush → блокировка родителя → INSERT → загрузка → сверка количества.</para>
	/// <para>Ссылка элемента на родителя:</para>
	/// <list type="bullet">
	/// <item>пустая: элемент привязывается к родителю коллекции, в FK пишется его Id, а после загрузки ссылка указывает на него;</item>
	/// <item>на этого же родителя (в том числе его прокси): допустимо;</item>
	/// <item>на другого родителя: <see cref="ArgumentException"/> до первого INSERT.</item>
	/// </list>
	/// <para>Транзакция:</para>
	/// <list type="bullet">
	/// <item>если у UoW нет открытой транзакции, метод открывает ее через <see cref="IUnitOfWork.OpenTransaction"/>. Строки без транзакции не вставляются никогда;</item>
	/// <item>транзакция, открытая в обход UoW (на сессии, на соединении или TransactionScope), дает <see cref="InvalidOperationException"/>:
	/// uow.Commit() такую транзакцию не закоммитит;</item>
	/// <item>метод сам не коммитит. Строки видны другим сессиям только после общего uow.Commit().</item>
	/// </list>
	/// <para>Блокировка родителя: до первого INSERT выполняется session.Lock. Родитель с Version: LockMode.Force,
	/// версия с проверкой увеличивается на 1 (повторный вызов в той же транзакции версию уже не меняет).
	/// Родитель без Version: LockMode.Upgrade, то есть SELECT ... FOR UPDATE. Строка родителя остается заблокированной до конца транзакции UoW.
	/// Причина: другая транзакция не должна одновременно менять родителя или массово вставлять в его коллекцию.
	/// Вторая такая вставка ждет конца первой, а для родителя с Version завершается ошибкой устаревшей версии.
	/// От вставки строк в обход этого метода блокировка не защищает.</para>
	/// <para>Ошибки:</para>
	/// <list type="bullet">
	/// <item>проверки до начала записи бросают стандартные исключения: <see cref="ArgumentNullException"/>, <see cref="ArgumentOutOfRangeException"/>,
	/// <see cref="ArgumentException"/>, <see cref="InvalidOperationException"/>, <see cref="NotSupportedException"/>, <see cref="TransientObjectException"/>.
	/// Транзакция при этом не откатывается, а незаписанные изменения сессии не уходят в БД;</item>
	/// <item>любая ошибка начиная с Flush и расхождение количества после всех вставок: транзакция UoW принудительно откатывается
	/// вместе с остальными изменениями этого UoW и бросается <see cref="BulkSqlInsertException"/> с исходным исключением внутри.
	/// После этого UoW нужно закрыть: uow.Commit() молча ничего не сделает;</item>
	/// <item>отмена токена после начала записи тоже откатывает транзакцию, но остается <see cref="OperationCanceledException"/>.</item>
	/// </list>
	/// <para>История изменений: после загрузки строк метод поднимает те же события PostInsert, что и NHibernate при обычном Save,
	/// поэтому для сущностей с [HistoryTrace] историю записывает трекер QS: после коммита, отдельным соединением, одним набором изменений
	/// на коммит вместе с остальными изменениями этого UoW. Если история в приложении не включена, ее нет, как и при обычном Save.
	/// Если история включена, а текущий пользователь не задан, до начала записи бросается <see cref="InvalidOperationException"/>,
	/// иначе трекер упал бы уже после коммита строк.</para>
	/// <para>Важно:</para>
	/// <list type="bullet">
	/// <item>Каскады и SessionScopeEntitySaved для новых строк не срабатывают.</item>
	/// <item>Переданные объекты не меняются и остаются транзиентными, после вставки работать нужно с элементами коллекции.</item>
	/// <item>Повторная передача тех же объектов вставит строки еще раз: метод не отличает дубль от намеренной копии, за это отвечает вызывающий код.</item>
	/// </list>
	/// </remarks>
	public static class CollectionBulkSqlInsertExtensions
	{
		private const int _defaultBatchSize = 1000;
		private const string _maxPacketQuery = "SELECT CAST(@@max_allowed_packet AS SIGNED)";

		/// <summary>
		/// Вставляет новые элементы в one-to-many коллекцию многострочными INSERT и загружает их в сессию и в коллекцию.
		/// Поведение для ссылки на родителя, транзакции, блокировки и ошибок описано у <see cref="CollectionBulkSqlInsertExtensions"/>
		/// </summary>
		/// <param name="collection">Коллекция родителя из сессии <paramref name="uow"/>, например <c>parent.Items</c></param>
		/// <param name="uow">UoW, в сессии которого находится родитель</param>
		/// <param name="newItems">Новые, еще не сохраненные элементы</param>
		/// <param name="batchSize">Максимальное количество строк в одном INSERT. Фактическая пачка может быть меньше из-за лимитов БД</param>
		public static void BulkSqlInsert<TItem>(
			this ICollection<TItem> collection,
			IUnitOfWork uow,
			IEnumerable<TItem> newItems,
			int batchSize = _defaultBatchSize)
			where TItem : class
		{
			var context = BulkInsertContext<TItem>.Create(collection, uow, newItems, batchSize, CancellationToken.None);

			if(context == null)
			{
				return;
			}

			context.OpenTransaction();

			try
			{
				context.SetStage("запись изменений сессии (Flush)");
				context.Session.Flush();

				context.SetStage("блокировка родителя");
				context.Session.Lock(context.Owner, context.ParentLockMode);

				context.SetStage("загрузка коллекции перед вставкой");
				NHibernateUtil.Initialize(collection);
				context.RememberExistingItems();

				context.SetStage("определение лимита размера пачки");
				var maxPacket = context.UsesMaxPacket
					? (long?)Convert.ToInt64(context.Session.CreateSQLQuery(_maxPacketQuery).UniqueResult())
					: null;

				context.SetStage("вставка строк");
				var affectedRows = 0;

				foreach(var command in context.CreateCommands(maxPacket))
				{
					try
					{
						affectedRows += context.Batcher.ExecuteNonQuery(command);
					}
					finally
					{
						context.Batcher.CloseCommand(command, null);
					}
				}

				context.EnsureAffectedRows(affectedRows);

				context.SetStage("загрузка вставленных строк");
				var items = context.Session.CreateFilter(collection, string.Empty).List<TItem>();
				var orderedIds = context.CreateOrderQuery()?.List<object>();

				context.SetStage("сверка количества");
				context.SynchronizeCollection(items, orderedIds);

				context.SetStage("передача записей в трекер истории");
				context.RaisePostInsertEvents();
			}
			catch(OperationCanceledException)
			{
				context.Rollback();
				throw;
			}
			catch(Exception ex)
			{
				var rollbackError = context.Rollback();
				throw context.CreateFailure(ex, rollbackError);
			}
		}

		/// <summary>
		/// Вставляет новые элементы в one-to-many коллекцию многострочными INSERT и загружает их в сессию и в коллекцию.
		/// Поведение для ссылки на родителя, транзакции, блокировки и ошибок описано у <see cref="CollectionBulkSqlInsertExtensions"/>
		/// </summary>
		/// <param name="collection">Коллекция родителя из сессии <paramref name="uow"/>, например <c>parent.Items</c></param>
		/// <param name="uow">UoW, в сессии которого находится родитель</param>
		/// <param name="newItems">Новые, еще не сохраненные элементы</param>
		/// <param name="batchSize">Максимальное количество строк в одном INSERT. Фактическая пачка может быть меньше из-за лимитов БД</param>
		/// <param name="cancellationToken">Токен отмены. Отмена после начала записи откатывает транзакцию UoW</param>
		public static async Task BulkSqlInsertAsync<TItem>(
			this ICollection<TItem> collection,
			IUnitOfWork uow,
			IEnumerable<TItem> newItems,
			int batchSize = _defaultBatchSize,
			CancellationToken cancellationToken = default)
			where TItem : class
		{
			var context = BulkInsertContext<TItem>.Create(collection, uow, newItems, batchSize, cancellationToken);

			if(context == null)
			{
				return;
			}

			context.OpenTransaction();

			try
			{
				context.SetStage("запись изменений сессии (Flush)");
				await context.Session.FlushAsync(cancellationToken);

				context.SetStage("блокировка родителя");
				await context.Session.LockAsync(context.Owner, context.ParentLockMode, cancellationToken);

				context.SetStage("загрузка коллекции перед вставкой");
				await NHibernateUtil.InitializeAsync(collection, cancellationToken);
				context.RememberExistingItems();

				context.SetStage("определение лимита размера пачки");
				var maxPacket = context.UsesMaxPacket
					? (long?)Convert.ToInt64(await context.Session.CreateSQLQuery(_maxPacketQuery).UniqueResultAsync(cancellationToken))
					: null;

				context.SetStage("вставка строк");
				var affectedRows = 0;

				foreach(var command in context.CreateCommands(maxPacket))
				{
					try
					{
						cancellationToken.ThrowIfCancellationRequested();
						affectedRows += await context.Batcher.ExecuteNonQueryAsync(command, cancellationToken);
					}
					finally
					{
						context.Batcher.CloseCommand(command, null);
					}
				}

				context.EnsureAffectedRows(affectedRows);

				context.SetStage("загрузка вставленных строк");
				var items = await context.Session.CreateFilter(collection, string.Empty).ListAsync<TItem>(cancellationToken);
				var orderQuery = context.CreateOrderQuery();
				var orderedIds = orderQuery == null ? null : await orderQuery.ListAsync<object>(cancellationToken);

				context.SetStage("сверка количества");
				context.SynchronizeCollection(items, orderedIds);

				context.SetStage("передача записей в трекер истории");
				context.RaisePostInsertEvents();
			}
			catch(OperationCanceledException)
			{
				await context.RollbackAsync();
				throw;
			}
			catch(Exception ex)
			{
				var rollbackError = await context.RollbackAsync();

				if(cancellationToken.IsCancellationRequested)
				{
					throw new OperationCanceledException(cancellationToken);
				}

				throw context.CreateFailure(ex, rollbackError);
			}
		}
	}
}
