using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Edo;

namespace Edo.Problems.Validation.Sources
{
	public class AnyOrderItemHavePositivePriceValidator : EdoTaskProblemValidatorSource
	{
		public override string Name => "Order.AnyOrderItemHavePositivePrice";
		public override EdoProblemImportance Importance => EdoProblemImportance.Problem;
		public override string Message => "В заказе должна быть хоть одна позиция с ценой больше 0";
		public override string Description => "Проверяет позиции заказа, чтобы цена была больше 0 хотя бы в одном из товаров";
		public override string Recommendation => "Проверьте товары в заказе, возможно не установлена цена в карточке номенклатуры";

		public override bool IsApplicable(EdoTask edoTask)
		{
			return edoTask is DocumentEdoTask docTask && docTask.DocumentType == EdoDocumentType.UPD;
		}

		public override string GetTemplatedMessage(EdoTask edoTask)
		{
			var orderEdoRequest = GetOrderEdoRequest(edoTask);
			return orderEdoRequest == null ?
				Message
				: $"Хотя бы один из товаров заказа №{orderEdoRequest.Order.Id} должен быть с ценой больше 0";
		}

		public override Task<EdoValidationResult> ValidateAsync(
			EdoTask edoTask,
			IServiceProvider serviceProvider,
			CancellationToken cancellationToken)
		{
			var orderEdoRequest = GetOrderEdoRequest(edoTask);
			var invalid = orderEdoRequest.Order.OrderItems.All(x => x.Price <= 0);

			return Task.FromResult(invalid ? EdoValidationResult.Invalid(this) : EdoValidationResult.Valid(this));
		}

		private PrimaryEdoRequest GetOrderEdoRequest(EdoTask edoTask)
		{
			return ((OrderEdoTask)edoTask).OrderEdoRequest;
		}
	}
}
