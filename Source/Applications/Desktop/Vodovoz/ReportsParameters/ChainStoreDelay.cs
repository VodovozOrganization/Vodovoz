using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Views;
using System.Collections.Generic;
using System.ComponentModel;
using Vodovoz.ViewModels.ReportsParameters.Payments;

namespace Vodovoz.ReportsParameters
{
	[ToolboxItem(true)]
	public partial class ChainStoreDelayReport : ViewBase<ChainStoreDelayReportViewModel>, ISingleUoWDialog
	{
		public ChainStoreDelayReport(ChainStoreDelayReportViewModel viewModel) : base(viewModel)
		{
			Build();

			ydatepicker.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.StartDate, w => w.DateOrNull)
				.InitializeFromSource();

			speciallistcomboboxReportBy.SetRenderTextFunc<KeyValuePair<string, string>>(node => node.Value);
			speciallistcomboboxReportBy.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.Modes, w => w.ItemsList)
				.AddBinding(vm => vm.Mode, w => w.SelectedItem)
				.AddBinding(vm => vm.ModeAllowed, w => w.Sensitive)
				.InitializeFromSource();

			entityviewmodelentryCounterparty.SetEntityAutocompleteSelectorFactory(ViewModel.CounterpartySelectorFactory);
			entityviewmodelentryCounterparty.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.Counterparty, w => w.Subject)
				.InitializeFromSource();

			entityviewmodelentrySellManager.SetEntityAutocompleteSelectorFactory(ViewModel.SellManagerSelectorFactory);
			entityviewmodelentrySellManager.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.SellManager, w => w.Subject)
				.InitializeFromSource();

			entityviewmodelentryOrderAuthor.SetEntityAutocompleteSelectorFactory(ViewModel.OrderAuthorSelectorFactory);
			entityviewmodelentryOrderAuthor.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.OrderAuthor, w => w.Subject)
				.InitializeFromSource();

			buttonRun.BindCommand(ViewModel.GenerateReportCommand);
		}

		public IUnitOfWork UoW => ViewModel.UoW;
	}
}
