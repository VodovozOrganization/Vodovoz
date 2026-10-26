using System;
using System.Threading;
using System.Threading.Tasks;
using QS.DomainModel.UoW;
using QS.Extensions.Observable.Collections.List;
using Vodovoz.Core.Domain.Sale;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Employees;
using VodovozBusiness.Domain.Orders;
using VodovozBusiness.Domain.Sale;

namespace Vodovoz.Core.Application.Sale
{
	public class OnlineOrderTemplateCreator
	{
		private readonly IEmployeeRepository _employeeRepository;

		public OnlineOrderTemplateCreator(
			IEmployeeRepository employeeRepository
			)
		{
			_employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
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

			//TODO 5965: подумать насчет расчета и идентификатора шаблона
			var templateSaleItems = new ObservableList<OnlineOrderTemplateSaleItem>();
			foreach(var orderItem in onlineOrder.OnlineOrderItems)
			{
				var product = OnlineOrderTemplateSaleItem.Create(
					template.Id,
					NewSaleItem.Create(
						orderItem.Nomenclature,
						orderItem.Count,
						orderItem.IsFixedPrice ? (SaleItemPriceType.Fixed, orderItem.Price) : (SaleItemPriceType.General, orderItem.Price),
						promoSet: orderItem.PromoSet,
						discountReasons: orderItem.DiscountReasons)
				);
				
				templateSaleItems.Add(product);
			}
			
			await uow.SaveAsync(template, cancellationToken: cancellationToken);

			foreach(var saleItem in templateSaleItems)
			{
				saleItem.TemplateId = template.Id;
			}
			
			onlineOrder.Template = template;
			await uow.SaveAsync(onlineOrder, cancellationToken: cancellationToken);
			
			//TODO-6115 отправка уведомления с пушем о подключении автозаказа

			return template;
		}
	}
}
