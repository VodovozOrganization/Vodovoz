using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using NHibernate.SqlCommand;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.TrueMark.TrueMarkProductCodes;
using Vodovoz.Settings.Edo;

namespace Edo.Transfer
{
	public class TransferTaskRepository
	{
		private readonly IEdoTransferSettings _transferSettings;

		public TransferTaskRepository(IEdoTransferSettings transferSettings)
		{
			_transferSettings = transferSettings ?? throw new ArgumentNullException(nameof(transferSettings));
		}

		public async Task<TransferEdoTask> FindTaskAsync(
			IUnitOfWork uow, 
			int fromOrg, 
			int toOrg, 
			CancellationToken cancellationToken
			)
		{
			var query = uow.Session.QueryOver<TransferEdoTask>()
				.Where(x => x.FromOrganizationId == fromOrg)
				.Where(x => x.ToOrganizationId == toOrg)
				.Where(x => x.TransferStatus == TransferEdoTaskStatus.WaitingRequests)
				.Take(1);

			return await query.SingleOrDefaultAsync<TransferEdoTask>(cancellationToken);
		}

		public async Task<IEnumerable<TransferEdoTask>> GetStaleTasksAsync(
			IUnitOfWork uow, 
			CancellationToken cancellationToken
			)
		{
			var timeoutProjection = Projections.SqlFunction(
				new SQLFunctionTemplate(
					NHibernateUtil.Date,
					"DATE_ADD(?1, INTERVAL ?2 MINUTE)"
					),
				NHibernateUtil.Date,
				Projections.Property<TransferEdoTask>(x => x.StartTime),
				Projections.Constant(_transferSettings.TransferTaskRequestsWaitingTimeoutMinute)
			);

			var query = uow.Session.QueryOver<TransferEdoTask>()
				.Where(x => x.TransferStatus == TransferEdoTaskStatus.WaitingRequests)
				.Where(x => x.TransferStartTime == null)
				.Where(Restrictions.Lt(timeoutProjection, DateTime.Now));

			return await query.ListAsync(cancellationToken);
		}

		public async Task<bool> IsTransferIterationCompletedAsync(
			IUnitOfWork uow, 
			int transferIterationId, 
			CancellationToken cancellationToken
			)
		{
			var sql = $@"
				SELECT Count(teri.id) = 0 as is_completed
				FROM edo_transfer_request_iterations teri
				INNER JOIN edo_transfer_requests etr ON etr.iteration_id = teri.id
				LEFT JOIN edo_tasks et ON et.id = etr.transfer_edo_task_id
				WHERE (et.id IS NULL OR et.status != 'Completed')
				AND teri.status = 'InProgress'
				AND teri.id = :transferIterationId;
			";

			var iterationCompleted = await uow.Session.CreateSQLQuery(sql)
					.SetParameter("transferIterationId", transferIterationId)
					.UniqueResultAsync<int>(cancellationToken);

			return iterationCompleted == 1;
		}

		public async Task<IEnumerable<TrueMarkWaterIdentificationCode>> GetAllCodesForTransferTaskAsync(IUnitOfWork uow, TransferEdoTask transferTask, CancellationToken cancellationToken)
		{
			TransferEdoRequest transferEdoRequestAlias = null;
			EdoTaskItem edoTaskItemAlias = null;
			TrueMarkProductCode trueMarkProductCodeAlias = null;
			TrueMarkWaterIdentificationCode codeAlias = null;

			var codes = await uow.Session.QueryOver(() => transferEdoRequestAlias)
				.JoinAlias(() => transferEdoRequestAlias.TransferedItems,
					() => edoTaskItemAlias, JoinType.InnerJoin)

				.JoinEntityAlias(() => trueMarkProductCodeAlias,
					() => edoTaskItemAlias.ProductCode.Id == trueMarkProductCodeAlias.Id)

				.JoinEntityAlias(() => codeAlias, 
					() => trueMarkProductCodeAlias.ResultCode.Id == codeAlias.Id)
				
				.Select(p => codeAlias.AsEntity())
				.Where(() => transferEdoRequestAlias.TransferEdoTask.Id == transferTask.Id)
				.ListAsync<TrueMarkWaterIdentificationCode>(cancellationToken);

			return codes;
		}
	}
}
