using System;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Domain.Orders;

namespace Edo.Problems.Validation.Sources
{
	public class OrderStatusEdoValidator : OrderEdoValidatorBase, IEdoTaskValidator
	{
		public override string Name
		{
			get => "Order.Status";
		}

		public override EdoProblemImportance Importance
		{
			get => EdoProblemImportance.Waiting;
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
			var orderEdoRequest = GetEdoRequest(edoTask);
			if(orderEdoRequest == null)
			{
				return Message;
			}
			return $"Заказ №{orderEdoRequest.Order.Id} должен как минимум быть отправлен со склада в путь";
		}

		public override Task<EdoValidationResult> ValidateAsync(EdoTask edoTask, IServiceProvider serviceProvider, CancellationToken cancellationToken)
		{
			var orderEdoRequest = GetEdoRequest(edoTask);
			var invalid = orderEdoRequest.Order.OrderStatus < OrderStatus.OnTheWay;

			if(orderEdoRequest.Order.SelfDelivery && orderEdoRequest.Order.IsOrderForResale)
			{
				return Task.FromResult(EdoValidationResult.Valid(this));
			}

			return Task.FromResult(invalid ? EdoValidationResult.Invalid(this) : EdoValidationResult.Valid(this));
		}
	}
}
