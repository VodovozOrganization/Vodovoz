using Vodovoz.Domain.Service;
using Vodovoz.EntityRepositories.Delivery;
using Vodovoz.EntityRepositories.Goods;
using Vodovoz.Settings.Nomenclature;
using VodovozBusiness.Controllers;
using VodovozBusiness.Domain.Service;
using VodovozBusiness.Factories;
using VodovozBusiness.Services.Sale;

namespace Vodovoz.Core.Application.Sale
{
	public class SaleWithTaxHandler : SaleHandler, ISaleWithTaxHandler
	{
		public SaleWithTaxHandler(
			SaleItemWithTaxHandler saleItemHandler,
			IGoodsPriceCalculator goodsPriceCalculator,
			IDeliveryPriceService deliveryPriceService,
			IFixedPriceGetter fixedPriceGetter,
			IDeliveryRepository deliveryRepository,
			INomenclatureSettings nomenclatureSettings,
			INomenclatureRepository nomenclatureRepository,
			IAddNomenclatureToSaleValidatorFactory addNomenclatureToSaleValidatorFactory,
			IAddPromoSetValidatorFactory addPromoSetValidatorFactory,
			ISaleItemFactory saleItemFactory
			) : base(
				saleItemHandler,
				goodsPriceCalculator,
				deliveryPriceService,
				fixedPriceGetter,
				deliveryRepository,
				nomenclatureSettings,
				nomenclatureRepository,
				addNomenclatureToSaleValidatorFactory,
				addPromoSetValidatorFactory,
				saleItemFactory
				)
		{
		}
	}
}
