using MySqlConnector;
using NHibernate;
using QS.DomainModel.UoW;
using System;
using System.Threading;
using System.Threading.Tasks;
using QS.Utilities.Debug;

namespace TrueMark.Codes.Pool
{
	/// <summary>
	/// Предоставляет возможность добавлять и забирать коды из пула кодов 
	/// </summary>
	public class TrueMarkCodesPool : ITrueMarkCodesPool
	{
		protected const string PoolTableName = "true_mark_codes_pool_new";
		protected const int CodeHoldTimeoutMinute = 10;
		protected readonly IUnitOfWork UoW;
		protected readonly IUnitOfWorkFactory UowFactory;
		protected readonly long ConnectionId;

		public TrueMarkCodesPool(IUnitOfWork uow, IUnitOfWorkFactory uowFactory)
		{
			UoW = uow ?? throw new ArgumentNullException(nameof(uow));
			UowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));

			SetSessionTimeout(UoW);
			UoW.OpenTransaction();

			ConnectionId = UoW.Session.CreateSQLQuery("SELECT CONNECTION_ID()")
				.UniqueResult<uint>();
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
			var sql = $@"INSERT INTO {PoolTableName} (code_id) VALUES(:code_id)";
			var query = UoW.Session.CreateSQLQuery(sql)
				.SetParameter("code_id", codeId);
			return query;
		}

		public virtual int TakeCode(string gtin)
		{
			using(var uow = UowFactory.CreateWithoutRoot())
			{
				uow.OpenTransaction();
				var holdCodeQuery = GetHoldCodeQuery(gtin);
				holdCodeQuery.ExecuteUpdate();
				uow.Commit();
			}

			var findCodeQuery = GetTakeCodeQuery(gtin);
			var codeId = findCodeQuery.UniqueResult<uint>();
			if(codeId == 0)
			{
				throw new EdoCodePoolMissingCodeException($"В пуле не найден код для gtin {gtin}");
			}
			return (int)codeId;
		}


		public virtual async Task<int> TakeCode(string gtin, CancellationToken cancellationToken)
		{
			using(var uow = UowFactory.CreateWithoutRoot())
			{
				uow.OpenTransaction();
				var holdCodeQuery = GetHoldCodeQuery(gtin);
				await holdCodeQuery.ExecuteUpdateAsync(cancellationToken);
				await uow.CommitAsync(cancellationToken);
			}

			var findCodeQuery = GetTakeCodeQuery(gtin);
			var codeId = await findCodeQuery.UniqueResultAsync<uint>(cancellationToken);
			if(codeId == 0)
			{
				throw new EdoCodePoolMissingCodeException($"В пуле не найден код для gtin {gtin}");
			}
			return (int)codeId;
		}

		private IQuery GetHoldCodeQuery(string gtin)
		{
			var sql = $@"
			UPDATE {PoolTableName} as pool
			INNER JOIN true_mark_identification_code code ON code.id = pool.code_id
			SET 
				pool.holded_by = :connection_id,
				pool.holded_until = CURRENT_TIMESTAMP() + INTERVAL :hold_timeout MINUTE
			WHERE holded_until < CURRENT_TIMESTAMP()
				AND pool.promoted
				AND code.gtin = :gtin
			ORDER BY pool.adding_time DESC
			LIMIT 1";

			var query = UoW.Session.CreateSQLQuery(sql)
				.SetParameter("gtin", gtin)
				.SetParameter("connection_id", ConnectionId)
				.SetParameter("hold_timeout", CodeHoldTimeoutMinute)
				;
			return query;
		}

		private IQuery GetTakeCodeQuery(string gtin)
		{
			var sql = $@"
			DELETE FROM {PoolTableName}
			WHERE id = (
				SELECT pool.id
				FROM {PoolTableName} pool
					INNER JOIN true_mark_identification_code code ON code.id = pool.code_id 
				WHERE pool.promoted 
					AND code.gtin = :gtin
					AND pool.holded_by = :connection_id
				ORDER BY pool.adding_time DESC 
				LIMIT 1
			)
			RETURNING code_id";

			var query = UoW.Session.CreateSQLQuery(sql)
				.SetParameter("gtin", gtin)
				.SetParameter("connection_id", ConnectionId)
				;
			return query;
		}

		private void SetSessionTimeout(IUnitOfWork uow)
		{
			uow.Session.CreateSQLQuery("SET SESSION wait_timeout = 30;").ExecuteUpdate();
		}
	}
}
