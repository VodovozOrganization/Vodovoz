using QS.ViewModels;
using System;

namespace Vodovoz.Presentation.ViewModels.Pacs
{
	public class PacsDashboardFilterViewModel : WidgetViewModelBase
	{
		private DateTime _dateFrom;

		public PacsDashboardFilterViewModel()
		{
			_dateFrom = DateTime.Today;
		}

		public virtual DateTime DateFrom
		{
			get => _dateFrom;
			set => SetField(ref _dateFrom, value);
		}
	}
}
