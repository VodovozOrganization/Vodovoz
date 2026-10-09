using System.Collections.Generic;

namespace CustomerApp.Contracts.Sale.Templates
{
	/// <summary>
	/// Доступность дня месяца
	/// </summary>
	public class MonthDayAvailability
	{
		/// <summary>
		/// Число месяца
		/// </summary>
		public int Day { get; set; }
		/// <summary>
		/// Доступность
		/// </summary>
		public bool Available { get; set; }

		public static MonthDayAvailability CreateAvailable(int day) =>
			new MonthDayAvailability
			{
				Day = day,
				Available = true
			};
	}
}
