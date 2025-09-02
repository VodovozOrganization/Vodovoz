using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Permissions
{
	/// <summary>
	/// Права группы товаров
	/// </summary>
	public static partial class ProductGroupPermissions
	{
		/// <summary>
		/// Изменение настроек групп для онлайн магазина
		/// </summary>
		[Display(
			Name = "Изменение настроек групп для онлайн магазина",
			Description = "Пользователь может изменять группы товаров влияющие на выгрузку в интернет магазин.")]
		public static string CanEditOnlineStoreParametersInProductGroups =>
			"can_edit_online_store";

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
