using System;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Orders.OrderEnums;

namespace Edo.TaskValidation.Validators
{
	public class OrderStatusEdoValidator : OrderEdoValidatorBase, IEdoTaskValidator
	{
		public override string Name
		{
			get => "Order.Status";
		}

		public override EdoValidationImportance Importance
		{
			get => EdoValidationImportance.Waiting;
		}

		public override string Message
		{
			get => "Заказ должен как минимум быть отправлен со склада в путь";
		}

		public override string Description
		{
			get => "Проверяет что заказ должен быть уже доставлен или отправлен в путь (для сетей)";
		}
		public override string Recommendation
		{
			get => "Подождать доставки заказа. Если сеть, то подождать отправки заказа со склада в путь";
		}

		public string GetTemplateMessage(EdoTask edoTask)
		{
			var orderEdoRequest = GetOrderEdoRequest(edoTask);
			if(orderEdoRequest == null)
			{
				return Message;
			}
			return $"Заказ №{orderEdoRequest.Order.Id} должен как минимум быть отправлен со склада в путь";
		}

		public override Task<bool> NotValidCondition(EdoTask edoTask, IServiceProvider serviceProvider, CancellationToken cancellationToken)
		{
			var orderEdoRequest = GetOrderEdoRequest(edoTask);
			var condition = orderEdoRequest.Order.OrderStatus < OrderStatus.OnTheWay;

			return Task.FromResult(condition);
		}
	}
}
