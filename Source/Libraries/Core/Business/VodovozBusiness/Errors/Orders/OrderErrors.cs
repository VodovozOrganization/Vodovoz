using Gamma.Utilities;
using System;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain.Orders;

namespace Vodovoz.Errors.Orders
{
	public static partial class OrderErrors
	{
		public static Error NotFound =>
			new Error(
				typeof(OrderErrors),
				nameof(NotFound),
				"Заказ не найден");

		public static Error CantEdit =>
			new Error(
				typeof(OrderErrors),
				nameof(CantEdit),
				"Заказ находится в статусе, при котором редактирование запрещено.");

		public static Error Validation =>
			new Error(
				typeof(OrderErrors),
				nameof(Validation),
				"Произошла ошибка валидации заказа.");

		public static Error HasNoValidCertificates =>
			new Error(
				typeof(OrderErrors),
				nameof(HasNoValidCertificates),
				"Не удалось найти действительные сертификаты.");

		public static Error AcceptAbortedByUser =>
			new Error(
				typeof(OrderErrors),
				nameof(AcceptAbortedByUser),
				"Подтверждение заказа отменено пользователем.");

		public static Error AcceptException => new Error(
			typeof(OrderErrors),
			nameof(AcceptException),
			"Исключение при подтверждении заказа");

		public static Error UnableToShipPromoSet =>
			new Error(
				typeof(OrderErrors),
				nameof(UnableToShipPromoSet),
				"По этому адресу/телефону уже была ранее отгрузка промо набора\n" +
				"Пожалуйста, удалите промо набор или поменяйте адрес доставки.");
		
		public static Error UnableToPartitionOrderWithBigDeposit =>
			new Error(
				typeof(OrderErrors),
				nameof(UnableToShipPromoSet),
				"Нельзя разделить заказ с большим залогом(сумма залога превышает суммы заказов, получаемых при разбиении)");
		
		public static Error UnableToShipPromoSetForNewClientsFromSelfDelivery =>
			new Error(
				typeof(OrderErrors),
				nameof(UnableToShipPromoSetForNewClientsFromSelfDelivery),
				"В самовывозе нельзя заказывать промо набор для новых клиентов\n" +
				"Пожалуйста, удалите промо набор.");
		
		public static Error UnableToShipPromoSetForNewClientsToUnknownClientOrDeliveryPoint =>
			new Error(
				typeof(OrderErrors),
				nameof(UnableToShipPromoSetForNewClientsToUnknownClientOrDeliveryPoint),
				"Нельзя заказывать промо набор для новых клиентов для неизвестного клиента или адреса\n" +
				"Пожалуйста, удалите промо набор или выберите нужного клиента с адресом, по которым не было таких отгрузок.");

		public static Error Save =>
			new Error(
				typeof(OrderErrors),
				nameof(Save),
				"Произошла ошибка при сохранении");

		public static Error NotInOnTheWayStatus =>
			new Error(
				typeof(OrderErrors),
				nameof(NotInOnTheWayStatus),
				"Заказ не в статусе в пути");

		public static Error FastDelivery19LBottlesLimitError(int water19lInOrder, int fastDelivery19LBottlesLimit) =>
			new Error(
				typeof(OrderErrors),
				nameof(FastDelivery19LBottlesLimitError),
				$"Максимально допустимое кол-во 19л воды для доставки за час - {fastDelivery19LBottlesLimit}, в заказе указано {water19lInOrder}");

		public static Error PaidCashlessOrderClientReplacementError =>
			new Error(
				typeof(OrderErrors),
				nameof(PaidCashlessOrderClientReplacementError),
				"Контрагента изменить невозможно пока на заказе распределен платеж, обратитесь к сотрудникам ОДЗ для снятия распределения.");

		public static Error OrderIsNotForPersonalUseError =>
			new Error(
				typeof(OrderErrors),
				nameof(OrderIsNotForPersonalUseError),
				"Заказ приобретается не для собственных нужд");

		public static Error IsNotSelfDelivery =>
			new Error(
				typeof(OrderErrors),
				nameof(IsNotSelfDelivery),
				"Заказ не является самовывозом");
		
		public static Error SplitOrderError =>
			new Error(
				typeof(OrderErrors),
				nameof(SplitOrderError),
				"Произошла ошибка при разбиении заказа");

		public static Error CannotCancelOrderInStatus(OrderStatus status) =>
		new Error(
				"408",
				$"Невозможно отменить заказ в статусе '{status.GetEnumTitle()}'");

		public static Error UnsupportedOrderStatusForCancellation(OrderStatus status) =>
			new Error(
				"400",
				$"Не поддерживаемый статус для отмены: {status.GetEnumTitle()}");

		public static Error RouteListItemNotFound(int orderId) =>
			new Error(
				"400",
				$"Позиция маршрутного листа не найдена для заказа {orderId}");

		public static Error RefundFailed(string reason) =>
			new Error(
				"400",
				$"Не удалось выполнить возврат платежа: {reason}");

		public static Error CannotTransferOrderInStatus(OrderStatus status) =>
			new Error(
				"408",
				$"Невозможно перенести заказ в статусе '{status.GetEnumTitle()}'");

		public static Error UnsupportedOrderStatusForTransfer(OrderStatus status) =>
			new Error(
				"400",
				$"Не поддерживаемый статус для переноса: {status.GetEnumTitle()}");

		public static Error DeliveryScheduleNotFound =>
			new Error(
				"400",
				$"Расписание доставки не найдено");

		public static Error InvalidDeliveryDate(DateTime date) =>
			new Error(
				"400",
				$"Дата доставки {date:dd.MM.yyyy} не может быть раньше сегодняшнего дня");

		public static Error SameDeliveryParameters =>
			new Error(
				"400",
				"Заказ уже запланирован на указанную дату и время доставки");

		public static Error TransferFailed(string reason) =>
			new Error(
				"500",
				$"Ошибка при переносе заказа: {reason}");
	}
}
