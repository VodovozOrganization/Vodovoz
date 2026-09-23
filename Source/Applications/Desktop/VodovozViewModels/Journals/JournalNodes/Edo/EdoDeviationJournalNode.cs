using Gamma.Utilities;
using QS.Project.Journal;
using QS.Project.Journal.DataLoader.Hierarchy;
using QS.Utilities;
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
		/// <summary>
		/// Заголовок строки: у строки заказа - его номер со счетчиком вложенных строк,
		/// у остальных - состояние, по которому строка попала в журнал
		/// </summary>
		public override string Title =>
			NodeType == EdoDeviationJournalNodeType.Order ? OrderTitle : Result;

		#region Поддержка иерархии

		/// <inheritdoc/>
		public int Id { get; set; }

		/// <inheritdoc/>
		public int? ParentId { get; set; }

		/// <inheritdoc/>
		public EdoDeviationJournalNode Parent { get; set; }

		/// <inheritdoc/>
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
		/// Тип отклонения
		/// </summary>
		public EdoDeviationType? DeviationType { get; set; }

		/// <summary>
		/// Описание источника проблемы
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
					case EdoDeviationJournalNodeType.UnknownProblem:
						return ProblemSourceDescription;
					default:
						return string.Empty;
				}
			}
		}

		/// <summary>
		/// Номер заказа со счетчиком вложенных строк
		/// </summary>
		public string OrderTitle => IsOrderNode ? $"{OrderId} ({GetChildrenSummary()})" : string.Empty;

		private string GetChildrenSummary()
		{
			var parts = new List<string>();

			if(DeviationsCount > 0)
			{
				parts.Add($"{DeviationsCount} "
					+ NumberToTextRus.Case(DeviationsCount, "отклонение", "отклонения", "отклонений"));
			}

			if(ProblemsCount > 0)
			{
				parts.Add($"{ProblemsCount} "
					+ NumberToTextRus.Case(ProblemsCount, "проблема", "проблемы", "проблем"));
			}

			return parts.Count > 0 ? string.Join(", ", parts) : "пусто";
		}

		#endregion Отображение
	}
}
