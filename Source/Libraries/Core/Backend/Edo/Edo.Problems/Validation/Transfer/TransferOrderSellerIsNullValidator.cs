using Edo.Problems.Exception.TransferOrders;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Results;

namespace Edo.Problems.Validation.Transfer
{
	public class TransferOrderSellerIsNullValidator : TransferOrderValidatorBase
	{
		public override string Name => "TransferOrder.SellerIsNull";
		public override EdoProblemImportance Importance => EdoProblemImportance.Problem;
		public override string Message => "В заказе перемещения товаров не указан продавец";
		public override string Description => "Проверяет, что продавец не является пустым";
		public override string Recommendation => "Проверьте, что в заказе указан продавец";

		public override bool IsApplicable(TransferOrder transferOrder)
		{
			return transferOrder is TransferOrder;
		}

		public override Task<Result<TransferOrder, TransferOrderValidationError>> Validate(TransferOrder transferOrder)
		{
			if(transferOrder.Seller == null)
			{
				return Task.FromResult(Result<TransferOrder, TransferOrderValidationError>
					.Failure(new TransferOrderValidationError(transferOrder)));
			}

			return Task.FromResult(Result<TransferOrder, TransferOrderValidationError>
				.Success(transferOrder));
		}
	}
}
