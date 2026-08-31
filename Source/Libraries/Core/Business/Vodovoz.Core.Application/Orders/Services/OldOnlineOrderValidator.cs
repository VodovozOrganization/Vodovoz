using Vodovoz.Domain.Service;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.Settings.Nomenclature;
using Vodovoz.Settings.Orders;
using VodovozBusiness.Controllers;
using VodovozBusiness.Domain.Orders;
using VodovozBusiness.Services.Orders;

namespace Vodovoz.Core.Application.Orders.Services
{
	public class OldOnlineOrderValidator : OrderFromOnlineOrderValidator
	{
		public OldOnlineOrderValidator(
			IGoodsPriceCalculator goodsPriceCalculator,
			IOnlineOrderDeliveryPriceGetter deliveryPriceGetter,
			INomenclatureSettings nomenclatureSettings,
			IClientDeliveryPointsChecker clientDeliveryPointsChecker,
			IDiscountController discountController,
			IFreeLoaderChecker freeLoaderChecker,
			IOrderOrganizationManager orderOrganizationManager,
			IOrderSettings orderSettings,
			IOrderRepository orderRepository
			)
			: base(
				goodsPriceCalculator,
				deliveryPriceGetter,
				nomenclatureSettings,
				clientDeliveryPointsChecker,
				discountController,
				freeLoaderChecker,
				orderOrganizationManager,
				orderSettings,
				orderRepository)
		{
		}
	}
}
