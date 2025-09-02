using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Permissions
{
	public static partial class LogisticPermissions
	{
		public static class Fuel
		{
			/// <summary>
			/// Изменение значения максимального суточного лимита для типов авто
			/// </summary>
			[Display(
				Name = "Изменение значения максимального суточного лимита для типов авто")]
			public static string CanEditMaxDailyFuelLimit => "can_edit_max_daily_fuel_limit";

			/// <summary>
			/// Может выдавать топливные лимиты
			/// </summary>
			[Display(
				Name = "Может выдавать топливные лимиты")]
			public static string CanGiveFuelLimits => "can_give_fuel_limits";
		}
	}
}
