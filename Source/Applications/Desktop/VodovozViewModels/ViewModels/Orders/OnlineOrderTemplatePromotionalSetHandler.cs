using Vodovoz.Core.Application.Orders.Validators;
using Vodovoz.EntityRepositories.Goods;
using Vodovoz.Settings.Nomenclature;
using VodovozBusiness.Factories;
using VodovozBusiness.Services;
using VodovozBusiness.Services.Orders;

namespace Vodovoz.ViewModels.ViewModels.Orders
{
	public class OnlineOrderTemplatePromotionalSetHandler : PromotionalSetHandler
	{
		public OnlineOrderTemplatePromotionalSetHandler(
			ISaleItemFactory saleItemFactory,
			INomenclatureSettings nomenclatureSettings,
			INomenclatureService nomenclatureService,
			INomenclatureRepository nomenclatureRepository,
			IGoodsPriceCalculator goodsPriceCalculator,
			IAddProductValidator addProductValidator)
			: base(saleItemFactory, nomenclatureSettings, nomenclatureService, nomenclatureRepository, goodsPriceCalculator, addProductValidator)
		{
		}
	}
}
