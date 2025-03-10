using MySqlConnector;
using Polly;
using Polly.Retry;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Data.Repositories;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Core.Domain.Organizations;
using Vodovoz.Core.Domain.TrueMark;
using Vodovoz.Settings.Edo;

namespace Edo.Transfer
{
	public class TransferDispatcher
	{
		private readonly TransferTaskRepository _transferTaskRepository;
		private readonly ITrueMarkCodeRepository _trueMarkCodeRepository;
		private readonly IEdoTransferSettings _edoTransferSettings;

		public TransferDispatcher(
			TransferTaskRepository transferTaskRepository,
			ITrueMarkCodeRepository trueMarkCodeRepository,
			IEdoTransferSettings edoTransferSettings
			)
		{
			_transferTaskRepository = transferTaskRepository ?? throw new ArgumentNullException(nameof(transferTaskRepository));
			_trueMarkCodeRepository = trueMarkCodeRepository ?? throw new ArgumentNullException(nameof(trueMarkCodeRepository));
			_edoTransferSettings = edoTransferSettings ?? throw new ArgumentNullException(nameof(edoTransferSettings));
		}

		public async Task<TransferEdoTask> AddRequestsToTask(
			IUnitOfWork uow,
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
				return await TryAddRequestsToTask(uow, requestsGroup, token);
			}, cancellationToken);

