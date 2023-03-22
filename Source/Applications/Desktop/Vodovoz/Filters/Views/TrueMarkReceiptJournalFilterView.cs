using QS.Views.GtkUI;
using System;
using Vodovoz.Domain.TrueMark;
using Vodovoz.ViewModels.Journals.FilterViewModels.TrueMark;

namespace Vodovoz.Filters.Views
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class TrueMarkReceiptJournalFilterView : FilterViewBase<CashReceiptJournalFilterViewModel>
	{
		public TrueMarkReceiptJournalFilterView(CashReceiptJournalFilterViewModel viewModel) : base(viewModel)
		{
			this.Build();
			Configure();
		}

		private void Configure()
		{
			dateRangeFilter.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.StartDate, w => w.StartDateOrNull)
				.AddBinding(vm => vm.EndDate, w => w.EndDateOrNull)
				.InitializeFromSource();

			yenumcomboStatus.ShowSpecialStateAll = true;
			yenumcomboStatus.ItemsEnum = typeof(CashReceiptStatus);
			yenumcomboStatus.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.Status, w => w.SelectedItemOrNull)
				.InitializeFromSource();

			ycheckbtnUnscannedReason.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.HasUnscannedReason, w => w.Active)
				.InitializeFromSource();
		}
	}
}
