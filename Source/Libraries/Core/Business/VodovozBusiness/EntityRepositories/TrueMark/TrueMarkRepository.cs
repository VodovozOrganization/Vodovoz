using QS.DomainModel.UoW;

namespace Vodovoz.EntityRepositories.TrueMark
{
	public class TrueMarkRepository : ITrueMarkRepository
	{
		public int GetCodeErrorsOrdersCount(IUnitOfWork uow)
		{
			var sql = $@"SELECT Count(id) FROM true_mark_cash_receipt_order WHERE status = 'CodeError';";
			var query = uow.Session.CreateSQLQuery(sql);
			var result = (int)query.UniqueResult<long>();
			return result;
		}
	}
}
