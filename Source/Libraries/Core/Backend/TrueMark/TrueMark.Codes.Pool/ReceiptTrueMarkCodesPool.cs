using System.Threading;
using System.Threading.Tasks;
using NHibernate;
using QS.DomainModel.UoW;

namespace TrueMark.Codes.Pool
{
	public class ReceiptTrueMarkCodesPool : TrueMarkCodesPool
	{
		public ReceiptTrueMarkCodesPool(IUnitOfWork uow) : base(uow)
		{
		}
		
		public override int TakeCode(string gtin)
		{
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
			var findCodeQuery = GetTakeCodeQuery(gtin);
			var codeId = await findCodeQuery.UniqueResultAsync<uint>(cancellationToken);
			if(codeId == 0)
			{
				throw new EdoCodePoolMissingCodeException($"В пуле не найден код для gtin {gtin}");
			}
			return (int)codeId;
		}

		private IQuery GetTakeCodeQuery(string gtin)
		{
			var sql = $@"
			DELETE FROM {_poolTableName}
			WHERE id = (
				SELECT pool.id
				FROM {_poolTableName} pool
					INNER JOIN true_mark_identification_code code ON code.id = pool.code_id 
				WHERE pool.promoted
					AND code.gtin = :gtin
					AND code.check_code is not null
				ORDER BY pool.adding_time DESC
				LIMIT 1
				FOR UPDATE SKIP LOCKED
			)
			RETURNING code_id";

			var query = UoW.Session.CreateSQLQuery(sql)
				.SetParameter("gtin", gtin);
			return query;
		}
	}
}
