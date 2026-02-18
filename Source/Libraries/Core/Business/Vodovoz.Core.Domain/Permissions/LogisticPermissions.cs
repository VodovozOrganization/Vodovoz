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

		/// <summary>
		/// Возможность работать с графиком водителей
		/// </summary>
		[Display(
			Name = "Возможность работать с графиком водителей",
			Description = "Пользователь может работать с графиком водителей")]
		public static string CanWorkWithDriverSchedule => nameof(CanWorkWithDriverSchedule);

		/// <summary>
		/// Возможность изменять события и мощности после 13:00
		/// </summary>
		[Display(
			Name = "Возможность изменять события и мощности после 13:00",
			Description = "Пользователь может изменять события и мощности после 13:00")]
		public static string CanEditEventsAndCapacitiesAfter13 => nameof(CanEditEventsAndCapacitiesAfter13);
	}
}
