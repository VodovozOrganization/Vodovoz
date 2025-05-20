using Edo.Problems.Exception.TransferOrders;
using System;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Results;

namespace Edo.Problems.Validation.Transfer
{
	public class TransferOrderDateIsNotSetValidator : TransferOrderValidatorBase
	{
		public override string Name => "TransferOrder.DateIsNotSet";
		public override EdoProblemImportance Importance => EdoProblemImportance.Problem;
		public override string Message => "В заказе перемещения товаров не указана дата";
		public override string Description => "Проверяет, что дата указана";
		public override string Recommendation => "Проверьте, что в заказе указана дата";

		public override bool IsApplicable(TransferOrder transferOrder)
		{
			return transferOrder is TransferOrder
				&& transferOrder.Seller != null
				&& transferOrder.Customer != null;
		}

		public override Task<Result<TransferOrder, TransferOrderValidationError>> Validate(TransferOrder transferOrder)
		{
			if(transferOrder?.Date == null || transferOrder?.Date == default)
			{
				return Task.FromResult(Result<TransferOrder, TransferOrderValidationError>
					.Failure(new TransferOrderValidationError(transferOrder)));
			}

			return Task.FromResult(Result<TransferOrder, TransferOrderValidationError>
				.Success(transferOrder));
		}
	}
}
