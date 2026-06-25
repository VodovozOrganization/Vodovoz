using System;
using System.Collections.Generic;
using QS.DomainModel.UoW;
using QS.Extensions.Observable.Collections.List;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Orders;
using VodovozBusiness.Domain.Orders;
using VodovozBusiness.Services.Orders;

namespace Vodovoz.ViewModels.ViewModels.Orders
{
	public class AddWaterProductHandler : AddProductHandler
	{
		private IGoodsPriceCalculator _goodsPriceCalculator;
		
		public virtual void AddWaterForSale(
			IUnitOfWork uow,
			IEnumerable<ICalculatingPriceWithManyDiscounts> products,
			Nomenclature nomenclature,
			Domain.Client.Counterparty counterparty,
			DeliveryPoint deliveryPoint,
			decimal count,
			decimal discount = 0,
			bool isDiscountInMoney = false,
			bool needGetFixedPrice = true,
			DiscountReason reason = null,
			PromotionalSet proSet = null
		)
		{
			/*if(nomenclature.Category != NomenclatureCategory.water && !nomenclature.IsDisposableTare)
			{
				return;
			}

			//Если номенклатура промонабора добавляется по фиксе (без скидки), то у нового OrderItem убирается поле discountReason
			if(proSet != null && discount == 0) {
				var fixPricedNomenclaturesId = GetNomenclaturesWithFixPrices.Select(n => n.Id);
				if(fixPricedNomenclaturesId.Contains(nomenclature.Id))
				{
					reason = null;
				}
			}

			if(discount > 0 && reason == null && proSet == null)
			{
				throw new ArgumentException("Требуется указать причину скидки (reason), если она (discount) больше 0!");
			}

			var price = _goodsPriceCalculator.CalculatePrice(
				products,
				counterparty,
				deliveryPoint,
				nomenclature,
				proSet != null,
				HasPermissionsForAlternativePrice,
				count,
				needGetFixedPrice);
			
			AddOrderItem(
				uow,
				OnlineOrderTemplateProduct.Create(
					count,
					price,
					nomenclature,
					proSet,
					0,//templateId,
					new ObservableList<OnlineOrderTemplateProductDiscount>()
				)
			);*/
		}
	}

	public abstract class AddProductHandler
	{
		public AddProductHandler()
		{
			
		}
		
		public virtual void AddOrderItem(
			IUnitOfWork uow,
			OrderItem orderItem,
			bool forceUseAlternativePrice = false)
		{
			/*if(ObservableOrderItems.Contains(orderItem)) {
				return;
			}

			var curCount = orderItem.Nomenclature.IsWater19L
				? GetTotalWater19LCount(true, true)
				: orderItem.Count;
			
			//TODO: уточнить по поводу альтернативных цен, некоторые пользователи смогут их проставить если будут создавать шаблоны
			var isAlternativePriceCopiedFromUndelivery = orderItem.CopiedFromUndelivery != null && orderItem.IsAlternativePrice;
			var canApplyAlternativePrice =
				isAlternativePriceCopiedFromUndelivery
				|| (HasPermissionsForAlternativePrice
					&& orderItem.Nomenclature.AlternativeNomenclaturePrices.Any(x => x.MinCount <= curCount)
					&& orderItem.GetWaterFixedPrice() == null);

			orderItem.IsAlternativePrice = canApplyAlternativePrice;

			ObservableOrderItems.Add(orderItem);
			Recalculate();
			contractUpdater.UpdateContract(uow, this);

			if(orderItems.Any(x => x.Nomenclature.Id == _nomenclatureSettings.MasterCallNomenclatureId))
			{
				_nomenclatureService.CalculateMasterCallNomenclaturePriceIfNeeded(UoW, this);
			}*/
		}
	}
}
