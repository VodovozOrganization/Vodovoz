using System;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Edo;

namespace Edo.Problems.Validation.Sources
{
	public class ReceiptCashboxValidator : EdoTaskProblemValidatorSource, IEdoTaskValidator
	{
		public override string Name => "Receipt.Cashbox";

		public override EdoProblemImportance Importance
		{
			get => EdoProblemImportance.Problem;
		}

		public override string Message
		{
			get => "На найдена касса в задаче по чеку на отправку";
		}

		public override string Description
		{
			get => "Касса заполняется автоматически из организации на этапе подготовки чека";
		}
		public override string Recommendation
		{
			get => "Проверить кассу в организации в заказе";
		}

		public override bool IsApplicable(EdoTask edoTask)
		{
			var receiptTask = edoTask as ReceiptEdoTask;
			if(receiptTask == null)
			{
				return false;
			}
			return receiptTask.ReceiptStatus == EdoReceiptStatus.Sending;
		}

		public override async Task<EdoValidationResult> ValidateAsync(EdoTask edoTask, IServiceProvider serviceProvider, CancellationToken cancellationToken)
		{
			var receiptTask = edoTask as ReceiptEdoTask;
			if(receiptTask.CashboxId == null)
			{
				return await Task.FromResult(EdoValidationResult.Invalid(this));
			}
			return await Task.FromResult(EdoValidationResult.Valid(this));
		}
	}
}
