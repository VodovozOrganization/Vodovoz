using NHibernate;
using NHibernate.Criterion;
using NHibernate.Linq;
using NHibernate.SqlCommand;
using NHibernate.Transform;
using NHibernate.Type;
using NLog;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Data.NHibernate.Extensions;
using Vodovoz.Core.Data.Repositories;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Documents;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Core.Domain.Organizations;

namespace Vodovoz.Core.Data.NHibernate.Repositories.Edo
{
	public class EdoRepository : IEdoRepository
	{
		private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
		private readonly IUnitOfWorkFactory _uowFactory;

		public EdoRepository(IUnitOfWorkFactory uowFactory)
		{
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
		}

		public async Task<IEnumerable<OrganizationEntity>> GetEdoOrganizationsAsync(CancellationToken cancellationToken)
		{
			using(var uow = _uowFactory.CreateWithoutRoot())
			{
				var result = await uow.Session.QueryOver<OrganizationEntity>()
					.Where(x => x.OrganizationEdoType != OrganizationEdoType.WithoutEdo)
					.ListAsync(cancellationToken);

				return result;
			}
		}

		public async Task<IEnumerable<GtinEntity>> GetGtinsAsync(CancellationToken cancellationToken)
		{
			using(var uow = _uowFactory.CreateWithoutRoot())
			{
				var result = await uow.Session.QueryOver<GtinEntity>()
					.OrderBy(g => g.Priority).Asc
					.ListAsync(cancellationToken);

				return result;
			}
		}

		public async Task<IEnumerable<GroupGtinEntity>> GetGroupGtinsAsync(CancellationToken cancellationToken)
		{
			using(var uow = _uowFactory.CreateWithoutRoot())
			{
				var result = await uow.Session.QueryOver<GroupGtinEntity>()
					.ListAsync(cancellationToken);

				return result;
			}
		}

		public async Task<bool> HasReceiptOnSumToday(decimal sum, CancellationToken cancellationToken)
		{
			using(var uow = _uowFactory.CreateWithoutRoot())
			{
				FiscalMoneyPosition fiscalMoneyPositionAlias = null;
				EdoFiscalDocument fiscalDocumentAlias = null;

				var query = uow.Session.QueryOver(() => fiscalDocumentAlias)
					.JoinAlias(() => fiscalDocumentAlias.MoneyPositions, () => fiscalMoneyPositionAlias, JoinType.LeftOuterJoin)
					.Where(() => fiscalMoneyPositionAlias.Sum == sum)
					.Where(() => fiscalDocumentAlias.Index == 0)
					.WhereRestrictionOn(() => fiscalDocumentAlias.Status).IsIn(new[] { FiscalDocumentStatus.Printed, FiscalDocumentStatus.WaitForCallback, FiscalDocumentStatus.Completed })
					.Where(Restrictions.Eq(
						Projections.SqlFunction("DATE", NHibernateUtil.Date, Projections.Property(() => fiscalDocumentAlias.CheckoutTime)),
						DateTime.Today)
					)
					.ToRowCountQuery();

				var count = await query.SingleOrDefaultAsync<int>(cancellationToken);
				return count > 0;
			}
		}

		public IEnumerable<OrderEdoTask> GetEdoTaskByOrder(
			IUnitOfWork uow,
			int orderId
			)
		{
			FormalEdoRequest edoRequestAlias = null;
			OrderEdoTask orderEdoTaskAlias = null;

			var edoTasks = uow.Session.QueryOver(() => orderEdoTaskAlias)
				.Left.JoinAlias(() => orderEdoTaskAlias.FormalEdoRequest, () => edoRequestAlias)
				.Where(() => edoRequestAlias.Order.Id == orderId)
				.Where(() => edoRequestAlias.DocumentType == EdoDocumentType.UPD)
				.List()
				;
			return edoTasks;
		}

		public IEnumerable<OrderEdoTaskNode> GetEdoTasksForOrder(IUnitOfWork uow, int orderId)
		{
			FormalEdoRequest edoRequestAlias = null;
			OrderEdoTask orderEdoTaskAlias = null;
			OrderEdoTaskNode resultAlias = null;

			var edoTasks = uow.Session.QueryOver(() => orderEdoTaskAlias)
				.Left.JoinAlias(() => orderEdoTaskAlias.FormalEdoRequest, () => edoRequestAlias)
				.Where(() => edoRequestAlias.Order.Id == orderId)
				.SelectList(list => list
					.Select(() => edoRequestAlias.Time).WithAlias(() => resultAlias.RequestTime)
					.Select(() => edoRequestAlias.Source).WithAlias(() => resultAlias.RequestSource)
					.Select(() => orderEdoTaskAlias.Id).WithAlias(() => resultAlias.EdoTaskId)
					.Select(() => orderEdoTaskAlias.Status).WithAlias(() => resultAlias.EdoTaskStatus)
					.Select(Projections.SqlProjection(
						"{alias}.`type` as EdoTaskType",
						new[] { "EdoTaskType" },
						new IType[] { NHibernateUtil.String }
					)).WithAlias(() => resultAlias.EdoTaskTypeName)
				)
				.TransformUsing(Transformers.AliasToBean<OrderEdoTaskNode>())
				.List<OrderEdoTaskNode>()
				;
			return edoTasks;
		}

