using System.Collections.Generic;

namespace Vodovoz.ViewModels.ViewModels.Reports.Sales
{
	public partial class SalesBySubdivisionsAnalitycsWithDynamicsReport
	{
		public class SubTotalRow : Row
		{
			public IList<Row> NomenclatureRows { get; set; }
		}
	}
}
