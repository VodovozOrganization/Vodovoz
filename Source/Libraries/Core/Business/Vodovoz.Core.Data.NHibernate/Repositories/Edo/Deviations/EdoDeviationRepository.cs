using NHibernate.Linq;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Data.Repositories;
using Vodovoz.Core.Domain.Documents;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Orders;

namespace Vodovoz.Core.Data.NHibernate.Repositories.Edo.Deviations
{
	/// <inheritdoc cref="IEdoDeviationRepository"/>
	public class EdoDeviationRepository : IEdoDeviationRepository
	{
		/// <summary>
		/// Размер пачки идентификаторов в одном запросе.
		/// Ограничивает количество параметров запроса при выборке по списку кодов
		/// </summary>
		private const int _idsBatchSize = 500;

		/// <summary>
		/// Имя источника проблемы ожидания перемещения кодов в ГИС МТ.
		/// Проблему заводит валидатор Edo.Problems.Validation.Sources.CodeTransferedEdoValidator,
		/// а мониторинг меряет, сколько она висит, поэтому из общего отсева проблем она исключается
		/// </summary>
		private const string _codesNotMovedProblemSourceName = "Transfer.CodesTransfered";

		/// <summary>
		/// Проекция задачи отправки документа в состояние для мониторинга.
		/// Вынесена в поле, чтобы одинаково читаться и постраничной выборкой, и выборкой по кодам
		/// </summary>
		private static readonly Expression<Func<DocumentEdoTask, EdoTaskMonitoringNode>> _documentTaskProjection =
			x => new EdoTaskMonitoringNode
			{
				EdoTaskId = x.Id,
				TaskType = EdoTaskType.Document,
				TaskStatus = x.Status,
				TaskCreationTime = x.CreationTime,
				TaskStartTime = x.StartTime,
				DocumentStage = x.Stage
			};

		/// <summary>
		/// Проекция задачи отправки чека в состояние для мониторинга
		/// </summary>
		private static readonly Expression<Func<ReceiptEdoTask, EdoTaskMonitoringNode>> _receiptTaskProjection =
			x => new EdoTaskMonitoringNode
			{
				EdoTaskId = x.Id,
				TaskType = EdoTaskType.Receipt,
				TaskStatus = x.Status,
				TaskCreationTime = x.CreationTime,
				TaskStartTime = x.StartTime,
				ReceiptStatus = x.ReceiptStatus
			};

		/// <summary>
		/// Проекция задачи трансфера в состояние для мониторинга
		/// </summary>
		private static readonly Expression<Func<TransferEdoTask, EdoTransferTaskMonitoringNode>>
			_transferTaskProjection =
				x => new EdoTransferTaskMonitoringNode
				{
					EdoTaskId = x.Id,
					TaskStatus = x.Status,
					TransferStage = x.TransferStatus,
					TaskCreationTime = x.CreationTime,
					TaskStartTime = x.StartTime,
					TransferStartTime = x.TransferStartTime
				};

		/// <inheritdoc/>
		public async Task<IList<EdoRequestMonitoringNode>> GetRequestsWithoutTaskAsync(
			IUnitOfWork uow,
			DateTime createdBefore,
			int afterRequestId,
			int limit,
			CancellationToken cancellationToken)
		{
			if(uow is null)
			{
				throw new ArgumentNullException(nameof(uow));
			}

			var requestIdsWithActiveDeviations = GetRequestIdsWithActiveDeviations(uow);

			return await uow.Session.Query<FormalEdoRequest>()
				.Where(x => x.Task == null)
				.Where(x => x.Id > afterRequestId)
				.Where(x => !requestIdsWithActiveDeviations.Contains(x.Id))
				.Where(x => x.Time < createdBefore)
				.Where(x => x is PrimaryEdoRequest || x is ManualEdoRequest)
				.OrderBy(x => x.Id)
				.Take(limit)
				.Select(x => new EdoRequestMonitoringNode
				{
					RequestId = x.Id,
					RequestTime = x.Time,
					HasTask = false
				})
				.ToListAsync(cancellationToken);
		}

