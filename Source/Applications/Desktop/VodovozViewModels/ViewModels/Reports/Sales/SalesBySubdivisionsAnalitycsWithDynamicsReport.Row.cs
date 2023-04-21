using System.Collections.Generic;

namespace Vodovoz.ViewModels.ViewModels.Reports.Sales
{
	public partial class SalesBySubdivisionsAnalitycsWithDynamicsReport
	{
		public class Row : DisplayRow
		{
			private List<string> _dynamicRows;

			public IList<SalesBySubdivisionRowPart> SalesBySubdivision { get; set; }

			public override IList<string> DynamicColumns
			{
				get
				{
					if(_dynamicRows == null)
					{
						_dynamicRows = new List<string>();

						foreach(var sale in SalesBySubdivision)
						{
							_dynamicRows.Add(sale.FirstPeriodAmount.ToString(_numericDefaultFormat));
							_dynamicRows.Add(sale.FirstPeriodPrice.ToString(_financialDefaultFormat));
							_dynamicRows.Add(sale.SecondPeriodAmount.ToString(_numericDefaultFormat));
							_dynamicRows.Add(sale.SecondPeriodPrice.ToString(_financialDefaultFormat));
							_dynamicRows.Add(sale.AmountUnitsDynamic.ToString(_numericDefaultFormat));
							_dynamicRows.Add(sale.AmountPercentDynamic.ToString(_percentDefaultFormat));
							_dynamicRows.Add(sale.PriceMoneyDynamic.ToString(_financialDefaultFormat));
							_dynamicRows.Add(sale.PricePercentDynamic.ToString(_percentDefaultFormat));
						}
					}

					return _dynamicRows;
				}
			}
		}
	}
}
