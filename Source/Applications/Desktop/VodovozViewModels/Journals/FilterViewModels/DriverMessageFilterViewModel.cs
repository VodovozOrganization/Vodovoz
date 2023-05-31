using System;
using QS.Project.Filter;
namespace Vodovoz.ViewModels.Journals.FilterViewModels
{
	public class DriverMessageFilterViewModel : FilterViewModelBase<DriverMessageFilterViewModel>
	{
		private DateTime? _startDate;
		private DateTime? _endDate;

		public DriverMessageFilterViewModel()
		{
			_startDate = _endDate = DateTime.Now;
		}

		public virtual DateTime? StartDate
		{
			get => _startDate;
			set => UpdateFilterField(ref _startDate, value);
		}

		public virtual DateTime? EndDate
		{
			get => _endDate;
			set => UpdateFilterField(ref _endDate, value);
		}
	}
}
