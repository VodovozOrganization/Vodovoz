using NHibernate.Criterion;
using QS.DomainModel.UoW;
using System.Collections.Generic;
using Vodovoz.Domain.Cash;

namespace Vodovoz.EntityRepositories.Cash
{
	public class CategoryRepository : ICategoryRepository
	{
		public IList<ExpenseCategory> ExpenseSelfDeliveryCategories(IUnitOfWork uow)
		{
			return uow.Session.QueryOver<ExpenseCategory>()
				.Where(x => x.ExpenseDocumentType == ExpenseInvoiceDocumentType.ExpenseInvoiceSelfDelivery)
				.OrderBy(ec => ec.Name).Asc()
				.List();
		}
	}
}

