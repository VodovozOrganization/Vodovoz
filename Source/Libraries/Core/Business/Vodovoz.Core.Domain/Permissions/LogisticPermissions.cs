using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Permissions
{
	public static partial class LogisticPermissions
	{
		/// <summary>
		/// Логистика - Доступ ко вкладке Логистика
		/// </summary>
		[Display(
			Name = "Логистика",
			Description = "Доступ ко вкладке Логистика")]
		public static string IsLogistician => "logistican";

		/// <summary>
		/// Доступ к настройке интервала ДЗЧ
		/// </summary>
		[Display(
			Name = "Доступ к настройке интервала ДЗЧ")]
		public static string CanEditFastDeliveryIntervalFromSetting => nameof(CanEditFastDeliveryIntervalFromSetting);
	}
}
