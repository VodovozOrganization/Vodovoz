using System;
using System.Collections.Generic;
using System.Linq;
using CustomerOrders.Contracts.V5.Orders;
using CustomerOrders.Contracts.V5.Orders.OrderItem;
using CustomerOrders.Contracts.V5.Orders.Templates;
using CustomerOrdersApi.Library.Extensions;
using QS.DomainModel.UoW;
using QS.Extensions.Observable.Collections.List;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Core.Domain.Orders.OnlineOrders;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Goods.Rent;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Sale;
using VodovozBusiness.Controllers;
using VodovozBusiness.Domain.Orders;
using VodovozBusiness.Extensions;
using VodovozBusiness.Factories;
using VodovozBusiness.Nodes;

namespace CustomerOnlineOrdersRegistrar.V5.Factories
{
	/// <inheritdoc/>
	public class OnlineOrderFactoryV5 : IOnlineOrderFactoryV5
	{
		private readonly IOnlineOrderAuthorFactory _onlineOrderAuthorFactory;
		private readonly IDiscountController _discountController;

		public OnlineOrderFactoryV5(
			IOnlineOrderAuthorFactory onlineOrderAuthorFactory,
			IDiscountController discountController)
		{
			_onlineOrderAuthorFactory = onlineOrderAuthorFactory ?? throw new ArgumentNullException(nameof(onlineOrderAuthorFactory));
			_discountController = discountController ?? throw new ArgumentNullException(nameof(discountController));
		}
		
		/// <inheritdoc/>
		public OnlineOrder CreateOnlineOrder(
			IUnitOfWork uow,
			ICreatingOnlineOrder creatingOnlineOrder,
			int fastDeliveryScheduleId,
			int selfDeliveryDiscountReasonId)
		{
			var onlineOrder = new OnlineOrder
			{
				Source = creatingOnlineOrder.Source.ToSource(),
				CounterpartyId = creatingOnlineOrder.CounterpartyErpId,
				ExternalCounterpartyId = creatingOnlineOrder.ExternalCounterpartyId,
				ExternalOrderId = creatingOnlineOrder.ExternalOrderId,
				DeliveryPointId = creatingOnlineOrder.DeliveryPointId,
				DeliveryDate = creatingOnlineOrder.DeliveryDate,
				CallBeforeArrivalMinutes = creatingOnlineOrder.CallBeforeArrivalMinutes,
				SelfDeliveryGeoGroupId = creatingOnlineOrder.SelfDeliveryGeoGroupId,
				IsSelfDelivery = creatingOnlineOrder.IsSelfDelivery,
				IsFastDelivery = creatingOnlineOrder.IsFastDelivery,
				IsNeedConfirmationByCall = creatingOnlineOrder.IsNeedConfirmationByCall,
				BottlesReturn = creatingOnlineOrder.BottlesReturn,
				Trifle = creatingOnlineOrder.Trifle,
				ContactPhone = creatingOnlineOrder.ContactPhone,
				OnlineOrderPaymentType = creatingOnlineOrder.OnlineOrderPaymentType.ToOnlineOrderPaymentType(),
				OnlineOrderPaymentStatus = creatingOnlineOrder.OnlineOrderPaymentStatus.ToOnlineOrderPaymentStatus(),
				OnlinePaymentSource = creatingOnlineOrder.OnlinePaymentSource.ToOnlinePaymentSource(),
				OnlinePayment = creatingOnlineOrder.OnlinePayment,
				OnlineOrderSum = creatingOnlineOrder.OrderSum,
				DontArriveBeforeInterval = creatingOnlineOrder.DontArriveBeforeInterval
			};

			onlineOrder.DeliveryScheduleId = onlineOrder.IsFastDelivery ? fastDeliveryScheduleId : creatingOnlineOrder.DeliveryScheduleId;

			if(!onlineOrder.IsFastDelivery && onlineOrder.DeliveryScheduleId == fastDeliveryScheduleId)
			{
				onlineOrder.IsFastDelivery = true;
			}

			if(onlineOrder.OnlineOrderPaymentStatus == OnlineOrderPaymentStatus.UnPaid
				&& onlineOrder.OnlineOrderPaymentType == OnlineOrderPaymentType.PaidOnline)
			{
				onlineOrder.OnlineOrderStatus = OnlineOrderStatus.WaitingForPayment;
			}
			else
			{
				onlineOrder.OnlineOrderStatus = OnlineOrderStatus.New;
			}

			UpdateOnlineComment(onlineOrder, creatingOnlineOrder.OnlineOrderComment);
			InitializeOnlineOrderReferences(uow, onlineOrder, creatingOnlineOrder);
			AddOrderItems(uow, onlineOrder, selfDeliveryDiscountReasonId, creatingOnlineOrder.OnlineOrderItems);
			AddRentPackages(uow, onlineOrder, creatingOnlineOrder.OnlineRentPackages);
			onlineOrder.Created = DateTime.Now;

			return onlineOrder;
		}
		
