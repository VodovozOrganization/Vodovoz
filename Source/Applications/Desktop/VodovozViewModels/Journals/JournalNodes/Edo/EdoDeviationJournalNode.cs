using Gamma.Utilities;
using QS.Project.Journal;
using QS.Project.Journal.DataLoader.Hierarchy;
using System;
using System.Collections.Generic;
using Vodovoz.Core.Domain.Edo;

namespace Vodovoz.ViewModels.Journals.JournalNodes.Edo
{
	/// <summary>
	/// Строка журнала отклонений документооборота ЭДО
	/// Узел первого уровня описывает заказ, узлы второго уровня его отклонения,
	/// проблемы и задачи
	/// </summary>
	public class EdoDeviationJournalNode : JournalNodeBase, IHierarchicalNode<EdoDeviationJournalNode>
	{
		public override string Title =>
			NodeType == EdoDeviationJournalNodeType.Order ? OrderTitle : Result;

		#region Поддержка иерархии

		public int Id { get; set; }
		public int? ParentId { get; set; }
		public EdoDeviationJournalNode Parent { get; set; }
		public IList<EdoDeviationJournalNode> Children { get; set; }

		#endregion Поддержка иерархии

		/// <summary>
		/// Вид строки
		/// </summary>
		public EdoDeviationJournalNodeType NodeType { get; set; }

		#region Данные для переходов

		/// <summary>
		/// Номер заказа. У строки-ребенка - номер заказа родителя
		/// </summary>
		public int OrderId { get; set; }

		/// <summary>
		/// Идентификатор контрагента по заказу
		/// </summary>
		public int? CounterpartyId { get; set; }

		/// <summary>
		/// Номер задачи ЭДО
		/// </summary>
		public int? EdoTaskId { get; set; }

		#endregion Данные для переходов

		#region Данные строки

		/// <summary>
		/// Дата доставки заказа
		/// </summary>
		public DateTime? DeliveryDate { get; set; }

		/// <summary>
		/// Наименование контрагента по заказу
		/// </summary>
		public string CounterpartyName { get; set; }

		/// <summary>
		/// Тип задачи ЭДО
		/// </summary>
		public EdoTaskType? TaskType { get; set; }

		/// <summary>
		/// Статус задачи ЭДО
		/// </summary>
		public EdoTaskStatus? TaskStatus { get; set; }

		/// <summary>
		/// Номер задачи ЭДО, если она оказалась задачей отправки документа
		/// </summary>
		public int? DocumentTaskId { get; set; }

		/// <summary>
		/// Номер задачи ЭДО, если она оказалась задачей отправки чека
		/// </summary>
		public int? ReceiptTaskId { get; set; }

		/// <summary>
		/// Тип отклонения
		/// </summary>
		public EdoDeviationType? DeviationType { get; set; }

		/// <summary>
		/// Описание источника проблемы (только у строки проблемы)
		/// </summary>
		public string ProblemSourceDescription { get; set; }

		/// <summary>
		/// Идентификатор источника проблемы (только у строки проблемы)
		/// </summary>
		public string ProblemSourceName { get; set; }

		/// <summary>
		/// Детали отклонения либо сообщение проблемы
		/// </summary>
		public string Description { get; set; }

		/// <summary>
		/// Описание источника отклонения либо рекомендация источника проблемы
		/// </summary>
		public string Recommendation { get; set; }

		/// <summary>
		/// Дата и время обнаружения отклонения либо проблемы.
		/// У строки задачи в проблемном статусе - время последнего изменения задачи:
		/// момента перехода в проблемный статус нигде не записано
		/// </summary>
		public DateTime? DetectedTime { get; set; }

		/// <summary>
		/// Состояние задачи проблемы/отклонения
		/// </summary>
		public TaskProblemState? State { get; set; }

		/// <summary>
		/// Причина разрешения отклонения
		/// </summary>
		public EdoDeviationResolveReason? ResolveReason { get; set; }

