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
	public class TrueMarkCodesPoolManager
	{
		private const string _poolTableName = "true_mark_codes_pool_new";

		private readonly IUnitOfWorkFactory _uowFactory;

		public TrueMarkCodesPoolManager(IUnitOfWorkFactory uowFactory)
		{
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
		}

		public IEnumerable<int> SelectCodes(int count, bool promoted)
		{
			using(var uow = CreateUow())
			{
				var query = GetSelectCodesQuery(uow, count, promoted);
				var result = query.List<uint>().Select(x => (int)x);
				return result;
			}
		}

		public async Task<IEnumerable<int>> SelectCodesAsync(int count, bool promoted, CancellationToken cancellationToken)
		{
			using(var uow = CreateUow())
			{
				var query = GetSelectCodesQuery(uow, count, promoted);
				var result = await query.ListAsync<uint>(cancellationToken);
				return result.Select(x => (int)x);
			}
		}

		private IQuery GetSelectCodesQuery(IUnitOfWork uow, int count, bool promoted)
		{
			var sql = $@"
					SELECT code_id FROM {_poolTableName}
					WHERE promoted = :promoted 
					ORDER BY adding_time DESC
					LIMIT :count
					;";
			var query = uow.Session.CreateSQLQuery(sql)
				.SetParameter("count", count)
				.SetParameter("promoted", promoted);
			return query;
		}

		public void PromoteCodes(IEnumerable<int> codeIds, int extraSecond)
		{
			using(var uow = CreateUow())
			{
				uow.OpenTransaction();

				var query = GetPromoteSelectForUpdateQuery(uow, codeIds);
				var codesToUpdate = query.List<uint>().ToArray();

				var updateQuery = GetPromoteUpdateQuery(uow, codesToUpdate, extraSecond);
				updateQuery.ExecuteUpdate();

				uow.Commit();
			}
		}

		public async Task PromoteCodesAsync(IEnumerable<int> codeIds, int extraSecond, CancellationToken cancellationToken)
		{
			using(var uow = CreateUow())
			{
				uow.OpenTransaction();

				var query = GetPromoteSelectForUpdateQuery(uow, codeIds);
				var codesToUpdateList = await query.ListAsync<uint>(cancellationToken);
				var codesToUpdate = codesToUpdateList.ToArray();

				var updateQuery = GetPromoteUpdateQuery(uow, codesToUpdate, extraSecond);
				await updateQuery.ExecuteUpdateAsync(cancellationToken);

				await uow.CommitAsync(cancellationToken);
			}
		}

		private IQuery GetPromoteSelectForUpdateQuery(IUnitOfWork uow, IEnumerable<int> codeIds)
		{
			var sql = $@"
					SELECT code_id FROM {_poolTableName}
					WHERE code_id in (:code_ids)
					FOR UPDATE SKIP LOCKED
					;";
			var query = uow.Session.CreateSQLQuery(sql)
				.SetParameterList("code_ids", codeIds.ToArray());
			return query;
		}

		private IQuery GetPromoteUpdateQuery(IUnitOfWork uow, uint[] codesToUpdate, int extraSecond)
		{
			var updateSql = $@"
					UPDATE {_poolTableName} SET
						adding_time = ADDDATE(current_timestamp(), INTERVAL :extra_second SECOND),
						promoted = 1
					WHERE code_id in (:code_ids)
					;";
			var updateQuery = uow.Session.CreateSQLQuery(updateSql)
				.SetParameterList("code_ids", codesToUpdate)
				.SetParameter("extra_second", extraSecond);
			return updateQuery;
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
					INNER JOIN true_mark_identification_code tmic ON tmic.id = pool.code_id
					GROUP BY tmic.gtin
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
