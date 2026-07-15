using NHibernate;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace TrueMark.Codes.Pool
{
	/// <summary>
	/// Предоставляет возможность управлять пулом кодов, такие как: <br/>
	/// - выборка <br/>
	/// - предоставление кол-ва по gtin<br/>
	/// - продвижения кодов в верх пула <br/>
	/// - удаления <br/>
	/// </summary>
	public class TrueMarkCodesPoolManager : ITrueMarkCodesPoolManager
	{
		private const string _poolTableName = "true_mark_codes_pool_new";

		private readonly IUnitOfWorkFactory _uowFactory;

		public TrueMarkCodesPoolManager(IUnitOfWorkFactory uowFactory)
		{
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
		}

		public async Task<IEnumerable<int>> SelectCodesForCheckAsync(int count, CancellationToken cancellationToken)
		{
			using(var uow = CreateUow())
			{
				var sql = $@"
					SELECT code_id FROM {_poolTableName}
					WHERE expiration_date IS NULL
					ORDER BY adding_time DESC
					LIMIT :count
					;";
				var query = uow.Session.CreateSQLQuery(sql)
					.SetParameter("count", count);
				var result = await query.ListAsync<uint>(cancellationToken);
				return result.Select(x => (int)x);
			}
		}

		public async Task UpdateCodesExpirationAsync(
			IDictionary<int, DateTime> codeExpirationMap,
			CancellationToken cancellationToken)
		{
			using(var uow = CreateUow())
			{
				uow.OpenTransaction();

				foreach(var kvp in codeExpirationMap)
				{
					var sql = $@"
						UPDATE {_poolTableName}
						SET expiration_date = :expirationDate
						WHERE code_id = :codeId
						;";
					var query = uow.Session.CreateSQLQuery(sql)
						.SetParameter("codeId", kvp.Key)
						.SetParameter("expirationDate", kvp.Value);

					await query.ExecuteUpdateAsync(cancellationToken);
				}

				await uow.CommitAsync(cancellationToken);
			}
		}

		public async Task<int> DeleteExpiredCodesAsync(CancellationToken cancellationToken)
		{
			using(var uow = CreateUow())
			{
				uow.OpenTransaction();

				var sql = $@"
					DELETE FROM {_poolTableName}
					WHERE expiration_date IS NOT NULL 
						AND expiration_date < NOW()
					;";
				var query = uow.Session.CreateSQLQuery(sql);
				var deletedCount = await query.ExecuteUpdateAsync(cancellationToken);

				await uow.CommitAsync(cancellationToken);
				return deletedCount;
			}
		}

		public void DeleteCodes(IEnumerable<int> codeIds)
		{
			using(var uow = CreateUow())
			{
				uow.OpenTransaction();

				var query = GetDeleteCodesQuery(uow, codeIds);
				query.ExecuteUpdate();
				uow.Commit();
			}
		}

		public async Task DeleteCodesAsync(IEnumerable<int> codeIds, CancellationToken cancellationToken)
		{
			using(var uow = CreateUow())
			{
				uow.OpenTransaction();

				var query = GetDeleteCodesQuery(uow, codeIds);
				await query.ExecuteUpdateAsync(cancellationToken);
				await uow.CommitAsync(cancellationToken);
			}
		}

		private IQuery GetDeleteCodesQuery(IUnitOfWork uow, IEnumerable<int> codeIds)
		{
			var sql = $@"
					DELETE FROM {_poolTableName}
					WHERE code_id in (:code_ids)
					;";
			var query = uow.Session.CreateSQLQuery(sql)
				.SetParameterList("code_ids", codeIds.ToArray());
			return query;
		}

		public int GetTotalCount()
		{
			using(var uow = CreateUow())
			{
				var query = GetTotalCountQuery(uow);
				var result = (int)query.UniqueResult<long>();
				return result;
			}
		}

		public async Task<int> GetTotalCountAsync(CancellationToken cancellationToken)
		{
			using(var uow = CreateUow())
			{
				var query = GetTotalCountQuery(uow);
				var result = await query.UniqueResultAsync<long>(cancellationToken);
				return (int)result;
			}
		}

		private IQuery GetTotalCountQuery(IUnitOfWork uow)
		{
			var sql = $@"SELECT Count(id) FROM {_poolTableName};";
			var query = uow.Session.CreateSQLQuery(sql);
			return query;
		}

		public IDictionary<string, long> GetTotalCountByGtin()
		{
			using(var uow = CreateUow())
			{
				var query = GetTotalCountByGtinQuery(uow);
				var objects = query.List<object[]>();
				var result = objects.ToDictionary(x => (string)x[0], x => (long)x[1]);
				return result;
			}
		}

		public async Task<IDictionary<string, long>> GetTotalCountByGtinAsync(CancellationToken cancellationToken)
		{
			using(var uow = CreateUow())
			{
				var query = GetTotalCountByGtinQuery(uow);
				var objects = await query.ListAsync<object[]>(cancellationToken);
				var result = objects.ToDictionary(x => (string)x[0], x => (long)x[1]);
				return result;
			}
		}

		private IQuery GetTotalCountByGtinQuery(IUnitOfWork uow)
		{
			var sql = $@"
					SELECT
						tmic.gtin,
						Count(pool.code_id)
					FROM {_poolTableName} pool
					GROUP BY pool.gtin
					;";
			var query = uow.Session.CreateSQLQuery(sql);
			return query;
		}

		private IUnitOfWork CreateUow()
		{
			var uow = _uowFactory.CreateWithoutRoot();
			SetSessionTimeout(uow);

			return uow;
		}

		private void SetSessionTimeout(IUnitOfWork uow)
		{
			using(var command = uow.Session.Connection.CreateCommand())
			{
				command.CommandText = $"SET SESSION wait_timeout = 30;";
				command.ExecuteNonQuery();
			}
		}
	}
}
