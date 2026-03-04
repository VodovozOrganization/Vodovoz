namespace Vodovoz.ViewModels.ViewModels.Reports.WageCalculation.CallCenterMotivation
{
	public partial class CallCenterMotivationReport
	{
		/// <summary>
		/// Значение колонки
		/// </summary>
		public class ValueColumn
		{
			/// <summary>
			/// Продано
			/// </summary>
			public decimal Sold { get; set; }

			/// <summary>
			/// Премия
			/// </summary>
			public decimal Premium { get; set; }
		}
	}
}
