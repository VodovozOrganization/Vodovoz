using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Permissions
{
	public static partial class Logistic
	{
		public static class Fuel
		{
			/// <summary>
			/// Изменение значения максимального суточного лимита для типов авто
			/// </summary>
			[Display(
				Name = "Изменение значения максимального суточного лимита для типов авто")]
			public static string CanEditMaxDailyFuelLimit => "can_edit_max_daily_fuel_limit";
		}
	}
}
