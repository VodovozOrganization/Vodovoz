using System.Globalization;

namespace Vodovoz.ViewModels.ViewModels.Reports.WageCalculation.CallCenterMotivation
{
	public static class CallCenterMotivationReportExtensions
	{
		/// <summary>
		/// Денежный формат
		/// </summary>
		/// <param name="value"></param>
		/// <param name="withRubles"></param>
		/// <returns></returns>
		public static string ToFormattedUnitString(this decimal value, bool withRubles)
		{
			var result = string.Empty;

			result = withRubles
				? $"{value:N2} ₽"
				: value.ToString("N0", new CultureInfo("ru-RU"));

			return result;
		}
	}
}
