using System;
using System.Collections.Generic;

namespace Vodovoz.Infrastructure.Report.SelectableParametersFilter
{
	public class SelectableParameterSetSelectionChanged : EventArgs
	{
		public SelectableParameterSetSelectionChanged(string name, params SelectableParameterSelectionChangedEventArgs[] selectableParameterSelectionChangedEventArgs)
		{
			Name = name;
			ParametersChanged = selectableParameterSelectionChangedEventArgs;
		}

		public string Name { get; }

		public IEnumerable<SelectableParameterSelectionChangedEventArgs> ParametersChanged { get; }
	}
}
