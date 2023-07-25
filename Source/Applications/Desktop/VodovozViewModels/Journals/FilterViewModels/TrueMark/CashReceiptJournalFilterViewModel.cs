﻿using QS.Project.Filter;
using System;
using QS.Services;
using Vodovoz.Domain.TrueMark;

namespace Vodovoz.ViewModels.Journals.FilterViewModels.TrueMark
{
	public class CashReceiptJournalFilterViewModel : FilterViewModelBase<CashReceiptJournalFilterViewModel>
	{
		private readonly ICommonServices _commonServices;
		private DateTime? _startDate;
		private DateTime? _endDate;
		private bool _hasUnscannedReason;
		private CashReceiptStatus? _status;
		
		public CashReceiptJournalFilterViewModel(ICommonServices commonServices)
		{
			_commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
			var showOnlyCodeErrorStatusReceipts =
				_commonServices.CurrentPermissionService.ValidatePresetPermission("CashReceipt.ShowOnlyCodeErrorStatusReceipts");
			var showOnlyReceiptSendErrorStatusReceipts =
				_commonServices.CurrentPermissionService.ValidatePresetPermission("CashReceipt.ShowOnlyReceiptSendErrorStatusReceipts");

			SetAvailableReceiptStatuses(showOnlyCodeErrorStatusReceipts, showOnlyReceiptSendErrorStatusReceipts);
		}

		public DateTime? StartDate
		{
			get => _startDate;
			set => UpdateFilterField(ref _startDate, value);
		}

		public DateTime? EndDate
		{
			get => _endDate;
			set => UpdateFilterField(ref _endDate, value);
		}

		public bool HasUnscannedReason
		{
			get => _hasUnscannedReason;
			set => UpdateFilterField(ref _hasUnscannedReason, value);
		}

		public bool CanChangeStatus { get; private set; } = true;

		public CashReceiptStatus? Status
		{
			get => _status;
			set => UpdateFilterField(ref _status, value);
		}
		
		public AvailableReceiptStatuses AvailableReceiptStatuses { get; private set; }
		
		private void SetAvailableReceiptStatuses(
			bool showOnlyCodeErrorStatusReceipts,
			bool showOnlyReceiptSendErrorStatusReceipts)
		{
			if(_commonServices.UserService.GetCurrentUser(UoW).IsAdmin)
			{
				return;
			}
			
			if(showOnlyCodeErrorStatusReceipts && showOnlyReceiptSendErrorStatusReceipts)
			{
				AvailableReceiptStatuses = AvailableReceiptStatuses.CodeErrorAndReceiptSendError;
			}
			else if(showOnlyCodeErrorStatusReceipts)
			{
				AvailableReceiptStatuses = AvailableReceiptStatuses.OnlyCodeError;
				CanChangeStatus = false;
			}
			else if(showOnlyReceiptSendErrorStatusReceipts)
			{
				AvailableReceiptStatuses = AvailableReceiptStatuses.OnlyReceiptSendError;
				CanChangeStatus = false;
			}
		}
	}
}
