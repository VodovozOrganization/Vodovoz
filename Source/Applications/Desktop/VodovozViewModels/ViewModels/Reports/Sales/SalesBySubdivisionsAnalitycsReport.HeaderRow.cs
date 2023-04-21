using System.Collections.Generic;
using System.Linq;

namespace Vodovoz.ViewModels.ViewModels.Reports.Sales
{
	public partial class SalesBySubdivisionsAnalitycsReport
	{
		public class HeaderRow : DisplayRow
		{
			public IEnumerable<string> SubdivisionsTitles { get; set; }

			public IEnumerable<string> WarehousesTitles { get; set; }

			private List<string> _dynamicRows;

			public override IList<string> DynamicColumns
			{
				get
				{
					if(_dynamicRows == null)
					{
						_dynamicRows = SubdivisionsTitles.Concat(WarehousesTitles).ToList();
					}

					return _dynamicRows;
				}
			}
		}
	}
}
