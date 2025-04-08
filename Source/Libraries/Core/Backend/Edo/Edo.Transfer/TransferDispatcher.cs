using MySqlConnector;
using NHibernate;
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
		private readonly IUnitOfWork _uow;
		private readonly TransferTaskRepository _transferTaskRepository;
		private readonly ITrueMarkCodeRepository _trueMarkCodeRepository;
		private readonly IEdoTransferSettings _edoTransferSettings;

		public TransferDispatcher(
			IUnitOfWork uow,
			TransferTaskRepository transferTaskRepository,
			ITrueMarkCodeRepository trueMarkCodeRepository,
			IEdoTransferSettings edoTransferSettings
			)
		{
			_uow = uow ?? throw new ArgumentNullException(nameof(uow));
			_transferTaskRepository = transferTaskRepository ?? throw new ArgumentNullException(nameof(transferTaskRepository));
			_trueMarkCodeRepository = trueMarkCodeRepository ?? throw new ArgumentNullException(nameof(trueMarkCodeRepository));
			_edoTransferSettings = edoTransferSettings ?? throw new ArgumentNullException(nameof(edoTransferSettings));
		}

		public async Task<TransferEdoTask> AddRequestsToTask(
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
				return await TryAddRequestsToTask(requestsGroup, token);
			}, cancellationToken);

			return result;
		}

		private async Task<TransferEdoTask> TryAddRequestsToTask(
			IGrouping<TransferDirection, TransferEdoRequest> requestsGroup,
			CancellationToken cancellationToken)
		{
			var direction = requestsGroup.Key;
			var transferedItemsCount = requestsGroup.Sum(x => x.TransferedItems.Count);

			TransferEdoTask task = null;
			if(transferedItemsCount < _edoTransferSettings.MinCodesCountForStartTransfer)
			{
				task = await _transferTaskRepository.FindTaskAsync(
					_uow,
					direction.FromOrganizationId,
					direction.ToOrganizationId,
					cancellationToken
				);
			}

			if(task == null)
			{
				task = new TransferEdoTask();
				task.Status = EdoTaskStatus.InProgress;
				task.StartTime = DateTime.Now;
				task.FromOrganizationId = direction.FromOrganizationId;
				task.ToOrganizationId = direction.ToOrganizationId;
				task.TransferStatus = TransferEdoTaskStatus.WaitingRequests;
			}

			if(transferedItemsCount >= _edoTransferSettings.MinCodesCountForStartTransfer)
			{
				task.TransferStatus = TransferEdoTaskStatus.ReadyToSend;
			}

			foreach(var request in requestsGroup)
			{
				request.TransferEdoTask = task;
				await _uow.SaveAsync(request, cancellationToken: cancellationToken);
			}

			await TrySendTransfer(task, requestsGroup, cancellationToken);

			return task;
		}

		private async Task TrySendTransfer(
			TransferEdoTask transferEdoTask, 
			IEnumerable<TransferEdoRequest> currentTransferEdoRequests,
			CancellationToken cancellationToken)
		{
			if(transferEdoTask.TransferStatus == TransferEdoTaskStatus.ReadyToSend)
			{
				await SendTransfer(transferEdoTask, cancellationToken);
				return;
			}

			var transferRequestsFromDb = await _uow.Session.QueryOver<TransferEdoRequest>()
				.Where(x => x.TransferEdoTask.Id == transferEdoTask.Id)
				.ListAsync();
			var transferRequests = transferRequestsFromDb.Union(currentTransferEdoRequests);

			var codesCountInTask = transferRequests.Sum(x => x.TransferedItems.Count);

			if(codesCountInTask >= _edoTransferSettings.MinCodesCountForStartTransfer)
			{
				await SendTransfer(transferEdoTask, cancellationToken);
			}
		}

		public async Task<IEnumerable<TransferEdoTask>> SendStaleTasksAsync(CancellationToken cancellationToken)
		{
			var staleTasks = await _transferTaskRepository.GetStaleTasksAsync(_uow, cancellationToken);
			foreach(var staleTask in staleTasks)
			{
				await SendTransfer(staleTask, cancellationToken);
			}

			return staleTasks;
		}

		public async Task SendTransfer(TransferEdoTask transferEdoTask, CancellationToken cancellationToken)
		{
			transferEdoTask.TransferStatus = TransferEdoTaskStatus.ReadyToSend;
			transferEdoTask.TransferStartTime = DateTime.Now;

			await CreateTransferOrder(transferEdoTask, cancellationToken);

			await _uow.SaveAsync(transferEdoTask, cancellationToken: cancellationToken);
		}

		private async Task CreateTransferOrder(TransferEdoTask transferEdoTask, CancellationToken cancellationToken)
		{
			var transferedCodes = await _transferTaskRepository.GetAllCodesForTransferTaskAsync(_uow, transferEdoTask, cancellationToken);
			var groupGtins = await _uow.Session.QueryOver<GroupGtinEntity>()
					.ListAsync(cancellationToken);
			var gtins = await _uow.Session.QueryOver<GtinEntity>()
					.ListAsync(cancellationToken);

			var transferOrder = new TransferOrder();
			transferOrder.Date = transferEdoTask.StartTime.Value;
			transferOrder.Seller = new OrganizationEntity { Id = transferEdoTask.FromOrganizationId };
			transferOrder.Customer = new OrganizationEntity { Id = transferEdoTask.ToOrganizationId };

			var transferRequests = await _uow.Session.QueryOver<TransferEdoRequest>()
				.Fetch(SelectMode.Fetch, x => x.Iteration)
				.Where(x => x.TransferEdoTask.Id == transferEdoTask.Id)
				.ListAsync();

			var orderTaskIds = transferRequests.Select(x => x.Iteration.OrderEdoTask.Id);

			await _uow.Session.QueryOver<DocumentEdoTask>()
				.Fetch(SelectMode.Fetch, x => x.UpdInventPositions)
				.WhereRestrictionOn(x => x.Id).IsIn(orderTaskIds.ToArray())
				.ListAsync();

			var taskItems = await _uow.Session.QueryOver<EdoTaskItem>()
				.Fetch(SelectMode.Fetch, x => x.ProductCode)
				.Fetch(SelectMode.Fetch, x => x.ProductCode.SourceCode)
				.Fetch(SelectMode.Fetch, x => x.ProductCode.SourceCode.Tag1260CodeCheckResult)
				.Fetch(SelectMode.Fetch, x => x.ProductCode)
				.Fetch(SelectMode.Fetch, x => x.ProductCode.ResultCode)
				.Fetch(SelectMode.Fetch, x => x.ProductCode.ResultCode.Tag1260CodeCheckResult)
				.WhereRestrictionOn(x => x.CustomerEdoTask.Id).IsIn(orderTaskIds.ToArray())
				.ListAsync();

			var sourceCodes = taskItems
				.Where(x => x.ProductCode.SourceCode != null)
				.Select(x => x.ProductCode.SourceCode);

			var resultCodes = taskItems
				.Where(x => x.ProductCode.ResultCode != null)
				.Select(x => x.ProductCode.ResultCode);

			var codesToPreload = sourceCodes.Union(resultCodes).Distinct();
			await _trueMarkCodeRepository.PreloadCodes(codesToPreload, cancellationToken);

			await _uow.SaveAsync(transferOrder, cancellationToken: cancellationToken);

			foreach(var transferEdoRequest in transferEdoTask.TransferEdoRequests)
			{
				foreach(var transferedItem in transferEdoRequest.TransferedItems)
				{
					TransferOrderTrueMarkCode transferOrderTrueMarkCode = null;
					switch(transferEdoRequest.Iteration.OrderEdoTask.TaskType)
					{
						case EdoTaskType.Document:
							var documentEdoTask = transferEdoRequest.Iteration.OrderEdoTask.As<DocumentEdoTask>();
							transferOrderTrueMarkCode = await CreateTransferCodeItem(
								documentEdoTask, 
								transferedItem,
								groupGtins,
								gtins,
								cancellationToken
							);
							break;
						case EdoTaskType.Receipt:
							var receiptEdoTask = transferEdoRequest.Iteration.OrderEdoTask.As<ReceiptEdoTask>();
							transferOrderTrueMarkCode = await CreateTransferCodeItem(
								receiptEdoTask,
								transferedItem,
								groupGtins,
								gtins,
								cancellationToken
							);
							break;
						default:
							throw new NotSupportedException($"Тип задачи " +
								$"{transferEdoRequest.Iteration.OrderEdoTask.TaskType} не поддерживается.");
					}
					var isGroupCodeItem = transferOrderTrueMarkCode.GroupCode != null;
					if(isGroupCodeItem)
					{
						var groupCodeItems = transferOrder.Items
							.Where(x => x.GroupCode != null);
						if(groupCodeItems.Any(x => x.GroupCode.Id == transferOrderTrueMarkCode.GroupCode.Id))
						{
							continue;
						}
					}

					transferOrderTrueMarkCode.TransferOrder = transferOrder;
					transferOrder.Items.Add(transferOrderTrueMarkCode);
					await _uow.SaveAsync(transferOrderTrueMarkCode, cancellationToken: cancellationToken);
				}
			}

			transferEdoTask.TransferOrderId = transferOrder.Id;
		}

		private async Task<TransferOrderTrueMarkCode> CreateTransferCodeItem(
			DocumentEdoTask edoTask,
			EdoTaskItem edoTaskItem,
			IEnumerable<GroupGtinEntity> groupGtins,
			IEnumerable<GtinEntity> gtins,
			CancellationToken cancellationToken
			)
		{
			TransferOrderTrueMarkCode transferOrderTrueMarkCode = null;

			if(edoTaskItem.ProductCode.ResultCode.ParentWaterGroupCodeId != null)
			{
				var groupCode = await _trueMarkCodeRepository.GetGroupCode(
					edoTaskItem.ProductCode.ResultCode.ParentWaterGroupCodeId.Value,
					cancellationToken
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
					.FirstOrDefault();
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
					.FirstOrDefault();
			}

			return nomenclature;
		}

		private async Task<TransferOrderTrueMarkCode> CreateTransferCodeItem(
			ReceiptEdoTask edoTask,
			EdoTaskItem edoTaskItem,
			IEnumerable<GroupGtinEntity> groupGtins,
			IEnumerable<GtinEntity> gtins,
			CancellationToken cancellationToken
			)
		{
			TransferOrderTrueMarkCode transferOrderTrueMarkCode = null;

			if(edoTaskItem.ProductCode.ResultCode.ParentWaterGroupCodeId != null)
			{
				var groupCode = await _trueMarkCodeRepository.GetGroupCode(
					edoTaskItem.ProductCode.ResultCode.ParentWaterGroupCodeId.Value,
					cancellationToken
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
				.Select(x => x.OrderItems.FirstOrDefault().Nomenclature)
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
				.Select(x => x.OrderItems.FirstOrDefault().Nomenclature)
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
	}
}
