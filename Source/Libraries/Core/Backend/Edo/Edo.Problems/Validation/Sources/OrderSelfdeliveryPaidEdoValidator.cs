﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Edo;

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
			var isSelfdelivery = orderEdoRequest.Order.SelfDelivery;
			return isOrder && isSelfdelivery;
		}

		public override Task<EdoValidationResult> ValidateAsync(EdoTask edoTask, IServiceProvider serviceProvider, CancellationToken cancellationToken)
		{
			var orderEdoRequest = GetOrderEdoRequest(edoTask);
			if(!orderEdoRequest.Order.SelfDelivery)
			{
				return Task.FromResult(EdoValidationResult.Valid(this));
			}

			var invalid = !orderEdoRequest.Order.IsSelfDeliveryPaid;

			if(invalid)
			{
				return Task.FromResult(EdoValidationResult.Invalid(this));
			}

			return Task.FromResult(EdoValidationResult.Valid(this));
		}
	}
}
