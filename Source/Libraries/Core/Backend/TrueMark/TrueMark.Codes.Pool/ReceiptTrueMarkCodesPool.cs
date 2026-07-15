using NHibernate;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace TrueMark.Codes.Pool
{
	public class ReceiptTrueMarkCodesPool : TrueMarkCodesPool
	{
		public ReceiptTrueMarkCodesPool(IUnitOfWork uow) : base(uow)
		{
		}
		
		public override int TakeCode(string gtin)
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

		public override async Task<int> TakeCode(string gtin, CancellationToken cancellationToken)
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

		public override async Task<IList<int>> TakeCodes(string gtin, int count, CancellationToken cancellationToken)
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
				SELECT pool.id
				FROM {_poolTableName} pool
				WHERE pool.gtin = :gtin
					AND pool.has_check_code = 1
					AND pool.expiration_date > NOW()
				LIMIT 1
				FOR UPDATE SKIP LOCKED";

			var query = UoW.Session.CreateSQLQuery(sql)
				.SetParameter("gtin", gtin);
			return query;
		}

		private IQuery GetSelectCodesQuery(string gtin, int count)
		{
			var sql = $@"
				SELECT id FROM {_poolTableName}
				WHERE gtin = :gtin
					AND pool.has_check_code = 1
					AND expiration_date > NOW()
				LIMIT :count
				FOR UPDATE SKIP LOCKED";

			var query = UoW.Session.CreateSQLQuery(sql)
				.SetParameter("gtin", gtin)
				.SetParameter("count", count);
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

		private IQuery GetDeleteCodesQuery(IEnumerable<uint> codeIds)
		{
			var ids = string.Join(",", codeIds);
			var sql = $@"
				DELETE FROM {_poolTableName}
				WHERE id IN ({ids})
				RETURNING code_id";

			return UoW.Session.CreateSQLQuery(sql);
		}
	}
}
