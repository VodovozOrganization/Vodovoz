using QS.DomainModel.UoW;
using System.Collections.Generic;
using Vodovoz.Domain.TrueMark;

namespace Vodovoz.EntityRepositories.TrueMark
{
	public class TrueMarkRepository : ITrueMarkRepository
	{
		private readonly IUnitOfWorkFactory _uowFactory;

		public TrueMarkRepository(IUnitOfWorkFactory uowFactory)
		{
			_uowFactory = uowFactory ?? throw new System.ArgumentNullException(nameof(uowFactory));
		}

		public IEnumerable<TrueMarkWaterIdentificationCode> LoadWaterCodes(List<int> codeIds)
		{
			using(var uow = _uowFactory.CreateWithoutRoot())
			{
				var result = uow.Session.QueryOver<TrueMarkWaterIdentificationCode>()
					.WhereRestrictionOn(x => x.Id).IsIn(codeIds)
					.List();
				return result;
			}
		}

		public int GetCodeErrorsOrdersCount(IUnitOfWork uow)
		{
			var sql = $@"SELECT Count(id) FROM cash_receipts WHERE status = 'CodeError';";
			var query = uow.Session.CreateSQLQuery(sql);
			var result = (int)query.UniqueResult<long>();
			return result;
		}
	}
}