		/// <inheritdoc/>
		public async Task<IList<EdoTaskMonitoringNode>> GetMonitoredTasksAsync(
			IUnitOfWork uow,
			int afterTaskId,
			int limit,
			CancellationToken cancellationToken)
		{
			if(uow is null)
			{
				throw new ArgumentNullException(nameof(uow));
			}

			var taskIdsWithActiveProblems = GetTaskIdsWithActiveProblems(uow);
			var taskIdsWithActiveDeviations = GetTaskIdsWithActiveDeviations(uow);

			var documentNodes = await uow.Session.Query<DocumentEdoTask>()
				.Where(x => x.Id > afterTaskId)
				.Where(x => !EdoTaskStatuses.Finished.Contains(x.Status))
				.Where(x => !taskIdsWithActiveProblems.Contains(x.Id))
				.Where(x => !taskIdsWithActiveDeviations.Contains(x.Id))
				.OrderBy(x => x.Id)
				.Take(limit)
				.Select(_documentTaskProjection)
				.ToListAsync(cancellationToken);

			var receiptNodes = await uow.Session.Query<ReceiptEdoTask>()
				.Where(x => x.Id > afterTaskId)
				.Where(x => !EdoTaskStatuses.Finished.Contains(x.Status))
				.Where(x => !taskIdsWithActiveProblems.Contains(x.Id))
				.Where(x => !taskIdsWithActiveDeviations.Contains(x.Id))
				.OrderBy(x => x.Id)
				.Take(limit)
				.Select(_receiptTaskProjection)
				.ToListAsync(cancellationToken);

			var nodes = documentNodes
				.Concat(receiptNodes)
				.OrderBy(x => x.EdoTaskId)
				.Take(limit)
				.ToList();

			await FillNodesAsync(uow, nodes, cancellationToken);

			return nodes;
		}

		/// <inheritdoc/>
		public async Task<IList<int>> GetTaskIdsWithFinishedDocflowAsync(
			IUnitOfWork uow,
			DateTime gisMtTrackedFrom,
			int afterTaskId,
			int limit,
			CancellationToken cancellationToken)
		{
			if(uow is null)
			{
				throw new ArgumentNullException(nameof(uow));
			}

			var taskIdsWithActiveProblems = GetTaskIdsWithActiveProblems(uow);
			var taskIdsWithActiveDeviations = GetTaskIdsWithActiveDeviations(uow);
			var taskIdsWithOldDelivery = GetTaskIdsDeliveredBefore(uow, gisMtTrackedFrom);

			var documentIdsWithFinishedDocflow = GetDocumentIdsWithFinishedDocflow(uow);
			var documentIdsWithAcceptedTraceability = GetDocumentIdsWithAcceptedTraceability(uow);

			var taskIdsWithFinishedDocflow = uow.Session.Query<OrderEdoDocument>()
				.Where(x => documentIdsWithFinishedDocflow.Contains(x.Id))
				.Where(x => !documentIdsWithAcceptedTraceability.Contains(x.Id))
				.Select(x => x.DocumentTaskId);

			return await uow.Session.Query<DocumentEdoTask>()
				.Where(x => x.Id > afterTaskId)
				.Where(x => taskIdsWithFinishedDocflow.Contains(x.Id))
				.Where(x => !taskIdsWithOldDelivery.Contains(x.Id))
				.Where(x => !taskIdsWithActiveProblems.Contains(x.Id))
				.Where(x => !taskIdsWithActiveDeviations.Contains(x.Id))
				.OrderBy(x => x.Id)
				.Take(limit)
				.Select(x => x.Id)
				.ToListAsync(cancellationToken);
		}

		/// <inheritdoc/>
		public async Task<IList<EdoTransferTaskMonitoringNode>> GetMonitoredTransferTasksAsync(
			IUnitOfWork uow,
			int afterTaskId,
			int limit,
			CancellationToken cancellationToken)
		{
			if(uow is null)
			{
				throw new ArgumentNullException(nameof(uow));
			}

			var taskIdsWithOtherProblems = GetTaskIdsWithActiveProblems(uow, _codesNotMovedProblemSourceName);
			var taskIdsWithActiveDeviations = GetTaskIdsWithActiveDeviations(uow);

			var nodes = await uow.Session.Query<TransferEdoTask>()
				.Where(x => x.Id > afterTaskId)
				.Where(x => !EdoTaskStatuses.Finished.Contains(x.Status))
				.Where(x => !taskIdsWithOtherProblems.Contains(x.Id))
				.Where(x => !taskIdsWithActiveDeviations.Contains(x.Id))
				.OrderBy(x => x.Id)
				.Take(limit)
				.Select(_transferTaskProjection)
				.ToListAsync(cancellationToken);

			await FillTransferNodesAsync(uow, nodes, cancellationToken);

			return nodes;
		}