		public IEnumerable<EdoDocflowForOrderNode> GetEdoDocflowsForOrder(IUnitOfWork uow, int orderId)
		{
			var sql = @"
select
	eod.document_task_id as :document_task_id,
	eod.transfer_task_id as :transfer_task_id,
	eod.edo_type as :edo_type,
	td.id as :taxcom_document_id,
	td.docflow_id as :taxcom_docflow_id,
	(
		SELECT tda.state 
		FROM taxcom_docflow_actions tda 
		WHERE tda.taxcom_docflow_id = td.id
		ORDER BY tda.`time` DESC
		LIMIT 1
	) as :last_taxcom_state
from taxcom_docflows td
left join edo_outgoing_documents eod on eod.id = td.edo_document_id
left join edo_customer_requests ecr on ecr.order_task_id = eod.document_task_id
where eod.`type` = 'Order' and ecr.order_id = :order_id
union all
select
	eod.document_task_id as :document_task_id,
	eod.transfer_task_id as :transfer_task_id,
	eod.edo_type as :edo_type,
	td.id as :taxcom_document_id,
	td.docflow_id as :taxcom_docflow_id,
	(
		SELECT tda.state 
		FROM taxcom_docflow_actions tda 
		WHERE tda.taxcom_docflow_id = td.id
		ORDER BY tda.`time` DESC
		LIMIT 1
	) as :last_taxcom_state
from taxcom_docflows td
left join edo_outgoing_documents eod on eod.id = td.edo_document_id
left join edo_transfer_requests etr on etr.transfer_edo_task_id = eod.transfer_task_id
left join edo_transfer_request_iterations etri on etri.id = etr.iteration_id
left join edo_customer_requests ecr on ecr.order_task_id = etri.order_edo_task_id
where eod.`type` = 'Transfer' and ecr.order_id = :order_id
;
";

			var query = uow.Session.CreateSQLQuery(sql)
					.MapParametersToNode<EdoDocflowForOrderNode>()
					.Map("document_task_id", x => x.OrderTaskId, NHibernateUtil.Int32)
					.Map("transfer_task_id", x => x.TransferTaskId, NHibernateUtil.Int32)
					.Map("edo_type", x => x.EdoType, new EnumStringType<EdoType>())
					.Map("taxcom_document_id", x => x.TaxcomDocumentId, NHibernateUtil.Int32)
					.Map("taxcom_docflow_id", x => x.TaxcomDocflowId, NHibernateUtil.String)
					.Map("last_taxcom_state", x => x.TaxcomDocflowStatus, new EnumStringType<EdoDocFlowStatus>())
					.SetResultTransformer();

			query.SetParameter("order_id", orderId);
			var result = query.List<EdoDocflowForOrderNode>();

			return result;
		}

		public IEnumerable<OrderEdoDocument> GetOrderEdoDocumentsByOrderId(IUnitOfWork uow, int orderId)
		{
			var edoDocuments = from doc in uow.Session.Query<OrderEdoDocument>()
							   join task in uow.Session.Query<DocumentEdoTask>()
								   on doc.DocumentTaskId equals task.Id
							   join request in uow.Session.Query<FormalEdoRequest>()
								   on task.Id equals request.Task.Id
							   where request.Order.Id == orderId
							   select doc;

			return edoDocuments.ToList();
		}

