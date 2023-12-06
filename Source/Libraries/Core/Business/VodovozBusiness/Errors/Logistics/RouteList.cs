namespace Vodovoz.Errors.Logistics
{
	public static partial class RouteList
	{
		public static Error CarIsEmpty =>
			new Error(
				typeof(RouteList),
				nameof(CarIsEmpty),
				"Не заполнен автомобиль");

		public static Error IncorrectStatusForAccept =>
			new Error(
				typeof(RouteList),
				nameof(IncorrectStatusForAccept),
				"Неподходящий статус для подтверждения МЛ");

		public static Error IncorrectStatusForEdit =>
			new Error(
				typeof(RouteList),
				nameof(IncorrectStatusForEdit),
				"Неподходящий статус для редактирования МЛ");

		public static Error HasCarLoadingDocuments =>
			new Error(
				typeof(RouteList),
				nameof(HasCarLoadingDocuments),
				"К МЛ привязаны документы погрузки");

		public static Error ValidationFailure =>
			new Error(
				typeof(RouteList),
				nameof(ValidationFailure),
				"МЛ не прошёл валидацию");

		public static Error HasOverweight =>
		new Error(
			typeof(RouteList),
			nameof(HasOverweight),
			"Перегруз");

		public static Error CreateHasOverweight(string message) =>
			new Error(
				typeof(RouteList),
				nameof(HasOverweight),
				message);
	}
}