		/// <inheritdoc/>
		public async Task<IList<int>> GetTransferTaskIdsWithFinishedDocflowAsync(
			IUnitOfWork uow,
			DateTime gisMtTrackedFrom,
			int afterTaskId,
			int limit,
			CancellationToken cancellationToken)
		{
			if(uow is null)
			{
				throw new ArgumentNullException(nameof(uow));
			}

			var taskIdsWithOtherProblems = GetTaskIdsWithActiveProblems(uow, _codesNotMovedProblemSourceName);
			var taskIdsWithActiveDeviations = GetTaskIdsWithActiveDeviations(uow);
			var documentIdsWithFinishedDocflow = GetDocumentIdsWithFinishedDocflow(uow);
			var documentIdsWithAcceptedTraceability = GetDocumentIdsWithAcceptedTraceability(uow);

			var taskIdsWithFinishedDocflow = uow.Session.Query<TransferEdoDocument>()
				.Where(x => documentIdsWithFinishedDocflow.Contains(x.Id))
				.Where(x => !documentIdsWithAcceptedTraceability.Contains(x.Id))
				.Select(x => x.TransferTaskId);

			return await uow.Session.Query<TransferEdoTask>()
				.Where(x => x.Id > afterTaskId)
				.Where(x => taskIdsWithFinishedDocflow.Contains(x.Id))
				.Where(x => x.CreationTime >= gisMtTrackedFrom)
				.Where(x => !taskIdsWithOtherProblems.Contains(x.Id))
				.Where(x => !taskIdsWithActiveDeviations.Contains(x.Id))
				.OrderBy(x => x.Id)
				.Take(limit)
				.Select(x => x.Id)
				.ToListAsync(cancellationToken);
		}

		/// <inheritdoc/>
		public async Task<IList<EdoTransferTaskMonitoringNode>> GetTransferTaskNodesAsync(
			IUnitOfWork uow,
			IReadOnlyCollection<int> edoTaskIds,
			CancellationToken cancellationToken)
		{
			if(uow is null)
			{
				throw new ArgumentNullException(nameof(uow));
			}

			if(edoTaskIds is null || !edoTaskIds.Any())
			{
				return new List<EdoTransferTaskMonitoringNode>();
			}

			var nodes = new List<EdoTransferTaskMonitoringNode>();

			foreach(var idsBatch in SplitToBatches(edoTaskIds))
			{
				var batch = await uow.Session.Query<TransferEdoTask>()
					.Where(x => idsBatch.Contains(x.Id))
					.Select(_transferTaskProjection)
					.ToListAsync(cancellationToken);

				nodes.AddRange(batch);
			}

			if(!nodes.Any())
			{
				return nodes;
			}

			EdoTaskMonitoringNodeBuilder.FillTransferFinished(nodes);

			await FillTransferNodesAsync(uow, nodes, cancellationToken);

			return nodes;
		}

		/// <inheritdoc/>
		public async Task<IList<EdoTaskDeviation>> GetActiveDeviationsPageAsync(
			IUnitOfWork uow,
			int afterDeviationId,
			int limit,
			CancellationToken cancellationToken)
		{
			if(uow is null)
			{
				throw new ArgumentNullException(nameof(uow));
			}

			// курсорная страница с явной сортировкой по коду: IGenericRepository
			// упорядочивать выборку не умеет, поэтому здесь свой метод
			return await uow.Session.Query<EdoTaskDeviation>()
				.Where(x => x.State == TaskProblemState.Active)
				.Where(x => x.Id > afterDeviationId)
				.OrderBy(x => x.Id)
				.Take(limit)
				.ToListAsync(cancellationToken);
		}

		/// <inheritdoc/>
		public async Task<IList<EdoTaskMonitoringNode>> GetTaskNodesAsync(
			IUnitOfWork uow,
			IReadOnlyCollection<int> edoTaskIds,
			CancellationToken cancellationToken)
		{
			if(uow is null)
			{
				throw new ArgumentNullException(nameof(uow));
			}

			if(edoTaskIds is null || !edoTaskIds.Any())
			{
				return new List<EdoTaskMonitoringNode>();
			}

			var nodes = new List<EdoTaskMonitoringNode>();

			foreach(var idsBatch in SplitToBatches(edoTaskIds))
			{
				var documentNodes = await uow.Session.Query<DocumentEdoTask>()
					.Where(x => idsBatch.Contains(x.Id))
					.Select(_documentTaskProjection)
					.ToListAsync(cancellationToken);

				var receiptNodes = await uow.Session.Query<ReceiptEdoTask>()
					.Where(x => idsBatch.Contains(x.Id))
					.Select(_receiptTaskProjection)
					.ToListAsync(cancellationToken);

				nodes.AddRange(documentNodes);
				nodes.AddRange(receiptNodes);
			}

			if(!nodes.Any())
			{
				return nodes;
			}

			EdoTaskMonitoringNodeBuilder.FillFinished(nodes);
			EdoTaskMonitoringNodeBuilder.FillActiveProblems(
				nodes,
				await GetTaskIdsWithActiveProblemsAsync(
					uow,
					nodes.Select(x => x.EdoTaskId).ToArray(),
					null,
					cancellationToken));

			await FillNodesAsync(uow, nodes, cancellationToken);

			return nodes;
		}

