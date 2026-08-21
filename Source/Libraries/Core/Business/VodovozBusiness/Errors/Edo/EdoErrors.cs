using Core.Infrastructure;
using Renci.SshNet.Messages;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Core.Domain.Documents;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain.Orders.Documents;

namespace VodovozBusiness.Errors.Edo
{
	/// <summary>
	/// Содержит ошибки, связанные с ЭДО
	/// </summary>
	public static class EdoErrors
	{
		/// <summary>
		/// Ошибка: УПД уже оплачен
		/// </summary>
		public static Error AlreadyPaidUpd =>
			new Error(typeof(EdoErrors),
				nameof(AlreadyPaidUpd),
				"Маршрутный лист не найден");

		/// <summary>
		/// Создает ошибку о том, что УПД по заказу уже оплачен
		/// </summary>
		/// <param name="orderId">Идентификатор заказа</param>
		/// <param name="type">Тип контейнера документа</param>
		/// <returns>Ошибка с описанием</returns>
		public static Error CreateAlreadyPaidUpd(int orderId, DocumentContainerType type) =>
			 new Error(
				typeof(EdoErrors),
				nameof(AlreadyPaidUpd),
				$"Счет по заказу №{orderId} оплачен.\r\nПроверьте, пожалуйста, статус {type.GetEnumDisplayName()} в ЭДО перед повторной отправкой на предмет аннулирован/не аннулирован, подписан/не подписан.");

		/// <summary>
		/// Создает ошибку о том, что УПД по заказу уже оплачен
		/// </summary>
		/// <param name="orderId">Идентификатор заказа</param>
		/// <param name="type">Тип документа ЭДО</param>
		/// <returns>Ошибка с описанием</returns>
		public static Error CreateAlreadyPaidUpd(int orderId, EdoDocumentType type) =>
			 new Error(
				typeof(EdoErrors),
				nameof(AlreadyPaidUpd),
				$"Счет по заказу №{orderId} оплачен.\r\nПроверьте, пожалуйста, статус {type.GetEnumDisplayName()} в ЭДО перед повторной отправкой на предмет аннулирован/не аннулирован, подписан/не подписан.");

		/// <summary>
		/// Ошибка: документы уже успешно отправлены
		/// </summary>
		public static Error AlreadySuccefullSended =>
			new Error(typeof(EdoErrors),
				nameof(AlreadySuccefullSended),
				"Документы уже успешно отправлены");

		/// <summary>
		/// Ошибка: истек срок переотправки документа
		/// </summary>
		public static Error ResendTimeLimitExceeded =>
			new Error(typeof(EdoErrors),
				nameof(ResendTimeLimitExceeded),
				"Истек срок переотправки документа");

		/// <summary>
		/// Ошибка: документ ещё действителен
		/// </summary>
		public static Error ResendableEdoDocumentStatuses =>
			new Error(typeof(EdoErrors),
				nameof(ResendableEdoDocumentStatuses),
				"Документ ещё действителен");

		/// <summary>
		/// Ошибка: некорректный тип документа
		/// </summary>
		public static Error InvalidOutgoingDocumentType =>
			new Error(typeof(EdoErrors),
				nameof(InvalidOutgoingDocumentType),
				"Некорректный тип документа");

		/// <summary>
		/// Ошибка: нет отмененной задачи ЭДО для переотправки
		/// </summary>
		public static Error NoCancelledEdoTaskForResend =>
			new Error(typeof(EdoErrors),
				nameof(NoCancelledEdoTaskForResend),
				"Нет отмененной ЭДО задачи для переотправки");

		/// <summary>
		/// Ошибка: нет задачи ЭДО
		/// </summary>
		public static Error NoEdoTask =>
			new Error(typeof(EdoErrors),
				nameof(NoEdoTask),
				"Нет ЭДО задачи");

		/// <summary>
		/// Ошибка: нет документооборота Taxcom
		/// </summary>
		public static Error NoTaxcomDocflow =>
			new Error(typeof(EdoErrors),
				nameof(NoTaxcomDocflow),
				"Нет документооборота Taxcom");

		/// <summary>
		/// Ошибка: произошла ошибка во время переотправки документа
		/// </summary>
		public static Error HasProblem =>
			new Error(typeof(EdoErrors),
				nameof(HasProblem),
				"Произошла ошибка во время переотправки документа");

		/// <summary>
		/// Ошибка: невозможно переотправить документ у отмененного заказа
		/// </summary>
		public static Error IsUndeliveredOrder =>
			new Error(typeof(EdoErrors),
				nameof(IsUndeliveredOrder),
				"Невозможно переотправить документ у отмененного заказа");

