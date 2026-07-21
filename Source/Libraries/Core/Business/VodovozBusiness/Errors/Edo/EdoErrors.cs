using System.Collections.Generic;
using System.Linq;
using Vodovoz.Core.Domain.Documents;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain.Orders.Documents;
using Vodovoz.Extensions;

namespace Vodovoz.Errors.Edo
{
	public static class EdoErrors
	{
		public static Error AlreadyPaidUpd =>
			new Error(typeof(EdoErrors),
				nameof(AlreadyPaidUpd),
				"Маршрутный лист не найден");

		public static Error CreateAlreadyPaidUpd(int orderId, DocumentContainerType type) =>
			 new Error(
				typeof(EdoErrors),
				nameof(AlreadyPaidUpd),
				$"Счет по заказу №{orderId} оплачен.\r\nПроверьте, пожалуйста, статус {type.GetEnumDisplayName()} в ЭДО перед повторной отправкой на предмет аннулирован/не аннулирован, подписан/не подписан.");
		
		public static Error CreateAlreadyPaidUpd(int orderId, EdoDocumentType type) =>
			 new Error(
				typeof(EdoErrors),
				nameof(AlreadyPaidUpd),
				$"Счет по заказу №{orderId} оплачен.\r\nПроверьте, пожалуйста, статус {type.GetEnumDisplayName()} в ЭДО перед повторной отправкой на предмет аннулирован/не аннулирован, подписан/не подписан.");

		public static Error AlreadySuccefullSended =>
			new Error(typeof(EdoErrors),
				nameof(AlreadySuccefullSended),
				"Документы уже успешно отправлены");

		public static Error ResendTimeLimitExceeded =>
			new Error(typeof(EdoErrors),
				nameof(ResendTimeLimitExceeded),
				"Истек срок переотправки документа");

		public static Error ResendableEdoDocumentStatuses =>
			new Error(typeof(EdoErrors),
				nameof(ResendableEdoDocumentStatuses),
				"Документ ещё действителен");

		public static Error InvalidOutgoingDocumentType =>
			new Error(typeof(EdoErrors),
				nameof(InvalidOutgoingDocumentType),
				"Некорректный тип документа");

		public static Error NoActiveEdoTaskForResend =>
			new Error(typeof(EdoErrors),
				nameof(NoActiveEdoTaskForResend),
				"Нет активной ЭДО задачи для переотправки");

		public static Error HasProblem =>
			new Error(typeof(EdoErrors),
				nameof(HasProblem),
				"Произошла ошибка во время переотправки документа");

		public static Error CreateAlreadySuccefullSended(EdoContainer edoContainer) =>
			 new Error(
				typeof(EdoErrors),
				nameof(AlreadySuccefullSended),
				$"Для заказа №" +
				 $"{edoContainer.Order?.Id ?? edoContainer.OrderWithoutShipmentForDebt?.Id ?? edoContainer.OrderWithoutShipmentForPayment?.Id ?? edoContainer.OrderWithoutShipmentForAdvancePayment?.Id} " +
				 $"имеется документ со статусом \"{edoContainer.EdoDocFlowStatus.GetEnumDisplayName()}\"");

		public static Error CreateAlreadySuccefullSended(OrderEntity order, OrderEdoDocument edoDocument) =>
			 new Error(
				typeof(EdoErrors),
				nameof(AlreadySuccefullSended),
				$"Для заказа № {order?.Id} " +
				 $"имеется документ со статусом \"{edoDocument.Status.GetEnumDisplayName()}\"");

		public static Error CreateResendableEdoDocumentStatuses(int orderId, IEnumerable<EdoDocumentStatus> statuses) =>
			 new Error(
				typeof(EdoErrors),
				nameof(ResendableEdoDocumentStatuses),
				$"Документ по заказу {orderId} можно переотправить только в статусах: " +
				 $"{string.Join(", ", statuses.Select(s => s.GetEnumDisplayName()))}");

		public static Error CreateInvalidOutgoingDocumentType(int orderId, OutgoingEdoDocumentType documentType) =>
			 new Error(
				typeof(EdoErrors),
				nameof(InvalidOutgoingDocumentType),
				$"У заказа {orderId} некорректный тип исходящего документа {documentType.GetEnumDisplayName()}");

		public static Error CreateResendTimeLimitExceeded(OutgoingEdoDocument edoDocument, int orderId) =>
		new Error(
			typeof(EdoErrors),
			nameof(ResendTimeLimitExceeded),
			$"Для заказа №{orderId} " +
			$"истек срок переотправки документа. " +
			$"Документ был отправлен {edoDocument.SendTime?.ToString("dd.MM.yyyy HH:mm")}, " +
			$"переотправка возможна в течение 3 дней");

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
