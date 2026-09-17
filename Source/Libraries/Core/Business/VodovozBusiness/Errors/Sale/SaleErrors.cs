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
		
		//TODO-5967 возможно это условие будет верным только для заказа
		public static Error CantAddNonServiceNomenclatureToServiceOrderError() => new Error(
			typeof(SaleErrors),
			nameof(CantAddNonServiceNomenclatureToServiceOrderError),
			"В сервисный заказ нельзя добавить не сервисную услугу");
		
		//TODO-5967 возможно это условие будет верным только для заказа
		public static Error CantAddServiceNomenclatureToNonServiceOrderError() => new Error(
			typeof(SaleErrors),
			nameof(CantAddServiceNomenclatureToNonServiceOrderError),
			"В не сервисный заказ нельзя добавить сервисную услугу");
		
		public static Error DontHavePermissionsToAddOnlineStoreProductError() => new Error(
			typeof(SaleErrors),
			nameof(DontHavePermissionsToAddOnlineStoreProductError),
			"У Вас недостаточно прав для добавления на продажу номенклатуры интернет магазина");
	}
}
