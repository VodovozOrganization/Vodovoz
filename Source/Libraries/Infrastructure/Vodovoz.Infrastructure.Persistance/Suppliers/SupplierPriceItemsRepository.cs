using System;
using System.Collections.Generic;
using NHibernate.Criterion;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Suppliers;
using Vodovoz.EntityRepositories.Suppliers;

namespace Vodovoz.Infrastructure.Persistance.Suppliers
{
	internal sealed class SupplierPriceItemsRepository : ISupplierPriceItemsRepository
	{
		public IEnumerable<SupplierPriceItem> GetSupplierPriceItemsForNomenclature(IUnitOfWork uow, Nomenclature nomenclature, SupplierOrderingType orderingType, AvailabilityForSale[] availabilityForSale, bool withDelayOnly)
		{
			Counterparty counterpartyAlias = null;

			var query = uow.Session.QueryOver<SupplierPriceItem>()
						   .Where(i => i.NomenclatureToBuy.Id == nomenclature.Id)
						   .Where(i => i.AvailabilityForSale.IsIn(availabilityForSale))
						   .OrderBy(i => i.Price).Asc
						   ;

			if(withDelayOnly)
				query.JoinAlias(i => i.Supplier, () => counterpartyAlias)
					 .Where(() => counterpartyAlias.DelayDaysForProviders > 0);

			switch(orderingType)
			{
				case SupplierOrderingType.All:
					return query.List();
				case SupplierOrderingType.TheCheapest:
					return query.Take(1).List();
				case SupplierOrderingType.Top3:
					return query.Take(3).List();
				default:
					throw new NotImplementedException(string.Format("значение перечисления \"{0}\" не известно", orderingType));
			}
		}
	}
}
