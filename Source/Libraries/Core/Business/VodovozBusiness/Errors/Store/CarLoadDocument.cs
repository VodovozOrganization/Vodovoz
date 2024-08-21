namespace Vodovoz.Errors.Store
{
	public static partial class CarLoadDocument
	{
		public static Error DocumentNotFound =>
			new Error(
				typeof(CarLoadDocument),
				nameof(DocumentNotFound),
				"Талон погрузки не найден");

		public static Error CreateDocumentNotFound(int? id) =>
			id is null ? DocumentNotFound : new Error(
				typeof(CarLoadDocument),
				nameof(DocumentNotFound),
				$"Талон погрузки #{id} не найден");
		public static Error OrderNotFound =>
			new Error(
				typeof(CarLoadDocument),
				nameof(OrderNotFound),
				"Заказ не найден");

		public static Error CreateOrderNotFound(int? id) =>
			id is null ? DocumentNotFound : new Error(
				typeof(CarLoadDocument),
				nameof(OrderNotFound),
				$"Заказ #{id} не найден");

		public static Error LoadingIsAlreadyInProgress =>
			new Error(
				typeof(CarLoadDocument),
				nameof(LoadingIsAlreadyInProgress),
				"Талон погрузки уже в процессе погрузки");

		public static Error CreateLoadingIsAlreadyInProgress(int? id) =>
			id is null ? LoadingIsAlreadyInProgress : new Error(
				typeof(CarLoadDocument),
				nameof(LoadingIsAlreadyInProgress),
				$"Талон погрузки #{id} уже в процессе погрузки");

		public static Error LoadingIsAlreadyDone =>
			new Error(
				typeof(CarLoadDocument),
				nameof(LoadingIsAlreadyDone),
				"Талон погрузки уже полностью погружен");

		public static Error CreateLoadingIsAlreadyDone(int? id) =>
			id is null ? LoadingIsAlreadyDone : new Error(
				typeof(CarLoadDocument),
				nameof(LoadingIsAlreadyDone),
				$"Талон погрузки #{id} уже полностью погружен");

		public static Error OrderItemsExistInMultipleDocuments =>
			new Error(
				typeof(CarLoadDocument),
				nameof(OrderItemsExistInMultipleDocuments),
				"Строки одного заказа сетевого клиента присутствуют в нескольких талонах погрузки");

		public static Error CreateOrderItemsExistInMultipleDocuments(int? id) =>
			id is null ? OrderItemsExistInMultipleDocuments : new Error(
				typeof(CarLoadDocument),
				nameof(OrderItemsExistInMultipleDocuments),
				$"Строки заказа #{id} сетевого клиента присутствуют в нескольких талонах погрузки");
	}
}
