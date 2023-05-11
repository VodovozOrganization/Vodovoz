using System.Collections.Generic;

namespace Vodovoz.ViewModels.ViewModels.Reports.Sales
{
	public partial class SalesBySubdivisionsAnalitycsReport
	{
		public class DisplayRow
		{
			protected const string _numericDefaultFormat = "0";
			protected const string _financialDefaultFormat = "# ### ### ##0.00";

			public virtual string Title { get; set; }

			public virtual IList<string> DynamicColumns { get; set; }
		}
	}
}
