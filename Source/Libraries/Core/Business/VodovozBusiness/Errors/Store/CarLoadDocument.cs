namespace Vodovoz.Errors.Store
{
	public static partial class CarLoadDocument
	{
		public static Error NotFound =>
			new Error(
				typeof(CarLoadDocument),
				nameof(NotFound),
				"Талон погрузки не найден");

		public static Error CreateNotFound(int? id) =>
			id is null ? NotFound : new Error(
				typeof(CarLoadDocument),
				nameof(NotFound),
				$"Талон погрузки #{id} не найден");

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
	}
}