		/// <summary>
		/// Создает ошибку о том, что документ уже успешно отправлен
		/// </summary>
		/// <param name="edoContainer">Контейнер ЭДО</param>
		/// <returns>Ошибка с описанием</returns>
		public static Error CreateAlreadySuccefullSended(EdoContainer edoContainer) =>
			 new Error(
				typeof(EdoErrors),
				nameof(AlreadySuccefullSended),
				$"Для заказа №" +
				 $"{edoContainer.Order?.Id ?? edoContainer.OrderWithoutShipmentForDebt?.Id ?? edoContainer.OrderWithoutShipmentForPayment?.Id ?? edoContainer.OrderWithoutShipmentForAdvancePayment?.Id} " +
				 $"имеется документ со статусом \"{edoContainer.EdoDocFlowStatus.GetEnumDisplayName()}\"");

		/// <summary>
		/// Создает ошибку о том, что документ уже успешно отправлен
		/// </summary>
		/// <param name="order">Заказ</param>
		/// <param name="edoDocument">Документ ЭДО</param>
		/// <returns>Ошибка с описанием</returns>
		public static Error CreateAlreadySuccefullSended(OrderEntity order, OrderEdoDocument edoDocument) =>
			 new Error(
				typeof(EdoErrors),
				nameof(AlreadySuccefullSended),
				$"Для заказа № {order?.Id} " +
				 $"имеется документ со статусом \"{edoDocument.Status.GetEnumDisplayName()}\"");

		/// <summary>
		/// Создает ошибку о том, что документ можно переотправить только в определенных статусах
		/// </summary>
		/// <param name="orderId">Идентификатор заказа</param>
		/// <param name="statuses">Список допустимых статусов</param>
		/// <returns>Ошибка с описанием</returns>
		public static Error CreateResendableEdoDocumentStatuses(int orderId, IEnumerable<EdoDocumentStatus> statuses) =>
			 new Error(
				typeof(EdoErrors),
				nameof(ResendableEdoDocumentStatuses),
				$"Документ по заказу {orderId} можно переотправить только в статусах: " +
				 $"{string.Join(", ", statuses.Select(s => s.GetEnumDisplayName()))}");

		/// <summary>
		/// Создает ошибку о некорректном типе исходящего документа
		/// </summary>
		/// <param name="orderId">Идентификатор заказа</param>
		/// <param name="documentType">Тип исходящего документа</param>
		/// <returns>Ошибка с описанием</returns>
		public static Error CreateInvalidOutgoingDocumentType(int orderId, OutgoingEdoDocumentType documentType) =>
			 new Error(
				typeof(EdoErrors),
				nameof(InvalidOutgoingDocumentType),
				$"У заказа {orderId} некорректный тип исходящего документа {documentType.GetEnumDisplayName()}");

		/// <summary>
		/// Ошибка: не указан идентификатор заказа-источника для переноса кодов маркировки
		/// </summary>
		public static Error SourceOrderIdMissing =>
			new Error(
				typeof(EdoErrors),
				nameof(SourceOrderIdMissing),
				"Не указан заказ-источник.");

		/// <summary>
		/// Ошибка: не указан идентификатор заказа-получателя для переноса кодов маркировки
		/// </summary>
		public static Error TargetOrderIdMissing =>
			new Error(
				typeof(EdoErrors),
				nameof(TargetOrderIdMissing),
				"Не указан заказ, в который нужно перенести коды.");

		/// <summary>
		/// Ошибка: заказ-источник и заказ-получатель совпадают
		/// </summary>
		public static Error SameSourceAndTargetOrder =>
			new Error(
				typeof(EdoErrors),
				nameof(SameSourceAndTargetOrder),
				"Нельзя перенести коды в тот же самый заказ.");

		/// <summary>
		/// Ошибка: заказ-источник не найден
		/// </summary>
		public static Error SourceOrderNotFound =>
			new Error(
				typeof(EdoErrors),
				nameof(SourceOrderNotFound),
				"Заказ-источник не найден.");

		/// <summary>
		/// Ошибка: заказ-источник не отменен полностью
		/// </summary>
		public static Error SourceOrderNotCanceled =>
			new Error(
				typeof(EdoErrors),
				nameof(SourceOrderNotCanceled),
				"Переносить коды можно только из полностью отмененного заказа.");

