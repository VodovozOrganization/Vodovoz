using System;
using QS.Project.Filter;
using Vodovoz.Core.Domain.Edo;

namespace Vodovoz.ViewModels.Journals.FilterViewModels.Edo
{
	public class EdoProblemFilterViewModel : FilterViewModelBase<EdoProblemFilterViewModel>
	{
		private int? _orderId;
		private DateTime _deliveryDateFrom;
		private DateTime _deliveryDateTo;
		private CustomerEdoRequestSource? _requestSource;
		private EdoTaskStatus? _edoTaskStatus;
		private TaskProblemState? _taskProblemState;
		private string _sourceId;
		private bool? _hasProblemItems;
		private bool? _hasProblemItemGtins;
		private int? _taskId;

		public EdoProblemFilterViewModel()
		{
			_deliveryDateFrom = DateTime.Today.AddDays(-7);
			_deliveryDateTo = DateTime.Today;
		}

		public virtual int? OrderId
		{
			get => _orderId;
			set => UpdateFilterField(ref _orderId, value);
		}
		
		public virtual int? TaskId
		{
			get => _taskId;
			set => UpdateFilterField(ref _taskId, value);
		}


		public virtual string SourceId
		{
			get => _sourceId;
			set => UpdateFilterField(ref _sourceId, value);
		}

		public virtual DateTime DeliveryDateFrom
		{
			get => _deliveryDateFrom;
			set => UpdateFilterField(ref _deliveryDateFrom, value);
		}

		public virtual DateTime DeliveryDateTo
		{
			get => _deliveryDateTo;
			set => UpdateFilterField(ref _deliveryDateTo, value);
		}

		public virtual EdoTaskStatus? EdoTaskStatus
		{
			get => _edoTaskStatus;
			set => UpdateFilterField(ref _edoTaskStatus, value);
		}

		public virtual TaskProblemState? TaskProblemState
		{
			get => _taskProblemState;
			set => UpdateFilterField(ref _taskProblemState, value);
		}

		public virtual bool? HasProblemItems
		{
			get => _hasProblemItems;
			set => UpdateFilterField(ref _hasProblemItems, value);
		}

		public virtual bool? HasProblemItemGtins
		{
			get => _hasProblemItemGtins;
			set => UpdateFilterField(ref _hasProblemItemGtins, value);
		}
	}
}
