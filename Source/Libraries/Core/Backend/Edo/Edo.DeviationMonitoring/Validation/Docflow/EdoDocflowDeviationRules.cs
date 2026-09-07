using Core.Infrastructure;
using System;
using System.Linq;
using Vodovoz.Core.Data.Repositories;
using Vodovoz.Core.Domain.Documents;

namespace Edo.DeviationMonitoring.Validation.Docflow
{
	/// <summary>
	/// Условия и детали отклонений по документообороту у провайдера ЭДО
	/// </summary>
	public static class EdoDocflowDeviationRules
	{
		/// <summary>
		/// Состояния, в которых документ получен оператором и дальнейший ход
		/// документооборота зависит от контрагента
		/// </summary>
		private static readonly EdoDocFlowStatus[] _clientProcessingStates =
		{
			EdoDocFlowStatus.Sent,
			EdoDocFlowStatus.InProgress
		};

		/// <summary>
		/// Документ создан, но документооборот у провайдера ЭДО не заведен
		/// </summary>
		public static DateTime? GetDocumentNotSentTime(IEdoDocflowMonitoringNode node) =>
			node.DocflowCreationTime is null ? node.OutgoingDocumentCreationTime : null;

		/// <summary>
		/// Документооборот заведен, но ни одного ответа провайдера ЭДО по нему нет.
		/// Проверяется отсутствие любого ответа, а не только последнего:
		/// действие NotStarted мы заводим сами, ответом оно не является
		/// </summary>
		public static DateTime? GetNoProviderAnswerTime(IEdoDocflowMonitoringNode node)
		{
			if(node.DocflowCreationTime is null)
			{
				return null;
			}

			return node.HasProviderAnswer ? null : node.DocflowCreationTime;
		}

		/// <summary>
		/// Документ получен оператором, но контрагент не завершает документооборот
		/// </summary>
		public static DateTime? GetClientNotAcceptedTime(IEdoDocflowMonitoringNode node)
		{
			if(node.LastActionState is null)
			{
				return null;
			}

			return _clientProcessingStates.Contains(node.LastActionState.Value)
				? node.FirstSentActionTime
				: null;
		}

		/// <summary>
		/// Проверяет, отслеживается ли результат обработки кодов в ГИС МТ по записи.
		/// По документообороту, завершенному до начала отслеживания, результат уже не придет
		/// </summary>
		/// <param name="entityDate">Дата записи: доставки заказа или создания задачи трансфера</param>
		/// <param name="trackingStartDate">Дата, с которой отслеживается результат ГИС МТ</param>
		public static bool IsGisMtTracked(DateTime entityDate, DateTime trackingStartDate) =>
			entityDate.Date >= trackingStartDate.Date;

		/// <summary>
		/// Документооборот аннулирован. Аннулирование конечно: дальше по такому
		/// документообороту не придет ни результат ГИС МТ, ни смена статуса,
		/// поэтому отклонения по результату ГИС МТ на нем перестают быть актуальными
		/// </summary>
		public static bool IsDocflowCancelled(IEdoDocflowMonitoringNode node) =>
			node.CancelledActionTime != null;

		/// <summary>
		/// Документооборот завершен, но результат обработки кодов в ГИС МТ не получен.
		/// По аннулированному документообороту результата уже не будет,
		/// поэтому отклонение по нему не заводится и снимается
		/// </summary>
		public static DateTime? GetGisMtResultMissingTime(IEdoDocflowMonitoringNode node)
		{
			if(IsDocflowCancelled(node))
			{
				return null;
			}

			return node.TraceabilityStatus is null ? node.SucceedActionTime : null;
		}

		/// <summary>
		/// ГИС МТ не приняла коды по документообороту.
		/// Условие не про время: отказ — это исторический факт, который сам собой
		/// не отменится. Единственное, что снимает такое отклонение, —
		/// аннулирование документооборота
		/// </summary>
		public static DateTime? GetGisMtRejectedTime(IEdoDocflowMonitoringNode node)
		{
			if(IsDocflowCancelled(node))
			{
				return null;
			}

			if(node.TraceabilityStatus is null)
			{
				return null;
			}

			return TrueMarkTraceabilityStatuses.Rejected.Contains(node.TraceabilityStatus.Value)
				? node.TraceabilityActionTime
				: null;
		}

		/// <summary>
		/// Описание отклонения "документ не передан провайдеру ЭДО"
		/// </summary>
		public static string BuildDocumentNotSentDetails(
			IEdoDocflowMonitoringNode node,
			TimeSpan timeout,
			TimeSpan elapsed) =>
			$"Документ создан {EdoDeviationTextFormatter.FormatTime(node.OutgoingDocumentCreationTime.Value)} "
			+ $"в статусе \"{node.OutgoingDocumentStatus.Value.GetEnumDisplayName()}\", "
			+ $"но не передан провайдеру ЭДО за "
			+ $"{EdoDeviationTextFormatter.FormatElapsed(elapsed, timeout)}";

		/// <summary>
		/// Описание отклонения "нет ответа провайдера ЭДО"
		/// </summary>
		public static string BuildNoProviderAnswerDetails(
			IEdoDocflowMonitoringNode node,
			TimeSpan timeout,
			TimeSpan elapsed) =>
			$"Документооборот передан провайдеру ЭДО "
			+ $"{EdoDeviationTextFormatter.FormatTime(node.DocflowCreationTime.Value)}, "
			+ $"ответ не получен за {EdoDeviationTextFormatter.FormatElapsed(elapsed, timeout)}";

		/// <summary>
		/// Описание отклонения "нет результата обработки кодов в ГИС МТ"
		/// </summary>
		public static string BuildGisMtResultMissingDetails(
			IEdoDocflowMonitoringNode node,
			TimeSpan timeout,
			TimeSpan elapsed) =>
			$"Документооборот завершен {EdoDeviationTextFormatter.FormatTime(node.SucceedActionTime.Value)}, "
			+ $"результат обработки кодов в ГИС МТ не получен за "
			+ $"{EdoDeviationTextFormatter.FormatElapsed(elapsed, timeout)}";

		/// <summary>
		/// Описание отклонения "коды не приняты в ГИС МТ"
		/// </summary>
		public static string BuildGisMtRejectedDetails(IEdoDocflowMonitoringNode node) =>
			$"ГИС МТ вернула статус \"{node.TraceabilityStatus.Value.GetEnumDisplayName()}\" "
			+ $"{EdoDeviationTextFormatter.FormatTime(node.TraceabilityActionTime.Value)}. "
			+ "Коды по документообороту не приняты";
	}
}
