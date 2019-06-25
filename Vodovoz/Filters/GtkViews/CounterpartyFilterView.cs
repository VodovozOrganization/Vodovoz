using System;
using QS.Views.GtkUI;
using Vodovoz.Filters.ViewModels;

namespace Vodovoz.Filters.GtkViews
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class CounterpartyFilterView : FilterViewBase<CounterpartyJournalFilterViewModel>
	{
		public CounterpartyFilterView(CounterpartyJournalFilterViewModel counterpartyJournalFilterViewModel) : base(counterpartyJournalFilterViewModel)
		{
			this.Build();
			Configure();
		}

		private void Configure()
		{
			yentryTag.RepresentationModel = ViewModel.TagVM;
			yentryTag.Binding.AddBinding(ViewModel, vm => vm.Tag, w => w.Subject).InitializeFromSource();
			checkCustomer.Binding.AddBinding(ViewModel, vm => vm.RestrictIncludeCustomer, w => w.Active).InitializeFromSource();
			checkPartner.Binding.AddBinding(ViewModel, vm => vm.RestrictIncludePartner, w => w.Active).InitializeFromSource();
			checkSupplier.Binding.AddBinding(ViewModel, vm => vm.RestrictIncludeSupplier, w => w.Active).InitializeFromSource();
			checkIncludeArhive.Binding.AddBinding(ViewModel, vm => vm.RestrictIncludeArchive, w => w.Active).InitializeFromSource();
		}
	}
}
