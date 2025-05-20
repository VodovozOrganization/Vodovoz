using System;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Edo;

namespace Edo.Problems.Validation.Sources
{
	public class TransferOrderIsNullValidator : TransferOrderValidatorBase
	{
		public override string Name => "TransferOrder.IsNull";
		public override EdoProblemImportance Importance => EdoProblemImportance.Problem;
		public override string Message => "Заказ по перемещению подотчетного в ЧЗ товара между организациями не может быть пустым";
		public override string Description => "Проверяет, что заказ по перемещению подотчетного в ЧЗ товара между организациями существует и указан в задаче по перемещению";
		public override string Recommendation => "Проверьте, что в задаче по перемещению указан заказ по перемещению подотчетного в ЧЗ товара между организациями";

		public override bool IsApplicable(EdoTask edoTask)
		{
			return edoTask is TransferEdoTask;
		}

		public override Task<EdoValidationResult> ValidateAsync(
			EdoTask edoTask,
			IServiceProvider serviceProvider,
			CancellationToken cancellationToken)
		{
			var transferOrder = GetTransferOrder(edoTask as TransferEdoTask, serviceProvider);

			if(transferOrder is null)
			{
				return Task.FromResult(EdoValidationResult.Invalid(this));
			}

			return Task.FromResult(EdoValidationResult.Valid(this));
		}
	}
}
