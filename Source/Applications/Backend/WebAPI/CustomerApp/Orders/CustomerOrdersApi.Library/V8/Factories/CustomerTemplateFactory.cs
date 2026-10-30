using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CustomerApp.Contracts.Common;
using CustomerApp.Contracts.Sale;
using CustomerApp.Contracts.Sale.Templates;
using CustomerOrders.Contracts.V8.Common;
using CustomerOrders.Contracts.V8.Orders;
using CustomerOrders.Contracts.V8.Orders.Items;
using CustomerOrders.Contracts.V8.Sale.Templates;
using CustomerOrdersApi.Library.V8.Dto.Templates;
using CustomerOrdersApi.Library.V8.Extensions;
using CustomerOrdersApi.Library.V8.Factories.DeliverySchedules;
using CustomerOrdersApi.Library.V8.Repositories;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.Domain.Orders;
using Vodovoz.Settings.Nomenclature;
using VodovozBusiness.Domain.Orders;
using VodovozBusiness.Domain.Sale;
using VodovozBusiness.Extensions;
using VodovozBusiness.Services.Sale;

namespace CustomerOrdersApi.Library.V8.Factories
{
	public class CustomerTemplateFactory : ICustomerTemplateFactory
	{
		private readonly INomenclatureSettings _nomenclatureSettings;
		private readonly IGenericRepository<OnlineOrder> _onlineOrderRepository;
		private readonly ICustomerTemplateRepository _customerTemplateRepository;
		private readonly IOnlineOrderItemDtoFactory _onlineOrderItemFactory;
		private readonly ITemplateDeliveryScheduleDescriptionFactory _deliveryScheduleDescriptionFactory;
		private readonly ITemplateNextDeliveryDateGenerator _deliveryDateGenerator;
		private readonly ITemplateActionsGenerator _templateActionsGenerator;

		public CustomerTemplateFactory(
			INomenclatureSettings nomenclatureSettings,
			IGenericRepository<OnlineOrder> onlineOrderRepository,
			ICustomerTemplateRepository customerTemplateRepository,
			IOnlineOrderItemDtoFactory onlineOrderItemFactory,
			ITemplateDeliveryScheduleDescriptionFactory deliveryScheduleDescriptionFactory,
			ITemplateNextDeliveryDateGenerator deliveryDateGenerator,
			ITemplateActionsGenerator templateActionsGenerator
			)
		{
			_nomenclatureSettings = nomenclatureSettings ?? throw new ArgumentNullException(nameof(nomenclatureSettings));
			_onlineOrderRepository = onlineOrderRepository ?? throw new ArgumentNullException(nameof(onlineOrderRepository));
			_customerTemplateRepository = customerTemplateRepository ?? throw new ArgumentNullException(nameof(customerTemplateRepository));
			_onlineOrderItemFactory = onlineOrderItemFactory ?? throw new ArgumentNullException(nameof(onlineOrderItemFactory));
			_deliveryScheduleDescriptionFactory =
				deliveryScheduleDescriptionFactory ?? throw new ArgumentNullException(nameof(deliveryScheduleDescriptionFactory));
			_deliveryDateGenerator = deliveryDateGenerator ?? throw new ArgumentNullException(nameof(deliveryDateGenerator));
			_templateActionsGenerator = templateActionsGenerator ?? throw new ArgumentNullException(nameof(templateActionsGenerator));
		}
		
		public CheckOrderTemplateResponse CreateCheckOrderTemplateResponse(
			IUnitOfWork uow,
			CheckOrderTemplateRequest request
			)
		{
			var onlineOrder = _onlineOrderRepository.GetFirstOrDefault(uow, o => o.Id == request.OnlineOrderId);

			if(onlineOrder is null)
			{
				throw new InvalidOperationException("Сюда должно передаваться существующее значение идентификатора онлайн заказа");
			}

			var deliverySchedule = onlineOrder.DeliverySchedule;
			var itemsCount = 0m;
			var orderSumDto = new OnlineOrderSumDto();
			
			var response = new CheckOrderTemplateResponse
			{
				Schedule = TemplateDeliveryScheduleDetailedDto.Create(
					request.Schedule,
					DeliveryScheduleDto.Create(deliverySchedule.Id, deliverySchedule.DeliveryTime),
					_deliveryScheduleDescriptionFactory.Create(request.Schedule, deliverySchedule.From, deliverySchedule.To)
					),
				DeliveryPointId = onlineOrder.DeliveryPoint.Id,
				DeliveryAddress = onlineOrder.DeliveryPoint.ShortAddress,
				PaymentMethod = onlineOrder.ToTemplatePaymentType()
			};
			
			AddOnlineOrders(
				request.Source,
				onlineOrder.OnlineOrderItems
					.Where(x => x.PromoSet is null)
					.ToList(),
				response,
				orderSumDto,
				ref itemsCount);

			if(onlineOrder is OnlineOrderV1)
			{
				AddPromoSets(onlineOrder.OnlineOrderItems, response, orderSumDto, ref itemsCount);
			}
			else if(onlineOrder is OnlineOrderV2 onlineOrderV2)
			{
				AddPromoSets(onlineOrderV2.PromoSets, response, orderSumDto, ref itemsCount);
			}
			else
			{
				throw new InvalidOperationException("Неизвестная версия онлайн заказа!");
			}

			AddRentPackages(onlineOrder.OnlineRentPackages, response, orderSumDto, ref itemsCount);
			response.ItemsCount = Math.Round(itemsCount, 2);
			response.OrderSum = RoundProperties(orderSumDto);
			response.NextDeliveries = CreateNextDeliveriesInfo(response.Schedule);

			return response;
		}

