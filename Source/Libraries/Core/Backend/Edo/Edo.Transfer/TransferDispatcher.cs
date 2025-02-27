using MySqlConnector;
using Polly;
using Polly.Retry;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Organizations;
using Vodovoz.Settings.Edo;

namespace Edo.Transfer
{
	public class TransferDispatcher
	{
		private readonly TransferTaskRepository _transferTaskRepository;
		private readonly IEdoTransferSettings _edoTransferSettings;

		public TransferDispatcher(TransferTaskRepository transferTaskRepository, IEdoTransferSettings edoTransferSettings)
		{
			_transferTaskRepository = transferTaskRepository ?? throw new ArgumentNullException(nameof(transferTaskRepository));
			_edoTransferSettings = edoTransferSettings ?? throw new ArgumentNullException(nameof(edoTransferSettings));
		}

		public async Task<TransferEdoTask> AddRequestsToTask(
			IUnitOfWork uow,
			int documentEdoTaskId,
			IGrouping<TransferDirection, TransferEdoRequest> requestsGroup,
			CancellationToken cancellationToken)
		{
			var shouldHandle = new PredicateBuilder()
				.HandleInner<MySqlException>(x => x.Number == (int)MySqlErrorCode.LockWaitTimeout)
				.HandleInner<MySqlException>(x => x.Number == (int)MySqlErrorCode.DuplicateKeyEntry)
				.HandleInner<MySqlException>(x => x.Number == (int)MySqlErrorCode.LockDeadlock);

			var options = new RetryStrategyOptions();
			options.MaxRetryAttempts = 5;
			options.ShouldHandle = shouldHandle;
			options.Delay = TimeSpan.FromSeconds(2);

			var pipeline = new ResiliencePipelineBuilder()
				.AddRetry(options)
				.Build();

			var result = await pipeline.ExecuteAsync(async token => {
				return await TryAddRequestsToTask(uow, documentEdoTaskId, requestsGroup, token);
			}, cancellationToken);

			return result;
		}

		private async Task<TransferEdoTask> TryAddRequestsToTask(
			IUnitOfWork uow,
			int documentEdoTaskId,
			IGrouping<TransferDirection, TransferEdoRequest> requestsGroup,
			CancellationToken cancellationToken)
		{
			var direction = requestsGroup.Key;
			var task = await _transferTaskRepository.FindTaskAsync(
				uow,
				direction.FromOrganizationId,
				direction.ToOrganizationId,
				cancellationToken
			);

			if(task == null)
			{
				task = new TransferEdoTask();
				task.Status = EdoTaskStatus.InProgress;
				task.StartTime = DateTime.Now;
				task.FromOrganizationId = direction.FromOrganizationId;
				task.ToOrganizationId = direction.ToOrganizationId;
				task.TransferStatus = TransferEdoTaskStatus.WaitingRequests;
				task.DocumentEdoTaskId = documentEdoTaskId;
			}

			foreach(var request in requestsGroup)
			{
				request.TransferEdoTask = task;
				await uow.SaveAsync(request, cancellationToken: cancellationToken);
			}

			await TrySendTransfer(uow, task, cancellationToken);

			return task;
		}

		private async Task TrySendTransfer(IUnitOfWork uow, TransferEdoTask transferEdoTask, CancellationToken cancellationToken)
		{
			var transferRequests = await uow.Session.QueryOver<TransferEdoRequest>()
				.Where(x => x.TransferEdoTask.Id == transferEdoTask.Id)
				.ListAsync();

			var codesCountInTask = transferRequests.Sum(x => x.TransferedItems.Count);

			if(codesCountInTask >= _edoTransferSettings.MinCodesCountForStartTransfer)
			{
				await SendTransfer(uow, transferEdoTask, cancellationToken);
			}
		}

		public async Task<IEnumerable<TransferEdoTask>> SendStaleTasksAsync(IUnitOfWork uow, CancellationToken cancellationToken)
		{
			var staleTasks = await _transferTaskRepository.GetStaleTasksAsync(uow, cancellationToken);
			foreach(var staleTask in staleTasks)
			{
				await SendTransfer(uow, staleTask, cancellationToken);
			}

			return staleTasks;
		}

		private async Task SendTransfer(IUnitOfWork uow, TransferEdoTask transferEdoTask, CancellationToken cancellationToken)
		{
			transferEdoTask.TransferStatus = TransferEdoTaskStatus.InProgress;
			transferEdoTask.TransferStartTime = DateTime.Now;

			await CreateTransferOrder(uow, transferEdoTask, cancellationToken);

			await uow.SaveAsync(transferEdoTask, cancellationToken: cancellationToken);
		}

		private async Task CreateTransferOrder(IUnitOfWork uow, TransferEdoTask transferEdoTask, CancellationToken cancellationToken)
		{
			var transferedCodes = await _transferTaskRepository.GetAllCodesForTransferTaskAsync(uow, transferEdoTask, cancellationToken);
			
			var transferOrder = new TransferOrder();
			transferOrder.Date = transferEdoTask.StartTime.Value;
			transferOrder.Seller = new OrganizationEntity { Id = transferEdoTask.FromOrganizationId };
			transferOrder.Customer = new OrganizationEntity { Id = transferEdoTask.ToOrganizationId };

			foreach(var transferedCode in transferedCodes)
			{
				var transferOrderTrueMarkCode = new TransferOrderTrueMarkCode();
				transferOrderTrueMarkCode.TrueMarkCode = transferedCode;
				transferOrderTrueMarkCode.TransferOrder = transferOrder;
				transferOrder.TrueMarkCodes.Add(transferOrderTrueMarkCode);
			}
			await uow.SaveAsync(transferOrder, cancellationToken: cancellationToken);
			transferEdoTask.TransferOrderId = transferOrder.Id;
			await uow.SaveAsync(transferEdoTask, cancellationToken: cancellationToken);
		}

		public void CompleteTransfer(TransferEdoTask transferTask)
		{
			transferTask.TransferStatus = TransferEdoTaskStatus.Completed;
			transferTask.Status = EdoTaskStatus.Completed;
			transferTask.EndTime = DateTime.Now;
		}

		public async Task<bool> IsAllTransfersComplete(IUnitOfWork uow, TransferEdoTask transferTask, CancellationToken cancellationToken)
		{
			var shouldHandle = new PredicateBuilder()
				.HandleInner<MySqlException>(x => x.Number == (int)MySqlErrorCode.LockWaitTimeout)
				.HandleInner<MySqlException>(x => x.Number == (int)MySqlErrorCode.DuplicateKeyEntry)
				.HandleInner<MySqlException>(x => x.Number == (int)MySqlErrorCode.LockDeadlock);

			var options = new RetryStrategyOptions();
			options.MaxRetryAttempts = 5;
			options.ShouldHandle = shouldHandle;
			options.Delay = TimeSpan.FromSeconds(2);

			var pipeline = new ResiliencePipelineBuilder()
				.AddRetry(options)
				.Build();

			var result = await pipeline.ExecuteAsync(async token => {
				return await TryGetAllTransfersComplete(uow, transferTask, token);
			}, cancellationToken);

			return result;
		}

		public async Task<bool> TryGetAllTransfersComplete(IUnitOfWork uow, TransferEdoTask transferTask, CancellationToken cancellationToken)
		{
			var relatedTasks = await _transferTaskRepository.GetAllRelatedTransferTasksAsync(uow, transferTask, cancellationToken);
			return relatedTasks.All(x => x.TransferStatus == TransferEdoTaskStatus.Completed);
		}
	}
}
