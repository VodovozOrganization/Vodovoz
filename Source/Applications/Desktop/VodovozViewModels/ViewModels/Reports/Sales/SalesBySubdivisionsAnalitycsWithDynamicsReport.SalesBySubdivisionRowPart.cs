namespace Vodovoz.ViewModels.ViewModels.Reports.Sales
{
	public partial class SalesBySubdivisionsAnalitycsWithDynamicsReport
	{
		public class SalesBySubdivisionRowPart
		{
			public decimal FirstPeriodAmount { get; set; }

			public decimal FirstPeriodPrice { get; set; }

			public decimal SecondPeriodAmount { get; set; }

			public decimal SecondPeriodPrice { get; set; }

			public decimal AmountUnitsDynamic => FirstPeriodAmount - SecondPeriodAmount;

			public decimal AmountPercentDynamic => SecondPeriodAmount == 0 ? 1m : FirstPeriodAmount / SecondPeriodAmount - 1m;

			public decimal PriceMoneyDynamic => FirstPeriodPrice - SecondPeriodPrice;

			public decimal PricePercentDynamic => SecondPeriodPrice == 0 ? 1m : FirstPeriodPrice / SecondPeriodPrice - 1m;
		}
	}
}