		#endregion Данные строки

		#region Счетчики узла заказа

		/// <summary>
		/// Количество отклонений ЭДО в заказе
		/// </summary>
		public int DeviationsCount { get; set; }

		/// <summary>
		/// Количество проблем ЭДО в заказе
		/// </summary>
		public int ProblemsCount { get; set; }

		#endregion Счетчики узла заказа

		#region Отображение

		/// <summary>
		/// Является ли узел строкой заказа
		/// </summary>
		private bool IsOrderNode => NodeType == EdoDeviationJournalNodeType.Order;

		/// <summary>
		/// Состояние, по которому строка попала в журнал
		/// </summary>
		public string Result
		{
			get
			{
				switch(NodeType)
				{
					case EdoDeviationJournalNodeType.Deviation:
						return DeviationType.HasValue ? DeviationType.Value.GetEnumTitle() : string.Empty;
					case EdoDeviationJournalNodeType.Problem:
						return ProblemSourceDescription;
					case EdoDeviationJournalNodeType.UnknownProblem:
						return NodeType.GetEnumTitle();
					default:
						return string.Empty;
				}
			}
		}

		/// <summary>
		/// Номер заказа со счетчиком вложенных строк
		/// </summary>
		public string OrderTitle => IsOrderNode ? $"{OrderId} ({GetChildrenSummary()})" : string.Empty;

		/// <summary>
		/// Дата доставки заказа в формате dd.MM.yyyy
		/// </summary>
		public string DeliveryDateText =>
			IsOrderNode && DeliveryDate.HasValue ? DeliveryDate.Value.ToString("dd.MM.yyyy") : string.Empty;

		/// <summary>
		/// Наименование контрагента по заказу
		/// </summary>
		public string CounterpartyText => IsOrderNode ? CounterpartyName : string.Empty;

		/// <summary>
		/// Номер задачи ЭДО в виде строки
		/// </summary>
		public string EdoTaskIdText => EdoTaskId.HasValue ? EdoTaskId.Value.ToString() : string.Empty;

		/// <summary>
		/// Тип задачи ЭДО в виде строки
		/// </summary>
		public string TaskTypeText => TaskType.HasValue ? TaskType.Value.GetEnumTitle() : string.Empty;

		/// <summary>
		/// Статус задачи ЭДО в виде строки
		/// </summary>
		public string TaskStatusText => TaskStatus.HasValue ? TaskStatus.Value.GetEnumTitle() : string.Empty;

		/// <summary>
		/// Идентификатор источника проблемы в виде строки: у строк других видов
		/// своего источника проблемы нет
		/// </summary>
		public string ProblemSourceNameText =>
			NodeType == EdoDeviationJournalNodeType.Problem ? ProblemSourceName : string.Empty;

		/// <summary>
		/// Дата и время обнаружения отклонения либо проблемы
		/// </summary>
		public string DetectedTimeText =>
			DetectedTime.HasValue ? DetectedTime.Value.ToString("dd.MM.yyyy HH:mm") : string.Empty;

		/// <summary>
		/// Состояние задачи проблемы/отклонения в виде строки
		/// </summary>
		public string StateText => State.HasValue ? State.Value.GetEnumTitle() : string.Empty;

		/// <summary>
		/// Причина разрешения отклонения в виде строки
		/// </summary>
		public string ResolveReasonText =>
			ResolveReason.HasValue ? ResolveReason.Value.GetEnumTitle() : string.Empty;

		private string GetChildrenSummary()
		{
			var parts = new List<string>();

			if(DeviationsCount > 0)
			{
				parts.Add($"{DeviationsCount} отклонений");
			}

			if(ProblemsCount > 0)
			{
				parts.Add($"{ProblemsCount} проблем");
			}

			return parts.Count > 0 ? string.Join(", ", parts) : "пусто";
		}

		#endregion Отображение
	}
}