		/// <summary>
		/// Ошибка: заказ-получатель не найден
		/// </summary>
		public static Error TargetOrderNotFound =>
			new Error(
				typeof(EdoErrors),
				nameof(TargetOrderNotFound),
				"Целевой заказ не найден.");

		/// <summary>
		/// Ошибка: заказ-получатель отменен
		/// </summary>
		public static Error TargetOrderCanceled =>
			new Error(
				typeof(EdoErrors),
				nameof(TargetOrderCanceled),
				"Нельзя перенести коды в отмененный заказ.");

		/// <summary>
		/// Ошибка: в заказе-источнике отсутствуют отклоненные коды маркировки
		/// </summary>
		public static Error RejectedCodesNotFound =>
			new Error(
				typeof(EdoErrors),
				nameof(RejectedCodesNotFound),
				"В отмененном заказе нет отклоненных кодов для переноса.");

		/// <summary>
		/// Ошибка: в заказе-источнике обнаружены повторяющиеся отклоненные коды маркировки
		/// </summary>
		public static Error DuplicateRejectedCodes =>
			new Error(
				typeof(EdoErrors),
				nameof(DuplicateRejectedCodes),
				"В отмененном заказе есть повторяющиеся коды. Перенос отменен.");

		/// <summary>
		/// Ошибка: переносимые коды маркировки уже используются в другом заказе или документе
		/// </summary>
		public static Error ProductCodesAlreadyUsed =>
			new Error(
				typeof(EdoErrors),
				nameof(ProductCodesAlreadyUsed),
				"Часть кодов уже используется в другом заказе или документе. Перенос отменен.");

		/// <summary>
		/// Ошибка: в заказе-получателе отсутствуют товары, требующие коды маркировки
		/// </summary>
		public static Error TargetOrderItemsNotFound =>
			new Error(
				typeof(EdoErrors),
				nameof(TargetOrderItemsNotFound),
				"В целевом заказе нет товаров, требующих коды маркировки.");

		/// <summary>
		/// Создает ошибку о недостаточном количестве товаров с указанным GTIN в заказе-получателе
		/// </summary>
		/// <param name="gtin">GTIN переносимого кода маркировки</param>
		/// <returns>Ошибка с описанием</returns>
		public static Error CreateInsufficientTargetOrderItems(string gtin) =>
			new Error(
				typeof(EdoErrors),
				nameof(CreateInsufficientTargetOrderItems),
				$"В целевом заказе недостаточно товаров с GTIN {gtin} для переноса кодов.");

		/// <summary>
		/// Создает ошибку об истекшем сроке переотправки документа
		/// </summary>
		/// <param name="edoDocument">Документ ЭДО</param>
		/// <param name="orderId">Номер заказа</param>
		/// <returns>Ошибка с описанием</returns>
		public static Error CreateResendTimeLimitExceeded(OutgoingEdoDocument edoDocument, int orderId) =>
			new Error(
				typeof(EdoErrors),
				nameof(ResendTimeLimitExceeded),
				$"Для заказа №{orderId} " +
				$"истек срок переотправки документа. " +
				$"Документ был отправлен {edoDocument.SendTime?.ToString("dd.MM.yyyy HH:mm")}, " +
				$"переотправка возможна в течение 3х месяцев");

		/// <summary>
		/// Создает ошибку о невозможности переотправить завершенную задачу
		/// </summary>
		/// <param name="taskId">Идентификатор задачи</param>
		/// <returns>Ошибка с описанием</returns>
		public static Error CreateCannotResendCompletedTask(int taskId) =>
			new Error(
				typeof(EdoErrors),
				nameof(CreateCannotResendCompletedTask),
				$"Нельзя переотправить завершенную задачу {taskId}");

		/// <summary>
		/// Создает ошибку о невозможности переотправить завершенный чек
		/// </summary>
		/// <param name="taskId">Идентификатор задачи</param>
		/// <returns>Ошибка с описанием</returns>
		public static Error CreateCannotResendCompletedReceipt(int taskId) =>
			new Error(
				typeof(EdoErrors),
				nameof(CreateCannotResendCompletedReceipt),
				$"Нельзя переотправить завершенный чек {taskId}");

		/// <summary>
		/// Создает ошибку о невозможности переотправить чек из пула
		/// </summary>
		/// <param name="taskId">Идентификатор задачи</param>
		/// <returns>Ошибка с описанием</returns>
		public static Error CreateCannotResendReceiptFromSavedToPool(int taskId) =>
			new Error(
				typeof(EdoErrors),
				nameof(CreateCannotResendReceiptFromSavedToPool),
				$"Нельзя переотправить чек {taskId} из пула");
	}
}
