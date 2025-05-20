using Edo.Problems.Exception.TransferOrders;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Results;

namespace Edo.Problems.Validation.Transfer
{
	public class TransferOrderCustomerIsNullValidator : TransferOrderValidatorBase
	{
		public override string Name => "TransferOrder.CustomerIsNull";
		public override EdoProblemImportance Importance => EdoProblemImportance.Problem;
		public override string Message => "В заказе перемещения товаров не указан покупатель";
		public override string Description => "Проверяет, что заказ не является пустым";
		public override string Recommendation => "Проверьте, что в заказе указан покупатель";

		public override bool IsApplicable(TransferOrder transferOrder)
		{
			return transferOrder is TransferOrder
				&& transferOrder.Seller != null;
		}

		public override Task<Result<TransferOrder, TransferOrderValidationError>> Validate(TransferOrder transferOrder)
		{
			if(transferOrder.Customer == null)
			{
				return Task.FromResult(Result<TransferOrder, TransferOrderValidationError>
					.Failure(new TransferOrderValidationError(transferOrder)));
			}

			return Task.FromResult(Result<TransferOrder, TransferOrderValidationError>
				.Success(transferOrder));
		}
	}
}
