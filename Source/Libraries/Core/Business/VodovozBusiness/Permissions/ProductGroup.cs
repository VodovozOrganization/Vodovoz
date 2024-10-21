using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Permissions
{
	/// <summary>
	/// Права группы товаров
	/// </summary>
	public static partial class ProductGroup
	{
		/// <summary>
		/// Доступ к настройке "Требует доп. контроля водителя" в группе товаров
		/// </summary>
		[Display(
			Name = "Доступ к настройке \"Требует доп. контроля водителя\" в группе товаров",
			Description = "При наличии права чекбокс \"Требует доп. контроля водителя\" в диалоге групп товаров доступен для изменения")]
		public static string CanEditAdditionalControlSettingsInProductGroups =>
			"ProductGroup.CanEditAdditionalControlSettings";
	}
}
