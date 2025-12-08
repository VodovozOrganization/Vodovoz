using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Permissions
{
	/// <summary>
	/// Права в настройках
	/// </summary>
	public static class SettingsPermissions
	{
		/// <summary>
		/// Пользователь может добавлять/менять фиксу для сотрудников ВВ в Общих настройках - Заказы
		/// </summary>
		[Display(
			Name = "Назначение фикс цены сотрудникам ВВ",
			Description = "Пользователь может добавлять/менять фиксу для сотрудников ВВ в Общих настройках - Заказы")]
		public static string CanEditEmployeeFixedPrices => "CanEditEmployeeFixedPrices";


		/// <summary>
		/// Изменение настроек выбора организаций для заказа
		/// </summary>
		[Display(
			Name = "Изменение настроек выбора организаций для заказа",
			Description = "Пользователь может менять настройки юридических лиц для заказа в общих настройках")]
		public static string CanEditOrderOrganizationsSettings => "GeneralSettings.CanEditOrderOrganizationsSettings";
	}
}
