using System;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Edo;

namespace Edo.Problems.Validation.Sources
{
	public class TransferOrderDateIsNotSetValidator : TransferOrderValidatorBase
	{
		public override string Name => "TransferOrder.DateIsNotSet";
		public override EdoProblemImportance Importance => EdoProblemImportance.Problem;
		public override string Message => "В заказе перемещения товаров не указана дата";
		public override string Description => "Проверяет, что дата указана";
		public override string Recommendation => "Проверьте, что в заказе указана дата";
		public override bool IsApplicable(EdoTask edoTask)
		{
			return edoTask is TransferEdoTask
				&& GetTransferOrder(edoTask as TransferEdoTask) is TransferOrder transferOrder
				&& transferOrder.Seller != null
				&& transferOrder.Customer != null;
		}
		public override Task<EdoValidationResult> ValidateAsync(
			EdoTask edoTask,
			IServiceProvider serviceProvider,
			CancellationToken cancellationToken)
		{
			var transferOrder = GetTransferOrder(edoTask as TransferEdoTask, serviceProvider);

			if(transferOrder?.Date == null || transferOrder?.Date == default)
			{
				return Task.FromResult(EdoValidationResult.Invalid(this));
			}

			return Task.FromResult(EdoValidationResult.Valid(this));
		}
	}
}
