using Gamma.Utilities;
using System;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Domain.Orders;

namespace Vodovoz.ViewModels.Journals.JournalNodes.Edo
{
	public class EdoProcessJournalNode
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

		public int OrderId { get; set; }

		public DateTime DeliveryDate { get; set; }

		public string CustomerRequestTimeTitle { get; set; }
		public DateTime? CustomerRequestTime
		{
			get => _customerRequestTime;
			set
			{
				_customerRequestTime = value;
				CustomerRequestTimeTitle = value == null ? "" : value.Value.ToString("dd.MM.yyyy HH:mm");
			}
		}

		public string OrderStatusTitle { get; set; }
		public OrderStatus? OrderStatus
		{
			get => _orderStatus;
			set
			{
				_orderStatus = value;
				OrderStatusTitle = value == null ? "" : value.GetEnumTitle();
			}
		}

		public string CustomerRequestSourceTitle { get; set; }
		public CustomerEdoRequestSource? CustomerRequestSource
		{
			get => _customerRequestSource;
			set
			{
				_customerRequestSource = value;
				CustomerRequestSourceTitle = value == null ? "" : value.GetEnumTitle();
			}
		}

		public int? OrderTaskId { get; set; }

		public string OrderTaskTypeTitle { get; set; }
		public EdoTaskType? OrderTaskType
		{
			get => _orderTaskType;
			set
			{
				_orderTaskType = value;
				OrderTaskTypeTitle = value == null ? "" : value.GetEnumTitle();
			}
		}

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

		public int TransfersCompleted { get; set; }
		public int TransfersTotal { get; set; }
		public string TransfersCompletedTitle
		{
			get
			{
				if(TransfersTotal == 0)
				{
					return "";
				}

				if(TransfersCompleted == TransfersTotal)
				{
					return "Да";
				}
				else
				{
					return "Нет";
				}
			}
		}

		public bool? TransfersHasProblems { get; set; }
		public string TransfersHasProblemsTitle
		{
			get
			{
				if(TransfersHasProblems == null)
				{
					return "";
				}

				if(TransfersHasProblems.Value)
				{
					return "Да";
				}
				else
				{
					return "Нет";
				}
			}
		}

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
