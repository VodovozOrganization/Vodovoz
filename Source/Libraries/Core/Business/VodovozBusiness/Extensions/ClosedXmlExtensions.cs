using ClosedXML.Excel;
using QS.Utilities;

namespace VodovozBusiness.Extensions
{
	public static class ClosedXmlExtensions
	{
		/// <summary>
		/// Финансовый формат
		/// </summary>
		/// <param name="cell">ячейка</param>
		/// <returns></returns>
		public static IXLCell SetCurrencyFormat(this IXLCell cell)
		{
			cell.Style.NumberFormat
				.SetFormat(@"_-* #,##0.00\ ""₽""_-;\-* #,##0.00\ ""₽""_-;_-* ""-""??\ ""₽""_-;_-@_-");

			return cell;
		}
		
		public static IXLCell SetBoldFont(this IXLCell cell)
		{
			cell.Style.Font.Bold = true;

			return cell;
		}
	}
}
