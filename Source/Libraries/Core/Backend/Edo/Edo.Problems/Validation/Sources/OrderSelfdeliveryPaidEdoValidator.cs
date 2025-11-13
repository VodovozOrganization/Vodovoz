using System;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders;

namespace Edo.Problems.Validation.Sources
{
	public class OrderSelfdeliveryPaidEdoValidator : OrderEdoValidatorBase
	{
		public override string Name
		{
			get => "Order.SelfdeliveryPaid";
		}

		public override EdoProblemImportance Importance
		{
			get => EdoProblemImportance.Waiting;
		}

		public override string Message
		{
			get => "Самовывоз должен быть оплачен";
		}

		public override string Description
		{
			get => "Проверяет что заказ самовывоза оплачен";
		}
		public override string Recommendation
		{
			get => "Необходимо удостоверится в оплате и отметить заказ как оплаченный.";
		}

		public string GetTemplateMessage(EdoTask edoTask)
		{
			var orderEdoRequest = GetOrderEdoRequest(edoTask);
			if(orderEdoRequest == null)
			{
				return Message;
			}
			return $"Самовывоз №{orderEdoRequest.Order.Id} должен быть оплачен";
		}

		public override bool IsApplicable(EdoTask edoTask)
		{
			var isOrder = base.IsApplicable(edoTask);
			if(!isOrder)
			{
				return isOrder;
			}
			
			var orderEdoTask = (OrderEdoTask)edoTask;
			var orderEdoRequest = orderEdoTask.OrderEdoRequest;
			var order = orderEdoRequest.Order;
			return isOrder && order.SelfDelivery && !order.IsOrderForResale;
		}

		public override Task<EdoValidationResult> ValidateAsync(EdoTask edoTask, IServiceProvider serviceProvider, CancellationToken cancellationToken)
		{
			var orderEdoRequest = GetOrderEdoRequest(edoTask);
			if(!orderEdoRequest.Order.SelfDelivery)
			{
				return Task.FromResult(EdoValidationResult.Valid(this));
			}

			switch(orderEdoRequest.Order.PaymentType)
			{
				case PaymentType.Cash:
				case PaymentType.Terminal:
				case PaymentType.DriverApplicationQR:
				case PaymentType.SmsQR:
				case PaymentType.PaidOnline:
				case PaymentType.Barter:
				case PaymentType.ContractDocumentation:
					if(!orderEdoRequest.Order.IsSelfDeliveryPaid)
					{
						return Task.FromResult(EdoValidationResult.Invalid(this));
					}
					break;
				case PaymentType.Cashless:
					if(orderEdoRequest.Order.OrderPaymentStatus != OrderPaymentStatus.Paid)
					{
						return Task.FromResult(EdoValidationResult.Invalid(this));
					}
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(orderEdoRequest.Order.PaymentType));
			}

			return Task.FromResult(EdoValidationResult.Valid(this));
		}
	}
}
