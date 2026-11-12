using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Core.Domain.Sale;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Employees;
using VodovozBusiness.Domain.Orders;
using VodovozBusiness.Domain.Sale;
using VodovozBusiness.Services.Orders;
using VodovozBusiness.Services.Sale;

namespace Vodovoz.Core.Application.Sale
{
	public class OnlineOrderTemplateCreator : IOnlineOrderTemplateCreator
	{
		private readonly ILogger<OnlineOrderTemplateCreator> _logger;
		private readonly IEmployeeRepository _employeeRepository;
		private readonly IUnPaidOnlineOrderHandler _unPaidOnlineOrderHandler;

		public OnlineOrderTemplateCreator(
			ILogger<OnlineOrderTemplateCreator> logger, 
			IEmployeeRepository employeeRepository,
			IUnPaidOnlineOrderHandler unPaidOnlineOrderHandler
			)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
			_unPaidOnlineOrderHandler = unPaidOnlineOrderHandler ?? throw new ArgumentNullException(nameof(unPaidOnlineOrderHandler));
		}

		public async Task<OnlineOrderTemplate> CreateAsync(
			IUnitOfWork uow,
			OnlineOrder onlineOrder,
			TemplateDeliveryScheduleData schedule,
			CancellationToken cancellationToken
		)
		{
			var author = await _employeeRepository.GetEmployeeBySourceAsync(
				uow,
				onlineOrder.Source,
				cancellationToken);
			
			var template = OnlineOrderTemplate.Create(
				onlineOrder.Source,
				author.Id,
				onlineOrder.ExternalCounterpartyId,
				onlineOrder.CounterpartyId.Value,
				onlineOrder.DeliveryPointId.Value,
				onlineOrder.IsSelfDelivery,
				onlineOrder.SelfDeliveryGeoGroupId,
				onlineOrder.IsFastDelivery,
				onlineOrder.IsNeedConfirmationByCall,
				onlineOrder.DontArriveBeforeInterval,
				onlineOrder.CallBeforeArrivalMinutes,
				onlineOrder.BottlesReturn,
				onlineOrder.OnlineOrderPaymentType,
				onlineOrder.ContactPhone,
				onlineOrder.OnlineOrderComment,
				schedule,
				onlineOrder.Trifle
			);

			await uow.SaveAsync(template, cancellationToken: cancellationToken);
			
			var countItems = await AddItems(uow, onlineOrder, template, cancellationToken);

			if(countItems == 0)
			{
				throw new InvalidOperationException("Шаблон автозаказа не может быть без единого товара!");
			}

			onlineOrder.Template = template;

			if(template.NeedAdvancePayment())
			{
				await ProcessAdvancePaymentOnlineOrder(uow, onlineOrder, cancellationToken);
			}
			else
			{
				//TODO-6121 в этом методе коммит происходит напрямую, если нам нужно внешнее управление - переделать
				await _unPaidOnlineOrderHandler.ProcessPostPaymentOnlineOrderWithTemplate(uow, onlineOrder, cancellationToken);
			}
			
			//TODO-6115 отправка уведомления с пушем о подключении автозаказа

			return template;
		}

		private static async Task ProcessAdvancePaymentOnlineOrder(
			IUnitOfWork uow,
			OnlineOrder onlineOrder,
			CancellationToken cancellationToken
			)
		{
			var timers = uow.GetAll<OnlineOrderTimers>().FirstOrDefault();

			if(timers is null)
			{
				throw new InvalidOperationException(
					$"Не найдены временные метки для онлайн заказа, проверьте {nameof(OnlineOrderTimers)} и их настройки в базе");
			}

			onlineOrder.ToPayTemplate((int)timers.AutoOrderPayTime.TotalSeconds, OnlineOrderTimestampType.AutoOrderPayTime);
			await uow.SaveAsync(onlineOrder.TemplatePayStartTime, cancellationToken: cancellationToken);
			await uow.SaveAsync(onlineOrder, cancellationToken: cancellationToken);
		}

		private async Task<int> AddItems(
			IUnitOfWork uow,
			OnlineOrder onlineOrder,
			OnlineOrderTemplate template,
			CancellationToken cancellationToken
			)
		{
			var countItems = 0;
			
			countItems += await AddSaleItems(uow, onlineOrder, template, cancellationToken);
			countItems += await AddPromoSets(uow, onlineOrder, template, cancellationToken);
			countItems += await AddRentPackages(uow, onlineOrder, template, cancellationToken);
			
			return countItems;
		}

		private async Task<int> AddSaleItems(
			IUnitOfWork uow,
			OnlineOrder onlineOrder,
			OnlineOrderTemplate template,
			CancellationToken cancellationToken)
		{
			var count = 0;
			
			foreach(var item in onlineOrder.OnlineOrderItems)
			{
				if(item.GiftItem)
				{
					continue;
				}
				
				var saleItem = OnlineOrderTemplateSaleItem.Create(
					template.Id,
					NewSaleItem.Create(
						item.Nomenclature,
						item.Count,
						item.IsFixedPrice ? (SaleItemPriceType.Fixed, item.Price) : (SaleItemPriceType.General, item.Price),
						promoSet: item.PromoSet,
						discountReasons: item.DiscountReasons)
				);
				
				await uow.SaveAsync(saleItem, cancellationToken: cancellationToken);
				template.SaleItems.Add(saleItem.Id);
				count += (int)saleItem.Count;
			}
			
			return count;
		}

		private async Task<int> AddPromoSets(
			IUnitOfWork uow,
			OnlineOrder onlineOrder,
			OnlineOrderTemplate template,
			CancellationToken cancellationToken
		)
		{
			var count = 0;
			var onlineOrderV2 = onlineOrder.As<OnlineOrderV2>();

			if(onlineOrderV2 is null)
			{
				throw new InvalidOperationException($"Для создания промонаборов шаблона онлайн заказ должен быть типом {nameof(OnlineOrderV2)}");
			}

			foreach(var onlineOrderPromoSet in onlineOrderV2.PromoSets)
			{
				var templatePromoSet = OnlineOrderTemplatePromoSet.Create(
					template.Id,
					onlineOrderPromoSet.Count,
					onlineOrderPromoSet.Price,
					onlineOrderPromoSet.PromoSet,
					onlineOrderPromoSet.PersonalDiscount,
					onlineOrderPromoSet.DiscountReasons
				);
				
				await uow.SaveAsync(templatePromoSet, cancellationToken: cancellationToken);
				template.PromoSets.Add(templatePromoSet.Id);
				count += (int)templatePromoSet.Count;
			}

			return count;
		}
		
		private async Task<int> AddRentPackages(
			IUnitOfWork uow,
			OnlineOrder onlineOrder,
			OnlineOrderTemplate template,
			CancellationToken cancellationToken
		)
		{
			var count = 0;
			
			foreach(var onlineRentPackage in onlineOrder.OnlineRentPackages)
			{
				var templateFreeRentPackage = OnlineOrderTemplateFreeRentPackage.Create(
					template.Id,
					onlineRentPackage.Count,
					onlineRentPackage.Price,
					onlineRentPackage.FreeRentPackage
				);
				
				await uow.SaveAsync(templateFreeRentPackage, cancellationToken: cancellationToken);
				template.FreeRentPackages.Add(templateFreeRentPackage.Id);
				count += templateFreeRentPackage.Count;
			}
			
			return count;
		}
	}
}
