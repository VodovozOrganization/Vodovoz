using Core.Infrastructure;
using System;
using System.Collections.Generic;
using System.Text;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Domain.Orders;

namespace Edo.OrderDocuments
{
	public class EdoOrderValidator
	{
		public EdoOrderValidationResult Validate(OrderEntity order)
		{
			if(order.OrderSum <= 0)
			{
				return new EdoOrderValidationResult
				{
					State = EdoOrderValidationState.Problem,
					Message = "Сумма заказа не может быть меньше или равна нулю"
				};
			}

			//Статус заказа

			//Разрешение отправки документов

			//Самовывоз

			//Оплата самовывоза

			//Валидность кодов

			if(order.OrderStatus.IsIn() == OrderStatus.DeliveryCanceled)
			{

			}

		}
	}

	public class EdoOrderValidationResult
	{
		public EdoOrderValidationState State { get; set; }
		public string Message { get; set; }
	}

	public enum EdoOrderValidationState
	{
		Valid,
		Problem,
		Waiting
	}
}