		public async Task<IList<TimedOutOrderDocumentTaskNode>> GetTimedOutOrderDocumentTasks(
			IUnitOfWork uow,
			int timeoutDays,
			CancellationToken cancellationToken)
		{
			var thresholdDate = DateTime.Today.AddDays(-timeoutDays);

			var documentOrderTasks =
				from task in uow.Session.Query<DocumentEdoTask>()
					.Fetch(t => t.FormalEdoRequest)
				join orderEdoDocument in uow.Session.Query<OrderEdoDocument>()
					on task.Id equals orderEdoDocument.DocumentTaskId
				join taxcomDocflow in uow.Session.Query<TaxcomDocflow>()
					on orderEdoDocument.Id equals taxcomDocflow.EdoDocumentId
				join formalEdoRequest in uow.Session.Query<FormalEdoRequest>()
					on task.FormalEdoRequest.Id equals formalEdoRequest.Id
				join order in uow.Session.Query<OrderEntity>()
					on formalEdoRequest.Order.Id equals order.Id
				join client in uow.Session.Query<CounterpartyEntity>()
					on order.Client.Id equals client.Id
				join contract in uow.Session.Query<CounterpartyContractEntity>()
					on order.Contract.Id equals contract.Id
				join edoAccount in uow.Session.Query<CounterpartyEdoAccountEntity>()
					on new
					{
						ClientId = client.Id,
						OrganizationId = contract.Organization.Id,
						IsDefault = true
					}
					equals new
					{
						ClientId = edoAccount.Counterparty.Id,
						OrganizationId = edoAccount.OrganizationId ?? 0,
						IsDefault = edoAccount.IsDefault
					}
				join wer in uow.Session.Query<WithdrawalEdoRequest>()
					on order.Id equals wer.Order.Id into withdrawalEdoRequests
				from withdrawalEdoRequest in withdrawalEdoRequests.DefaultIfEmpty()

				let taskItemCodesCount =
					(int?)(from taskItem in uow.Session.Query<EdoTaskItem>()
						   where
						   taskItem.CustomerEdoTask.Id == task.Id
						   && taskItem.ProductCode != null
						   select taskItem.Id)
						   .Count() ?? 0

				where
					task.Status == EdoTaskStatus.InProgress
					&& orderEdoDocument.CreationTime < thresholdDate
					&& orderEdoDocument.Status == EdoDocumentStatus.InProgress
					&& orderEdoDocument.AcceptTime == null
					&& taxcomDocflow.IsReceived
					&& order.PaymentType == Vodovoz.Domain.Client.PaymentType.Cashless
					&& client.PersonType == PersonType.legal
					&& client.ReasonForLeaving == ReasonForLeaving.ForOwnNeeds
					&& edoAccount.ConsentForEdoStatus == ConsentForEdoStatus.Agree
					&& withdrawalEdoRequest.Id == null
					&& taskItemCodesCount > 0

				select new TimedOutOrderDocumentTaskNode
				{
					ClientId = client.Id,
					ClientInn = client.INN,
					RegistrationInChestnyZnakStatus = client.RegistrationInChestnyZnakStatus,
					Order = order,
					Task = task
				};

			var orderTasks = await documentOrderTasks.ToListAsync(cancellationToken);

			return orderTasks;
		}

		public async Task<IList<TimedOutDocFlowGrouppedNode>> GetTimedOutDocFlows(
			IUnitOfWork unitOfWork,
			int timeoutDays,
			CancellationToken cancellationToken)
		{
			var thresholdDate = DateTime.Today.AddDays(-timeoutDays);

			var timedOutDocFlowNodes =
				from task in unitOfWork.Session.Query<DocumentEdoTask>()
				join orderEdoDocument in unitOfWork.Session.Query<OrderEdoDocument>()
					on task.Id equals orderEdoDocument.DocumentTaskId
				join taxcomDocflow in unitOfWork.Session.Query<TaxcomDocflow>()
					on orderEdoDocument.Id equals taxcomDocflow.EdoDocumentId
				join formalEdoRequest in unitOfWork.Session.Query<FormalEdoRequest>()
					on task.FormalEdoRequest.Id equals formalEdoRequest.Id
				join order in unitOfWork.Session.Query<OrderEntity>()
					on formalEdoRequest.Order.Id equals order.Id
				join client in unitOfWork.Session.Query<CounterpartyEntity>()
					on order.Client.Id equals client.Id
				join contract in unitOfWork.Session.Query<CounterpartyContractEntity>()
					on order.Contract.Id equals contract.Id
				join taxcomSettings in unitOfWork.Session.Query<TaxcomEdoSettings>()
					on contract.Organization.Id equals taxcomSettings.OrganizationId
				join organization in unitOfWork.Session.Query<OrganizationEntity>()
					on contract.Organization.Id equals organization.Id
				join edoAccount in unitOfWork.Session.Query<CounterpartyEdoAccountEntity>()
					on new
					{
						ClientId = client.Id,
						OrganizationId = contract.Organization.Id,
						IsDefault = true
					}
					equals new
					{
						ClientId = edoAccount.Counterparty.Id,
						OrganizationId = edoAccount.OrganizationId ?? 0,
						IsDefault = edoAccount.IsDefault
					}
				join wer in unitOfWork.Session.Query<WithdrawalEdoRequest>()
					on order.Id equals wer.Order.Id into withdrawalEdoRequests
				from withdrawalEdoRequest in withdrawalEdoRequests.DefaultIfEmpty()

				let updNum =
					(from docCounter in unitOfWork.Session.Query<DocumentOrganizationCounter>()
					 where docCounter.Order.Id == order.Id
						 && docCounter.Organization.Id == organization.Id
					 orderby docCounter.Id descending
					 select docCounter.DocumentNumber)
					.FirstOrDefault()

				where
					task.Status == EdoTaskStatus.InProgress
					&& taxcomDocflow.CreationTime < thresholdDate && taxcomDocflow.CreationTime >= thresholdDate.AddDays(-1)
					&& orderEdoDocument.Status == EdoDocumentStatus.InProgress
					&& orderEdoDocument.AcceptTime == null
					&& taxcomDocflow.IsReceived
					&& order.PaymentType == Vodovoz.Domain.Client.PaymentType.Cashless
					&& client.PersonType == PersonType.legal
					&& client.ReasonForLeaving == ReasonForLeaving.ForOwnNeeds
					&& edoAccount.ConsentForEdoStatus == ConsentForEdoStatus.Agree
					&& withdrawalEdoRequest == null
					&& !taxcomDocflow.IsReminderToAcceptUpdEmailSent

				select new TimedOutDocFlowRow
				{
					Client = client,
					Organization = organization,
					Order = order,
					TaxcomDocflow = taxcomDocflow,
					OurEdoAccount = taxcomSettings.EdoAccount,
					UpdNum = updNum
				};

			var result = (await timedOutDocFlowNodes.ToListAsync(cancellationToken))
				.GroupBy(x => new { ClientId = x.Client.Id, OrganizationId = x.Organization.Id })
				.Select(g => new TimedOutDocFlowGrouppedNode
				{
					Client = g.First().Client,
					Organization = g.First().Organization,
					Documents = g.Distinct().Select(x => new TimedOutDocFlowDocumentNode
					{
						Order = x.Order,
						TaxcomDocflow = x.TaxcomDocflow,
						OurEdoAccount = x.OurEdoAccount,
						UpdNum = x.UpdNum
					}).ToList()
				})
				.ToList();

			return result;
		}

