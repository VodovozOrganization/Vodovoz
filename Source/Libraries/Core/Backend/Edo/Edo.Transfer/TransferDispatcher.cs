using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Settings.Edo;

namespace Edo.Transfer
{
	public class TransferDispatcher
	{
		private readonly IUnitOfWork _uow;
		private readonly TransferTaskRepository _transferTaskRepository;
		private readonly IEdoTransferSettings _edoTransferSettings;

		public TransferDispatcher(
			IUnitOfWork uow,
			TransferTaskRepository transferTaskRepository,
			IEdoTransferSettings edoTransferSettings
			)
		{
			_uow = uow ?? throw new ArgumentNullException(nameof(uow));
			_transferTaskRepository = transferTaskRepository ?? throw new ArgumentNullException(nameof(transferTaskRepository));
			_edoTransferSettings = edoTransferSettings ?? throw new ArgumentNullException(nameof(edoTransferSettings));
		}

		public async Task<TransferEdoTask> AddRequestsToTask(
			IGrouping<TransferDirection, TransferEdoRequest> requestsGroup,
			CancellationToken cancellationToken)
		{
			var direction = requestsGroup.Key;
			var transferedItemsCount = requestsGroup.Sum(x => x.TransferedItems.Count);

			if(transferedItemsCount >= _edoTransferSettings.MinCodesCountForStartTransfer)
			{
				return await CreateExclusiveTransfer(requestsGroup, cancellationToken);
			}

			var task = await _transferTaskRepository.FindTaskAsync(
				_uow,
				direction.FromOrganizationId,
				direction.ToOrganizationId,
				cancellationToken
			);

			if(task == null)
			{
				task = CreateTransferTask(direction);
				task.TransferStatus = TransferEdoTaskStatus.WaitingRequests;
			}

			await AddRequests(task, requestsGroup, cancellationToken);
			await TrySendTransfer(task, requestsGroup, cancellationToken);

			return task;
		}

		private async Task<TransferEdoTask> CreateExclusiveTransfer(
			IGrouping<TransferDirection, TransferEdoRequest> requestsGroup,
			CancellationToken cancellationToken
			)
		{
			var task = CreateTransferTask(requestsGroup.Key);
			task.TransferStatus = TransferEdoTaskStatus.PreparingToSend;

			await AddRequests(task, requestsGroup, cancellationToken);
			await MoveToPrepareToSend(task, cancellationToken);
			return task;
		}

		private async Task AddRequests(
			TransferEdoTask transferEdoTask, 
			IEnumerable<TransferEdoRequest> requests,
			CancellationToken cancellationToken
			)
		{
			foreach(var request in requests)
			{
				request.TransferEdoTask = transferEdoTask;
				await _uow.SaveAsync(request, cancellationToken: cancellationToken);
			}
		}

		private TransferEdoTask CreateTransferTask(TransferDirection direction)
		{
			var task = new TransferEdoTask
			{
				Status = EdoTaskStatus.InProgress,
				StartTime = DateTime.Now,
				FromOrganizationId = direction.FromOrganizationId,
				ToOrganizationId = direction.ToOrganizationId,
			};
			return task;
		}

		private async Task TrySendTransfer(
			TransferEdoTask transferEdoTask,
			IEnumerable<TransferEdoRequest> currentTransferEdoRequests,
			CancellationToken cancellationToken)
		{
			var otherTransferRequests = await _uow.Session.QueryOver<TransferEdoRequest>()
				.Where(x => x.TransferEdoTask.Id == transferEdoTask.Id)
				.ListAsync();
			var totalTransferRequestsInTask = currentTransferEdoRequests.Union(otherTransferRequests);
			var codesCountInTask = totalTransferRequestsInTask.Sum(x => x.TransferedItems.Count);

			if(codesCountInTask >= _edoTransferSettings.MinCodesCountForStartTransfer)
			{
				await MoveToPrepareToSend(transferEdoTask, cancellationToken);
			}
		}

		public async Task<IEnumerable<TransferEdoTask>> SendStaleTasksAsync(CancellationToken cancellationToken)
		{
			var staleTasks = await _transferTaskRepository.GetStaleTasksAsync(_uow, cancellationToken);
			foreach(var staleTask in staleTasks)
			{
				await MoveToPrepareToSend(staleTask, cancellationToken);
			}

			return staleTasks;
		}

		public async Task MoveToPrepareToSend(TransferEdoTask transferEdoTask, CancellationToken cancellationToken)
		{
			transferEdoTask.TransferStatus = TransferEdoTaskStatus.PreparingToSend;
			await _uow.SaveAsync(transferEdoTask, cancellationToken: cancellationToken);
		}
	}
}
