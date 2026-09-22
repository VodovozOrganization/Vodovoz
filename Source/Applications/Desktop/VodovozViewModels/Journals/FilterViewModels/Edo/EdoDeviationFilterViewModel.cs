using QS.Commands;
using QS.Dialog;
using QS.Project.Filter;
using System;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.ViewModels.Journals.JournalNodes.Edo;
using Vodovoz.ViewModels.Journals.JournalViewModels.Edo.Deviations;

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
		private EdoTaskType? _edoTaskType;
		private EdoTaskStatus? _edoTaskStatus;
		private TaskProblemState? _state;
		private string _problemSourceName;
		private bool? _hasProblemTaskItems;
		private bool? _hasProblemItemGtins;

		private readonly IInteractiveMessage _interactiveMessage;

		public EdoDeviationFilterViewModel(IInteractiveMessage interactiveMessage)
		{
			_interactiveMessage = interactiveMessage ?? throw new ArgumentNullException(nameof(interactiveMessage));

			_deliveryDateFrom = DateTime.Today.AddDays(-_defaultDeliveryDatePeriodInDays);
			_deliveryDateTo = DateTime.Today;
			_state = TaskProblemState.Active;

			HelpCommand = new DelegateCommand(ShowHelp);
		}

		/// <summary>
		/// Отображение диалога справки по журналу
		/// </summary>
		public DelegateCommand HelpCommand { get; }

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
		/// Тип задачи ЭДО
		/// </summary>
		public virtual EdoTaskType? EdoTaskType
		{
			get => _edoTaskType;
			set => UpdateFilterField(ref _edoTaskType, value);
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

		/// <summary>
		/// Наличие у задачи заказа связанных строк с кодами
		/// </summary>
		public virtual bool? HasProblemTaskItems
		{
			get => _hasProblemTaskItems;
			set => UpdateFilterField(ref _hasProblemTaskItems, value);
		}

		/// <summary>
		/// Наличие у проблемы связанных GTIN
		/// </summary>
		public virtual bool? HasProblemItemGtins
		{
			get => _hasProblemItemGtins;
			set => UpdateFilterField(ref _hasProblemItemGtins, value);
		}

		private void ShowHelp()
		{
			_interactiveMessage.ShowMessage(ImportanceLevel.Info, EdoDeviationHelpTexts.BuildHelpMessage(), EdoDeviationHelpTexts.HelpTitle);
		}

	}
}
