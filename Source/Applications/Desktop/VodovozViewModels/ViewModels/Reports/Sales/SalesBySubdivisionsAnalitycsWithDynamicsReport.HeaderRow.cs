using System.Collections.Generic;
using System.Linq;

namespace Vodovoz.ViewModels.ViewModels.Reports.Sales
{
	public partial class SalesBySubdivisionsAnalitycsWithDynamicsReport
	{
		public class HeaderRow : DisplayRow
		{
			private List<string> _dynamicRows;

			public IEnumerable<string> SubdivisionsTitles { get; set; }

			public override IList<string> DynamicColumns
			{
				get
				{
					if(_dynamicRows == null)
					{
						_dynamicRows = SubdivisionsTitles.ToList();
					}

					return _dynamicRows;
				}
			}
		}
	}
}
