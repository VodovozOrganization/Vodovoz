using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Core.Data.Repositories;
using Vodovoz.Core.Domain.Documents;
using Vodovoz.Core.Domain.Edo;

namespace Vodovoz.Core.Data.NHibernate.Repositories.Edo.Deviations
{
	/// <summary>
	/// Собирает состояние задачи ЭДО для мониторинга отклонений из строк, прочитанных репозиторием
	/// </summary>
	internal static class EdoTaskMonitoringNodeBuilder
	{
		/// <summary>
		/// Состояния, в которых документ отправлен оператору и дальнейший ход документооборота зависит от провайдера и контрагента
		/// </summary>
		private static readonly EdoDocFlowStatus[] _sentDocflowStates =
		{
			EdoDocFlowStatus.Sent,
			EdoDocFlowStatus.InProgress
		};

		/// <summary>
		/// Проставляет признак завершенности задачи ЭДО
		/// </summary>
		/// <param name="nodes">Строки мониторинга задач ЭДО</param>
		public static void FillFinished(IReadOnlyCollection<EdoTaskMonitoringNode> nodes)
		{
			foreach(var node in nodes)
			{
				node.IsFinished = EdoTaskStatuses.Finished.Contains(node.TaskStatus);
			}
		}

		/// <summary>
		/// Проставляет признак наличия активной проблемы по задаче
		/// </summary>
		/// <param name="nodes">Строки мониторинга задач ЭДО</param>
		/// <param name="taskIdsWithActiveProblems">Идентификаторы задач с активными проблемами</param>
		public static void FillActiveProblems(
			IReadOnlyCollection<EdoTaskMonitoringNode> nodes,
			ICollection<int> taskIdsWithActiveProblems)
		{
			foreach(var node in nodes)
			{
				node.HasActiveProblem = taskIdsWithActiveProblems.Contains(node.EdoTaskId);
			}
		}

		/// <summary>
		/// Проставляет признак завершенности задачи трансфера
		/// </summary>
		/// <param name="nodes">Строки мониторинга задач трансфера</param>
		public static void FillTransferFinished(IReadOnlyCollection<EdoTransferTaskMonitoringNode> nodes)
		{
			foreach(var node in nodes)
			{
				node.IsFinished = EdoTaskStatuses.Finished.Contains(node.TaskStatus);
			}
		}

		/// <summary>
		/// Заполняет информацию о проблемах по задачам трансфера
		/// </summary>
		/// <param name="nodes">Строки мониторинга задач трансфера</param>
		/// <param name="codesNotMovedProblemTimes">Словарь времени проблем с не перемещенными кодами</param>
		/// <param name="taskIdsWithOtherProblems">Идентификаторы задач с другими проблемами</param>
		public static void FillTransferProblems(
			IReadOnlyCollection<EdoTransferTaskMonitoringNode> nodes,
			IReadOnlyDictionary<int, DateTime> codesNotMovedProblemTimes,
			ICollection<int> taskIdsWithOtherProblems)
		{
			foreach(var node in nodes)
			{
				node.HasActiveProblem = taskIdsWithOtherProblems.Contains(node.EdoTaskId);

				if(codesNotMovedProblemTimes.TryGetValue(node.EdoTaskId, out var problemTime))
				{
					node.CodesNotMovedProblemTime = problemTime;
				}
			}
		}

		/// <summary>
		/// Заполняет данные запросов по ЭДО
		/// </summary>
		/// <param name="nodes">Строки мониторинга задач ЭДО</param>
		/// <param name="requests">Запросы</param>
		/// <param name="orderDeliveryDates">Даты доставки заказов</param>
		public static void FillRequests(
			IReadOnlyCollection<EdoTaskMonitoringNode> nodes,
			IReadOnlyCollection<RequestRow> requests,
			IReadOnlyDictionary<int, DateTime?> orderDeliveryDates)
		{
			var requestsByTask = requests
				.GroupBy(x => x.TaskId)
				.ToDictionary(x => x.Key, x => x.First());

			foreach(var node in nodes)
			{
				if(!requestsByTask.TryGetValue(node.EdoTaskId, out var request))
				{
					continue;
				}

				node.RequestId = request.RequestId;

				if(request.OrderId != null
					&& orderDeliveryDates.TryGetValue(request.OrderId.Value, out var deliveryDate))
				{
					node.OrderDeliveryDate = deliveryDate;
				}
			}
		}

		/// <summary>
		/// Заполняет состояние трансферов у провайдера ЭДО
		/// </summary>
		/// <param name="nodes">Строки мониторинга задач ЭДО</param>
		/// <param name="transferRequests">Запросы на трансфер</param>
		/// <param name="transferTasks">Задачи трансфера</param>
		public static void FillTransfers(
			IReadOnlyCollection<EdoTaskMonitoringNode> nodes,
			IReadOnlyCollection<TransferRequestRow> transferRequests,
			IReadOnlyCollection<TransferTaskRow> transferTasks)
		{
			var transferTasksByRequest = transferTasks.ToDictionary(x => x.RequestId);

			var pendingByTask = transferRequests
				.Where(x => !IsTransferFinished(x, transferTasksByRequest))
				.GroupBy(x => x.OrderTaskId)
				.ToDictionary(x => x.Key, x => x.ToList());

			foreach(var node in nodes)
			{
				if(!pendingByTask.TryGetValue(node.EdoTaskId, out var taskTransfers))
				{
					continue;
				}

				var notStarted = taskTransfers
					.Where(x => GetTransferStartTime(x, transferTasksByRequest) is null)
					.ToList();

				node.HasNotStartedTransfer = notStarted.Any();

				if(notStarted.Any())
				{
					node.PendingTransferIterationTime = notStarted.Min(x => x.IterationTime);
				}
			}
		}

