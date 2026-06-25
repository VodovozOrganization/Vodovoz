using Vodovoz.Core.Application.Orders.Services.ItemsHandlers;
using Vodovoz.Core.Application.Orders.Validators;
using Vodovoz.EntityRepositories.Goods;
using Vodovoz.Settings.Nomenclature;
using VodovozBusiness.Factories;
using VodovozBusiness.Handlers;
using VodovozBusiness.Services;
using VodovozBusiness.Services.Orders;

namespace Vodovoz.Core.Application.Templates
{
	public class OnlineOrderTemplateProductHandler : ProductHandler, IOnlineOrderTemplateProductHandler
	{
		public OnlineOrderTemplateProductHandler(
			ISaleItemFactory saleItemFactory,
			INomenclatureSettings nomenclatureSettings,
			INomenclatureService nomenclatureService,
			INomenclatureRepository nomenclatureRepository,
			IGoodsPriceCalculator goodsPriceCalculator,
			IFixedPriceHandler fixedPriceHandler,
			IAddProductValidator addProductValidator)
			: base(
				saleItemFactory,
				nomenclatureSettings,
				nomenclatureService,
				nomenclatureRepository,
				goodsPriceCalculator,
				fixedPriceHandler,
				addProductValidator)
		{
			
		}
	}
}
