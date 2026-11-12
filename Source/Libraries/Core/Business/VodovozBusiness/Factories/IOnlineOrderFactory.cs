using System;
using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Sale;
using Vodovoz.Domain.Orders;
using VodovozBusiness.Domain.Sale;

namespace VodovozBusiness.Factories
{
	public interface IOnlineOrderFactory
	{
		OnlineOrder Create(
			IUnitOfWork uow,
			OnlineOrderTemplate template,
			decimal templateSum,
			DateTime deliveryDate,
			IEnumerable<(decimal Sum, OnlineOrderTemplateSaleItem SaleItem)> saleItemsWithSum,
			IEnumerable<OnlineOrderTemplatePromoSet> promoSets,
			IEnumerable<OnlineOrderTemplateFreeRentPackage> freeRentPackages
			);
	}
}
