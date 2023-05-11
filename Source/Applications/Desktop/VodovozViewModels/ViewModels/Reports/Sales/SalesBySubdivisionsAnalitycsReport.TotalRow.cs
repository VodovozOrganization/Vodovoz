using System.Collections.Generic;

namespace Vodovoz.ViewModels.ViewModels.Reports.Sales
{
	public partial class SalesBySubdivisionsAnalitycsReport
	{
		public class TotalRow : Row
		{
			public override string Title => "Итого";

			public IList<SubTotalRow> SubTotalRows { get; set; }
		}
	}
}