		/// <inheritdoc/>
		public async Task<IList<EdoRequestMonitoringNode>> GetRequestNodesAsync(
			IUnitOfWork uow,
			IReadOnlyCollection<int> edoRequestIds,
			CancellationToken cancellationToken)
		{
			if(uow is null)
			{
				throw new ArgumentNullException(nameof(uow));
			}

			if(edoRequestIds is null || !edoRequestIds.Any())
			{
				return new List<EdoRequestMonitoringNode>();
			}

			var nodes = new List<EdoRequestMonitoringNode>();

			foreach(var idsBatch in SplitToBatches(edoRequestIds))
			{
				var batch = await uow.Session.Query<FormalEdoRequest>()
					.Where(x => idsBatch.Contains(x.Id))
					.Select(x => new EdoRequestMonitoringNode
					{
						RequestId = x.Id,
						RequestTime = x.Time,
						HasTask = x.Task != null
					})
					.ToListAsync(cancellationToken);

				nodes.AddRange(batch);
			}

			return nodes;
		}

		/// <summary>
		/// Дочитывает состояние задач: заявки, трансферы, документооборот и фискальные документы.
		/// Запросы отдают плоские строки, трактовкой занимается <see cref="EdoTaskMonitoringNodeBuilder"/>
		/// </summary>
		private async Task FillNodesAsync(
			IUnitOfWork uow,
			IReadOnlyCollection<EdoTaskMonitoringNode> nodes,
			CancellationToken cancellationToken)
		{
			if(!nodes.Any())
			{
				return;
			}

			var taskIds = nodes.Select(x => x.EdoTaskId).ToArray();

			var requests = await GetRequestRowsAsync(uow, taskIds, cancellationToken);

			EdoTaskMonitoringNodeBuilder.FillRequests(
				nodes,
				requests,
				await GetOrderDeliveryDatesAsync(
					uow,
					requests.Where(x => x.OrderId != null).Select(x => x.OrderId.Value).Distinct().ToArray(),
					cancellationToken));

			var transferRequests = await GetTransferRequestRowsAsync(uow, taskIds, cancellationToken);
			var transferRequestIds = transferRequests.Select(x => x.RequestId).ToArray();

			EdoTaskMonitoringNodeBuilder.FillTransfers(
				nodes,
				transferRequests,
				await GetTransferTaskRowsAsync(uow, transferRequestIds, cancellationToken));

			var documents = await GetDocumentRowsAsync(uow, taskIds, cancellationToken);
			var documentIds = documents.Select(x => x.DocumentId).ToArray();
			var docflows = await GetDocflowRowsAsync(uow, documentIds, cancellationToken);
			var docflowIds = docflows.Select(x => x.DocflowId).ToArray();

			EdoTaskMonitoringNodeBuilder.FillDocflows(
				nodes,
				documents,
				docflows,
				await GetActionRowsAsync(uow, docflowIds, cancellationToken));

			EdoTaskMonitoringNodeBuilder.FillFiscalDocuments(
				nodes,
				await GetFiscalDocumentRowsAsync(uow, taskIds, cancellationToken));
		}

		/// <summary>
		/// Коды задач с незакрытым отклонением. Такие задачи в регистрацию не попадают:
		/// пока отклонение не снято, новое по этой задаче не заводится
		/// </summary>
		private static IQueryable<int> GetTaskIdsWithActiveDeviations(IUnitOfWork uow) =>
			uow.Session.Query<EdoTaskDeviation>()
				.Where(x => x.State == TaskProblemState.Active)
				.Where(x => x.EdoTask != null)
				.Select(x => x.EdoTask.Id);

		/// <summary>
		/// Коды заявок с незакрытым отклонением, зарегистрированным до появления задачи
		/// </summary>
		private static IQueryable<int> GetRequestIdsWithActiveDeviations(IUnitOfWork uow) =>
			uow.Session.Query<EdoTaskDeviation>()
				.Where(x => x.State == TaskProblemState.Active)
				.Where(x => x.EdoTask == null && x.EdoRequest != null)
				.Select(x => x.EdoRequest.Id);

		/// <summary>
		/// Коды задач, по которым есть проблема любого источника
		/// </summary>
		private static IQueryable<int> GetTaskIdsWithActiveProblems(IUnitOfWork uow) =>
			GetTaskIdsWithActiveProblems(uow, null);

