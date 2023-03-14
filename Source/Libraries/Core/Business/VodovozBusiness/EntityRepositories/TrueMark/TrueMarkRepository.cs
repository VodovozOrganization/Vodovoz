using NHibernate;
using NHibernate.Criterion;
using NHibernate.Util;
using QS.DomainModel.UoW;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

		public int GetCodeErrorsOrdersCount(IUnitOfWork uow)
		{
			var result = uow.Session.QueryOver<TrueMarkCashReceiptOrder>()
				.Where(x => x.Status == TrueMarkCashReceiptOrderStatus.CodeError)
				.ToRowCountQuery()
				.List<int>().First();
			return result;
		}

		public TrueMarkCashReceiptOrder LoadReceipt(IUnitOfWork uow, int receiptId)
		{
			TrueMarkCashReceiptOrder receiptAlias = null;
			TrueMarkCashReceiptProductCode productCodeAlias = null;

			IQueryOver<TrueMarkCashReceiptOrder, TrueMarkCashReceiptOrder> CreateLastOrdersBaseQuery()
			{
				var baseQuery = uow.Session.QueryOver(() => receiptAlias)
					.Where(() => receiptAlias.Id == receiptId);
				return baseQuery;
			}

			// Порядок составления запросов важен.
			// Запрос построен так, чтобы в одном обращении к базе были загружены
			// все используемые поля

			var receiptQuery = CreateLastOrdersBaseQuery()
				.Future<TrueMarkCashReceiptOrder>();

			var orderQuery = CreateLastOrdersBaseQuery()
				.Fetch(SelectMode.Fetch, () => receiptAlias.Order)
				.Future<TrueMarkCashReceiptOrder>();

			var counterpartyQuery = CreateLastOrdersBaseQuery()
				.Fetch(SelectMode.Fetch, () => receiptAlias.Order.Client)
				.Future<TrueMarkCashReceiptOrder>();

			var codesQuery = CreateLastOrdersBaseQuery()
				.Left.JoinAlias(() => receiptAlias.ScannedCodes, () => productCodeAlias)
				.Fetch(SelectMode.Fetch, () => productCodeAlias.SourceCode)
				.Fetch(SelectMode.Fetch, () => productCodeAlias.ResultCode)
				.Future<TrueMarkCashReceiptOrder>();

			var result = codesQuery.SingleOrDefault<TrueMarkCashReceiptOrder>();
			return result;
		}

		public IEnumerable<int> GetReceiptIdsForPrepare()
		{
			using(var uow = _uowFactory.CreateWithoutRoot())
			{
				TrueMarkCashReceiptOrder trueMarkCashReceiptOrderAlias = null;
				var result = uow.Session.QueryOver(() => trueMarkCashReceiptOrderAlias)
					.Where(() => trueMarkCashReceiptOrderAlias.Status == TrueMarkCashReceiptOrderStatus.New
						|| trueMarkCashReceiptOrderAlias.Status == TrueMarkCashReceiptOrderStatus.CodeError)
					.Select(Projections.Id())
					.List<int>();
				return result;
			}
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
	}
}
