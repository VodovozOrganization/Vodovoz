using System;

namespace Vodovoz.Infrastructure.Report.SelectableParametersFilter
{
	public class SelectableParameterSelectionChangedEventArgs : EventArgs
	{
		public SelectableParameterSelectionChangedEventArgs(object id, string title, bool value)
		{
			Id = id;
			Title = title;
			Value = value;
		}

		public object Id { get; }

		public string Title { get; }

		public bool Value { get; }
	}
}