		/// <summary>
		/// Коды задач, по которым есть проблема, кроме проблемы указанного источника.
		/// <para>
		/// Проблемой считается и активная запись <see cref="EdoTaskProblem"/>,
		/// и сам статус <see cref="EdoTaskStatus.Problem"/> у задачи: обработчик может
		/// перевести задачу в проблемный статус, не заводя записи о проблеме,
		/// и такая задача мониторингу отклонений тоже не интересна
		/// </para>
		/// </summary>
		private static IQueryable<int> GetTaskIdsWithActiveProblems(IUnitOfWork uow, string exceptSourceName)
		{
			var problemsQuery = uow.Session.Query<EdoTaskProblem>()
				.Where(x => x.State == TaskProblemState.Active);

			if(exceptSourceName != null)
			{
				problemsQuery = problemsQuery.Where(x => x.SourceName != exceptSourceName);
			}

			var taskIdsWithRegisteredProblems = problemsQuery.Select(x => x.EdoTask.Id);

			return uow.Session.Query<EdoTask>()
				.Where(x => x.Status == EdoTaskStatus.Problem || taskIdsWithRegisteredProblems.Contains(x.Id))
				.Select(x => x.Id);
		}

		/// <summary>
		/// Коды задач с проблемой среди указанных, кроме проблемы указанного источника.
		/// Проблема определяется так же, как в <see cref="GetTaskIdsWithActiveProblems(IUnitOfWork, string)"/>:
		/// по активной записи проблемы и по проблемному статусу самой задачи
		/// </summary>
		private static async Task<ICollection<int>> GetTaskIdsWithActiveProblemsAsync(
			IUnitOfWork uow,
			IReadOnlyCollection<int> taskIds,
			string exceptSourceName,
			CancellationToken cancellationToken)
		{
			var taskIdsWithProblems = new HashSet<int>();

			foreach(var idsBatch in SplitToBatches(taskIds))
			{
				var query = uow.Session.Query<EdoTaskProblem>()
					.Where(x => x.State == TaskProblemState.Active)
					.Where(x => idsBatch.Contains(x.EdoTask.Id));

				if(exceptSourceName != null)
				{
					query = query.Where(x => x.SourceName != exceptSourceName);
				}

				taskIdsWithProblems.UnionWith(await query.Select(x => x.EdoTask.Id).ToListAsync(cancellationToken));

				var taskIdsInProblemStatus = await uow.Session.Query<EdoTask>()
					.Where(x => x.Status == EdoTaskStatus.Problem)
					.Where(x => idsBatch.Contains(x.Id))
					.Select(x => x.Id)
					.ToListAsync(cancellationToken);

				taskIdsWithProblems.UnionWith(taskIdsInProblemStatus);
			}

			return taskIdsWithProblems;
		}

		/// <summary>
		/// Заявки ЭДО, породившие задачи, вместе с заказами этих заявок
		/// </summary>
		private static async Task<IReadOnlyCollection<RequestRow>> GetRequestRowsAsync(
			IUnitOfWork uow,
			IReadOnlyCollection<int> taskIds,
			CancellationToken cancellationToken)
		{
			var rows = new List<RequestRow>();

			foreach(var idsBatch in SplitToBatches(taskIds))
			{
				var batch = await uow.Session.Query<FormalEdoRequest>()
					.Where(x => x.Task != null && idsBatch.Contains(x.Task.Id))
					.Select(x => new RequestRow
					{
						TaskId = x.Task.Id,
						RequestId = x.Id,
						OrderId = (int?)x.Order.Id
					})
					.ToListAsync(cancellationToken);

				rows.AddRange(batch);
			}

			return rows;
		}

		/// <summary>
		/// Даты доставки заказов. Читаются отдельным запросом, а не соединением с заявкой:
		/// заявка без заказа выпала бы из выборки вместе со своей задачей
		/// </summary>
		private static async Task<IReadOnlyDictionary<int, DateTime?>> GetOrderDeliveryDatesAsync(
			IUnitOfWork uow,
			IReadOnlyCollection<int> orderIds,
			CancellationToken cancellationToken)
		{
			var deliveryDates = new Dictionary<int, DateTime?>();

			foreach(var idsBatch in SplitToBatches(orderIds))
			{
				var batch = await uow.Session.Query<OrderEntity>()
					.Where(x => idsBatch.Contains(x.Id))
					.Select(x => new OrderRow
					{
						OrderId = x.Id,
						DeliveryDate = x.DeliveryDate
					})
					.ToListAsync(cancellationToken);

				foreach(var row in batch)
				{
					deliveryDates[row.OrderId] = row.DeliveryDate;
				}
			}

			return deliveryDates;
		}

