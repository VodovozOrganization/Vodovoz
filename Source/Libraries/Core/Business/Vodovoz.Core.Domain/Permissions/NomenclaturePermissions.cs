using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Permissions
{
	/// <summary>
	/// Права Номенклатура
	/// </summary>
	public static class NomenclaturePermissions
	{
		/// <summary>
		/// Может создавать номенклатуры и устанавливать галочку архивный
		/// </summary>
		[Display(
			Name = "Создание и архивирование номенклатур",
			Description = "Пользователь может создавать номенклатуры и устанавливать галочку архивный.")]
		public static string CanCreateAndArcNomenclatures => "can_create_and_arc_nomenclatures";
		
		/// <summary>
		/// Может редактировать альтернативные цены (Kuler Sale)
		/// </summary>
		[Display(
			Name = "Редактирование альтернативных цен (Kuler Sale)",
			Description = "Редактирование альтернативных цен (Kuler Sale)")]
		public static string CanEditAlternativeNomenclaturePrices => "сan_edit_alternative_nomenclature_prices";
		
		/// <summary>
		/// Может создавать номенклатуры с инвентарным учетом
		/// </summary>
		[Display(
			Name = "Создание номенклатур с инвентарным учетом",
			Description = "Пользователь может создавать номенклатуры с инвентарным учетом")]
		public static string CanCreateNomenclaturesWithInventoryAccounting => "can_create_nomenclatures_with_inventory_accounting";

		/// <summary>
		/// Доступ к вкладке Сайты и приложения и изменению параметров в ней,
		/// а также изменение списка складов в общих настройках для расчета остатков для ИПЗ
		/// </summary>
		[Display(
			Name = "Номенклатура. Доступ к вкладке Сайты и приложения",
			Description = "Позволяет менять онлайн параметры номенклатуры и в общих настройках устанавливать склады для расчета остатков")]
		public static string HasAccessToSitesAndAppsTab => "Nomenclature.HasAccessToSitesAndAppsTab";
	}
}
