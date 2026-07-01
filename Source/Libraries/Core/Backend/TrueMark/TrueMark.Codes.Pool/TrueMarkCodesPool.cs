using MySqlConnector;
using NHibernate;
using QS.DomainModel.UoW;
using QS.Utilities.Debug;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace TrueMark.Codes.Pool
{
	/// <summary>
	/// Предоставляет возможность добавлять и забирать коды из пула кодов 
	/// </summary>
	public class TrueMarkCodesPool : ITrueMarkCodesPool
	{
		protected const string _poolTableName = "true_mark_codes_pool_new";
		protected readonly IUnitOfWork UoW;

		public TrueMarkCodesPool(IUnitOfWork uow)
		{
			UoW = uow ?? throw new ArgumentNullException(nameof(uow));
			SetSessionTimeout(UoW);
			UoW.OpenTransaction();
		}

		public virtual void PutCode(int codeId)
		{
			var query = GetPutCodeQuery(codeId);
			try
			{
				query.ExecuteUpdate();
			}
			catch(Exception ex)
			{
				var mySqlException = ex.FindExceptionTypeInInner<MySqlException>();

				if(mySqlException != null && mySqlException.Number == (int)MySqlErrorCode.DuplicateKeyEntry)
				{
					return;
				}
			}
		}

		public virtual async Task PutCodeAsync(int codeId, CancellationToken cancellationToken)
		{
			var query = GetPutCodeQuery(codeId);
			try
			{
				await query.ExecuteUpdateAsync(cancellationToken);
			}
			catch(Exception ex)
			{
				var mySqlException = ex.FindExceptionTypeInInner<MySqlException>();

				if(mySqlException != null && mySqlException.Number == (int)MySqlErrorCode.DuplicateKeyEntry)
				{
					return;
				}
			}
		}

		private IQuery GetPutCodeQuery(int codeId)
		{
			var sql = $@"INSERT INTO {_poolTableName} (code_id) VALUES(:code_id)";
			var query = UoW.Session.CreateSQLQuery(sql)
				.SetParameter("code_id", codeId);
			return query;
		}

		public virtual async Task UpdateCodeExpirationAsync(int codeId, DateTime expirationDate, CancellationToken cancellationToken)
		{
			var sql = $@"
				UPDATE {_poolTableName}
				SET expiration_date = :expirationDate
				WHERE id = :codeId";

			var query = UoW.Session.CreateSQLQuery(sql)
				.SetParameter("codeId", codeId)
				.SetParameter("expirationDate", expirationDate);

			await query.ExecuteUpdateAsync(cancellationToken);
		}

		public virtual int TakeCode(string gtin)
		{
			var selectCodeQuery = GetSelectCodeQuery(gtin);
			var codeToTakeId = selectCodeQuery.UniqueResult<uint>();

			var findCodeQuery = GetTakeCodeQuery(codeToTakeId);
			var codeId = findCodeQuery.UniqueResult<uint>();
			if(codeId == 0)
			{
				throw new EdoCodePoolMissingCodeException($"В пуле не найден код для gtin {gtin}");
			}
			return (int)codeId;
		}

		public virtual async Task<int> TakeCode(string gtin, CancellationToken cancellationToken)
		{
			var selectCodeQuery = GetSelectCodeQuery(gtin);
			var codeToTakeId = await selectCodeQuery.UniqueResultAsync<uint>(cancellationToken);

			var findCodeQuery = GetTakeCodeQuery(codeToTakeId);
			var codeId = await findCodeQuery.UniqueResultAsync<uint>(cancellationToken);
			if(codeId == 0)
			{
				throw new EdoCodePoolMissingCodeException($"В пуле не найден код для gtin {gtin}");
			}
			return (int)codeId;
		}

		public virtual async Task<IList<int>> TakeCodes(string gtin, int count, CancellationToken cancellationToken)
		{
			if(count <= 0)
			{
				throw new ArgumentException("Количество кодов должно быть больше 0", nameof(count));
			}

			var selectCodesQuery = GetSelectCodesQuery(gtin, count);
			var codeIdsToTake = await selectCodesQuery.ListAsync<uint>(cancellationToken);

			if(!codeIdsToTake.Any())
			{
				throw new EdoCodePoolMissingCodeException($"В пуле не найдены коды для gtin {gtin}");
			}

			var deleteQuery = GetDeleteCodesQuery(codeIdsToTake);
			var deletedCodeIds = await deleteQuery.ListAsync<uint>(cancellationToken);

			return deletedCodeIds.Select(id => (int)id).ToList();
		}

		private IQuery GetSelectCodeQuery(string gtin)
		{
			var sql = $@"
				SELECT id FROM {_poolTableName}
				WHERE gtin = :gtin AND promoted = 1
				ORDER BY adding_time DESC 
				LIMIT 1
				FOR UPDATE SKIP LOCKED";

			var query = UoW.Session.CreateSQLQuery(sql)
				.SetParameter("gtin", gtin);
			return query;
		}

		private IQuery GetTakeCodeQuery(uint codeId)
		{
			var sql = $@"
				DELETE FROM {_poolTableName}
				WHERE id = :code_id
				RETURNING code_id";

			var query = UoW.Session.CreateSQLQuery(sql)
				.SetParameter("code_id", codeId);
			return query;
		}

		private IQuery GetSelectCodesQuery(string gtin, int count)
		{
			var sql = $@"
				SELECT id FROM {_poolTableName}
				WHERE gtin = :gtin 
					AND promoted = 1 
					AND (expiration_date IS NULL OR expiration_date > NOW())
				ORDER BY adding_time DESC 
				LIMIT :count
				FOR UPDATE SKIP LOCKED";

			var query = UoW.Session.CreateSQLQuery(sql)
				.SetParameter("gtin", gtin)
				.SetParameter("count", count);
			return query;
		}

		private IQuery GetDeleteCodesQuery(IEnumerable<uint> codeIds)
		{
			var ids = string.Join(",", codeIds);
			var sql = $@"
				DELETE FROM {_poolTableName}
				WHERE id IN ({ids})
				RETURNING code_id";

			return UoW.Session.CreateSQLQuery(sql);
		}

		private void SetSessionTimeout(IUnitOfWork uow)
		{
			uow.Session.CreateSQLQuery("SET SESSION wait_timeout = 30;").ExecuteUpdate();
		}
	}
}