		/// <summary>
		/// Заявки на трансфер незавершенных итераций. Трансферная задача здесь не читается:
		/// на момент создания заявки ее еще нет, а соединение с ней выкинуло бы из выборки
		/// как раз те заявки, по которым трансфер так и не запустился
		/// </summary>
		private static async Task<IReadOnlyCollection<TransferRequestRow>> GetTransferRequestRowsAsync(
			IUnitOfWork uow,
			IReadOnlyCollection<int> taskIds,
			CancellationToken cancellationToken)
		{
			var rows = new List<TransferRequestRow>();

			foreach(var idsBatch in SplitToBatches(taskIds))
			{
				var batch = await uow.Session.Query<TransferEdoRequest>()
					.Where(x => x.Iteration.Status == TransferEdoRequestIterationStatus.InProgress)
					.Where(x => idsBatch.Contains(x.Iteration.OrderEdoTask.Id))
					.Select(x => new TransferRequestRow
					{
						RequestId = x.Id,
						OrderTaskId = x.Iteration.OrderEdoTask.Id,
						IterationTime = x.Iteration.Time
					})
					.ToListAsync(cancellationToken);

				rows.AddRange(batch);
			}

			return rows;
		}

		/// <summary>
		/// Задачи переноса кодов по заявкам на трансфер, у которых задача уже подобрана
		/// </summary>
		private static async Task<IReadOnlyCollection<TransferTaskRow>> GetTransferTaskRowsAsync(
			IUnitOfWork uow,
			IReadOnlyCollection<int> transferRequestIds,
			CancellationToken cancellationToken)
		{
			var rows = new List<TransferTaskRow>();

			foreach(var idsBatch in SplitToBatches(transferRequestIds))
			{
				var batch = await uow.Session.Query<TransferEdoRequest>()
					.Where(x => x.TransferEdoTask != null)
					.Where(x => idsBatch.Contains(x.Id))
					.Select(x => new TransferTaskRow
					{
						RequestId = x.Id,
						Status = x.TransferEdoTask.Status,
						TransferStartTime = x.TransferEdoTask.TransferStartTime
					})
					.ToListAsync(cancellationToken);

				rows.AddRange(batch);
			}

			return rows;
		}

		/// <summary>
		/// Исходящие документы заказов по задачам отправки документов
		/// </summary>
		private static async Task<IReadOnlyCollection<DocumentRow>> GetDocumentRowsAsync(
			IUnitOfWork uow,
			IReadOnlyCollection<int> taskIds,
			CancellationToken cancellationToken)
		{
			var rows = new List<DocumentRow>();

			foreach(var idsBatch in SplitToBatches(taskIds))
			{
				var batch = await uow.Session.Query<OrderEdoDocument>()
					.Where(x => idsBatch.Contains(x.DocumentTaskId))
					.Select(x => new DocumentRow
					{
						DocumentId = x.Id,
						TaskId = x.DocumentTaskId,
						CreationTime = x.CreationTime,
						Status = x.Status
					})
					.ToListAsync(cancellationToken);

				rows.AddRange(batch);
			}

			return rows;
		}

		/// <summary>
		/// Документообороты, заведенные у провайдера ЭДО по исходящим документам.
		/// Общий метод для документов заказов и трансферов: документооборот у них один и тот же
		/// </summary>
		private static async Task<IReadOnlyCollection<DocflowRow>> GetDocflowRowsAsync(
			IUnitOfWork uow,
			IReadOnlyCollection<int> documentIds,
			CancellationToken cancellationToken)
		{
			var rows = new List<DocflowRow>();

			foreach(var idsBatch in SplitToBatches(documentIds))
			{
				var batch = await uow.Session.Query<TaxcomDocflow>()
					.Where(x => idsBatch.Contains(x.EdoDocumentId))
					.Select(x => new DocflowRow
					{
						DocflowId = x.Id,
						DocumentId = x.EdoDocumentId,
						CreationTime = x.CreationTime
					})
					.ToListAsync(cancellationToken);

				rows.AddRange(batch);
			}

			return rows;
		}