		public async Task<IList<int>> GetExistingWithdrawalEdoRequestOrders(IUnitOfWork uow, IEnumerable<int> orderIds, CancellationToken cancellationToken)
		{
			var existingOrders = await uow.Session.Query<WithdrawalEdoRequest>()
				.Where(wer => orderIds.Contains(wer.Order.Id))
				.Select(wer => wer.Order.Id)
				.ToListAsync(cancellationToken);
			return existingOrders;
		}

		public async Task<IList<T>> GetProblemEdoTasks<T>(
			IUnitOfWork uow,
			string problemSourceName,
			DateTime minCreationTime,
			CancellationToken cancellationToken,
			DateTime? maxCreationTime = null
			)
			where T : OrderEdoTask
		{
			var tasksIdsQuery =
				from problem in uow.Session.Query<EdoTaskProblem>()
				join edoTask in uow.Session.Query<T>() on problem.EdoTask.Id equals edoTask.Id
				join edoRequest in uow.Session.Query<FormalEdoRequest>() on edoTask.FormalEdoRequest.Id equals edoRequest.Id
				where
					problem.SourceName == problemSourceName
					&& problem.State == TaskProblemState.Active
					&& edoTask.CreationTime >= minCreationTime
					&& (maxCreationTime == null || edoTask.CreationTime <= maxCreationTime)
				select edoTask.Id;

			var taskIds = await tasksIdsQuery.Distinct().ToListAsync(cancellationToken);

			if(!taskIds.Any())
			{
				return new List<T>();
			}

			var tasks = await uow.Session.Query<T>()
				.Where(t => taskIds.Contains(t.Id))
				.Fetch(t => t.FormalEdoRequest)
				.ThenFetch(r => r.Order)
				.ThenFetch(o => o.Client)
				.ToListAsync(cancellationToken);

			return tasks;
		}

		public async Task<IList<int>> GetSendErrorFiscalDocumentsEdoTasksIds(
			IUnitOfWork uow,
			DateTime minFiscalDocumentCreationTime,
			CancellationToken cancellationToken)
		{
			var query =
				from fiscalDocument in uow.Session.Query<EdoFiscalDocument>()
				join receiptEdoTask in uow.Session.Query<ReceiptEdoTask>()
					on fiscalDocument.ReceiptEdoTask.Id equals receiptEdoTask.Id
				where fiscalDocument.CreationTime >= minFiscalDocumentCreationTime
					&& fiscalDocument.Status == FiscalDocumentStatus.SendError
					&& receiptEdoTask.ReceiptStatus == EdoReceiptStatus.Sending
				select fiscalDocument.ReceiptEdoTask.Id;

			return await query.Distinct().ToListAsync(cancellationToken);
		}

		public async Task<IList<OrderEdoTask>> GetProblemEdoTasks(
			IUnitOfWork uow,
			string problemSourceName,
			DateTime minCreationTime,
			CancellationToken cancellationToken)
		{
			var query =
				from problem in uow.Session.Query<EdoTaskProblem>()
				where problem.SourceName == problemSourceName
					&& problem.State == TaskProblemState.Active
					&& problem.EdoTask.CreationTime >= minCreationTime
					&& problem.EdoTask is OrderEdoTask
				select (OrderEdoTask)problem.EdoTask;

			return await query
				.Distinct()
				.ToListAsync(cancellationToken);
		}

