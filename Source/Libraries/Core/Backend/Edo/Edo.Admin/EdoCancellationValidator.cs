using Core.Infrastructure;
using System;
using System.Linq;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Settings.Edo;

namespace Edo.Admin
{
	public class EdoCancellationValidator : IEdoCancellationValidator
	{
		private readonly IEdoTransferSettings _edoTransferSettings;

		public EdoCancellationValidator(IEdoTransferSettings edoTransferSettings)
		{
			_edoTransferSettings = edoTransferSettings ?? throw new System.ArgumentNullException(nameof(edoTransferSettings));
		}

		public bool CanCancelEdoTask(EdoTask edoTask)
		{
			if(edoTask.Status == EdoTaskStatus.Completed)
			{
				return false;
			}

			switch(edoTask.TaskType)
			{
				case EdoTaskType.Transfer:
					return CanCancelTransferEdoTask((TransferEdoTask)edoTask);

				case EdoTaskType.Document:
					return CanCancelDocumentEdoTask((DocumentEdoTask)edoTask);

				case EdoTaskType.Receipt:
					return CanCancelReceiptEdoTask((ReceiptEdoTask)edoTask);

				case EdoTaskType.Tender:
					return CanCancelTenderEdoTask((TenderEdoTask)edoTask);

				case EdoTaskType.SaveCode:
				case EdoTaskType.BulkAccounting:
				case EdoTaskType.Withdrawal:
				default:
					return false;
			}
		}

		private bool CanCancelTransferEdoTask(TransferEdoTask edoTask)
		{
			if(edoTask.TransferStartTime.HasValue)
			{
				var timeSinceStart = DateTime.Now - edoTask.TransferStartTime.Value;
				if(timeSinceStart < _edoTransferSettings.TransferTimeoutInterval)
				{
					return false;
				}
			}

			var canCancel = edoTask.TransferStatus.IsIn(
				TransferEdoTaskStatus.WaitingRequests,
				TransferEdoTaskStatus.PreparingToSend,
				TransferEdoTaskStatus.ReadyToSend,
				TransferEdoTaskStatus.InProgress
			);
			return canCancel;
		}

		private bool CanCancelDocumentEdoTask(DocumentEdoTask edoTask)
		{
			var canCancelTransfers = CanCancelRelatedTransfers(edoTask);

			var canCancel = edoTask.Stage.IsIn(
				DocumentEdoTaskStage.New,
				DocumentEdoTaskStage.Transfering,
				DocumentEdoTaskStage.Sending,
				DocumentEdoTaskStage.Sent
			);
			return canCancel && canCancelTransfers;
		}

		private bool CanCancelReceiptEdoTask(ReceiptEdoTask edoTask)
		{
			var canCancelTransfers = CanCancelRelatedTransfers(edoTask);

			var canCancel = edoTask.ReceiptStatus.IsIn(
				EdoReceiptStatus.New,
				EdoReceiptStatus.Transfering
			);
			return canCancel && canCancelTransfers;
		}

		private bool CanCancelTenderEdoTask(TenderEdoTask edoTask)
		{
			var canCancelTransfers = CanCancelRelatedTransfers(edoTask);

			var canCancel = edoTask.Stage.IsIn(
				TenderEdoTaskStage.New,
				TenderEdoTaskStage.Transfering
			);
			return canCancel && canCancelTransfers;
		}

		private bool CanCancelRelatedTransfers(OrderEdoTask edoTask)
		{
			var transferTasks = edoTask.TransferIterations
				.SelectMany(x => x.TransferEdoRequests.Select(t => t.TransferEdoTask));
			var allTransfersCompleted = transferTasks.All(x => x.Status.IsIn(
				EdoTaskStatus.Completed,
				EdoTaskStatus.Cancelled
			));

			if(allTransfersCompleted)
			{
				return true;
			}

			var canCancelTransfers = transferTasks.All(x => CanCancelTransferEdoTask(x));
			return canCancelTransfers;
		}
	}
}
