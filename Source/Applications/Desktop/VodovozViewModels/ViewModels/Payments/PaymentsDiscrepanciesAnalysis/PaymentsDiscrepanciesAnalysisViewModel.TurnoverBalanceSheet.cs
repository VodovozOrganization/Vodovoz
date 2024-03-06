namespace Vodovoz.ViewModels.ViewModels.Payments.PaymentsDiscrepanciesAnalysis
{
	public partial class PaymentsDiscrepanciesAnalysisViewModel
	{
		/// <summary>
		/// Оборотно-сальдовая ведомость
		/// </summary>
		public class TurnoverBalanceSheet
		{
			private TurnoverBalanceSheet()
			{
				
			}

			public static TurnoverBalanceSheet CreateFromXls(string fileName)
			{
				var rowsFromXls = XlsParseHelper.GetRowsFromXls2(fileName);

				return new TurnoverBalanceSheet();
			}
		}
	}
}
