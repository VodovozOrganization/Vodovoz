using Edo.Common;
using Edo.Contracts.Messages.Events;
using Edo.Problems;
using MassTransit;
using Microsoft.Extensions.Logging;
using NHibernate;
using OneOf.Types;
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
using Vodovoz.Core.Domain.Results;
using Vodovoz.Core.Domain.TrueMark;

namespace Edo.Transfer.Sender
{
	public class TransferSendPreparer : IDisposable
	{
		private readonly ILogger<TransferSendPreparer> _logger;
		private readonly IUnitOfWork _uow;
		private readonly TransferTaskRepository _transferTaskRepository;
		private readonly ITrueMarkCodeRepository _trueMarkCodeRepository;
		private readonly EdoProblemRegistrar _edoProblemRegistrar;
		private readonly IBus _messageBus;

		public TransferSendPreparer(
			ILogger<TransferSendPreparer> logger,
			IUnitOfWork uow,
			TransferTaskRepository transferTaskRepository,
			ITrueMarkCodeRepository trueMarkCodeRepository,
			EdoProblemRegistrar edoProblemRegistrar,
			IBus messageBus
			)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_uow = uow ?? throw new ArgumentNullException(nameof(uow));
			_transferTaskRepository = transferTaskRepository ?? throw new ArgumentNullException(nameof(transferTaskRepository));
			_trueMarkCodeRepository = trueMarkCodeRepository ?? throw new ArgumentNullException(nameof(trueMarkCodeRepository));
			_edoProblemRegistrar = edoProblemRegistrar ?? throw new ArgumentNullException(nameof(edoProblemRegistrar));
			_messageBus = messageBus ?? throw new ArgumentNullException(nameof(messageBus));
		}

		public async Task PrepareSendAsync(
			int transferEdoTaskId,
			CancellationToken cancellationToken)
		{
			var transferEdoTask = await _uow.Session.GetAsync<TransferEdoTask>(transferEdoTaskId, cancellationToken);
			if(transferEdoTask == null)
			{
				_logger.LogWarning("Не найдена задача на трансфер Id {TransferEdoTaskId}", transferEdoTaskId);
				return;
			}

			if(transferEdoTask.Status == EdoTaskStatus.Completed)
			{
				_logger.LogWarning("Задача на трансфер Id {TransferEdoTaskId} уже завершена", transferEdoTaskId);
				return;
			}

			if(transferEdoTask.TransferStatus <= TransferEdoTaskStatus.WaitingRequests)
			{
				_logger.LogWarning("Задача на трансфер Id {TransferEdoTaskId} еще ожидает заполнения запросами на трансфер", 
					transferEdoTaskId);
				return;
			}

			if(transferEdoTask.TransferStatus > TransferEdoTaskStatus.PreparingToSend)
			{
				_logger.LogWarning("Задача на трансфер Id {TransferEdoTaskId} уже была подготовлена к отправке",
					transferEdoTaskId);
				return;
			}

			try
			{
				await CreateTransferOrder(transferEdoTask, cancellationToken);
				transferEdoTask.TransferStatus = TransferEdoTaskStatus.ReadyToSend;
			}
			catch(EdoProblemException ex)
			{
				var registered = await _edoProblemRegistrar.TryRegisterExceptionProblem(transferEdoTask, ex, cancellationToken);
				if(!registered)
				{
					throw;
				}
			}
			catch(Exception ex)
			{
				var registered = await _edoProblemRegistrar.TryRegisterExceptionProblem(transferEdoTask, ex, cancellationToken);
				if(!registered)
				{
					throw;
				}
			}

			await _uow.SaveAsync(transferEdoTask, cancellationToken: cancellationToken);
			await _uow.CommitAsync(cancellationToken);

			var message = new TransferTaskReadyToSendEvent
			{
				TransferTaskId = transferEdoTask.Id
			};

			await _messageBus.Publish(message, cancellationToken);
		}

		private async Task CreateTransferOrder(TransferEdoTask transferEdoTask, CancellationToken cancellationToken)
		{
			var transferedCodes = await _transferTaskRepository.GetAllCodesForTransferTaskAsync(_uow, transferEdoTask, cancellationToken);
			var groupGtins = await _uow.Session.QueryOver<GroupGtinEntity>()
				.ListAsync(cancellationToken);
			var gtins = await _uow.Session.QueryOver<GtinEntity>()
				.ListAsync(cancellationToken);

			if(!transferEdoTask.StartTime.HasValue)
			{
				await _edoProblemRegistrar
				   .RegisterCustomProblem<EdoTransferTaskProblemCreateSource>(
					   transferEdoTask,
					   cancellationToken,
					   Vodovoz.Core.Domain.Errors.Edo.TransferOrder.TransferOrderCreateDateMissing.Message);

				return;
			}

			var transferOrderResult = TransferOrder.Create(
				transferEdoTask.StartTime.Value,
				new OrganizationEntity { Id = transferEdoTask.FromOrganizationId },
				new OrganizationEntity { Id = transferEdoTask.ToOrganizationId });

			var transferOrder = transferOrderResult.Match(
				to => to,
				async errors => await _edoProblemRegistrar
					.RegisterCustomProblem<EdoTransferTaskProblemCreateSource>(
						transferEdoTask,
						cancellationToken,
						string.Join(", ", errors.Select(e => e.Message))));

			if(transferOrder == null)
			{
				return;
			}
		
			var transferRequests = await _uow.Session.QueryOver<TransferEdoRequest>()
				.Fetch(SelectMode.Fetch, x => x.Iteration)
				.Where(x => x.TransferEdoTask.Id == transferEdoTask.Id)
				.ListAsync();

			var orderTaskIds = transferRequests.Select(x => x.Iteration.OrderEdoTask.Id);

			await _uow.Session.QueryOver<IUpdEnventPositionsTask>()
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
						case EdoTaskType.Tender:
							var tenderEdoTask = transferEdoRequest.Iteration.OrderEdoTask.As<TenderEdoTask>();
							transferOrderTrueMarkCode = await CreateTransferCodeItem(
								tenderEdoTask,
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
			IUpdEnventPositionsTask edoTask,
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
			IUpdEnventPositionsTask edoTask,
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
			IUpdEnventPositionsTask edoTask,
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

		public void Dispose()
		{
			_uow?.Dispose();
		}
	}
}
