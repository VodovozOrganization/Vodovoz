using QS.Project.Filter;
using System;
using Vodovoz.Core.Domain.Edo;

namespace Vodovoz.ViewModels.Journals.FilterViewModels.Edo
{
	public class EdoProcessFilterViewModel : FilterViewModelBase<EdoProcessFilterViewModel>
	{
		private int? _orderId;
		private DateTime _deliveryDateFrom;
		private DateTime _deliveryDateTo;
		private CustomerEdoRequestSource? _requestSource;
		private EdoTaskType? _edoTaskType;
		private EdoTaskStatus? _edoTaskStatus;
		private DocumentEdoTaskStage? _documentTaskStage;
		private EdoReceiptStatus? _receiptTaskStage;
		private bool? _allTransfersComplete;
		private bool? _hasTransferProblem;

		public EdoProcessFilterViewModel()
		{
			_deliveryDateFrom = DateTime.Today.AddDays(-7);
			_deliveryDateTo = DateTime.Today;
		}

		public virtual int? OrderId
		{
			get => _orderId;
			set => UpdateFilterField(ref _orderId, value);
		}

		public virtual DateTime DeliveryDateFrom
		{
			get => _deliveryDateFrom;
			set => UpdateFilterField(ref _deliveryDateFrom, value);
		}

		public virtual DateTime DeliveryDateTo
		{
			get => _deliveryDateTo;
			set => UpdateFilterField(ref _deliveryDateTo, value);
		}

		public virtual CustomerEdoRequestSource? RequestSource
		{
			get => _requestSource;
			set => UpdateFilterField(ref _requestSource, value);
		}

		public virtual EdoTaskType? EdoTaskType
		{
			get => _edoTaskType;
			set => UpdateFilterField(ref _edoTaskType, value);
		}

		public virtual EdoTaskStatus? EdoTaskStatus
		{
			get => _edoTaskStatus;
			set => UpdateFilterField(ref _edoTaskStatus, value);
		}

		public virtual DocumentEdoTaskStage? DocumentTaskStage
		{
			get => _documentTaskStage;
			set => UpdateFilterField(ref _documentTaskStage, value);
		}

		public virtual EdoReceiptStatus? ReceiptTaskStage
		{
			get => _receiptTaskStage;
			set => UpdateFilterField(ref _receiptTaskStage, value);
		}

		public virtual bool? AllTransfersComplete
		{
			get => _allTransfersComplete;
			set => UpdateFilterField(ref _allTransfersComplete, value);
		}

		public virtual bool? HasTransferProblem
		{
			get => _hasTransferProblem;
			set => UpdateFilterField(ref _hasTransferProblem, value);
		}
	}
}
