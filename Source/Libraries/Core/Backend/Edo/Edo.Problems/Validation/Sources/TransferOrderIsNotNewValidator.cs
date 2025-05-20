using System;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Edo;

namespace Edo.Problems.Validation.Sources
{
	public class TransferOrderIsNotNewValidator : TransferOrderValidatorBase
	{
		public override string Name => "TransferOrder.IsNotNew";
		public override EdoProblemImportance Importance => EdoProblemImportance.Problem;
		public override string Message => "При заполнении данных в УПД необходимо, чтобы заказ перемещения товаров был предварительно сохранен";
		public override string Description => "Проверяет, что заказ не является новым";
		public override string Recommendation => "Проверьте, что заказ сохранен";
		public override bool IsApplicable(EdoTask edoTask)
		{
			return edoTask is TransferEdoTask
				&& GetTransferOrder(edoTask as TransferEdoTask) is TransferOrder transferOrder
				&& transferOrder.Seller != null
				&& transferOrder.Customer != null
				&& transferOrder.Date != null
				&& transferOrder.Date != default;
		}
		public override Task<EdoValidationResult> ValidateAsync(
			EdoTask edoTask,
			IServiceProvider serviceProvider,
			CancellationToken cancellationToken)
		{
			var transferOrder = GetTransferOrder(edoTask as TransferEdoTask, serviceProvider);

			if(transferOrder.Id == 0)
			{
				return Task.FromResult(EdoValidationResult.Invalid(this));
			}

			return Task.FromResult(EdoValidationResult.Valid(this));
		}
	}
}
