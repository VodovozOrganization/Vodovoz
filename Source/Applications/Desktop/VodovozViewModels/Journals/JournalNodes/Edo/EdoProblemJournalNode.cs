using System;
using Gamma.Utilities;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Domain.Orders;

namespace Vodovoz.ViewModels.Journals.JournalNodes.Edo
{
	public class EdoProblemJournalNode
	{
		private DateTime? _customerRequestTime;
		private OrderStatus? _orderStatus;
		private CustomerEdoRequestSource? _customerRequestSource;
		private EdoTaskType? _orderTaskType;
		private EdoTaskStatus? _orderTaskStatus;
		private DocumentEdoTaskStage? _orderTaskDocumentStage;
		private EdoReceiptStatus? _edoReceiptStatus;
		private TimeSpan? _totalTransferTimeByTransferTasks;
		private TimeSpan? _orderTaskTimeInProgress;
		private TaskProblemState? _taskProblemState;

		public int OrderId { get; set; }

		public int? OrderTaskId { get; set; }

		public string SourceId { get; set; }

		public string Message { get; set; }

		public string Description { get; set; }

		public string Recomendation { get; set; }

		public DateTime DeliveryDate { get; set; }

		public string OrderTaskStatusTitle { get; set; }

		public EdoTaskStatus? OrderTaskStatus
		{
			get => _orderTaskStatus;
			set
			{
				_orderTaskStatus = value;
				OrderTaskStatusTitle = value == null ? "" : value.GetEnumTitle();
			}
		}

		public string TaskProblemStateTitle { get; set; }

		public TaskProblemState? TaskProblemState
		{
			get => _taskProblemState;
			set
			{
				_taskProblemState = value;
				TaskProblemStateTitle = value == null ? "" : value.GetEnumTitle();
			}
		}

		public string OrderTaskDocumentStageTitle { get; set; }

		public DocumentEdoTaskStage? OrderTaskDocumentStage
		{
			get => _orderTaskDocumentStage;
			set
			{
				_orderTaskDocumentStage = value;
				if(_orderTaskDocumentStage != null)
				{
					OrderTaskDocumentStageTitle = value == null ? "" : value.GetEnumTitle();
				}
			}
		}

		public string OrderTaskReceiptStageTitle { get; set; }

		public EdoReceiptStatus? OrderTaskReceiptStage
		{
			get => _edoReceiptStatus;
			set
			{
				_edoReceiptStatus = value;
				if(_edoReceiptStatus != null)
				{
					OrderTaskReceiptStageTitle = value == null ? "" : value.GetEnumTitle();
				}
			}
		}

		public string TaskStage
		{
			get
			{
				if(OrderTaskDocumentStage != null)
				{
					return OrderTaskDocumentStageTitle;
				}

				if(OrderTaskReceiptStage != null)
				{
					return OrderTaskReceiptStageTitle;
				}

				return "";
			}
		}
	}
}
