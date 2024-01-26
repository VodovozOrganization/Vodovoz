using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Permissions
{
	public static partial class Logistic
	{
		/// <summary>
		/// Логистика - Доступ ко вкладке Логистика
		/// </summary>
		[Display(
			Name = "Логистика",
			Description = "Доступ ко вкладке Логистика")]
		public static string IsLogistician => "logistican";
	}
}
