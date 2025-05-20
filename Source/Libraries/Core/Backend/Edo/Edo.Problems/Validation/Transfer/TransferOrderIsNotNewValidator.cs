using Edo.Problems.Exception.TransferOrders;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Results;

namespace Edo.Problems.Validation.Transfer
{
	public class TransferOrderIsNotNewValidator : TransferOrderValidatorBase
	{
		public override string Name => "TransferOrder.IsNotNew";
		public override EdoProblemImportance Importance => EdoProblemImportance.Problem;
		public override string Message => "При заполнении данных в УПД необходимо, чтобы заказ перемещения товаров был предварительно сохранен";
		public override string Description => "Проверяет, что заказ не является новым";
		public override string Recommendation => "Проверьте, что заказ сохранен";

		public override bool IsApplicable(TransferOrder transferOrder)
		{
			return transferOrder is TransferOrder
				&& transferOrder.Seller != null
				&& transferOrder.Customer != null
				&& transferOrder.Date != null
				&& transferOrder.Date != default;
		}

		public override Task<Result<TransferOrder, TransferOrderValidationError>> Validate(TransferOrder transferOrder)
		{
			if(transferOrder.Id == 0)
			{
				return Task.FromResult(Result<TransferOrder, TransferOrderValidationError>
					.Failure(new TransferOrderValidationError(transferOrder)));
			}

			return Task.FromResult(Result<TransferOrder, TransferOrderValidationError>
				.Success(transferOrder));
		}
	}
}
