using Gamma.Utilities;
using System;
using Vodovoz.Core.Domain.Edo;

namespace Vodovoz.ViewModels.Journals.JournalNodes.Edo
{
	public class EdoProcessJournalNode
	{
		private CustomerEdoRequestSource? _customerRequestSource;
		private EdoTaskType? _orderTaskType;
		private EdoTaskStatus? _orderTaskStatus;
		private DocumentEdoTaskStage? _orderTaskDocumentStage;
		private EdoReceiptStatus? _edoReceiptStatus;
		private TimeSpan? _totalTransferTimeByTransferTasks;
		private TimeSpan? _orderTaskTimeInProgress;

		public int OrderId { get; set; }

		public DateTime DeliveryDate { get; set; }

		public DateTime CustomerRequestTime { get; internal set; }

		public string CustomerRequestSourceTitle { get; set; }
		public CustomerEdoRequestSource? CustomerRequestSource
		{
			get => _customerRequestSource;
			set
			{
				_customerRequestSource = value;
				CustomerRequestSourceTitle = value.GetEnumTitle();
			}
		}

		public int OrderTaskId { get; set; }

		public string OrderTaskTypeTitle { get; set; }
		public EdoTaskType? OrderTaskType
		{
			get => _orderTaskType;
			set
			{
				_orderTaskType = value;
				OrderTaskTypeTitle = value.GetEnumTitle();
			}
		}

		public string OrderTaskStatusTitle { get; set; }
		public EdoTaskStatus? OrderTaskStatus
		{
			get => _orderTaskStatus;
			set
			{
				_orderTaskStatus = value;
				OrderTaskStatusTitle = value.GetEnumTitle();
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
					OrderTaskDocumentStageTitle = value.GetEnumTitle();
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
					OrderTaskReceiptStageTitle = value.GetEnumTitle();
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

		public int TransfersCompleted { get; set; }
		public int TransfersTotal { get; set; }
		public string TransfersCompletedTitle => $"{TransfersCompleted}/{TransfersTotal}";

		public bool TransfersHasProblems { get; set; }

		public string TotalTransferTimeByTransferTasksTitle { get; set; }
		public TimeSpan? TotalTransferTimeByTransferTasks
		{
			get => _totalTransferTimeByTransferTasks;
			set
			{
				_totalTransferTimeByTransferTasks = value;
				if(TotalTransferTimeByTransferTasks != null)
				{
					TotalTransferTimeByTransferTasksTitle = _totalTransferTimeByTransferTasks.Value.ToString(@"hh\:mm\:ss");
				}
			}
		}

		public string OrderTaskTimeInProgressTitle { get; set; }
		public TimeSpan? OrderTaskTimeInProgress
		{
			get => _orderTaskTimeInProgress;
			set
			{
				_orderTaskTimeInProgress = value;
				if(OrderTaskTimeInProgress != null)
				{
					OrderTaskTimeInProgressTitle = OrderTaskTimeInProgressTitle = _orderTaskTimeInProgress.Value.ToString(@"hh\:mm\:ss");
				}
			}
		}
	}
}
