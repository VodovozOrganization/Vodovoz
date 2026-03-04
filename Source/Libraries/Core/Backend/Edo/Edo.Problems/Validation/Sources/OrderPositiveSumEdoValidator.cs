using System;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Edo;

namespace Edo.Problems.Validation.Sources
{
	public class OrderPositiveSumEdoValidator : OrderEdoValidatorBase
	{
		public override string Name => "Order.PositiveSum";
		public override EdoProblemImportance Importance => EdoProblemImportance.Problem;
		public override string Message => "Сумма заказа должна быть больше нуля";
		public override string Description => "Проверяет сумму заказа, чтобы сумма заказа была более нуля";
		public override string Recommendation => "Проверьте товары в заказе, возможно установлена не корректная скидка";

		public override string GetTemplatedMessage(EdoTask edoTask)
		{
			var orderEdoRequest = GetEdoRequest(edoTask);
			if(orderEdoRequest == null)
			{
				return Message;
			}
			return $"Сумма заказа №{orderEdoRequest.Order.Id} должна быть больше нуля";
		}

		public override Task<EdoValidationResult> ValidateAsync(EdoTask edoTask, IServiceProvider serviceProvider, CancellationToken cancellationToken)
		{
			var orderEdoRequest = GetEdoRequest(edoTask);
			var invalid = orderEdoRequest.Order.OrderSum <= 0;

			if(invalid)
			{
				return Task.FromResult(EdoValidationResult.Invalid(this));
			}
			else
			{
				return Task.FromResult(EdoValidationResult.Valid(this));
			}
		}
	}
}
