using Vodovoz.Core.Domain.Results;

namespace Vodovoz.Errors.Sale
{
	public static class SaleErrors
	{
		/// <summary>
		/// Ошибка при добавлении залоговую позиции к не залоговым
		/// </summary>
		/// <param name="source">Источник(заказ, шаблон и т.д.)</param>
		/// <returns></returns>
		public static Error CantAddDepositItemError(string source) => new Error(
			typeof(SaleErrors),
			nameof(CantAddDepositItemError),
			$"Нельзя добавить залоговую позицию, если в {source} уже есть не залоговые позиции.");
		
		/// <summary>
		/// Ошибка при добавлении не залоговой позиции к залоговым
		/// </summary>
		/// <param name="source">Источник(заказ, шаблон и т.д.)</param>
		/// <returns></returns>
		public static Error CantAddNonDepositItemError(string source) => new Error(
			typeof(SaleErrors),
			nameof(CantAddNonDepositItemError),
			$"Нельзя добавить не залоговую позицию, если в {source} уже есть залоговые позиции.");
		
		public static Error CantAddSaleItemToDeliverySaleWithoutDeliveryPointError() => new Error(
			typeof(SaleErrors),
			nameof(CantAddSaleItemToDeliverySaleWithoutDeliveryPointError),
			"Для добавления позиции на продажу должна быть выбрана точка доставки");
		
		public static Error CantAddSaleItemWithoutCounterpartyError() => new Error(
			typeof(SaleErrors),
			nameof(CantAddSaleItemWithoutCounterpartyError),
			"Для добавления позиции на продажу должен быть выбран клиент");
		
		public static Error CantAddNonServiceNomenclatureToServiceOrderError(string source) => new Error(
			typeof(SaleErrors),
			nameof(CantAddNonServiceNomenclatureToServiceOrderError),
			$"В сервисный {source} нельзя добавить не сервисную услугу");
		
		public static Error CantAddServiceNomenclatureToNonServiceOrderError(string source) => new Error(
			typeof(SaleErrors),
			nameof(CantAddServiceNomenclatureToNonServiceOrderError),
			$"В не сервисный {source} нельзя добавить сервисную услугу");
		
		public static Error DontHavePermissionsToAddOnlineStoreProductError() => new Error(
			typeof(SaleErrors),
			nameof(DontHavePermissionsToAddOnlineStoreProductError),
			"У Вас недостаточно прав для добавления на продажу номенклатуры интернет магазина");
	}
}
