using System;
using System.Collections.Generic;
using Vodovoz.Infrastructure.Report.SelectableParametersFilter;

namespace Vodovoz.ViewModels.Reports
{
	public class SelectableParameterReportFilterSelectionChangedArgs : EventArgs
	{
		public SelectableParameterReportFilterSelectionChangedArgs(string name, params SelectableParameterSelectionChangedEventArgs[] selectableParameterSelectionChangedEventArgs)
		{
			Name = name;
			ParametersChanged = selectableParameterSelectionChangedEventArgs;
		}

		public string Name { get; }

		public IEnumerable<SelectableParameterSelectionChangedEventArgs> ParametersChanged { get; }
	}
}
