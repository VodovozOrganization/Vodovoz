using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CustomerApp.Contracts.Sale;
using CustomerApp.Contracts.Sale.Templates;
using CustomerOrders.Contracts.V8.Carts;
using CustomerOrders.Contracts.V8.Sale;
using CustomerOrders.Contracts.V8.Sale.Templates;
using CustomerOrdersApi.Library.V8.Dto.Templates;
using CustomerOrdersApi.Library.V8.Extensions;
using NHibernate.Linq;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Payments;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Core.Domain.Sale;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Sale;
using VodovozBusiness.Domain.Orders;
using VodovozBusiness.Domain.Sale;

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

		public async Task<OrderTemplateInfoRawData> GetOrderTemplateInfoAsync(IUnitOfWork uow, int templateId, CancellationToken cancellationToken)
		{
			var query =
				from template in uow.Session.Query<OnlineOrderTemplate>()
				join deliveryPoint in uow.Session.Query<DeliveryPoint>()
					on template.DeliveryPointId equals deliveryPoint.Id
				join recurrentPayment in uow.Session.Query<OnlineRecurrentPayment>()
					on template.RecurrentPayment.Id equals recurrentPayment.Id
				join deliverySchedule in uow.Session.Query<DeliverySchedule>()
					on template.Schedule.DeliveryScheduleId equals deliverySchedule.Id
				where template.Id == templateId

				let cardInfo =
					recurrentPayment != null && !string.IsNullOrWhiteSpace(recurrentPayment.Token)
						? UserCardInfo.Create(recurrentPayment.CardLastFour, recurrentPayment.CardType)
						: null

				let firstOnlineOrder =
					(int?)(from onlineOrder in uow.Session.Query<OnlineOrder>()
					where onlineOrder.Template.Id == templateId
					select onlineOrder.Id)
					.FirstOrDefault()

				let firstOrder =
					(int?)(from order in uow.Session.Query<Order>()
						where order.OnlineOrder.Id == firstOnlineOrder
						select order.Id)
					.FirstOrDefault()

				let deliveryInterval = DeliveryScheduleData.Create(
					deliverySchedule.Id,
					deliverySchedule.From,
					deliverySchedule.To
					)
				
				select OrderTemplateInfoRawData.Create(
					new OrderTemplateInfoDto
					{
						OrderTemplateId = template.Id,
						CreatedDate = template.CreatedAt.ToString("yyyy-MM-dd"),
						Status = template.Status.ToApiTemplateStatus(),
						DeliveryPointId = deliveryPoint.Id,
						DeliveryAddress = deliveryPoint.ShortAddress,
						PaymentMethod = template.PaymentType.ToTemplatePaymentType(),
						RecurrentPayment = cardInfo,
						SourceOrder = TemplateSourceOrderDto.Create(firstOnlineOrder, firstOrder)
					},
					template.Schedule,
					deliveryInterval
					);
			
			var templateInfoRawData = await query.FirstOrDefaultAsync(cancellationToken);

			if(templateInfoRawData is null)
			{
				return null;
			}
			
			templateInfoRawData.TemplateInfo.UpcomingOrder =
				await GetIQueryableTemplateUpcomingOrder(uow, templateId)
					.FirstOrDefaultAsync(cancellationToken);

			templateInfoRawData.SaleItems =
				await GetIQueryableOnlineOrderTemplateSaleItems(uow, templateId)
					.ToListAsync(cancellationToken);
			
			templateInfoRawData.PromoSets =
				await GetIQueryableOnlineOrderTemplatePromoSets(uow, templateId)
					.ToListAsync(cancellationToken);
			
			templateInfoRawData.RentPackages = 
				await GetIQueryableOnlineOrderTemplateFreeRentPackages(uow, templateId)
					.ToListAsync(cancellationToken);

			templateInfoRawData.DeliveryPointFixedPrices = 
				await GetIQueryableDeliveryPointFixedPrices(uow, templateId)
					.ToListAsync(cancellationToken);

			templateInfoRawData.CounterpartyFixedPrices = 
				await GetIQueryableCounterpartyFixedPrices(uow, templateId)
					.ToListAsync(cancellationToken);

			return templateInfoRawData;
		}

		public Task<OrderTemplateInfoDto> GetOrderTemplatesAsync(
			IUnitOfWork uow,
			int erpCounterpartyId,
			int skipElements,
			int templatesCountOnPage)
		{
			throw new NotImplementedException();
		}

		private IQueryable<TemplateUpcomingOrderDto> GetIQueryableTemplateUpcomingOrder(IUnitOfWork uow, int templateId)
		{
			var upcomingOrder =
				from onlineOrder in uow.Session.Query<OnlineOrder>()
				join order in uow.Session.Query<Order>()
					on onlineOrder.Id equals order.OnlineOrder.Id into orders
				from order in orders.DefaultIfEmpty()
				where onlineOrder.Template.Id == templateId
					&& onlineOrder.DeliveryDate >= DateTime.Today
				select TemplateUpcomingOrderDto.Create(onlineOrder.Id, onlineOrder.DeliveryDate, order.Id);
			
			return upcomingOrder;
		}

		private IQueryable<OnlineOrderTemplateSaleItem> GetIQueryableOnlineOrderTemplateSaleItems(IUnitOfWork uow, int templateId)
		{
			var saleItems =
				from saleItem in uow.Session.Query<OnlineOrderTemplateSaleItem>()
				where saleItem.TemplateId == templateId
				select saleItem;
			return saleItems;
		}

		private IQueryable<OnlineOrderTemplatePromoSet> GetIQueryableOnlineOrderTemplatePromoSets(IUnitOfWork uow, int templateId)
		{
			var promoSets =
				from promoSet in uow.Session.Query<OnlineOrderTemplatePromoSet>()
				where promoSet.TemplateId == templateId
				select promoSet;
			return promoSets;
		}

		private IQueryable<OnlineOrderTemplateFreeRentPackage> GetIQueryableOnlineOrderTemplateFreeRentPackages(IUnitOfWork uow, int templateId)
		{
			var rentPackages =
				from rentPackage in uow.Session.Query<OnlineOrderTemplateFreeRentPackage>()
				where rentPackage.TemplateId == templateId
				select rentPackage;
			return rentPackages;
		}
		
		private IQueryable<NomenclatureFixedPrice> GetIQueryableDeliveryPointFixedPrices(IUnitOfWork uow, int templateId)
		{
			var fixedPrices =
				from onlineOrder in uow.Session.Query<OnlineOrder>()
				join deliveryPoint in uow.Session.Query<DeliveryPoint>()
					on onlineOrder.DeliveryPointId equals deliveryPoint.Id
				join fixedPrice in uow.Session.Query<NomenclatureFixedPrice>()
					on deliveryPoint.Id equals fixedPrice.DeliveryPoint.Id
				where onlineOrder.Template.Id == templateId
				select fixedPrice;
			
			return  fixedPrices;
		}
		
		private IQueryable<NomenclatureFixedPrice> GetIQueryableCounterpartyFixedPrices(IUnitOfWork uow, int templateId)
		{
			var fixedPrices =
				from onlineOrder in uow.Session.Query<OnlineOrder>()
				join counterparty in uow.Session.Query<Counterparty>()
					on onlineOrder.CounterpartyId equals counterparty.Id
				join fixedPrice in uow.Session.Query<NomenclatureFixedPrice>()
					on counterparty.Id equals fixedPrice.Counterparty.Id
				where onlineOrder.Template.Id == templateId
				select fixedPrice;
			
			return  fixedPrices;
		}
	}
}