		public CreateOrderTemplateResponse CreateOrderTemplateResponse(
			IUnitOfWork uow,
			OnlineOrder onlineOrder,
			CreateOrderTemplateRequest request
			)
		{
			if(onlineOrder.Template is null)
			{
				throw new InvalidOperationException(
					"Для формирования ответа по созданному шаблону автозаказа этот шаблон должен быть уже привязан к онлайн заказу!");
			}
			
			var response = new CreateOrderTemplateResponse
			{
				OnlineOrderId = onlineOrder.Id,
				Status = onlineOrder.Template.Status.ToApiTemplateStatus(),
				OrderSum = onlineOrder.OnlineOrderSum,
				OrderTemplateId = onlineOrder.Template.Id
			};

			var timers = uow.GetAll<OnlineOrderTimers>().FirstOrDefault();

			if(timers is null)
			{
				throw new InvalidOperationException("Не настроены таймеры для онлайн заказа");
			}
			
			response.SetPaymentMethod(
				onlineOrder.ToTemplatePaymentType(),
				OnlineOrderTimerSetupDto.Create(
					onlineOrder.Template.CreatedAt,
					(int)timers.AutoOrderPayTime.TotalSeconds)
				);

			response.NextDeliveryDate = _deliveryDateGenerator.GetNextDeliveryDate(
				request.Schedule.Type,
				request.Schedule.FirstDeliveryDate,
				request.Schedule.RepeatEveryWeeks,
				request.Schedule.DayOfMonth
				);
			
			return response;
		}

		public async Task<OrderTemplateInfoDto> CreateOrderTemplateInfoAsync(
			IUnitOfWork uow,
			ExternalSource source,
			int templateId,
			CancellationToken cancellationToken
			)
		{
			var templateInfoRawData = await _customerTemplateRepository.GetOrderTemplateInfoAsync(uow, templateId, cancellationToken);

			if(templateInfoRawData?.TemplateInfo is null)
			{
				return null;
			}

			var orderSumDto = new OnlineOrderSumDto();
			var templateInfo = templateInfoRawData.TemplateInfo;
			var templateDeliverySchedule = templateInfoRawData.Schedule.ToTemplateDeliveryScheduleDto();
			var deliveryInterval = templateInfoRawData.DeliveryInterval;

			var schedule = TemplateDeliveryScheduleDetailedDto.Create(
				templateDeliverySchedule,
				DeliveryScheduleDto.Create(deliveryInterval.Id, $"{deliveryInterval.From}-{deliveryInterval.To}"),
				_deliveryScheduleDescriptionFactory.Create(templateInfoRawData.Schedule, deliveryInterval.From, deliveryInterval.To)
			);
			
			templateInfo.Schedule = schedule;
			templateInfo.SaleItems = ProcessSaleItems(
				uow,
				source,
				templateInfoRawData,
				orderSumDto);
			
			templateInfo.OrderSum = RoundProperties(orderSumDto);
			
			//TODO-6121 уточнить по статусам. В доке написаны Paused, AwaitingPayment, ManualProcessing
			if(templateInfo.Status != TemplateStatusDto.Active)
			{
				templateInfo.NextDeliveries = Array.Empty<TemplateNextDeliveriesDto>();
			}
			else
			{
				templateInfo.NextDeliveries = CreateNextDeliveriesInfo(schedule);
			}
			
			templateInfo.Actions = _templateActionsGenerator.Generate(templateInfoRawData.TemplateInfo.Status);
			
			return  templateInfo;
		}

