using System;
using QS.Project.Filter;
namespace Vodovoz.ViewModels.Journals.FilterViewModels
{
	public class DriverMessageFilterViewModel : FilterViewModelBase<DriverMessageFilterViewModel>
	{
		private static readonly DateTime _defaultDate = DateTime.Now;
		public DriverMessageFilterViewModel()
		{
		}

		private DateTime? startDate = _defaultDate;
		public virtual DateTime? StartDate
		{
			get => startDate;
			set => SetField(ref startDate, value);
		}

		private DateTime? endDate = _defaultDate.AddDays(1);
		public virtual DateTime? EndDate
		{
			get => endDate;
			set => SetField(ref endDate, value);
		}
	}
}