		/// <summary>
		/// Действия документооборотов у провайдера ЭДО.
		/// Какое из них считать последним, решает <see cref="EdoTaskMonitoringNodeBuilder"/>
		/// </summary>
		private static async Task<IReadOnlyCollection<ActionRow>> GetActionRowsAsync(
			IUnitOfWork uow,
			IReadOnlyCollection<int> docflowIds,
			CancellationToken cancellationToken)
		{
			var rows = new List<ActionRow>();

			foreach(var idsBatch in SplitToBatches(docflowIds))
			{
				var batch = await uow.Session.Query<TaxcomDocflowAction>()
					.Where(x => x.TaxcomDocflowId != null && idsBatch.Contains(x.TaxcomDocflowId.Value))
					.Select(x => new ActionRow
					{
						ActionId = x.Id,
						DocflowId = x.TaxcomDocflowId.Value,
						Time = x.Time,
						State = x.DocFlowState,
						TraceabilityStatus = x.TrueMarkTraceabilityStatus
					})
					.ToListAsync(cancellationToken);

				rows.AddRange(batch);
			}

			return rows;
		}

		/// <summary>
		/// Фискальные документы задач отправки чеков
		/// </summary>
		private static async Task<IReadOnlyCollection<FiscalDocumentRow>> GetFiscalDocumentRowsAsync(
			IUnitOfWork uow,
			IReadOnlyCollection<int> taskIds,
			CancellationToken cancellationToken)
		{
			var rows = new List<FiscalDocumentRow>();

			foreach(var idsBatch in SplitToBatches(taskIds))
			{
				var batch = await uow.Session.Query<EdoFiscalDocument>()
					.Where(x => x.ReceiptEdoTask != null && idsBatch.Contains(x.ReceiptEdoTask.Id))
					.Select(x => new FiscalDocumentRow
					{
						FiscalDocumentId = x.Id,
						TaskId = x.ReceiptEdoTask.Id,
						CreationTime = x.CreationTime,
						StatusChangeTime = x.StatusChangeTime,
						Stage = x.Stage,
						Status = x.Status,
						FiscalNumber = x.FiscalNumber
					})
					.ToListAsync(cancellationToken);

				rows.AddRange(batch);
			}

			return rows;
		}

		/// <summary>
		/// Коды исходящих документов, документооборот которых завершился действием Succeed.
		/// Общий подзапрос для документов заказов и трансферов
		/// </summary>
		private static IQueryable<int> GetDocumentIdsWithFinishedDocflow(IUnitOfWork uow)
		{
			var finishedDocflowIds = uow.Session.Query<TaxcomDocflowAction>()
				.Where(x => x.TaxcomDocflowId != null)
				.Where(x => x.DocFlowState == EdoDocFlowStatus.Succeed)
				.Select(x => x.TaxcomDocflowId.Value);

			return uow.Session.Query<TaxcomDocflow>()
				.Where(x => finishedDocflowIds.Contains(x.Id))
				.Select(x => x.EdoDocumentId);
		}

		/// <summary>
		/// Коды исходящих документов, по которым ГИС МТ приняла коды и ни одного отказа
		/// по документообороту не приходило. Проверять по ним больше нечего: ни отсутствие
		/// результата, ни отказ ГИС МТ уже не наступят, поэтому они исключаются из выборки —
		/// иначе она росла бы с каждым завершенным документооборотом.
		/// <para>
		/// Документооборот, по которому был хотя бы один отказ, из выборки не убирается:
		/// после принятых кодов может прийти отказ в аннулировании, и его нужно увидеть
		/// </para>
		/// </summary>
		private static IQueryable<int> GetDocumentIdsWithAcceptedTraceability(IUnitOfWork uow)
		{
			var rejectedDocflowIds = uow.Session.Query<TaxcomDocflowAction>()
				.Where(x => x.TaxcomDocflowId != null)
				.Where(x => x.TrueMarkTraceabilityStatus != null)
				.Where(x => TrueMarkTraceabilityStatuses.Rejected.Contains(x.TrueMarkTraceabilityStatus.Value))
				.Select(x => x.TaxcomDocflowId.Value);

			var acceptedDocflowIds = uow.Session.Query<TaxcomDocflowAction>()
				.Where(x => x.TaxcomDocflowId != null)
				.Where(x => x.TrueMarkTraceabilityStatus != null)
				.Where(x => !TrueMarkTraceabilityStatuses.Rejected.Contains(x.TrueMarkTraceabilityStatus.Value))
				.Where(x => !rejectedDocflowIds.Contains(x.TaxcomDocflowId.Value))
				.Select(x => x.TaxcomDocflowId.Value);

			return uow.Session.Query<TaxcomDocflow>()
				.Where(x => acceptedDocflowIds.Contains(x.Id))
				.Select(x => x.EdoDocumentId);
		}

