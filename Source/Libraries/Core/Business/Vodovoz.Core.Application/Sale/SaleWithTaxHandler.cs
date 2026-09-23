using Vodovoz.Domain.Service;
using Vodovoz.EntityRepositories.Delivery;
using Vodovoz.EntityRepositories.Goods;
using Vodovoz.Settings.Nomenclature;
using VodovozBusiness.Domain.Service;
using VodovozBusiness.Factories;
using VodovozBusiness.Services.Sale;
using VodovozBusiness.Validation;

namespace Vodovoz.Core.Application.Sale
{
	public class SaleWithTaxHandler : SaleHandler
	{
		public SaleWithTaxHandler(
			SaleItemWithTaxHandler saleItemHandler,
			IGoodsPriceCalculator goodsPriceCalculator,
			IDeliveryPriceService deliveryPriceService,
			IFixedPriceGetter fixedPriceGetter,
			IDeliveryRepository deliveryRepository,
			INomenclatureSettings nomenclatureSettings,
			INomenclatureRepository nomenclatureRepository,
			IAddNomenclatureToSaleValidator addNomenclatureToSaleValidator,
			IAddPromoSetValidator addPromoSetValidator,
			ISaleItemFactory saleItemFactory
			) : base(
				saleItemHandler,
				goodsPriceCalculator,
				deliveryPriceService,
				fixedPriceGetter,
				deliveryRepository,
				nomenclatureSettings,
				nomenclatureRepository,
				addNomenclatureToSaleValidator,
				addPromoSetValidator,
				saleItemFactory
				)
		{
		}
	}
}
