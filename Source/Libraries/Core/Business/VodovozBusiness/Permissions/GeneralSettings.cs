using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Permissions
{
	public static class GeneralSettings
	{
		/// <summary>
		/// Изменение настроек выбора организаций для заказа
		/// </summary>
		[Display(
			Name = "Изменение настроек выбора организаций для заказа",
			Description = "Пользователь может менять настройки юридических лиц для заказа в общих настройках")]
		public static string CanEditOrderOrganizationsSettings => "GeneralSettings.CanEditOrderOrganizationsSettings";
	}
}
