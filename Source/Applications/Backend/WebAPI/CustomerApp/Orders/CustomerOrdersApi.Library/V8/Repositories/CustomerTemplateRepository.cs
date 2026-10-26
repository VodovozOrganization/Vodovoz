using System.Collections.Generic;
using System.Linq;
using CustomerApp.Contracts.Sale;
using CustomerApp.Contracts.Sale.Templates;
using CustomerOrders.Contracts.V8.Carts;
using CustomerOrders.Contracts.V8.Sale.Templates;
using CustomerOrdersApi.Library.V8.Extensions;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Sale;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Sale;
using VodovozBusiness.Domain.Orders;

namespace CustomerOrdersApi.Library.V8.Repositories
{
	public class CustomerTemplateRepository : ICustomerTemplateRepository
	{
		/// <inheritdoc/>
		public ILookup<WeekDayName, DeliveryScheduleDto> GetWeeklyDeliverySchedules(IUnitOfWork uow, int onlineOrderId)
		{
			var query = from deliveryScheduleRestriction in uow.Session.Query<DeliveryScheduleRestriction>()
				join deliverySchedule in uow.Session.Query<DeliverySchedule>()
					on deliveryScheduleRestriction.DeliverySchedule.Id equals deliverySchedule.Id
				join district in uow.Session.Query<District>()
					on deliveryScheduleRestriction.District.Id equals district.Id
				join deliveryPoint in uow.Session.Query<DeliveryPoint>()
					on district.Id equals deliveryPoint.District.Id
				join onlineOrder in uow.Session.Query<OnlineOrder>()
					on deliveryPoint.Id equals onlineOrder.DeliveryPoint.Id
				where deliveryScheduleRestriction.WeekDay != WeekDayName.Today
					&& onlineOrder.Id == onlineOrderId
				
				select new
				{
					WeekDay = deliveryScheduleRestriction.WeekDay,
					Schedule = DeliveryScheduleDto.Create(
						deliverySchedule.Id,
						$"{deliverySchedule.From:hh\\:mm} - {deliverySchedule.To:hh\\:mm}")
				};

			return query
				.ToLookup(x => x.WeekDay, y => y.Schedule);
		}

		public CanCreateTemplateDto GetCanCreateTemplateDataFromOnlineOrder(IUnitOfWork uow, int onlineOrderId)
		{
			var onlineOrderData = (
				from onlineOrder in uow.Session.Query<OnlineOrder>()
				where onlineOrder.Id == onlineOrderId
				select new CanCreateTemplateDto
				{
					DeliveryPointId = onlineOrder.DeliveryPointId,
					ErpCounterpartyId = onlineOrder.CounterpartyId,
					IsFastDelivery = onlineOrder.IsFastDelivery,
					IsSelfDelivery = onlineOrder.IsSelfDelivery
				})
				.FirstOrDefault();

			if(onlineOrderData is null)
			{
				return null;
			}

			var items = (
				from onlineOrderItem in uow.Session.Query<OnlineOrderItem>()
				join nomenclature in uow.Session.Query<Nomenclature>()
					on onlineOrderItem.Nomenclature.Id equals nomenclature.Id
				where onlineOrderItem.OnlineOrder.Id == onlineOrderId
				select new
				{
					nomenclature.Category,
					NomenclatureId = nomenclature.Id,
					onlineOrderItem.Count,
					PromoSetId = onlineOrderItem.PromoSetId
				})
				.ToList();
			
			var promoSets = (
				from onlinePromoSet in uow.Session.Query<OnlineOrderPromoSet>()
				where onlinePromoSet.OnlineOrder.Id == onlineOrderId
				select CartItemDto.Create(SaleItemType.PromoSet, onlinePromoSet.Count, onlinePromoSet.PromoSet.Id)
				)
				.ToList();
			
			var onlineRentPackages = (
				from onlineRentPackage in uow.Session.Query<OnlineFreeRentPackage>()
				where onlineRentPackage.OnlineOrder.Id == onlineOrderId
				select CartItemDto.Create(SaleItemType.RentPackage, onlineRentPackage.Count, onlineRentPackage.FreeRentPackage.Id)
				)
				.ToList();

			//т.к. для этих данных нам не важна достоверность количества, то для промиков онлайн заказа 1 версии берем просто из позиции
			var cartItems = (
				from item in items
				select item.PromoSetId != null
					? CartItemDto.Create(SaleItemType.PromoSet, item.Count, item.PromoSetId.Value)
					: CartItemDto.Create(item.Category.ToSaleItemType(), item.Count, item.NomenclatureId)
				)
				.ToList();

			cartItems.AddRange(promoSets);
			cartItems.AddRange(onlineRentPackages);
			onlineOrderData.CartItems = cartItems;

			return onlineOrderData;
		}
	}
}
