namespace Vodovoz.Errors.Orders
{
	public static partial class Order
	{
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
				"По этому адресу уже была ранее отгрузка промонабора на другое физ.лицо.\n" +
				"Пожалуйста удалите промо набор или поменяйте адрес доставки.");

		public static Error Save =>
			new Error(
				typeof(Order),
				nameof(Save),
				"Произошла ошибка при сохранении");
	}
}
