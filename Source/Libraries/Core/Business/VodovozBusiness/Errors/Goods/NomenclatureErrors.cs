using Vodovoz.Core.Domain.Results;

namespace Vodovoz.Errors.Goods
{
	public static class NomenclatureErrors
	{
		public static Error NotFound(int nomenclatureId) => new Error(
			typeof(NomenclatureErrors),
			nameof(NotFound),
			$"Номенклатура #{nomenclatureId} не найдена");

		public static Error HasResiduesInWarhouses = new Error(
			typeof(NomenclatureErrors),
			nameof(HasResiduesInWarhouses),
			"Есть остатки на складах");

		public static Error HasResiduesOnEmployees = new Error(
			typeof(NomenclatureErrors),
			nameof(HasResiduesOnEmployees),
			"Есть остатки у сотрудников");

		public static Error HasResiduesOnCars = new Error(
			typeof(NomenclatureErrors),
			nameof(HasResiduesOnCars),
			"Есть остатки на автомобилях");

		public static Error HasNotAcceptedTransfers = new Error(
			typeof(NomenclatureErrors),
			nameof(HasNotAcceptedTransfers),
			"Есть непринятые документы перемещения");

		public static Error IsNotAccountableInTrueMark = new Error(
			typeof(NomenclatureErrors),
			nameof(IsNotAccountableInTrueMark),
			"Номенклатура не подлежит учету в ЧЗ");

		public static Error CreateIsNotAccountableInTrueMark(string nomenclatureName) => new Error(
			typeof(NomenclatureErrors),
			nameof(IsNotAccountableInTrueMark),
			$"Номенклатура {nomenclatureName} не подлежит учету в ЧЗ");
	}
}
