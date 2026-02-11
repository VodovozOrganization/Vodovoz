using System.Threading;
using System.Threading.Tasks;
using NHibernate;
using QS.DomainModel.UoW;

namespace TrueMark.Codes.Pool
{
	public class ReceiptTrueMarkCodesPool : TrueMarkCodesPool
	{
		public ReceiptTrueMarkCodesPool(IUnitOfWork uow, IUnitOfWorkFactory uowFactory) : base(uow, uowFactory)
		{
		}
		
		public override int TakeCode(string gtin)
		{
			using(var uow = UowFactory.CreateWithoutRoot())
			{
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

		public override async Task<int> TakeCode(string gtin, CancellationToken cancellationToken)
		{
			using(var uow = UowFactory.CreateWithoutRoot())
			{
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
				AND code.check_code is not null
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
					AND code.check_code is not null
				ORDER BY pool.adding_time DESC 
				LIMIT 1
			)
			RETURNING code_id";

			var query = UoW.Session.CreateSQLQuery(sql)
				.SetParameter("gtin", gtin)
				.SetParameter("connection_id", ConnectionId);
			return query;
		}
	}
}
