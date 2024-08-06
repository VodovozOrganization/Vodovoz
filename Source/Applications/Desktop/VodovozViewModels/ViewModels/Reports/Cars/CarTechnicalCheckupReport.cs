using ClosedXML.Excel;
using System.Drawing;

namespace Vodovoz.ViewModels.ViewModels.Reports.Cars
{
	public class CarTechnicalCheckupReport
	{
		public static string ReportTitle => "Отчет по ГТО";

		private const int _columnsCount = 10;
		private const string _dateFormatString = "dd.MM.yyyy";
		private readonly XLColor _headersBgColor = XLColor.FromColor(Color.FromArgb(170, 200, 140));
		private readonly XLColor _noCarInsuranceBgColor = XLColor.FromColor(Color.FromArgb(200, 50, 50));

		private CarTechnicalCheckupReport() { }
	}
}
