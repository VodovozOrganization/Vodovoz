using NHibernate.Criterion;
using QS.DomainModel.UoW;
using System.Collections.Generic;
using Vodovoz.Domain.Cash;
using Vodovoz.EntityRepositories.Cash;

namespace Vodovoz.Infrastructure.Persistance.Cash
{
	internal sealed class CategoryRepository : ICategoryRepository
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