		private IEnumerable<OnlineOrderItemWithDiscountDetailsDto> ProcessSaleItems(
			IUnitOfWork uow,
			ExternalSource source,
			OrderTemplateInfoRawData templateInfoRawData,
			OnlineOrderSumDto orderSumDto
			)
		{
			var list = new List<OnlineOrderItemWithDiscountDetailsDto>();
			
			AddSaleItems(source, templateInfoRawData, orderSumDto, list);
			AddPromoSets(source, templateInfoRawData.PromoSets, orderSumDto, list);
			AddRentPackages(source, templateInfoRawData.RentPackages, orderSumDto, list);
			
			return list;
		}

		private void AddSaleItems(
			ExternalSource source,
			OrderTemplateInfoRawData templateInfoRawData,
			OnlineOrderSumDto orderSumDto,
			ICollection<OnlineOrderItemWithDiscountDetailsDto> detailedItems
			)
		{
			if(templateInfoRawData.SaleItems is null)
			{
				return;
			}

			var newDetailedItems = _onlineOrderItemFactory.CreateWithDiscountDetailsDto(
				templateInfoRawData.SaleItems,
				templateInfoRawData.DeliveryPointFixedPrices,
				templateInfoRawData.CounterpartyFixedPrices,
				source.ToGoodsOnlineParameterType()
				);

			foreach(var detailedItem in newDetailedItems)
			{
				detailedItems.Add(detailedItem);
				UpdateSum(orderSumDto, detailedItem);
			}
		}
		
		private void AddPromoSets(
			ExternalSource source,
			IEnumerable<OnlineOrderTemplatePromoSet> promoSets,
			OnlineOrderSumDto orderSumDto,
			ICollection<OnlineOrderItemWithDiscountDetailsDto> detailedItems)
		{
			if(promoSets is null)
			{
				return;
			}

			foreach(var promoSet in promoSets)
			{
				var detailedItem = _onlineOrderItemFactory.CreateWithDiscountDetailsDto(promoSet);
				detailedItems.Add(detailedItem);
				UpdateSum(orderSumDto, detailedItem);
			}
		}
		
		private void AddRentPackages(
			ExternalSource source,
			IEnumerable<OnlineOrderTemplateFreeRentPackage> rentPackages,
			OnlineOrderSumDto orderSumDto,
			ICollection<OnlineOrderItemWithDiscountDetailsDto> detailedItems)
		{
			if(rentPackages is null)
			{
				return;
			}

			foreach(var rentPackage in rentPackages)
			{
				var detailedItem = _onlineOrderItemFactory.CreateWithDiscountDetailsDto(rentPackage);
				detailedItems.Add(detailedItem);
				UpdateSum(orderSumDto, detailedItem);
			}
		}

		private void AddOnlineOrders(
			ExternalSource source,
			IEnumerable<OnlineOrderItem> onlineOrderItems,
			CheckOrderTemplateResponse response,
			OnlineOrderSumDto orderSumDto,
			ref decimal itemsCount
			)
		{
			response.OnlineOrderItems = new List<OnlineOrderItemWithDiscountDetailsDto>();
			
			foreach(var onlineOrderItem in onlineOrderItems)
			{
				var detailedItem = _onlineOrderItemFactory.CreateWithDiscountDetailsDto(onlineOrderItem, source.ToGoodsOnlineParameterType());
				response.OnlineOrderItems.Add(detailedItem);
				UpdateSumAndCount(orderSumDto, ref itemsCount, detailedItem);
			}
		}

		private void AddPromoSets(
			IEnumerable<IProduct> orderItems,
			CheckOrderTemplateResponse response,
			OnlineOrderSumDto orderSumDto,
			ref decimal itemsCount
			)
		{
			var promoSetsGroup = orderItems
				.Where(x => x.PromoSet != null)
				.ToLookup(x => x.PromoSet.Id);
			
			foreach(var orderItemGroup in promoSetsGroup)
			{
				var promo = orderItemGroup.First().PromoSet;
				var promoItemsCount = promo.PromotionalSetItems.Count;
				var detailedItem = _onlineOrderItemFactory
					.CreateWithDiscountDetailsDto(promo, orderItemGroup.Count() / promoItemsCount);
				
				response.OnlineOrderItems.Add(detailedItem);
				UpdateSumAndCount(orderSumDto, ref itemsCount, detailedItem);
			}
		}

