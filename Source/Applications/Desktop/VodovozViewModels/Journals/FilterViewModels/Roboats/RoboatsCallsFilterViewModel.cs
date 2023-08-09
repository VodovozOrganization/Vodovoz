using QS.Project.Filter;
using QS.Project.Journal;
using System;
using Vodovoz.Domain.Roboats;

namespace Vodovoz.ViewModels.Journals.FilterViewModels.Roboats
{
	public class RoboatsCallsFilterViewModel : FilterViewModelBase<RoboatsCallsFilterViewModel>
	{
		private RoboatsCallStatus? _status;
		private DateTime? _endDate;
		private DateTime? _startDate;
		private DateTime? _restrictStartDate;
		private DateTime? _restrictEndDate;

		public virtual RoboatsCallStatus? Status
		{
			get => _status;
			set => UpdateFilterField(ref _status, value);
		}

		public bool CanChangeStatus { get; private set; } = true;

		public virtual RoboatsCallStatus? RestrictStatus
		{
			get => Status;
			set
			{
				Status = value;
				CanChangeStatus = false;
			}
		}

		#region Date

		public bool CanChangeStartDate { get; private set; } = true;
		public virtual DateTime? StartDate
		{
			get => _startDate;
			set => UpdateFilterField(ref _startDate, value);
		}

		public bool CanChangeEndDate { get; private set; } = true;
		public virtual DateTime? EndDate
		{
			get => _endDate;
			set => UpdateFilterField(ref _endDate, value);
		}

		public virtual DateTime? RestrictStartDate
		{
			get => _restrictStartDate;
			set
			{
				if(SetField(ref _restrictStartDate, value))
				{
					StartDate = _restrictStartDate;
					CanChangeStartDate = _restrictStartDate == null;
				}
			}
		}

		public virtual DateTime? RestrictEndDate
		{
			get => _restrictEndDate;
			set
			{
				if(SetField(ref _restrictEndDate, value))
				{
					EndDate = _restrictEndDate;
					CanChangeEndDate = _restrictEndDate == null;
				}
			}
		}

		public override bool IsShow { get; set; } = true;

		#endregion Date
	}
}
