using System.Collections.Generic;
using System.Linq;
using Vodovoz.Core.Domain.Sale;
using Vodovoz.Domain.Goods;
using VodovozBusiness.Domain.Sale;

namespace VodovozBusiness.Nodes
{
	public class OnlineOrdersTemplatesData
	{
		public IEnumerable<OnlineOrderTemplate> Templates { get; private set; }
		public ILookup<int, NomenclatureFixedPrice> DeliveryPointFixedPricesLookup { get; private set; }
		public ILookup<int, NomenclatureFixedPrice> CounterpartyFixedPricesLookup { get; private set; }
		public ILookup<int, OnlineOrderTemplateSaleItem> SaleItemsLookup { get; private set; }
		public ILookup<int, OnlineOrderTemplatePromoSet> PromoSetsLookup { get; private set; }
		public ILookup<int, OnlineOrderTemplateFreeRentPackage> RentPackagesLookup { get; private set; }

		public static OnlineOrdersTemplatesData Create(
			IEnumerable<OnlineOrderTemplate> templates,
			ILookup<int, NomenclatureFixedPrice> deliveryPointFixedPrices,
			ILookup<int, NomenclatureFixedPrice> counterpartyFixedPrices,
			ILookup<int, OnlineOrderTemplateSaleItem> saleItemsLookup,
			ILookup<int, OnlineOrderTemplatePromoSet> promoSetsLookup,
			ILookup<int, OnlineOrderTemplateFreeRentPackage> rentPackagesLookup
			)
		{
			return new OnlineOrdersTemplatesData
			{
				Templates = templates,
				DeliveryPointFixedPricesLookup = deliveryPointFixedPrices,
				CounterpartyFixedPricesLookup = counterpartyFixedPrices,
				SaleItemsLookup = saleItemsLookup,
				PromoSetsLookup = promoSetsLookup,
				RentPackagesLookup = rentPackagesLookup
			};
		}
	}
}