		public (OnlineOrderTemplate OrderTemplate,
			IEnumerable<OnlineOrderTemplateProduct> OrderTemplateProducts,
			IEnumerable<OnlineOrderTemplateWeekday> OrderTemplateWeekDays)
			CreateOnlineOrderTemplate(
				IUnitOfWork uow,
				OnlineOrder creatingOnlineOrder,
				CreatingOrderTemplate creatingTemplate)
		{
			var template = OnlineOrderTemplate.Create(
				creatingOnlineOrder.Source,
				creatingOnlineOrder.EmployeeWorkWith?.Id ?? _onlineOrderAuthorFactory.Create(uow, creatingOnlineOrder.Source).Id,
				creatingOnlineOrder.ExternalCounterpartyId,
				creatingOnlineOrder.CounterpartyId.Value,
				creatingOnlineOrder.DeliveryPointId.Value,
				creatingTemplate.DeliveryScheduleId.Value,
				creatingOnlineOrder.IsSelfDelivery,
				creatingOnlineOrder.SelfDeliveryGeoGroupId,
				creatingOnlineOrder.IsFastDelivery,
				creatingOnlineOrder.IsNeedConfirmationByCall,
				creatingOnlineOrder.DontArriveBeforeInterval,
				creatingOnlineOrder.CallBeforeArrivalMinutes,
				creatingOnlineOrder.BottlesReturn,
				creatingTemplate.DeliveryFrequency.Value.ToOnlineOrderDeliveryFrequency(),
				creatingOnlineOrder.OnlineOrderPaymentType,
				creatingOnlineOrder.ContactPhone,
				creatingOnlineOrder.OnlineOrderComment,
				creatingOnlineOrder.Trifle
			);
			
			var templateWeekdays = creatingTemplate.Weekdays
				.Select(x => OnlineOrderTemplateWeekday.Create(template.Id, x.ToWeekDayName()));

			//TODO 5965: подумать насчет расчета и идентификатора шаблона
			var templateProducts = new ObservableList<OnlineOrderTemplateProduct>();
			foreach(var orderItem in creatingOnlineOrder.OnlineOrderItems)
			{
				var discounts = new ObservableList<OnlineOrderTemplateProductDiscount>();
				
				var product = OnlineOrderTemplateProduct.Create(
					template.Id,
					orderItem.Count,
					orderItem.Price,
					orderItem.Nomenclature,
					orderItem.PromoSet,
					discounts
				);
				
				foreach(var discountData in orderItem.Discounts)
				{
					var discount = OnlineOrderTemplateProductDiscount.Create(
						product,
						orderItem.Count,
						orderItem.Price,
						discountData.Discount,
						discountData.IsDiscountInMoney,
						discountData.DiscountReason);
					
					discounts.Add(discount);
				}
				
				templateProducts.Add(product);
			}

			return (template, templateProducts, templateWeekdays);
		}

		private void UpdateOnlineComment(OnlineOrder onlineOrder, string onlineOrderComment)
		{
			if(!string.IsNullOrWhiteSpace(onlineOrderComment)
				&& onlineOrderComment.Length > OnlineOrder.CommentMaxLength)
			{
				const int maxLength = OnlineOrder.CommentMaxLength - 3;
				onlineOrderComment = onlineOrderComment[..maxLength] + "...";
			}
			
			onlineOrder.OnlineOrderComment = onlineOrderComment;
		}

