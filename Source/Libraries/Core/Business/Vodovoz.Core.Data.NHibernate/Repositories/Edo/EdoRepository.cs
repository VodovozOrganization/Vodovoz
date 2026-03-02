using NHibernate;
using NHibernate.Criterion;
using NHibernate.Linq;
using NHibernate.SqlCommand;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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

		public IEnumerable<OrderEdoTask> GetEdoTaskByOrderAsync(
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

		public async Task<ILookup<OrderEntity, DocumentEdoTask>> GetTrueMarkConnectedClientsTimedOutOrderDocumentTasks(
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
					&& orderEdoDocument.AcceptTime == null
					&& taxcomDocflow.IsReceived
					&& order.PaymentType == Vodovoz.Domain.Client.PaymentType.Cashless
					&& client.PersonType == PersonType.legal
					&& client.ReasonForLeaving == ReasonForLeaving.ForOwnNeeds
					&& client.RegistrationInChestnyZnakStatus == RegistrationInChestnyZnakStatus.Registered
					&& edoAccount.ConsentForEdoStatus == ConsentForEdoStatus.Agree
					&& withdrawalEdoRequest.Id == null
					&& taskItemCodesCount > 0

				select new { Order = order, Task = task };

			var orderTasks =
				(await documentOrderTasks.ToListAsync(cancellationToken))
				.Distinct()
				.ToLookup(x => x.Order, x => x.Task);

			return orderTasks;
		}
	}
}
