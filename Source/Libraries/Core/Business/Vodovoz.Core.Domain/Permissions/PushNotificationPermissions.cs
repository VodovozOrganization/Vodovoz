using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Permissions
{
	/// <summary>
	/// Права в настройках пуш уведомлений
	/// </summary>
	public static class PushNotificationPermissions
	{
		/// <summary>
		/// Пользователь имеет доступ к справочнику Пуш-уведомлений для клиентского МП
		/// </summary>
		[Display(
			Name = "Работа со справочником Пуш-уведомлений для клиентского МП",
			Description = "Пользователь имеет доступ к справочнику Пуш-уведомлений для клиентского МП")]
		public static string CanChangeOnlineOrderNotificationSettings => "CanChangeOnlineOrderNotificationSettings";

	}
}
