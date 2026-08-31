using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Service;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.Settings.Nomenclature;
using Vodovoz.Settings.Orders;
using VodovozBusiness.Controllers;
using VodovozBusiness.Domain.Orders;
using VodovozBusiness.Services.Orders;

namespace Vodovoz.Core.Application.Orders.Services
{
	public class NewOnlineOrderValidator : OrderFromOnlineOrderValidator
	{
		public NewOnlineOrderValidator(
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
		
		private new OnlineOrderV2 OnlineOrder => base.OnlineOrder as OnlineOrderV2;
		
		protected override void ValidatePromoSet(IUnitOfWork uow, IDictionary<int, bool> archivedNomenclatures)
		{
			CheckFreeLoader(uow);
			
			foreach(var set in OnlineOrder.PromoSets)
			{
				var promoSet = set.PromoSet;
				
				if(promoSet.IsArchive)
				{
					ValidationResults.Add(Vodovoz.Errors.Orders.OnlineOrderErrors.IsArchivedOnlineOrderPromoSet(promoSet.Title));
				}

				var checkOnlineOrderSum = CheckOnlineOrderSum.Create(set.Count, set.Price, 0);
				
				CheckPromoSetForNewClientsCount(1, (int)set.Count, promoSet);
				ValidatePrice(set);
				CalculatedOrderItemPrices.Add(checkOnlineOrderSum);
			}
		}
		
		protected override void CheckFreeLoader(IUnitOfWork uow)
		{
			var hasPromoSetForNewClients = OnlineOrder
				.PromoSets
				.Where(x => x.PromoSet != null)
				.Select(x => x.PromoSet)
				.Any(x => x.PromotionalSetForNewClients);

			if(!hasPromoSetForNewClients)
			{
				return;
			}
			
			var contactNumber =
				!string.IsNullOrWhiteSpace(OnlineOrder.ContactPhone) && OnlineOrder.ContactPhone.Length > 2
					? OnlineOrder.ContactPhone.Substring(2)
					: null;

			var result = FreeLoaderChecker.CanOrderPromoSetForNewClientsFromOnline(
				uow,
				OnlineOrder.IsSelfDelivery,
				OnlineOrder.CounterpartyId,
				OnlineOrder.DeliveryPointId,
				contactNumber);

			if(result.IsSuccess)
			{
				return;
			}

			foreach(var error in result.Errors)
			{
				ValidationResults.Add(error);
			}
		}

		private void ValidatePrice(OnlineOrderPromoSet set)
		{
			var price = set.PromoSet.Sum();

			if(price != set.Price)
			{
				ValidationResults.Add(Vodovoz.Errors.Orders.OnlineOrderErrors.IncorrectPricePromoSetInOnlineOrder(
					set.PromoSet.Title, price, set.Price));
			}
		}
	}
}
