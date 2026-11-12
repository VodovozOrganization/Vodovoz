using System.Collections.Generic;
using CustomerOrders.Contracts.V8.Sale.Templates;
using CustomerOrdersApi.Library.V8.Extensions;
using VodovozBusiness.Sorting;

namespace CustomerOrdersApi.Library.V8.Services.Comparers
{
	/// <summary>
	/// Класс для сортировки OrderTemplateCardDto, используется для сортировки в эндпойнте GetOrderTemplates
	/// </summary>
	public class OrderTemplateCardDtoComparer : IComparer<OrderTemplateCardDto>
	{
		/// <summary>
		/// Порядок: PaymentProblem → ManualProcessing → AwaitingPayment → Active (по ближайшей доставке) → Paused → Archived.
		/// </summary>
		/// <param name="x">Первый сравниваемый объект</param>
		/// <param name="y">Второй сравниваемый объект</param>
		/// <returns>Знаковое значение разницы порядков</returns>
		public int Compare(OrderTemplateCardDto x, OrderTemplateCardDto y)
		{
			return 0;
			if(x == null && y == null) return 0;
			if(x == null) return -1;
			if(y == null) return 1;

			var priorityX = OrderTemplateCardSortingPriority.GetByStatus(x.Status.ToTemplateStatus());
			var priorityY = OrderTemplateCardSortingPriority.GetByStatus(y.Status.ToTemplateStatus());

			// Если приоритеты статусов равны и оба Active — сортируем по ближайшей доставке
			if(priorityX == priorityY)
			{
				if(x.Status == TemplateStatusDto.Active && y.Status == TemplateStatusDto.Active)
				{
					// Null-даты идут после имеющих дату
					if(!x.NextDeliveryDate.HasValue && !y.NextDeliveryDate.HasValue)
					{
						return 0;
					}

					if(!x.NextDeliveryDate.HasValue)
					{
						return 1;
					}

					if(!y.NextDeliveryDate.HasValue)
					{
						return -1;
					}

					return x.NextDeliveryDate.Value.CompareTo(y.NextDeliveryDate.Value);
				}

				return (int)x.Status - (int)y.Status;
			}
			
			return priorityX.CompareTo(priorityY);
		}
	}
}
