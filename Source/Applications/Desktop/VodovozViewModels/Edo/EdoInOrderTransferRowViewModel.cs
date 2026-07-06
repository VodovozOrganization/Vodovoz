using QS.ViewModels;
using System;
using Vodovoz.Core.Data.Repositories;
using Vodovoz.Core.Domain.Edo;
using Gamma.Utilities;

namespace Vodovoz.ViewModels.Edo
{
	public class EdoInOrderTransferRowViewModel : ViewModelBase
	{
		public EdoInOrderTransferRowViewModel(EdoInOrderTransferNode node)
		{
			if(node is null)
			{
				throw new ArgumentNullException(nameof(node));
			}

			Node = node;
			OrderTaskId = node.OrderTaskId;
			Time = node.RequestTime;
			TimeString = Time.ToString("dd.MM.yyyy HH:mm");
			From = node.OrganizationFrom;
			To = node.OrganizationTo;
			TransferStatus = node.RequestIterationStatus;
			TransferStatusString = TransferStatus.GetEnumTitle();
			TransferTaskStatus = node.Status;
		}
		public EdoInOrderTransferNode Node { get; }
		public int OrderTaskId { get; }
		public DateTime	Time { get; }
		public string TimeString { get; }
		public int AttemptNumber { get; } = 1;
		public string From { get; }
		public string To { get; }
		public TransferEdoRequestIterationStatus TransferStatus { get; }
		public string TransferStatusString { get; }
		public EdoTaskStatus TransferTaskStatus { get; }
	}
}