		private void AddOrderItems(
			IUnitOfWork uow,
			OnlineOrder onlineOrder,
			int selfDeliveryDiscountReasonId,
			IEnumerable<OnlineOrderItemDto> onlineOrderItemsDtos)
		{
			if(onlineOrderItemsDtos is null)
			{
				return;
			}

			foreach(var onlineOrderItemDto in onlineOrderItemsDtos)
			{
				//TODO при переделке модели на множественные скидки сделать нормальное их копирование
				var discounts = new List<DiscountData>();
				var nomenclature = uow.GetById<Nomenclature>(onlineOrderItemDto.NomenclatureId);

				DiscountReason applicableDiscountReason = null;

				foreach(var receivedDiscountData in onlineOrderItemDto.Discounts)
				{
					var discountData = DiscountData.Create(
						receivedDiscountData.IsDiscountInMoney,
						receivedDiscountData.Discount,
						receivedDiscountData.DiscountReasonId.HasValue
							? uow.GetById<DiscountReason>(receivedDiscountData.DiscountReasonId.Value)
							: null);
					
					discounts.Add(discountData);
				}

				var firstDiscount = discounts.FirstOrDefault() ?? DiscountData.Create(false, 0, null);
				
				/*if(onlineOrderItemDto.DiscountReasonId.HasValue)
				{
					applicableDiscountReason = uow.GetById<DiscountReason>(onlineOrderItemDto.DiscountReasonId.Value);
				}
				else if(onlineOrder.IsSelfDelivery
				        && !onlineOrderItemDto.PromoSetId.HasValue
				        && nomenclature != null)
				{
					var discountReason = uow.GetById<DiscountReason>(selfDeliveryDiscountReasonId);

					if(_discountController.IsApplicableDiscount(discountReason, nomenclature))
					{
						applicableDiscountReason = discountReason;
					}
				}*/
				
				PromotionalSet promoSet = null;

				if(onlineOrderItemDto.PromoSetId.HasValue)
				{
					promoSet = uow.GetById<PromotionalSet>(onlineOrderItemDto.PromoSetId.Value);
				}
				
				var onlineOrderItem = OnlineOrderItem.Create(
					onlineOrderItemDto.NomenclatureId,
					onlineOrderItemDto.Count,
					onlineOrderItemDto.IsFixedPrice,
					onlineOrderItemDto.Price,
					onlineOrderItemDto.PromoSetId,
					firstDiscount,
					nomenclature,
					promoSet,
					onlineOrder);

				onlineOrder.OnlineOrderItems.Add(onlineOrderItem);
			}
		}

		private void AddRentPackages(IUnitOfWork uow, OnlineOrder onlineOrder, IList<OnlineRentPackageDto> onlineRentPackagesDtos)
		{
			if(onlineRentPackagesDtos is null)
			{
				return;
			}

			foreach(var onlineRentPackageDto in onlineRentPackagesDtos)
			{
				var rentPackage = uow.GetById<FreeRentPackage>(onlineRentPackageDto.RentPackageId);
				
				var onlineRentPackage = OnlineFreeRentPackage.Create(
					onlineRentPackageDto.RentPackageId,
					onlineRentPackageDto.Count,
					onlineRentPackageDto.Price,
					rentPackage,
					onlineOrder);
					
				onlineOrder.OnlineRentPackages.Add(onlineRentPackage);
			}
		}
		
		private void InitializeOnlineOrderReferences(IUnitOfWork uow, OnlineOrder onlineOrder, ICreatingOnlineOrder creatingOnlineOrder)
		{
			if(creatingOnlineOrder.CounterpartyErpId.HasValue)
			{
				onlineOrder.Counterparty = uow.GetById<Counterparty>(creatingOnlineOrder.CounterpartyErpId.Value);
			}
			
			if(creatingOnlineOrder.DeliveryPointId.HasValue)
			{
				onlineOrder.DeliveryPoint = uow.GetById<DeliveryPoint>(creatingOnlineOrder.DeliveryPointId.Value);
			}
			
			if(creatingOnlineOrder.DeliveryScheduleId.HasValue)
			{
				onlineOrder.DeliverySchedule = uow.GetById<DeliverySchedule>(creatingOnlineOrder.DeliveryScheduleId.Value);
			}
			
			if(creatingOnlineOrder.SelfDeliveryGeoGroupId.HasValue)
			{
				onlineOrder.SelfDeliveryGeoGroup = uow.GetById<GeoGroup>(creatingOnlineOrder.SelfDeliveryGeoGroupId.Value);
			}
		}
	}
}
