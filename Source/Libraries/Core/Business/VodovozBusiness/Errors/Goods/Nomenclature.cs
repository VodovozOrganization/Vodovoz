namespace Vodovoz.Errors.Goods
{
	public static class Nomenclature
	{
		public static Error NotFound(int nomenclatureId) => new Error(
			typeof(Nomenclature),
			nameof(NotFound),
			$"Номенклатура #{nomenclatureId} не найдена");

		public static Error HasResiduesInWarhouses = new Error(
			typeof(Nomenclature),
			nameof(HasResiduesInWarhouses),
			"Есть остатки на складах");

		public static Error HasResiduesOnEmployees = new Error(
			typeof(Nomenclature),
			nameof(HasResiduesOnEmployees),
			"Есть остатки у сотрудников");

		public static Error HasResiduesOnCars = new Error(
			typeof(Nomenclature),
			nameof(HasResiduesOnCars),
			"Есть остатки на автомобилях");

		public static Error HasNotAcceptedTransfers = new Error(
			typeof(Nomenclature),
			nameof(HasNotAcceptedTransfers),
			"Есть непринятые документы перемещения");
	}
}
