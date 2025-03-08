using NHibernate;
using NHibernate.Criterion;
using NHibernate.Persister.Entity;
using NHibernate.SqlCommand;
using NHibernate.Util;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.TrueMark;
using Vodovoz.Core.Domain.TrueMark.TrueMarkProductCodes;
using Vodovoz.Settings.Edo;

namespace Edo.Transfer
{
	public class TransferTaskRepository
	{
		private readonly IEdoTransferSettings _transferSettings;

		private string _tableName;
		private string _transferStatusColName;
		private string _taskStartTimeColName;
		private string _transferStartTimeColName;
		private string _fromOrgColName;
		private string _toOrgColName;

		public TransferTaskRepository(ISessionFactory sessionFactory, IEdoTransferSettings transferSettings)
		{
			if(sessionFactory is null)
			{
				throw new ArgumentNullException(nameof(sessionFactory));
			}

			_transferSettings = transferSettings ?? throw new ArgumentNullException(nameof(transferSettings));

			var userMetadata = sessionFactory.GetClassMetadata(typeof(TransferEdoTask)) as AbstractEntityPersister;
			_tableName = userMetadata.TableName;
			_transferStatusColName = userMetadata.GetPropertyColumnNames(nameof(TransferEdoTask.TransferStatus)).First();
			_transferStartTimeColName = userMetadata.GetPropertyColumnNames(nameof(TransferEdoTask.TransferStartTime)).First();
			_taskStartTimeColName = userMetadata.GetPropertyColumnNames(nameof(TransferEdoTask.StartTime)).First();
			_fromOrgColName = userMetadata.GetPropertyColumnNames(nameof(TransferEdoTask.FromOrganizationId)).First();
			_toOrgColName = userMetadata.GetPropertyColumnNames(nameof(TransferEdoTask.ToOrganizationId)).First();
		}

		public async Task<TransferEdoTask> FindTaskAsync(IUnitOfWork uow, int fromOrg, int toOrg, CancellationToken cancellationToken)
		{
			//Поиск задачи по направлению и статусу

			var sql = $@"
				SELECT * FROM {_tableName}
				WHERE {_fromOrgColName} = :fromOrg
				AND {_toOrgColName} = :toOrg
				AND {_transferStatusColName} = :transferStatus
				FOR UPDATE NOWAIT;
			";

			var query = uow.Session.CreateSQLQuery(sql)
					.AddEntity(typeof(TransferEdoTask))
					.SetParameter("fromOrg", fromOrg)
					.SetParameter("toOrg", toOrg)
					.SetParameter("transferStatus", TransferEdoTaskStatus.WaitingRequests);

			return await query.UniqueResultAsync<TransferEdoTask>(cancellationToken);
		}

		public async Task<IEnumerable<TransferEdoTask>> GetStaleTasksAsync(IUnitOfWork uow, CancellationToken cancellationToken)
		{
			var sql = $@"
				SELECT * FROM {_tableName}
				WHERE {_transferStatusColName} = :transferStatus
				AND {_transferStartTimeColName} IS NULL
				AND DATE_ADD({_taskStartTimeColName}, INTERVAL :transferTaskTimeout MINUTE) < NOW()
				FOR UPDATE SKIP LOCKED;
			";

			var tasks = await uow.Session.CreateSQLQuery(sql)
					.AddEntity(typeof(TransferEdoTask))
					.SetParameter("transferStatus", nameof(TransferEdoTaskStatus.WaitingRequests))
					.SetParameter("transferTaskTimeout", _transferSettings.TransferTaskRequestsWaitingTimeoutMinute)
					.ListAsync<TransferEdoTask>();

			return tasks;
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
					.UniqueResultAsync<bool>(cancellationToken);

			return iterationCompleted;
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
					() => edoTaskItemAlias.ProductCode.Id == trueMarkProductCodeAlias.Id, JoinType.InnerJoin)

				.JoinEntityAlias(() => codeAlias, 
					() => trueMarkProductCodeAlias.ResultCode.Id == codeAlias.Id, JoinType.InnerJoin)
				
				.Select(p => codeAlias.AsEntity())
				.Where(() => transferEdoRequestAlias.TransferEdoTask.Id == transferTask.Id)
				.ListAsync<TrueMarkWaterIdentificationCode>();

			return codes;
		}
	}
}
