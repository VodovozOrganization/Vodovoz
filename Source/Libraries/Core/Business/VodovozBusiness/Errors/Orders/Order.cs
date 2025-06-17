using Vodovoz.Core.Domain.Results;

namespace Vodovoz.Errors.Orders
{
	public static partial class Order
	{
		public static Error NotFound =>
			new Error(
				typeof(Order),
				nameof(NotFound),
				"Заказ не найден");

		public static Error CantEdit =>
			new Error(
				typeof(Order),
				nameof(CantEdit),
				"Заказ находится в статусе, при котором редактирование запрещено.");

		public static Error Validation =>
			new Error(
				typeof(Order),
				nameof(Validation),
				"Произошла ошибка валидации заказа.");

		public static Error HasNoValidCertificates =>
			new Error(
				typeof(Order),
				nameof(HasNoValidCertificates),
				"Не удалось найти действительные сертификаты.");

		public static Error AcceptAbortedByUser =>
			new Error(
				typeof(Order),
				nameof(AcceptAbortedByUser),
				"Подтверждение заказа отменено пользователем.");

		public static Error AcceptException => new Error(
			typeof(Order),
			nameof(AcceptException),
			"Исключение при подтверждении заказа");

		public static Error UnableToShipPromoSet =>
			new Error(
				typeof(Order),
				nameof(UnableToShipPromoSet),
				"По этому адресу/телефону уже была ранее отгрузка промо набора\n" +
				"Пожалуйста, удалите промо набор или поменяйте адрес доставки.");
		
		public static Error UnableToShipPromoSetForNewClientsFromSelfDelivery =>
			new Error(
				typeof(Order),
				nameof(UnableToShipPromoSetForNewClientsFromSelfDelivery),
				"В самовывозе нельзя заказывать промо набор для новых клиентов\n" +
				"Пожалуйста, удалите промо набор.");
		
		public static Error UnableToShipPromoSetForNewClientsToUnknownClientOrDeliveryPoint =>
			new Error(
				typeof(Order),
				nameof(UnableToShipPromoSetForNewClientsToUnknownClientOrDeliveryPoint),
				"Нельзя заказывать промо набор для новых клиентов для неизвестного клиента или адреса\n" +
				"Пожалуйста, удалите промо набор или выберите нужного клиента с адресом, по которым не было таких отгрузок.");

		public static Error Save =>
			new Error(
				typeof(Order),
				nameof(Save),
				"Произошла ошибка при сохранении");

		public static Error NotInOnTheWayStatus =>
			new Error(
				typeof(Order),
				nameof(NotInOnTheWayStatus),
				"Заказ не в статусе в пути");

		public static Error FastDelivery19LBottlesLimitError(int water19lInOrder, int fastDelivery19LBottlesLimit) =>
			new Error(
				typeof(Order),
				nameof(FastDelivery19LBottlesLimitError),
				$"Максимально допустимое кол-во 19л воды для доставки за час - {fastDelivery19LBottlesLimit}, в заказе указано {water19lInOrder}");

		public static Error PaidCashlessOrderClientReplacementError =>
			new Error(
				typeof(Order),
				nameof(PaidCashlessOrderClientReplacementError),
				"Контрагента изменить невозможно пока на заказе распределен платеж, обратитесь к сотрудникам ОДЗ для снятия распределения.");

		public static Error OrderIsNotForPersonalUseError =>
			new Error(
				typeof(Order),
				nameof(OrderIsNotForPersonalUseError),
				"Заказ приобретается не для собственных нужд");
	}
}