		/// <summary>
		/// Заполняет состояние документооборотов и действий по ним у провайдера ЭДО
		/// </summary>
		/// <param name="nodes">Строки мониторинга задач ЭДО</param>
		/// <param name="documents">Документы</param>
		/// <param name="docflows">Документообороты</param>
		/// <param name="actions">Действия</param>
		public static void FillDocflows(
			IReadOnlyCollection<IEdoDocflowMonitoringNode> nodes,
			IReadOnlyCollection<DocumentRow> documents,
			IReadOnlyCollection<DocflowRow> docflows,
			IReadOnlyCollection<ActionRow> actions)
		{
			var documentsByTask = documents
				.GroupBy(x => x.TaskId)
				.ToDictionary(x => x.Key, x => x.OrderByDescending(d => d.DocumentId).First());

			var docflowsByDocument = docflows
				.GroupBy(x => x.DocumentId)
				.ToDictionary(x => x.Key, x => x.OrderByDescending(d => d.DocflowId).First());

			var actionsByDocflow = actions
				.GroupBy(x => x.DocflowId)
				.ToDictionary(x => x.Key, x => (IReadOnlyCollection<ActionRow>)x.ToList());

			foreach(var node in nodes)
			{
				if(!documentsByTask.TryGetValue(node.EdoTaskId, out var document))
				{
					continue;
				}

				node.OutgoingDocumentCreationTime = document.CreationTime;
				node.OutgoingDocumentStatus = document.Status;

				if(!docflowsByDocument.TryGetValue(document.DocumentId, out var docflow))
				{
					continue;
				}

				node.DocflowCreationTime = docflow.CreationTime;

				if(actionsByDocflow.TryGetValue(docflow.DocflowId, out var docflowActions))
				{
					FillActions(node, docflowActions);
				}
			}
		}

		/// <summary>
		/// Заполняет состояние фискальных документов у провайдера ЭДО.
		/// </summary>
		/// <param name="nodes">Строки мониторинга задач ЭДО</param>
		/// <param name="fiscalDocuments">Фискальные документы</param>
		public static void FillFiscalDocuments(
			IReadOnlyCollection<EdoTaskMonitoringNode> nodes,
			IReadOnlyCollection<FiscalDocumentRow> fiscalDocuments)
		{
			var documentsByTask = fiscalDocuments
				.GroupBy(x => x.TaskId)
				.ToDictionary(x => x.Key, x => x.OrderByDescending(d => d.FiscalDocumentId).First());

			foreach(var node in nodes)
			{
				if(!documentsByTask.TryGetValue(node.EdoTaskId, out var fiscalDocument))
				{
					continue;
				}

				node.FiscalDocumentStage = fiscalDocument.Stage;
				node.FiscalDocumentStatus = fiscalDocument.Status;
				node.FiscalDocumentTime = fiscalDocument.StatusChangeTime ?? fiscalDocument.CreationTime;
				node.FiscalNumber = fiscalDocument.FiscalNumber;
			}
		}

		private static void FillActions(IEdoDocflowMonitoringNode node, IReadOnlyCollection<ActionRow> actions)
		{
			var lastAction = actions
				.OrderByDescending(x => x.ActionId)
				.First();

			node.LastActionTime = lastAction.Time;
			node.LastActionState = lastAction.State;

			// действие NotStarted заводим мы сами при создании документооборота,
			// ответом провайдера ЭДО оно не является
			node.HasProviderAnswer = actions.Any(x => x.State != EdoDocFlowStatus.NotStarted);

			var sentActions = actions.Where(x => _sentDocflowStates.Contains(x.State)).ToList();
			if(sentActions.Any())
			{
				node.FirstSentActionTime = sentActions.Min(x => x.Time);
			}

			var succeedActions = actions.Where(x => x.State == EdoDocFlowStatus.Succeed).ToList();
			if(succeedActions.Any())
			{
				node.SucceedActionTime = succeedActions.Min(x => x.Time);
			}

			var cancellationActions = actions
				.Where(x => x.State == EdoDocFlowStatus.WaitingForCancellation)
				.ToList();

			if(cancellationActions.Any())
			{
				node.WaitingCancellationActionTime = cancellationActions.Min(x => x.Time);
			}

			var cancelledActions = actions
				.Where(x => x.State == EdoDocFlowStatus.Cancelled)
				.ToList();

			if(cancelledActions.Any())
			{
				node.CancelledActionTime = cancelledActions.Min(x => x.Time);
			}

			var traceabilityAction = actions
				.Where(x => x.TraceabilityStatus != null)
				.OrderByDescending(x => x.ActionId)
				.FirstOrDefault();

			if(traceabilityAction != null)
			{
				node.TraceabilityStatus = traceabilityAction.TraceabilityStatus;
				node.TraceabilityActionTime = traceabilityAction.Time;
			}
		}

		private static bool IsTransferFinished(
			TransferRequestRow transferRequest,
			IReadOnlyDictionary<int, TransferTaskRow> transferTasksByRequest)
		{
			return transferTasksByRequest.TryGetValue(transferRequest.RequestId, out var transferTask)
				&& EdoTaskStatuses.Finished.Contains(transferTask.Status);
		}

		private static DateTime? GetTransferStartTime(
			TransferRequestRow transferRequest,
			IReadOnlyDictionary<int, TransferTaskRow> transferTasksByRequest)
		{
			return transferTasksByRequest.TryGetValue(transferRequest.RequestId, out var transferTask)
				? transferTask.TransferStartTime
				: null;
		}
	}
}
