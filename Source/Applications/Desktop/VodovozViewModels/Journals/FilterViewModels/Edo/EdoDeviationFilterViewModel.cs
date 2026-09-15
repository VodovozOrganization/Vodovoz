using Core.Infrastructure;
using QS.Commands;
using QS.Dialog;
using QS.Project.Filter;
using System;
using System.Collections.Generic;
using System.Text;
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

		/// <summary>
		/// Заголовок окна справки по критериям отклонений
		/// </summary>
		private const string _helpTitle = "Критерии отклонений документооборота ЭДО";

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

		private void ShowHelp()
		{
			_interactiveMessage.ShowMessage(ImportanceLevel.Info, BuildHelpMessage(), _helpTitle);
		}

		private static string BuildHelpMessage()
		{
			var message = new StringBuilder();

			message
				.AppendLine("Отклонение заводится, когда заявка или задача ЭДО задерживается на своей стадии дольше таймаута,"
					+ " заданного для этого типа отклонения в справочнике источников отклонений.")
				.AppendLine()
				.AppendLine("Общие условия, без которых отклонение по задаче не заводится:")
				.AppendLine("• задача не завершена и не отменена (только проверки результата ГИС МТ работают и по завершенным);")
				.AppendLine("• по задаче нет активной проблемыи сама она не в статусе \"Проблема\";")
				.AppendLine("• по задаче нет другого активного отклонения;")
				.AppendLine("• не сработал ни один тип отклонения, идущий раньше по ходу документооборота.")
				.AppendLine()
				.AppendLine("Критерии по типам отклонений:");

			foreach(EdoDeviationType deviationType in Enum.GetValues(typeof(EdoDeviationType)))
			{
				if(!_deviationCriteria.TryGetValue(deviationType, out var criteria))
				{
					continue;
				}

				message.AppendLine($"• {deviationType.GetEnumDisplayName()} - {criteria}");
			}

			return message.ToString();
		}

		/// <summary>
		/// Условие, при котором мониторинг заводит отклонение каждого типа
		/// </summary>
		private static readonly IDictionary<EdoDeviationType, string> _deviationCriteria =
			new Dictionary<EdoDeviationType, string>
			{
				{
					EdoDeviationType.TaskNotCreated,
					"заявка создана, задача ЭДО по ней так и не создана"
				},
				{
					EdoDeviationType.TaskNotStarted,
					"задача создана и остается в статусе \"Новая\": обработчик к ней не приступал"
				},
				{
					EdoDeviationType.TransferNotStarted,
					"задача на стадии трансфера, но по заявкам на перенос кодов перенос не запущен"
				},
				{
					EdoDeviationType.DocumentNotSentToProvider,
					"исходящий документ создан, а документооборот у провайдера ЭДО не заведен"
				},
				{
					EdoDeviationType.ProviderNoResponse,
					"документооборот заведен, но провайдер ЭДО не прислал по нему ни одного действия"
				},
				{
					EdoDeviationType.ClientNotAcceptedDocflow,
					"последнее действие документооборота - \"Получено оператором\" или \"В процессе\":"
					+ " документ у клиента, и клиент его не завершает"
				},
				{
					EdoDeviationType.CancellationNotCompleted,
					"последнее действие документооборота - \"Ожидает аннулирования\""
				},
				{
					EdoDeviationType.GisMtResultMissing,
					"документооборот завершен и не аннулирован, но результат обработки кодов в ГИС МТ не пришел."
					+ " Проверяется и по завершенным задачам"
				},
				{
					EdoDeviationType.GisMtRejected,
					"последний статус ГИС МТ по документообороту - отказной: коды не приняты."
					+ " Проверяется и по завершенным задачам"
				},
				{
					EdoDeviationType.ReceiptNotFiscalized,
					"чек передан в отправку, но фискальный документ не создан либо не фискализирован."
					+ " Ожидание ответа кассы отклонением не считается"
				},
				{
					EdoDeviationType.TaskStalled,
					"задача не завершена, при этом ни один частный тип отклонения к ней не подходит:"
					+ " причина задержки не определена"
				},
				{
					EdoDeviationType.TransferWaitingRequestsTooLong,
					"задача трансфера остается на стадии \"Ожидает запросов\":"
					+ " досылка залежавшихся трансферов не сработала"
				},
				{
					EdoDeviationType.TransferDocumentNotCreated,
					"задача трансфера на стадии подготовки или отправки, документ на перенос кодов не создан"
				},
				{
					EdoDeviationType.TransferDocumentNotSentToProvider,
					"документ трансфера создан, а документооборот у провайдера ЭДО не заведен"
				},
				{
					EdoDeviationType.TransferProviderNoResponse,
					"документооборот трансфера заведен, ответа провайдера ЭДО по нему нет"
				},
				{
					EdoDeviationType.TransferGisMtResultMissing,
					"документооборот трансфера завершен, но результат обработки кодов в ГИС МТ не пришел."
					+ " Проверяется и по завершенным задачам"
				},
				{
					EdoDeviationType.TransferGisMtRejected,
					"ГИС МТ вернула отказной статус по документообороту трансфера."
					+ " Проверяется и по завершенным задачам"
				},
				{
					EdoDeviationType.TransferCodesNotMoved,
					"документооборот трансфера завершен, но коды не сменили владельца в ГИС МТ:"
					+ " по задаче висит незакрытая проблема ожидания перемещения"
				},
				{
					EdoDeviationType.TransferTooLong,
					"перенос кодов запущен и не завершается."
					+ " Ожидание перемещения кодов в ГИС МТ сюда не относится"
				},
				{
					EdoDeviationType.TransferStalled,
					"задача трансфера не завершена, при этом ни один частный тип отклонения к ней не подходит"
				}
			};
	}
}
