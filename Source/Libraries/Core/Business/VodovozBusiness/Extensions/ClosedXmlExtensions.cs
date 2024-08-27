using ClosedXML.Excel;
using QS.Utilities;

namespace VodovozBusiness.Extensions
{
	public static class ClosedXmlExtensions
	{
		public static IXLCell SetCurrencyFormat(this IXLCell cell)
		{
			cell.Style.NumberFormat
				.SetFormat($@"#,##0.00 {CurrencyWorks.CurrencyShortName};[Red]- #,##0.00 {CurrencyWorks.CurrencyShortName}");

			return cell;
		}
		
		public static IXLCell SetBoldFont(this IXLCell cell)
		{
			cell.Style.Font.Bold = true;

			return cell;
		}
	}
}
