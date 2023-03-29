using System;
using Vodovoz.Infrastructure.Report.SelectableParametersFilter;

namespace Vodovoz.ViewModels.Reports
{
	public class FilterTypeChangedArgs : EventArgs
	{
		public FilterTypeChangedArgs(SelectableFilterType filterType)
		{
			FilterType = filterType;
		}

		public SelectableFilterType FilterType { get; }
	}
}
