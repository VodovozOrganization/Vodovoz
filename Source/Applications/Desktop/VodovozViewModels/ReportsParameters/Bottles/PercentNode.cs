namespace Vodovoz.ViewModels.ReportsParameters.Bottles
{
	public partial class ProfitabilityBottlesByStockReportViewModel
	{
		public class PercentNode
		{
			public PercentNode(int pct) => Pct = pct;

			public int Pct { get; set; }
			public string Name => string.Format("{0}%", Pct);
		}
	}
}