		public IEnumerable<EdoInOrderDocumentNode> GetEdoInOrderDocuments(IUnitOfWork uow, int orderId)
		{
			var stopwatch = Stopwatch.StartNew();
			var sql = @"
select
	ecr.`time` as :request_time,
	ecr.id as :request_id,
	ecr.source as :request_source,
	null as :order_document_type,
	et.id as :task_id,
	et.`type` as :task_type,
	et.status as :task_status,
	document_task_stage as :task_upd_stage,
	receipt_status as :task_receipt_stage,
	tender_task_stage as :task_tender_stage,
	(select count(*) from true_mark_product_codes tmpc where tmpc.customer_request_id = ecr.id) as :codes_count
from edo_customer_requests ecr
left join edo_tasks et on et.id = ecr.order_task_id
where ecr.order_id = :order_id
	and et.`type` in ('Document', 'Receipt', 'Tender', 'InformalOrderDocument', 'SaveCode', 'Withdrawal')
union all
select
	eir.`time` as :request_time,
	eir.id as :request_id,
	eir.source as :request_source,
	eir.order_document_type as :order_document_type,
	et.id as :task_id,
	et.`type` as :task_type,
	et.status as :task_status,
	null as :task_upd_stage,
	null as :task_receipt_stage,
	null as :task_tender_stage,
	null as :codes_count
from edo_informal_requests eir
left join edo_tasks et on et.id = eir.order_document_task_id 
where eir.order_id = :order_id
	and et.`type` in ('Document', 'Receipt', 'Tender', 'InformalOrderDocument', 'SaveCode', 'Withdrawal')
;
";

			var query = uow.Session.CreateSQLQuery(sql)
				.MapParametersToNode<EdoInOrderDocumentNode>()
				.Map("request_time", x => x.RequestTime, NHibernateUtil.DateTime)
				.Map("request_id", x => x.RequestId, NHibernateUtil.Int32)
				.Map("request_source", x => x.RequestSource, new EnumStringType<EdoRequestSource>())
				.Map("order_document_type", x => x.InformalOrderDocumentType, new EnumStringType<OrderDocumentType>())
				.Map("task_id", x => x.TaskId, NHibernateUtil.Int32)
				.Map("task_type", x => x.TaskType, new EnumStringType<EdoTaskType>())
				.Map("task_status", x => x.TaskStatus, new EnumStringType<EdoTaskStatus>())
				.Map("task_upd_stage", x => x.TaskUpdStage, new EnumStringType<DocumentEdoTaskStage>())
				.Map("task_receipt_stage", x => x.TaskReceiptStage, new EnumStringType<EdoReceiptStatus>())
				.Map("task_tender_stage", x => x.TaskTenderStage, new EnumStringType<TenderEdoTaskStage>())
				.Map("codes_count", x => x.CodesQuantity, NHibernateUtil.Int32)
				.SetResultTransformer();

			query.SetParameter("order_id", orderId);
			var result = query.List<EdoInOrderDocumentNode>();
			_logger.Info(
				"ЭДО заказа {OrderId}: EdoRepository.GetEdoInOrderDocuments, строк {Count}: {Elapsed}",
				orderId,
				result.Count,
				stopwatch.Elapsed);

			return result;
		}

