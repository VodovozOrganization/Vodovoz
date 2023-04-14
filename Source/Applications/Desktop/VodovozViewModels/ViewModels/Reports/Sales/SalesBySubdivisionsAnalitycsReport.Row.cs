using System.Collections.Generic;
using static Vodovoz.ViewModels.ViewModels.Reports.Sales.SalesBySubdivisionsAnalitycsReportViewModel;

namespace Vodovoz.ViewModels.ViewModels.Reports.Sales
{
	public partial class SalesBySubdivisionsAnalitycsReport
	{

		public class Row : DisplayRow
		{
			private List<string> _dynamicRows;

			public IList<AmountPricePair> SalesBySubdivision { get; set; }

			public IList<decimal> ResiduesByWarehouse { get; set; }

			public override IList<string> DynamicColumns
			{
				get
				{
					if (_dynamicRows == null)
					{
						_dynamicRows = new List<string>();
						
						foreach (var sale in SalesBySubdivision)
						{
							_dynamicRows.Add(sale.Amount.ToString(_numericDefaultFormat));
							_dynamicRows.Add(sale.Price.ToString(_financialDefaultFormat));
						}

						foreach (var residue in ResiduesByWarehouse)
						{
							_dynamicRows.Add(residue.ToString(_numericDefaultFormat));
						}
					}
					
					return _dynamicRows;
				}
			}
		}
	}
}
