using System;
using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Core.Data.Orders.V5;
using Vodovoz.Core.Domain.Orders.OnlineOrders;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Orders;

namespace Vodovoz.Application.Orders.Services
{
	public class OnlineOrderTemplateHandler
	{
		private readonly IOnlineOrderRepository _onlineOrderRepository;

		public OnlineOrderTemplateHandler(
			IOnlineOrderRepository onlineOrderRepository)
		{
			_onlineOrderRepository = onlineOrderRepository ?? throw new ArgumentNullException(nameof(onlineOrderRepository));
		}
		
		public OrderTemplateInfoDto GetFreshOnlineOrderTemplateData(IUnitOfWork uow, int templateId)
		{
			var template = uow.GetById<OnlineOrderTemplate>(templateId);

			if(template is null)
			{
				return null;
			}

			var deliveryPoint = uow.GetById<DeliveryPoint>(template.DeliveryPointId);
			var deliverySchedule = uow.GetById<DeliverySchedule>(template.DeliveryScheduleId);
			var items = uow.GetById<OnlineOrderItem>(template.TemplateItems);
			var list = new List<OnlineOrderItemDto>();
			decimal orderSum = 0;

			foreach(var item in items)
			{
				var onlineOrderItem = new OnlineOrderItemDto
				{
					Count = item.Count,
					Discount = item.IsDiscountInMoney ? item.MoneyDiscount : item.PercentDiscount,
					DiscountReasonId = item.DiscountReason?.Id,
					IsDiscountInMoney = item.IsDiscountInMoney,
					IsFixedPrice = item.IsFixedPrice,
					Price = item.Price,
					NomenclatureId = item.Nomenclature.Id,
					PromoSetId = item.PromoSet?.Id
				};
				
				list.Add(onlineOrderItem);
				
				orderSum += item.Sum;
			}

			var lastExternalOnlineOrderId = _onlineOrderRepository.GetLastOnlineOrderExternalId(uow, template.CounterpartyId);

			var onlineOrderTemplateInfo = OrderTemplateInfoDto.Create(
				template,
				deliveryPoint.ShortAddress,
				deliverySchedule.DeliveryTime,
				lastExternalOnlineOrderId,
				list,
				orderSum);
			
			return onlineOrderTemplateInfo;
		}
	}
}