		public IEnumerable<EdoInOrderProblemNode> GetEdoProblemsForOrder(IUnitOfWork uow, int orderId)
		{
			var totalStopwatch = Stopwatch.StartNew();
			var stepStopwatch = Stopwatch.StartNew();
			OrderEdoTask orderEdoTaskAlias = null;
			FormalEdoRequest edoRequestAlias = null;
			EdoTaskProblem edoTaskProblemAlias = null;
			EdoTaskProblemCustomSourceEntity customProblemDescriptionSourceAlias = null;
			EdoTaskProblemValidatorSourceEntity validatorDescriptionSourceAlias = null;
			EdoTaskProblemExceptionSourceEntity exceptionDescriptionSourceAlias = null;
			TransferEdoRequestIteration transferIterationAlias = null;
			TransferEdoRequest transferRequestAlias = null;
			TransferEdoTask transferEdoTaskAlias = null;
			EdoInOrderProblemNode resultAlias = null;

			var edoTasksProblems = uow.Session.QueryOver(() => edoTaskProblemAlias)
				.Left.JoinAlias(() => edoTaskProblemAlias.EdoTask, () => orderEdoTaskAlias)
				.Left.JoinAlias(() => orderEdoTaskAlias.FormalEdoRequest, () => edoRequestAlias)
				.JoinEntityAlias(
					() => customProblemDescriptionSourceAlias,
					() => customProblemDescriptionSourceAlias.Name == edoTaskProblemAlias.SourceName,
					JoinType.LeftOuterJoin
				)
				.JoinEntityAlias(
					() => validatorDescriptionSourceAlias,
					() => validatorDescriptionSourceAlias.Name == edoTaskProblemAlias.SourceName,
					JoinType.LeftOuterJoin
				)
				.JoinEntityAlias(
					() => exceptionDescriptionSourceAlias,
					() => exceptionDescriptionSourceAlias.Name == edoTaskProblemAlias.SourceName,
					JoinType.LeftOuterJoin
				)
				.Where(() => edoRequestAlias.Order.Id == orderId)
				.SelectList(list => list
					.SelectGroup(() => edoTaskProblemAlias.Id)
					.Select(() => orderEdoTaskAlias.Id).WithAlias(() => resultAlias.OrderTaskId)
					.Select(() => edoTaskProblemAlias.CreationTime).WithAlias(() => resultAlias.Time)
					.Select(() => edoTaskProblemAlias.State).WithAlias(() => resultAlias.State)

					.Select(
						Projections.Conditional(
							Restrictions.IsNotNull(
								Projections.Property(() => exceptionDescriptionSourceAlias.Name)
							),
							Projections.Constant("Исключение"),
							Projections.SqlFunction(
								"coalesce",
								NHibernateUtil.String,
								Projections.Property(() => customProblemDescriptionSourceAlias.Message),
								Projections.Property(() => validatorDescriptionSourceAlias.Message)
							)
						)
					).WithAlias(() => resultAlias.Message)

					.Select(
						Projections.SqlFunction(
							"coalesce",
							NHibernateUtil.String,
							Projections.Property(() => customProblemDescriptionSourceAlias.Description),
							Projections.Property(() => validatorDescriptionSourceAlias.Description),
							Projections.Property(() => exceptionDescriptionSourceAlias.Description)
						)
					).WithAlias(() => resultAlias.Description)

					.Select(
						Projections.SqlFunction(
							"coalesce",
							NHibernateUtil.String,
							Projections.Property(() => customProblemDescriptionSourceAlias.Recommendation),
							Projections.Property(() => validatorDescriptionSourceAlias.Recommendation),
							Projections.Property(() => exceptionDescriptionSourceAlias.Recommendation)
						)
					).WithAlias(() => resultAlias.Recommendation)
				)
				.TransformUsing(Transformers.AliasToBean<EdoInOrderProblemNode>())
				.List<EdoInOrderProblemNode>();
			_logger.Info(
				"ЭДО заказа {OrderId}: загрузка проблем основных ЭДО-задач, строк {Count}: {Elapsed}",
				orderId,
				edoTasksProblems.Count,
				stepStopwatch.Elapsed);


			stepStopwatch.Restart();
			var edoTransferTasksProblems = uow.Session.QueryOver(() => edoTaskProblemAlias)
				.Left.JoinAlias(() => edoTaskProblemAlias.EdoTask, () => transferEdoTaskAlias)
				.JoinEntityAlias(
					() => transferRequestAlias,
					() => transferEdoTaskAlias.Id == transferRequestAlias.TransferEdoTask.Id,
					JoinType.LeftOuterJoin
				)
				.JoinEntityAlias(
					() => transferIterationAlias,
					() => transferIterationAlias.Id == transferRequestAlias.Iteration.Id,
					JoinType.LeftOuterJoin
				)
				.JoinEntityAlias(
					() => orderEdoTaskAlias,
					() => orderEdoTaskAlias.Id == transferIterationAlias.OrderEdoTask.Id,
					JoinType.LeftOuterJoin
				)
				.Left.JoinAlias(() => orderEdoTaskAlias.FormalEdoRequest, () => edoRequestAlias)
				.JoinEntityAlias(
					() => customProblemDescriptionSourceAlias,
					() => customProblemDescriptionSourceAlias.Name == edoTaskProblemAlias.SourceName,
					JoinType.LeftOuterJoin
				)
				.JoinEntityAlias(
					() => validatorDescriptionSourceAlias,
					() => validatorDescriptionSourceAlias.Name == edoTaskProblemAlias.SourceName,
					JoinType.LeftOuterJoin
				)
				.JoinEntityAlias(
					() => exceptionDescriptionSourceAlias,
					() => exceptionDescriptionSourceAlias.Name == edoTaskProblemAlias.SourceName,
					JoinType.LeftOuterJoin
				)
				.Where(() => edoRequestAlias.Order.Id == orderId)
				.SelectList(list => list
					.SelectGroup(() => edoTaskProblemAlias.Id)
					.Select(() => transferEdoTaskAlias.Id).WithAlias(() => resultAlias.TransferTaskId)
					.Select(() => edoTaskProblemAlias.CreationTime).WithAlias(() => resultAlias.Time)
					.Select(() => edoTaskProblemAlias.State).WithAlias(() => resultAlias.State)

					.Select(
						Projections.Conditional(
							Restrictions.IsNotNull(
								Projections.Property(() => exceptionDescriptionSourceAlias.Name)
							),
							Projections.Constant("Исключение"),
							Projections.SqlFunction(
								"coalesce",
								NHibernateUtil.String,
								Projections.Property(() => customProblemDescriptionSourceAlias.Message),
								Projections.Property(() => validatorDescriptionSourceAlias.Message)
							)
						)
					).WithAlias(() => resultAlias.Message)

					.Select(
						Projections.SqlFunction(
							"coalesce",
							NHibernateUtil.String,
							Projections.Property(() => customProblemDescriptionSourceAlias.Description),
							Projections.Property(() => validatorDescriptionSourceAlias.Description),
							Projections.Property(() => exceptionDescriptionSourceAlias.Description)
						)
					).WithAlias(() => resultAlias.Description)

					.Select(
						Projections.SqlFunction(
							"coalesce",
							NHibernateUtil.String,
							Projections.Property(() => customProblemDescriptionSourceAlias.Recommendation),
							Projections.Property(() => validatorDescriptionSourceAlias.Recommendation),
							Projections.Property(() => exceptionDescriptionSourceAlias.Recommendation)
						)
					).WithAlias(() => resultAlias.Recommendation)
				)
				.TransformUsing(Transformers.AliasToBean<EdoInOrderProblemNode>())
				.List<EdoInOrderProblemNode>();
			_logger.Info(
				"ЭДО заказа {OrderId}: загрузка проблем трансферных ЭДО-задач, строк {Count}: {Elapsed}",
				orderId,
				edoTransferTasksProblems.Count,
				stepStopwatch.Elapsed);

			var allProblems = edoTasksProblems
				.Union(edoTransferTasksProblems)
				.GroupBy(x => x.TaskProblemId)
				.ToDictionary(x => x.Key, x => x.First())
				;

			var problemIds = allProblems.Keys.ToList();
			if(!problemIds.Any())
			{
				_logger.Info(
					"ЭДО заказа {OrderId}: GetEdoProblemsForOrder без элементов проблем, всего {Count}: {Elapsed}",
					orderId,
					allProblems.Count,
					totalStopwatch.Elapsed);
				return allProblems.Values;
			}

			var sql = @"
select
	etpi.edo_task_problem_id as problem_id,
	IFNULL(result_tmic.raw_code, source_tmic.raw_code) as problem_item
from edo_task_problem_items etpi 
left join edo_order_task_items eoti on eoti.id = etpi.order_edo_task_item_id  
left join true_mark_product_codes tmpc on tmpc.id = eoti.product_code_id
left join true_mark_identification_code source_tmic on source_tmic.id = tmpc.source_code_id
left join true_mark_identification_code result_tmic on result_tmic.id = tmpc.result_code_id
where etpi.edo_task_problem_id in (:problem_ids)
group by etpi.id
union all
select
	etpci.edo_task_problem_id as problem_id,
	CAST(etpci.gtin_id AS CHAR) as problem_item
from edo_task_problem_custom_items etpci
where etpci.`type` = 'Gtin'
and etpci.edo_task_problem_id in (:problem_ids)
; 
";

			var query = uow.Session.CreateSQLQuery(sql)
				.AddScalar("problem_id", NHibernateUtil.Int32)
				.AddScalar("problem_item", NHibernateUtil.String)
				.SetParameterList("problem_ids", problemIds);
			stepStopwatch.Restart();
			var problemItems = query.List<object[]>()
				.Select(x => new
				{
					ProblemId = (int)x[0],
					ProblemItem = (string)x[1]
				});
			_logger.Info(
				"ЭДО заказа {OrderId}: загрузка элементов проблем, problemIds {ProblemIdsCount}: {Elapsed}",
				orderId,
				problemIds.Count,
				stepStopwatch.Elapsed);

			;
			foreach(var problemItemsGroup in problemItems.GroupBy(x => x.ProblemId))
			{
				if(allProblems.TryGetValue(problemItemsGroup.Key, out EdoInOrderProblemNode problem))
				{
					problem.ProblemItems = problemItemsGroup.Select(x => x.ProblemItem);
				}
			}

			_logger.Info(
				"ЭДО заказа {OrderId}: GetEdoProblemsForOrder всего проблем {Count}: {Elapsed}",
				orderId,
				allProblems.Count,
				totalStopwatch.Elapsed);
			return allProblems.Values;
		}

