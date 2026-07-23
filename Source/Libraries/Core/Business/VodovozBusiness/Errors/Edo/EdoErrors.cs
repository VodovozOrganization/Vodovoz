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

		public static Error IsUndeliveredOrder =>
			new Error(typeof(EdoErrors),
				nameof(IsUndeliveredOrder),
				"Невозможно переотправить документ у отмененного заказа");

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

		/// <summary>
		/// Не указан идентификатор заказа-источника для переноса кодов маркировки.
		/// </summary>
		public static Error SourceOrderIdMissing =>
			new Error(
				typeof(EdoErrors),
				nameof(SourceOrderIdMissing),
				"Не указан заказ-источник.");

		/// <summary>
		/// Не указан идентификатор заказа-получателя для переноса кодов маркировки.
		/// </summary>
		public static Error TargetOrderIdMissing =>
			new Error(
				typeof(EdoErrors),
				nameof(TargetOrderIdMissing),
				"Не указан заказ, в который нужно перенести коды.");

		/// <summary>
		/// Заказ-источник и заказ-получатель совпадают.
		/// </summary>
		public static Error SameTransferOrder =>
			new Error(
				typeof(EdoErrors),
				nameof(SameTransferOrder),
				"Нельзя перенести коды в тот же самый заказ.");

		/// <summary>
		/// Заказ-источник не найден.
		/// </summary>
		public static Error SourceOrderNotFound =>
			new Error(
				typeof(EdoErrors),
				nameof(SourceOrderNotFound),
				"Заказ-источник не найден.");

		/// <summary>
		/// Заказ-источник не отменен полностью.
		/// </summary>
		public static Error SourceOrderNotCanceled =>
			new Error(
				typeof(EdoErrors),
				nameof(SourceOrderNotCanceled),
				"Переносить коды можно только из полностью отмененного заказа.");

		/// <summary>
		/// Заказ-получатель не найден.
		/// </summary>
		public static Error TargetOrderNotFound =>
			new Error(
				typeof(EdoErrors),
				nameof(TargetOrderNotFound),
				"Целевой заказ не найден.");

		/// <summary>
		/// Заказ-получатель отменен.
		/// </summary>
		public static Error TargetOrderCanceled =>
			new Error(
				typeof(EdoErrors),
				nameof(TargetOrderCanceled),
				"Нельзя перенести коды в отмененный заказ.");

		/// <summary>
		/// В заказе-источнике отсутствуют отклоненные коды маркировки.
		/// </summary>
		public static Error RejectedCodesNotFound =>
			new Error(
				typeof(EdoErrors),
				nameof(RejectedCodesNotFound),
				"В отмененном заказе нет отклоненных кодов для переноса.");

		/// <summary>
		/// В заказе-источнике обнаружены повторяющиеся отклоненные коды маркировки.
		/// </summary>
		public static Error DuplicateRejectedCodes =>
			new Error(
				typeof(EdoErrors),
				nameof(DuplicateRejectedCodes),
				"В отмененном заказе есть повторяющиеся коды. Перенос отменен.");

		/// <summary>
		/// Переносимые коды маркировки уже используются в другом заказе или документе.
		/// </summary>
		public static Error ProductCodesAlreadyUsed =>
			new Error(
				typeof(EdoErrors),
				nameof(ProductCodesAlreadyUsed),
				"Часть кодов уже используется в другом заказе или документе. Перенос отменен.");

		/// <summary>
		/// В заказе-получателе отсутствуют товары, требующие коды маркировки.
		/// </summary>
		public static Error TargetOrderItemsNotFound =>
			new Error(
				typeof(EdoErrors),
				nameof(TargetOrderItemsNotFound),
				"В целевом заказе нет товаров, требующих коды маркировки.");

		/// <summary>
		/// В заказе-получателе недостаточно товаров с указанным GTIN для переноса кодов маркировки.
		/// </summary>
		/// <param name="gtin">GTIN переносимого кода маркировки</param>
		public static Error CreateInsufficientTargetOrderItems(string gtin) =>
			new Error(
				typeof(EdoErrors),
				nameof(CreateInsufficientTargetOrderItems),
				$"В целевом заказе недостаточно товаров с GTIN {gtin} для переноса кодов.");

		public static Error CreateResendTimeLimitExceeded(OutgoingEdoDocument edoDocument, int orderId) =>
			new Error(
				typeof(EdoErrors),
				nameof(ResendTimeLimitExceeded),
				$"Для заказа №{orderId} " +
				$"истек срок переотправки документа. " +
				$"Документ был отправлен {edoDocument.SendTime?.ToString("dd.MM.yyyy HH:mm")}, " +
				$"переотправка возможна в течение 3х месяцев");

		public static Error CreateCannotResendReceiptFromSavedToPoolTask(int orderId) =>
			new Error(
				typeof(EdoErrors),
				nameof(CreateCannotResendReceiptFromSavedToPoolTask),
				$"Помимо задачи на сохранение кодов по заказу {orderId}, есть другая задача");

		public static Error CreateCannotResendCompletedTask(int taskId) =>
			new Error(
				typeof(EdoErrors), 
				nameof(CreateCannotResendCompletedTask), 
				$"Нельзя переотправить завершенную задачу {taskId}");

		public static Error CreateCannotResendCompletedReceipt(int taskId) =>
			new Error(
				typeof(EdoErrors), 
				nameof(CreateCannotResendCompletedReceipt), 
				$"Нельзя переотправить завершенный чек {taskId}");

		public static Error CreateCannotResendReceiptFromSavedToPool(int taskId) =>
			new Error(
				typeof(EdoErrors), 
				nameof(CreateCannotResendReceiptFromSavedToPool), 
				$"Нельзя переотправить чек {taskId} из пула");

		public static Error CreateCannotResendReceiptWithFiscalNumber(int taskId) =>
			new Error(
				typeof(EdoErrors), 
				nameof(CreateCannotResendReceiptWithFiscalNumber), 
				$"Нельзя переотправить чек {taskId} с фискальным номером");

		public static Error CreateCannotResendPrintedOrCompletedReceipt(int taskId) =>
			new Error(
				typeof(EdoErrors), 
				nameof(CreateCannotResendPrintedOrCompletedReceipt), 
				$"Нельзя переотправить напечатанный или завершенный чек {taskId}");
	}
}