			return result;
		}

		private async Task<TransferEdoTask> TryAddRequestsToTask(
			IUnitOfWork uow,
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
			var groupGtins = await uow.Session.QueryOver<GroupGtinEntity>()
					.ListAsync(cancellationToken);
			var gtins = await uow.Session.QueryOver<GtinEntity>()
					.ListAsync(cancellationToken);

			var transferOrder = new TransferOrder();
			transferOrder.Date = transferEdoTask.StartTime.Value;
			transferOrder.Seller = new OrganizationEntity { Id = transferEdoTask.FromOrganizationId };
			transferOrder.Customer = new OrganizationEntity { Id = transferEdoTask.ToOrganizationId };

			foreach(var transferEdoRequest in transferEdoTask.TransferEdoRequests)
			{
				foreach(var transferedItem in transferEdoRequest.TransferedItems)
				{
					TransferOrderTrueMarkCode transferOrderTrueMarkCode = null;
					switch(transferEdoRequest.Iteration.OrderEdoTask.TaskType)
					{
						case EdoTaskType.Document:
							var documentEdoTask = transferEdoRequest.Iteration.OrderEdoTask.As<DocumentEdoTask>();
							transferOrderTrueMarkCode = CreateTransferCodeItem(
								uow, 
								documentEdoTask, 
								transferedItem,
								groupGtins,
								gtins
							);
							break;
						case EdoTaskType.Receipt:
							var receiptEdoTask = transferEdoRequest.Iteration.OrderEdoTask.As<ReceiptEdoTask>();
							transferOrderTrueMarkCode = CreateTransferCodeItem(
								uow,
								receiptEdoTask,
								transferedItem,
								groupGtins,
								gtins
							);
							break;
						default:
							throw new NotSupportedException($"Тип задачи " +
								$"{transferEdoRequest.Iteration.OrderEdoTask.TaskType} не поддерживается.");
					}

					transferOrderTrueMarkCode.TransferOrder = transferOrder;
					transferOrder.Items.Add(transferOrderTrueMarkCode);
				}
			}

			await uow.SaveAsync(transferOrder, cancellationToken: cancellationToken);
			transferEdoTask.TransferOrderId = transferOrder.Id;
		}

		private TransferOrderTrueMarkCode CreateTransferCodeItem(
			IUnitOfWork uow,
			DocumentEdoTask edoTask,
			EdoTaskItem edoTaskItem,
			IEnumerable<GroupGtinEntity> groupGtins,
			IEnumerable<GtinEntity> gtins
			)
		{
			TransferOrderTrueMarkCode transferOrderTrueMarkCode = null;

			if(edoTaskItem.ProductCode.ResultCode.ParentWaterGroupCodeId != null)
			{
				var groupCode = _trueMarkCodeRepository.GetParentGroupCode(
					uow, 
					edoTaskItem.ProductCode.ResultCode.ParentWaterGroupCodeId.Value
				);

				var groupCodeNomenclature = GetNomenclatureForTaskItem(edoTask, groupCode, groupGtins);
				var quantity = groupCode.GetAllCodes()
					.Where(x => x.IsTrueMarkWaterIdentificationCode)
					.Count();

				transferOrderTrueMarkCode = new TransferOrderTrueMarkCode();
				transferOrderTrueMarkCode.GroupCode = groupCode;
				transferOrderTrueMarkCode.Nomenclature = groupCodeNomenclature;
				transferOrderTrueMarkCode.Quantity = quantity;

				return transferOrderTrueMarkCode;
			}

			var individualCode = edoTaskItem.ProductCode.ResultCode;
			var nomenclature = GetNomenclatureForTaskItem(edoTask, individualCode, gtins);

			transferOrderTrueMarkCode = new TransferOrderTrueMarkCode();
			transferOrderTrueMarkCode.IndividualCode = individualCode;
			transferOrderTrueMarkCode.Nomenclature = nomenclature;
			transferOrderTrueMarkCode.Quantity = 1;

			return transferOrderTrueMarkCode;
		}

		private NomenclatureEntity GetNomenclatureForTaskItem(
			DocumentEdoTask edoTask, 
			TrueMarkWaterGroupCode groupCode,
			IEnumerable<GroupGtinEntity> groupGtins
			)
		{
			var nomenclature = edoTask.UpdInventPositions
				.Where(x => x.Codes.Any(c => c.GroupCode == groupCode))
				.Select(x => x.AssignedOrderItem.Nomenclature)
				.FirstOrDefault();

			if(nomenclature == null)
			{
				nomenclature = groupGtins.Where(x => x.GtinNumber == groupCode.GTIN)
					.Select(x => x.Nomenclature)
					.SingleOrDefault();
			}

			return nomenclature;
		}

		private NomenclatureEntity GetNomenclatureForTaskItem(
			DocumentEdoTask edoTask,
			TrueMarkWaterIdentificationCode individualCode,
			IEnumerable<GtinEntity> gtins
			)
		{
			var nomenclature = edoTask.UpdInventPositions
				.Where(x => x.Codes.Any(c => c.IndividualCode == individualCode))
				.Select(x => x.AssignedOrderItem.Nomenclature)
				.FirstOrDefault();

			if(nomenclature == null)
			{
				nomenclature = gtins.Where(x => x.GtinNumber == individualCode.GTIN)
					.Select(x => x.Nomenclature)
					.SingleOrDefault();
			}

			return nomenclature;
		}

		private TransferOrderTrueMarkCode CreateTransferCodeItem(
			IUnitOfWork uow,
			ReceiptEdoTask edoTask,
			EdoTaskItem edoTaskItem,
			IEnumerable<GroupGtinEntity> groupGtins,
			IEnumerable<GtinEntity> gtins
			)
		{
			TransferOrderTrueMarkCode transferOrderTrueMarkCode = null;

			if(edoTaskItem.ProductCode.ResultCode.ParentWaterGroupCodeId != null)
			{
				var groupCode = _trueMarkCodeRepository.GetParentGroupCode(
					uow,
					edoTaskItem.ProductCode.ResultCode.ParentWaterGroupCodeId.Value
				);

				var groupCodeNomenclature = GetNomenclatureForTaskItem(edoTask, groupCode, groupGtins);
				var quantity = groupCode.GetAllCodes()
					.Where(x => x.IsTrueMarkWaterIdentificationCode)
					.Count();

				transferOrderTrueMarkCode = new TransferOrderTrueMarkCode();
				transferOrderTrueMarkCode.GroupCode = groupCode;
				transferOrderTrueMarkCode.Nomenclature = groupCodeNomenclature;
				transferOrderTrueMarkCode.Quantity = quantity;

				return transferOrderTrueMarkCode;
			}

			var individualCode = edoTaskItem.ProductCode.ResultCode;
			var nomenclature = GetNomenclatureForTaskItem(edoTask, edoTaskItem, gtins);

			transferOrderTrueMarkCode = new TransferOrderTrueMarkCode();
			transferOrderTrueMarkCode.IndividualCode = individualCode;
			transferOrderTrueMarkCode.Nomenclature = nomenclature;
			transferOrderTrueMarkCode.Quantity = 1;

			return transferOrderTrueMarkCode;

		}

		private NomenclatureEntity GetNomenclatureForTaskItem(
			ReceiptEdoTask edoTask,
			TrueMarkWaterGroupCode groupCode,
			IEnumerable<GroupGtinEntity> groupGtins
			)
		{
			var nomenclature = edoTask.FiscalDocuments
				.SelectMany(x => x.InventPositions)
				.Where(x => x.GroupCode == groupCode)
				.Select(x => x.OrderItem.Nomenclature)
				.FirstOrDefault();

			if(nomenclature == null)
			{
				nomenclature = groupGtins.Where(x => x.GtinNumber == groupCode.GTIN)
					.Select(x => x.Nomenclature)
					.SingleOrDefault();
			}

			return nomenclature;
		}

		private NomenclatureEntity GetNomenclatureForTaskItem(
			ReceiptEdoTask edoTask,
			EdoTaskItem edoTaskItem,
			IEnumerable<GtinEntity> gtins
			)
		{
			var nomenclature = edoTask.FiscalDocuments
				.SelectMany(x => x.InventPositions)
				.Where(x => x.EdoTaskItem == edoTaskItem)
				.Select(x => x.OrderItem.Nomenclature)
				.FirstOrDefault();

			if(nomenclature == null)
			{
				var individualCode = edoTaskItem.ProductCode.ResultCode; 
				nomenclature = gtins.Where(x => x.GtinNumber == individualCode.GTIN)
					.Select(x => x.Nomenclature)
					.SingleOrDefault();
			}

			return nomenclature;
		}

		/*
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
		}*/
	}
}
