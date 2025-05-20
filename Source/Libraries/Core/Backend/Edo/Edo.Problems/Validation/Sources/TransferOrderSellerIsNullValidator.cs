using System;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Edo;

namespace Edo.Problems.Validation.Sources
{
	public class TransferOrderSellerIsNullValidator : TransferOrderValidatorBase
	{
		public override string Name => "TransferOrder.SellerIsNull";
		public override EdoProblemImportance Importance => EdoProblemImportance.Problem;
		public override string Message => "В заказе перемещения товаров не указан продавец";
		public override string Description => "Проверяет, что продавец не является пустым";
		public override string Recommendation => "Проверьте, что в заказе указан продавец";
		public override bool IsApplicable(EdoTask edoTask)
		{
			return edoTask is TransferEdoTask
				&& GetTransferOrder(edoTask as TransferEdoTask) is TransferOrder;
		}
		public override Task<EdoValidationResult> ValidateAsync(
			EdoTask edoTask,
			IServiceProvider serviceProvider,
			CancellationToken cancellationToken)
		{
			var transferOrder = GetTransferOrder(edoTask as TransferEdoTask, serviceProvider);
			if(transferOrder?.Seller == null)
			{
				return Task.FromResult(EdoValidationResult.Invalid(this));
			}

			return Task.FromResult(EdoValidationResult.Valid(this));
		}
	}
}
