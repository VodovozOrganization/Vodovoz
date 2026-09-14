using QS.Project.Filter;
using System;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.ViewModels.Journals.JournalNodes.Edo;

namespace Vodovoz.ViewModels.Journals.FilterViewModels.Edo
{
	/// <summary>
	/// Фильтр журнала отклонений документооборота ЭДО
	/// </summary>
	public class EdoDeviationFilterViewModel : FilterViewModelBase<EdoDeviationFilterViewModel>
	{
		/// <summary>
		/// Глубина выборки по умолчанию
		/// </summary>
		private const int _defaultDeliveryDatePeriodInDays = 30;

		private int? _orderId;
		private int? _taskId;
		private DateTime? _deliveryDateFrom;
		private DateTime? _deliveryDateTo;
		private EdoDeviationJournalNodeType? _rowType;
		private EdoDeviationType? _deviationType;
		private EdoTaskStatus? _edoTaskStatus;
		private TaskProblemState? _state;
		private string _problemSourceName;

		public EdoDeviationFilterViewModel()
		{
			_deliveryDateFrom = DateTime.Today.AddDays(-_defaultDeliveryDatePeriodInDays);
			_deliveryDateTo = DateTime.Today;
			_state = TaskProblemState.Active;
		}

		/// <summary>
		/// Номер заказа
		/// </summary>
		public virtual int? OrderId
		{
			get => _orderId;
			set => SetField(ref _orderId, value);
		}

		/// <summary>
		/// Идентификатор задачи ЭДО
		/// </summary>
		public virtual int? TaskId
		{
			get => _taskId;
			set => SetField(ref _taskId, value);
		}

		/// <summary>
		/// Идентификатор источника проблемы
		/// </summary>
		public virtual string ProblemSourceName
		{
			get => _problemSourceName;
			set => SetField(ref _problemSourceName, value);
		}

		/// <summary>
		/// Начало периода даты доставки заказа
		/// </summary>
		public virtual DateTime? DeliveryDateFrom
		{
			get => _deliveryDateFrom;
			set => UpdateFilterField(ref _deliveryDateFrom, value);
		}

		/// <summary>
		/// Конец периода даты доставки заказа
		/// </summary>
		public virtual DateTime? DeliveryDateTo
		{
			get => _deliveryDateTo;
			set => UpdateFilterField(ref _deliveryDateTo, value);
		}

		/// <summary>
		/// Вид строки журнала отклонений ЭДО
		/// </summary>
		public virtual EdoDeviationJournalNodeType? RowType
		{
			get => _rowType;
			set => UpdateFilterField(ref _rowType, value);
		}

		/// <summary>
		/// Тип отклонения
		/// </summary>
		public virtual EdoDeviationType? DeviationType
		{
			get => _deviationType;
			set => UpdateFilterField(ref _deviationType, value);
		}

		/// <summary>
		/// Статус задачи ЭДО
		/// </summary>
		public virtual EdoTaskStatus? EdoTaskStatus
		{
			get => _edoTaskStatus;
			set => UpdateFilterField(ref _edoTaskStatus, value);
		}

		/// <summary>
		/// Состояние отклонения или проблемы
		/// </summary>
		public virtual TaskProblemState? State
		{
			get => _state;
			set => UpdateFilterField(ref _state, value);
		}
	}
}
