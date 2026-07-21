using Vodovoz.Core.Domain.Results;

namespace Vodovoz.Errors.Edo
{
	public static partial class EdoErrors
	{
		public static Error SourceOrderIdMissing => CreateTransferError(
			nameof(SourceOrderIdMissing), "Не указан заказ-источник.");

		public static Error TargetOrderIdMissing => CreateTransferError(
			nameof(TargetOrderIdMissing), "Не указан заказ, в который нужно перенести коды.");

		public static Error SameTransferOrder => CreateTransferError(
			nameof(SameTransferOrder), "Нельзя перенести коды в тот же самый заказ.");

		public static Error SourceOrderNotFound => CreateTransferError(
			nameof(SourceOrderNotFound), "Заказ-источник не найден.");

		public static Error SourceOrderNotCanceled => CreateTransferError(
			nameof(SourceOrderNotCanceled), "Переносить коды можно только из полностью отмененного заказа.");

		public static Error TargetOrderNotFound => CreateTransferError(
			nameof(TargetOrderNotFound), "Целевой заказ не найден.");

		public static Error TargetOrderCanceled => CreateTransferError(
			nameof(TargetOrderCanceled), "Нельзя перенести коды в отмененный заказ.");

		public static Error RejectedCodesNotFound => CreateTransferError(
			nameof(RejectedCodesNotFound), "В отмененном заказе нет отклоненных кодов для переноса.");

		public static Error DuplicateRejectedCodes => CreateTransferError(
			nameof(DuplicateRejectedCodes), "В отмененном заказе есть повторяющиеся коды. Перенос отменен.");

		public static Error ProductCodesAlreadyUsed => CreateTransferError(
			nameof(ProductCodesAlreadyUsed),
			"Часть кодов уже используется в другом заказе или документе. Перенос отменен.");

		public static Error TargetOrderItemsNotFound => CreateTransferError(
			nameof(TargetOrderItemsNotFound), "В целевом заказе нет товаров, требующих коды маркировки.");

		public static Error CreateInsufficientTargetOrderItems(string gtin) => CreateTransferError(
			nameof(CreateInsufficientTargetOrderItems),
			$"В целевом заказе недостаточно товаров с GTIN {gtin} для переноса кодов.");

		private static Error CreateTransferError(string name, string message) =>
			new Error(typeof(EdoErrors), name, message);
	}
}
