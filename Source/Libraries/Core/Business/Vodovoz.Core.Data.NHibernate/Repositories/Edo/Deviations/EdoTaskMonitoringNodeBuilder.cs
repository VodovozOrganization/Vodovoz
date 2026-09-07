using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Core.Data.Repositories;
using Vodovoz.Core.Domain.Documents;
using Vodovoz.Core.Domain.Edo;

namespace Vodovoz.Core.Data.NHibernate.Repositories.Edo.Deviations
{
	/// <summary>
	/// Собирает состояние задачи ЭДО для мониторинга отклонений из строк, прочитанных репозиторием.
	/// Здесь, а не в запросах, живут правила трактовки: какая запись считается актуальной,
	/// какое действие документооборота последним и что означает отсутствие метки времени
	/// </summary>
	internal static class EdoTaskMonitoringNodeBuilder
	{
		/// <summary>
		/// Состояния действия, означающие, что документ получен оператором ЭДО
		/// </summary>
		private static readonly EdoDocFlowStatus[] _sentDocflowStates =
		{
			EdoDocFlowStatus.Sent,
			EdoDocFlowStatus.InProgress
		};

		/// <summary>
		/// Проставляет признак завершенности задачи
		/// </summary>
		public static void FillFinished(IReadOnlyCollection<EdoTaskMonitoringNode> nodes)
		{
			foreach(var node in nodes)
			{
				node.IsFinished = EdoTaskStatuses.Finished.Contains(node.TaskStatus);
			}
		}

		/// <summary>
		/// Проставляет признак наличия активной зарегистрированной проблемы
		/// </summary>
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
		public static void FillTransferFinished(IReadOnlyCollection<EdoTransferTaskMonitoringNode> nodes)
		{
			foreach(var node in nodes)
			{
				node.IsFinished = EdoTaskStatuses.Finished.Contains(node.TaskStatus);
			}
		}

		/// <summary>
		/// Раскладывает проблемы задачи трансфера: ожидание перемещения кодов мониторинг
		/// меряет сам, поэтому оно отделено от остальных проблем
		/// </summary>
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
		/// Связывает задачу с породившей ее заявкой и заказом
		/// </summary>
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
		/// Считает состояние трансферов задачи заказа.
		/// <para>
		/// Заявка на трансфер создается без трансферной задачи: задачу подбирает диспетчер трансферов.
		/// Поэтому "перенос кодов не запущен" — это и заявка без трансферной задачи,
		/// и заявка с задачей, у которой не проставлено время начала переноса
		/// </para>
		/// <para>
		/// Дальше стадий трансфера со стороны задачи заказа мониторинг не идет:
		/// сам трансфер проверяется отдельным семейством валидаторов по своей задаче
		/// </para>
		/// </summary>
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
					// таймаут отсчитывается от итерации, по которой перенос так и не начался,
					// а не от самой ранней незавершенной: при нескольких трансферах это разные итерации
					node.PendingTransferIterationTime = notStarted.Min(x => x.IterationTime);
				}
			}
		}

		/// <summary>
		/// Считает состояние документооборота у провайдера ЭДО.
		/// Актуальными считаются последний созданный документ задачи
		/// и последний заведенный по нему документооборот
		/// </summary>
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
		/// Считает состояние последнего фискального документа задачи отправки чека
		/// </summary>
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

		/// <summary>
		/// Считает состояние документооборота по его действиям.
		/// <para>
		/// Последнее действие определяется порядком записи, а не временем: время действия провайдера —
		/// это метка Такскома, а время действия <see cref="EdoDocFlowStatus.NotStarted"/> — наше,
		/// сравнивать их между собой нельзя
		/// </para>
		/// </summary>
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

		/// <summary>
		/// Заявка на трансфер без подобранной трансферной задачи незавершенной не считается:
		/// строки задач читаются только для заявок, у которых задача уже есть
		/// </summary>
		private static bool IsTransferFinished(
			TransferRequestRow transferRequest,
			IReadOnlyDictionary<int, TransferTaskRow> transferTasksByRequest)
		{
			return transferTasksByRequest.TryGetValue(transferRequest.RequestId, out var transferTask)
				&& EdoTaskStatuses.Finished.Contains(transferTask.Status);
		}

		/// <summary>
		/// Возвращает время начала переноса кодов по заявке на трансфер
		/// или <c>null</c>, если трансферная задача не подобрана или перенос еще не запущен
		/// </summary>
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
