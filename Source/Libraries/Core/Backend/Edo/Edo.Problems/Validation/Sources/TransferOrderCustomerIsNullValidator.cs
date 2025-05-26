using System;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Edo;

namespace Edo.Problems.Validation.Sources
{
	public class TransferOrderCustomerIsNullValidator : TransferOrderValidatorBase
	{
		public override string Name => "TransferOrder.CustomerIsNull";
		public override EdoProblemImportance Importance => EdoProblemImportance.Problem;
		public override string Message => "В заказе перемещения товаров не указан покупатель";
		public override string Description => "Проверяет, что заказ не является пустым";
		public override string Recommendation => "Проверьте, что в заказе указан покупатель";
		public override bool IsApplicable(EdoTask edoTask)
		{
			return edoTask is TransferEdoTask
				&& GetTransferOrder(edoTask as TransferEdoTask) is TransferOrder transferOrder
				&& transferOrder.Seller != null;
		}
		public override Task<EdoValidationResult> ValidateAsync(
			EdoTask edoTask,
			IServiceProvider serviceProvider,
			CancellationToken cancellationToken)
		{
			var transferOrder = GetTransferOrder(edoTask as TransferEdoTask, serviceProvider);
			if(transferOrder?.Customer == null)
			{
				return Task.FromResult(EdoValidationResult.Invalid(this));
			}

			return Task.FromResult(EdoValidationResult.Valid(this));
		}
	}
}
