using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Permissions
{
	/// <summary>
	/// Версии сервисных районов
	/// </summary>
	public static partial class ServiceDistrictsSetPermissions
	{
		/// <summary>
		/// Можно измененять условия доставки сервиса
		/// </summary>
		[Display(
			Name = "Изменение условий доставки сервиса",
			Description = "Пользователь может измененять условия доставки сервиса")]
		public static string CanEditServiceDeliveryRules => "can_edit_service_district_rules";

		/// <summary>
		/// Можно активировать версию сервисной доставки
		/// </summary>
		[Display(
			Name = "тивация версии районов СЦ",
			Description = "Пользователь может активировать версию районов СЦ")]
		public static string CanActivateServiceDistrictsSet => "can_activate_service_districts_set";
	}
}