		public IEnumerable<EdoInOrderTransferNode> GetTransferEdoTasksForOrder(IUnitOfWork uow, int orderId)
		{
			var totalStopwatch = Stopwatch.StartNew();
			var stepStopwatch = Stopwatch.StartNew();
			OrderEdoTask orderEdoTaskAlias = null;
			FormalEdoRequest edoRequestAlias = null;
			TransferEdoRequestIteration transferIterationAlias = null;
			TransferEdoRequest transferRequestAlias = null;
			TransferEdoTask transferEdoTaskAlias = null;
			OrganizationEntity organizationFromAlias = null;
			OrganizationEntity organizationToAlias = null;
			EdoInOrderTransferNode resultAlias = null;

			var transferTasks = uow.Session.QueryOver(() => transferEdoTaskAlias)
				.JoinEntityAlias(
					() => transferRequestAlias,
					() => transferEdoTaskAlias.Id == transferRequestAlias.TransferEdoTask.Id,
					JoinType.LeftOuterJoin
				)
				.JoinEntityAlias(
					() => transferIterationAlias,
					() => transferIterationAlias.Id == transferRequestAlias.Iteration.Id,
					JoinType.LeftOuterJoin
				)
				.JoinEntityAlias(
					() => orderEdoTaskAlias,
					() => orderEdoTaskAlias.Id == transferIterationAlias.OrderEdoTask.Id,
					JoinType.LeftOuterJoin
				)
				.Left.JoinAlias(() => orderEdoTaskAlias.FormalEdoRequest, () => edoRequestAlias)
				.JoinEntityAlias(
					() => organizationFromAlias,
					() => organizationFromAlias.Id == transferRequestAlias.FromOrganizationId,
					JoinType.LeftOuterJoin
				)
				.JoinEntityAlias(
					() => organizationToAlias,
					() => organizationToAlias.Id == transferRequestAlias.ToOrganizationId,
					JoinType.LeftOuterJoin
				)
				.Where(() => edoRequestAlias.Order.Id == orderId)
				.SelectList(list => list
					.SelectGroup(() => transferEdoTaskAlias.Id).WithAlias(() => resultAlias.TransferTaskId)
					.Select(() => orderEdoTaskAlias.Id).WithAlias(() => resultAlias.OrderTaskId)
					.Select(() => transferIterationAlias.Id).WithAlias(() => resultAlias.RequestIterationId)
					.Select(() => transferRequestAlias.Id).WithAlias(() => resultAlias.RequestId)
					.Select(() => transferIterationAlias.Time).WithAlias(() => resultAlias.RequestTime)
					.Select(() => transferIterationAlias.Status).WithAlias(() => resultAlias.RequestIterationStatus)
					.Select(() => organizationFromAlias.Id).WithAlias(() => resultAlias.OrganizationFromId)
					.Select(() => organizationFromAlias.Name).WithAlias(() => resultAlias.OrganizationFrom)
					.Select(() => organizationToAlias.Id).WithAlias(() => resultAlias.OrganizationToId)
					.Select(() => organizationToAlias.Name).WithAlias(() => resultAlias.OrganizationTo)
					.Select(() => transferEdoTaskAlias.Status).WithAlias(() => resultAlias.Status)
					.Select(() => transferEdoTaskAlias.TransferStatus).WithAlias(() => resultAlias.TransferStage)
				)
				.TransformUsing(Transformers.AliasToBean<EdoInOrderTransferNode>())
				.List<EdoInOrderTransferNode>()
			;
			_logger.Info(
				"ЭДО заказа {OrderId}: загрузка трансферных задач, строк {Count}: {Elapsed}",
				orderId,
				transferTasks.Count,
				stepStopwatch.Elapsed);

			var transferRequestIds = transferTasks.Select(x => x.RequestId);
			if(!transferRequestIds.Any())
			{
				_logger.Info(
					"ЭДО заказа {OrderId}: GetTransferEdoTasksForOrder без кодов, всего {Count}: {Elapsed}",
					orderId,
					transferTasks.Count,
					totalStopwatch.Elapsed);
				return transferTasks;
			}

			var sql = @"
select
	eti.transfer_edo_request_id as request_id,
	IFNULL(result_tmic.raw_code, source_tmic.raw_code) as transfered_code
from edo_transfered_items eti 
left join edo_order_task_items eoti on eoti.id = eti.order_edo_task_item_id  
left join true_mark_product_codes tmpc on tmpc.id = eoti.product_code_id
left join true_mark_identification_code source_tmic on source_tmic.id = tmpc.source_code_id
left join true_mark_identification_code result_tmic on result_tmic.id = tmpc.result_code_id
where eti.transfer_edo_request_id in (:request_ids)
; 
";

			var query = uow.Session.CreateSQLQuery(sql)
				.AddScalar("request_id", NHibernateUtil.Int32)
				.AddScalar("transfered_code", NHibernateUtil.String)
				.SetParameterList("request_ids", transferRequestIds);
			stepStopwatch.Restart();
			var transferedCodes = query.List<object[]>()
				.Select(x => new
				{
					RequestId = (int)x[0],
					TransferedCode = (string)x[1]
				});
			_logger.Info(
				"ЭДО заказа {OrderId}: загрузка кодов трансферных задач: {Elapsed}",
				orderId,
				stepStopwatch.Elapsed);

			var codesDic = transferedCodes
				.GroupBy(x => x.RequestId)
				.ToDictionary(x => x.Key, x => x.Select(g => g.TransferedCode));
			foreach(var transferTask in transferTasks)
			{
				if(codesDic.TryGetValue(transferTask.RequestId, out IEnumerable<string> codes))
				{
					transferTask.TransferedCodes = codes.ToList();
				}
			}

			_logger.Info(
				"ЭДО заказа {OrderId}: GetTransferEdoTasksForOrder всего задач {Count}: {Elapsed}",
				orderId,
				transferTasks.Count,
				totalStopwatch.Elapsed);
			return transferTasks;
		}
	}
}
