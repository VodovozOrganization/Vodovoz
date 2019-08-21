using System;
using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Suppliers;

namespace Vodovoz.EntityRepositories.Suppliers
{
	public class SupplierPriceItemsRepository : ISupplierPriceItemsRepository
	{
		public IEnumerable<SupplierPriceItem> GetSupplierPriceItemsForNomenclature(IUnitOfWork uow, Nomenclature nomenclature, SupplierOrderingType orderingType = SupplierOrderingType.TheCheapest)
		{
			var query = uow.Session.QueryOver<SupplierPriceItem>()
						   .Where(i => i.NomenclatureToBuy.Id == nomenclature.Id)
						   .OrderBy(i => i.Price).Asc
						   ;

			switch(orderingType) {
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