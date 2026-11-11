using NHibernate;
using QS.DomainModel.UoW;
using System;

namespace TrueMark.Codes.Pool
{
	public class ReceiptTrueMarkCodesPool : TrueMarkCodesPool
	{
		public ReceiptTrueMarkCodesPool(IUnitOfWork uow) : base(uow)
		{
		}

		protected override IQuery GetSelectCodeQuery(string gtin, DateTime? addingTimeNotAfter = null)
		{
			var addingTimeFilter = addingTimeNotAfter.HasValue ? " AND pool.adding_time <= :adding_time" : string.Empty;
			var sql = $@"
				SELECT pool.id
				FROM {_poolTableName} pool
				WHERE pool.gtin = :gtin
					AND pool.has_check_code = 1
					AND pool.expiration_date > NOW(){addingTimeFilter}
				LIMIT 1
				FOR UPDATE SKIP LOCKED";

			var query = UoW.Session.CreateSQLQuery(sql)
				.SetParameter("gtin", gtin);

			if(addingTimeNotAfter.HasValue)
			{
				query.SetParameter("adding_time", addingTimeNotAfter.Value);
			}

			return query;
		}

		protected override IQuery GetSelectCodesQuery(string gtin, int count, DateTime? addingTimeNotAfter = null)
		{
			var addingTimeFilter = addingTimeNotAfter.HasValue ? " AND pool.adding_time <= :adding_time" : string.Empty;
			var sql = $@"
				SELECT pool.id
				FROM {_poolTableName} pool
				WHERE pool.gtin = :gtin
					AND pool.has_check_code = 1
					AND pool.expiration_date > NOW(){addingTimeFilter}
				LIMIT :count
				FOR UPDATE SKIP LOCKED";

			var query = UoW.Session.CreateSQLQuery(sql)
				.SetParameter("gtin", gtin)
				.SetParameter("count", count);

			if(addingTimeNotAfter.HasValue)
			{
				query.SetParameter("adding_time", addingTimeNotAfter.Value);
			}

			return query;
		}
	}
}
