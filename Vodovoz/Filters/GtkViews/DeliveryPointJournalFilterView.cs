using System;
using QS.Views.GtkUI;
using Vodovoz.Filters.ViewModels;

namespace Vodovoz.Filters.GtkViews
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class DeliveryPointJournalFilterView : FilterViewBase<DeliveryPointJournalFilterViewModel>
	{
		public DeliveryPointJournalFilterView(DeliveryPointJournalFilterViewModel filterViewModel) : base(filterViewModel)
		{
			this.Build();
			Configure();
		}

		private void Configure()
		{
			ycheckWithoutStreet.Binding.AddBinding(ViewModel, vm => vm.RestrictOnlyWithoutStreet, w => w.Active).InitializeFromSource();
			ycheckOnlyNotFoundOsm.Binding.AddBinding(ViewModel, vm => vm.RestrictOnlyNotFoundOsm, w => w.Active).InitializeFromSource();
			ycheckRestrictActive.Binding.AddBinding(ViewModel, vm => vm.RestrictOnlyActive, w => w.Active).InitializeFromSource();
			entityVMentryCounterparty.Binding.AddBinding(ViewModel, vm => vm.Counterparty, w => w.Subject).InitializeFromSource();
		}
	}
}
