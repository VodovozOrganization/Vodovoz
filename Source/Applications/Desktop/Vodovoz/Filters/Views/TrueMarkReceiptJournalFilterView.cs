using System;
using System.Collections.Generic;
using QS.Views.GtkUI;
using System.Linq;
using Vodovoz.Domain.TrueMark;
using Vodovoz.ViewModels.Journals.FilterViewModels.TrueMark;

namespace Vodovoz.Filters.Views
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class TrueMarkReceiptJournalFilterView : FilterViewBase<CashReceiptJournalFilterViewModel>
	{
		public TrueMarkReceiptJournalFilterView(CashReceiptJournalFilterViewModel viewModel) : base(viewModel)
		{
			Build();
			Configure();
		}

		private void Configure()
		{
			dateRangeFilter.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.StartDate, w => w.StartDateOrNull)
				.AddBinding(vm => vm.EndDate, w => w.EndDateOrNull)
				.InitializeFromSource();

			ConfigureEnumComboStatus();

			ycheckbtnUnscannedReason.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.HasUnscannedReason, w => w.Active)
				.InitializeFromSource();
		}

		private void ConfigureEnumComboStatus()
		{
			switch(ViewModel.AvailableReceiptStatuses)
			{
				case AvailableReceiptStatuses.OnlyCodeError:
					yenumcomboStatus.AddEnumerableToHideList(
						GetStatusesToHide(new[]
						{
							CashReceiptStatus.CodeError
						}));
					break;
				case AvailableReceiptStatuses.OnlyReceiptSendError:
					yenumcomboStatus.AddEnumerableToHideList(
						GetStatusesToHide(new[]
							{
								CashReceiptStatus.ReceiptSendError
							}));
					break;
				case AvailableReceiptStatuses.CodeErrorAndReceiptSendError:
					yenumcomboStatus.AddEnumerableToHideList(
						GetStatusesToHide(new[]
							{
								CashReceiptStatus.CodeError,
								CashReceiptStatus.ReceiptSendError
							}));
					break;
			}
			
			yenumcomboStatus.ItemsEnum = typeof(CashReceiptStatus);
			yenumcomboStatus.Sensitive = ViewModel.CanChangeStatus;
			yenumcomboStatus.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.Status, w => w.SelectedItemOrNull)
				.InitializeFromSource();

			if(ViewModel.AvailableReceiptStatuses == AvailableReceiptStatuses.OnlyCodeError
				|| ViewModel.AvailableReceiptStatuses == AvailableReceiptStatuses.OnlyReceiptSendError)
			{
				yenumcomboStatus.DefaultFirst = true;
				yenumcomboStatus.ShowSpecialStateAll = false;
			}
			else
			{
				yenumcomboStatus.ShowSpecialStateAll = true;
			}
		}

		private IEnumerable<CashReceiptStatus> GetStatusesToHide(IEnumerable<CashReceiptStatus> statusesToExclude)
		{
			return Enum.GetValues(typeof(CashReceiptStatus))
				.OfType<CashReceiptStatus>()
				.Except(statusesToExclude);
		}
	}
}
