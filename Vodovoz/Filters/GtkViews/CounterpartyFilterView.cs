using System;
using QS.Views.GtkUI;
using Vodovoz.Domain.Client;
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
			yenumCounterpartyType.ItemsEnum = typeof(CounterpartyType);
			yenumCounterpartyType.Binding.AddBinding(ViewModel, vm => vm.CounterpartyType, w => w.SelectedItem).InitializeFromSource();
			checkIncludeArhive.Binding.AddBinding(ViewModel, vm => vm.RestrictIncludeArchive, w => w.Active).InitializeFromSource();
		}
	}
}
