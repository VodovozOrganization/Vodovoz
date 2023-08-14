using System;
using System.Collections.Generic;
using NHibernate.Criterion;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Cash;
using Vodovoz.Parameters;

namespace Vodovoz.EntityRepositories.Cash
{
	public class CategoryRepository : ICategoryRepository
	{
		private readonly IParametersProvider _parametersProvider;

		public CategoryRepository(IParametersProvider parametersProvider)
		{
			_parametersProvider = parametersProvider ?? throw new ArgumentNullException(nameof(parametersProvider));
		}

		public IList<ExpenseCategory> ExpenseSelfDeliveryCategories(IUnitOfWork uow)
		{
			return uow.Session.QueryOver<ExpenseCategory>()
				.Where(x => x.ExpenseDocumentType == ExpenseInvoiceDocumentType.ExpenseInvoiceSelfDelivery)
				.OrderBy(ec => ec.Name).Asc()
				.List();
		}
	}
}

