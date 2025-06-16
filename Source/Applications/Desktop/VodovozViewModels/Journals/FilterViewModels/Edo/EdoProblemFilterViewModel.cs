using System;
using QS.Project.Filter;
using Vodovoz.Core.Domain.Edo;

namespace Vodovoz.ViewModels.Journals.FilterViewModels.Edo
{
	public class EdoProblemFilterViewModel : FilterViewModelBase<EdoProblemFilterViewModel>
	{
		private int? _orderId;
		private DateTime? _deliveryDateFrom;
		private DateTime? _deliveryDateTo;
		private EdoTaskStatus? _edoTaskStatus;
		private TaskProblemState? _taskProblemState;
		private string _problemSourceName;
		private bool? _hasProblemTaskItems;
		private bool? _hasProblemItemGtins;
		private int? _taskId;

		public EdoProblemFilterViewModel()
		{
			_deliveryDateFrom = DateTime.Today.AddDays(-7);
			_taskProblemState = Core.Domain.Edo.TaskProblemState.Active;
			_deliveryDateTo = DateTime.Today;
		}

		public virtual int? OrderId
		{
			get => _orderId;
			set => SetField(ref _orderId, value);
		}

		public virtual int? TaskId
		{
			get => _taskId;
			set => SetField(ref _taskId, value);
		}


		public virtual string ProblemSourceName
		{
			get => _problemSourceName;
			set => SetField(ref _problemSourceName, value);
		}

		public virtual DateTime? DeliveryDateFrom
		{
			get => _deliveryDateFrom;
			set => UpdateFilterField(ref _deliveryDateFrom, value);
		}

		public virtual DateTime? DeliveryDateTo
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

		public virtual bool? HasProblemTaskItems
		{
			get => _hasProblemTaskItems;
			set => UpdateFilterField(ref _hasProblemTaskItems, value);
		}

		public virtual bool? HasProblemItemGtins
		{
			get => _hasProblemItemGtins;
			set => UpdateFilterField(ref _hasProblemItemGtins, value);
		}
	}
}