		private void AddPromoSets(
			IEnumerable<OnlineOrderPromoSet> onlineOrderPromoSets,
			CheckOrderTemplateResponse response,
			OnlineOrderSumDto orderSumDto,
			ref decimal itemsCount
			)
		{
			foreach(var onlineOrderPromoSet in onlineOrderPromoSets)
			{
				var detailedItem = _onlineOrderItemFactory.CreateWithDiscountDetailsDto(onlineOrderPromoSet);
				
				response.OnlineOrderItems.Add(detailedItem);
				UpdateSumAndCount(orderSumDto, ref itemsCount, detailedItem);
			}
		}

		private void AddRentPackages(
			IEnumerable<OnlineFreeRentPackage> freeRentPackages,
			CheckOrderTemplateResponse response,
			OnlineOrderSumDto orderSumDto,
			ref decimal itemsCount
			)
		{
			foreach(var freeRentPackage in freeRentPackages)
			{
				var detailedItem = _onlineOrderItemFactory.CreateWithDiscountDetailsDto(freeRentPackage);
				
				response.OnlineOrderItems.Add(detailedItem);
				UpdateSumAndCount(orderSumDto, ref itemsCount, detailedItem);
			}
		}

		private void UpdateSum(
			OnlineOrderSumDto orderSumDto,
			OnlineOrderItemWithDiscountDetailsDto detailedItem
		)
		{
			var rawSum = (detailedItem.PriceWithoutDiscount ?? detailedItem.Price) * detailedItem.Count;
			
			if(detailedItem.ItemType == SaleItemType.Other
				&& _nomenclatureSettings.PaidDeliveriesNomenclaturesIds.Contains(detailedItem.ErpId))
			{
				orderSumDto.Delivery += rawSum;
			}
			else
			{
				orderSumDto.RawSum += rawSum;
				orderSumDto.Total += detailedItem.CurrentSum;
			}
			
			orderSumDto.Discount += rawSum - detailedItem.CurrentSum;
		}

		private void UpdateSumAndCount(
			OnlineOrderSumDto orderSumDto,
			ref decimal itemsCount,
			OnlineOrderItemWithDiscountDetailsDto detailedItem
		)
		{
			itemsCount += detailedItem.Count;
			UpdateSum(orderSumDto, detailedItem);
		}
		
		private IEnumerable<TemplateNextDeliveriesDto> CreateNextDeliveriesInfo(TemplateDeliveryScheduleDetailedDto schedule)
		{
			IEnumerable<DateTime> dates = null;

			if(schedule.Type == CustomerTemplateDeliveryScheduleType.Weekly)
			{
				const string errorMessage = "Для генерации дат следующих доставок автозаказа с недельной периодичностью, {0} должна быть заполнена!";
				
				if(!schedule.FirstDeliveryDate.HasValue)
				{
					throw new InvalidOperationException(string.Format(errorMessage, "дата первой доставки"));
				}
				
				if(!schedule.RepeatEveryWeeks.HasValue)
				{
					throw new InvalidOperationException(string.Format(errorMessage, "периодичность"));
				}

				dates = _deliveryDateGenerator.GetTemplateNextDeliveryDates(
					schedule.FirstDeliveryDate.Value,
					schedule.RepeatEveryWeeks.Value
				);
			}
			else if(schedule.Type == CustomerTemplateDeliveryScheduleType.Monthly)
			{
				const string errorMessage = "Для генерации дат следующих доставок автозаказа с месячной периодичностью, {0} должно быть заполнено!";
				
				if(!schedule.DayOfMonth.HasValue)
				{
					throw new InvalidOperationException(string.Format(errorMessage, "число месяца"));
				}
				
				dates = _deliveryDateGenerator.GetTemplateNextDeliveryDates(schedule.DayOfMonth.Value);
			}
			else
			{
				throw new InvalidOperationException(
					$"Неизвестное значение периодичности автозаказа {schedule.Type}." +
					$" Добавьте это значение в условие метода {nameof(CreateNextDeliveriesInfo)}");
			}

			return dates.Select(nextDate =>
				TemplateNextDeliveriesDto.Create(
					nextDate,
					nextDate.DayOfWeek,
					DeliveryScheduleDto.Copy(schedule.DeliveryInterval))
				)
				.ToList();
		}
		
		private OnlineOrderSumDto RoundProperties(OnlineOrderSumDto orderSumDto)
		{
			orderSumDto.Delivery = Math.Round(orderSumDto.Delivery, 2);
			orderSumDto.RawSum = Math.Round(orderSumDto.RawSum, 2);
			orderSumDto.Discount = Math.Round(orderSumDto.Discount, 2);
			orderSumDto.Total = Math.Round(orderSumDto.Total, 2);
			
			return orderSumDto;
		}
	}
}