		/// <summary>
		/// Коды задач по заказам, доставленным раньше указанной даты.
		/// Задачи заказов без даты доставки в подзапрос не попадают: их отсекает
		/// валидатор по времени создания задачи
		/// </summary>
		private static IQueryable<int> GetTaskIdsDeliveredBefore(IUnitOfWork uow, DateTime deliveredFrom) =>
			uow.Session.Query<FormalEdoRequest>()
				.Where(x => x.Task != null)
				.Where(x => x.Order.DeliveryDate != null && x.Order.DeliveryDate < deliveredFrom)
				.Select(x => x.Task.Id);

		/// <summary>
		/// Дочитывает состояние задач трансфера: проблемы и документооборот.
		/// Документооборот у трансфера тот же, что и у документов заказа,
		/// поэтому строки собираются теми же методами
		/// </summary>
		private async Task FillTransferNodesAsync(
			IUnitOfWork uow,
			IReadOnlyCollection<EdoTransferTaskMonitoringNode> nodes,
			CancellationToken cancellationToken)
		{
			if(!nodes.Any())
			{
				return;
			}

			var taskIds = nodes.Select(x => x.EdoTaskId).ToArray();

			EdoTaskMonitoringNodeBuilder.FillTransferProblems(
				nodes,
				await GetCodesNotMovedProblemTimesAsync(uow, taskIds, cancellationToken),
				await GetTaskIdsWithActiveProblemsAsync(uow, taskIds, _codesNotMovedProblemSourceName, cancellationToken));

			var documents = await GetTransferDocumentRowsAsync(uow, taskIds, cancellationToken);
			var documentIds = documents.Select(x => x.DocumentId).ToArray();
			var docflows = await GetDocflowRowsAsync(uow, documentIds, cancellationToken);
			var docflowIds = docflows.Select(x => x.DocflowId).ToArray();

			EdoTaskMonitoringNodeBuilder.FillDocflows(
				nodes,
				documents,
				docflows,
				await GetActionRowsAsync(uow, docflowIds, cancellationToken));
		}

		/// <summary>
		/// Время регистрации незакрытых проблем ожидания перемещения кодов в ГИС МТ
		/// </summary>
		private static async Task<IReadOnlyDictionary<int, DateTime>> GetCodesNotMovedProblemTimesAsync(
			IUnitOfWork uow,
			IReadOnlyCollection<int> taskIds,
			CancellationToken cancellationToken)
		{
			var problemTimes = new Dictionary<int, DateTime>();

			foreach(var idsBatch in SplitToBatches(taskIds))
			{
				var batch = await uow.Session.Query<EdoTaskProblem>()
					.Where(x => x.State == TaskProblemState.Active)
					.Where(x => x.SourceName == _codesNotMovedProblemSourceName)
					.Where(x => idsBatch.Contains(x.EdoTask.Id))
					.Select(x => new ProblemRow
					{
						TaskId = x.EdoTask.Id,
						CreationTime = x.CreationTime
					})
					.ToListAsync(cancellationToken);

				foreach(var row in batch)
				{
					if(!problemTimes.TryGetValue(row.TaskId, out var registered) || row.CreationTime < registered)
					{
						problemTimes[row.TaskId] = row.CreationTime;
					}
				}
			}

			return problemTimes;
		}

		/// <summary>
		/// Исходящие документы трансфера по задачам трансфера.
		/// Строки складываются в тот же тип, что и документы заказов:
		/// дальше документооборот у них общий
		/// </summary>
		private static async Task<IReadOnlyCollection<DocumentRow>> GetTransferDocumentRowsAsync(
			IUnitOfWork uow,
			IReadOnlyCollection<int> taskIds,
			CancellationToken cancellationToken)
		{
			var rows = new List<DocumentRow>();

			foreach(var idsBatch in SplitToBatches(taskIds))
			{
				var batch = await uow.Session.Query<TransferEdoDocument>()
					.Where(x => idsBatch.Contains(x.TransferTaskId))
					.Select(x => new DocumentRow
					{
						DocumentId = x.Id,
						TaskId = x.TransferTaskId,
						CreationTime = x.CreationTime,
						Status = x.Status
					})
					.ToListAsync(cancellationToken);

				rows.AddRange(batch);
			}

			return rows;
		}

		/// <summary>
		/// Разбивает список идентификаторов на пачки,
		/// чтобы не упереться в ограничение количества параметров запроса
		/// </summary>
		private static IEnumerable<int[]> SplitToBatches(IReadOnlyCollection<int> ids)
		{
			for(var skipped = 0; skipped < ids.Count; skipped += _idsBatchSize)
			{
				yield return ids.Skip(skipped).Take(_idsBatchSize).ToArray();
			}
		}
	}
}
