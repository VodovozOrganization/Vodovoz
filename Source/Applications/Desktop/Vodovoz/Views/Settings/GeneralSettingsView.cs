using QS.Views;
using Vodovoz.ViewModels.ViewModels.Settings;

namespace Vodovoz.Views.Settings
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class GeneralSettingsView : ViewBase<GeneralSettingsViewModel>
	{
		public GeneralSettingsView(GeneralSettingsViewModel viewModel) : base(viewModel)
		{
			Build();
			Configure();
		}

		private void Configure()
		{
			btnSaveRouteListPrintedPhones.Clicked += (sender, args) => ViewModel.SaveRouteListPrintedFormPhonesCommand.Execute();
			btnSaveRouteListPrintedPhones.Binding.AddBinding(ViewModel, vm => vm.CanEditRouteListPrintedFormPhones, w => w.Sensitive)
				.InitializeFromSource();

			textviewRouteListPrintedFormPhones.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.RouteListPrintedFormPhones, w => w.Buffer.Text)
				.AddBinding(vm => vm.CanEditRouteListPrintedFormPhones, w => w.Sensitive)
				.InitializeFromSource();

			btnSaveCanAddForwardersToLargus.Clicked += (sender, args) => ViewModel.SaveCanAddForwardersToLargusCommand.Execute();
			btnSaveCanAddForwardersToLargus.Binding.AddBinding(ViewModel, vm => vm.CanEditCanAddForwardersToLargus, w => w.Sensitive)
				.InitializeFromSource();

			ycheckCanAddForwardersToLargus.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.CanAddForwardersToLargus, w => w.Active)
				.AddBinding(vm => vm.CanEditCanAddForwardersToLargus, w => w.Sensitive)
				.InitializeFromSource();

			roboatssettingsview1.ViewModel = ViewModel.RoboatsSettingsViewModel;

			complaintSubdivisionsView.ViewModel = ViewModel.ComplaintsSubdivisionSettingsViewModel;

			alternativePriceSubdivisionsView.ViewModel = ViewModel.AlternativePricesSubdivisionSettingsViewModel;

			btnSaveOrderAutoComment.Clicked += (sender, args) => ViewModel.SaveOrderAutoCommentCommand.Execute();
			btnSaveOrderAutoComment.Binding.AddBinding(ViewModel, vm => vm.CanEditOrderAutoComment, w => w.Sensitive).InitializeFromSource();

			btnOrderAutoCommentInfo.Clicked += (sender, args) => ViewModel.ShowAutoCommentInfoCommand.Execute();

			entryOrderAutoComment.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.OrderAutoComment, w => w.Text)
				.AddBinding(vm => vm.CanEditOrderAutoComment, w => w.IsEditable)
				.InitializeFromSource();

			yspinbuttonRouteListsCount.Binding
				.AddBinding(ViewModel, vm => vm.DriversUnclosedRouteListsHavingDebtCount, w => w.ValueAsInt)
				.InitializeFromSource();

			yspinbuttonRouteListsDebt.Binding
				.AddBinding(ViewModel, vm => vm.DriversRouteListsDebtMaxSum, w => w.ValueAsDecimal)
				.InitializeFromSource();

			ybuttonSaveDriversStopListSettings.Clicked += (sender, args) => ViewModel.SaveDriversStopListPropertiesCommand.Execute();
			ybuttonSaveDriversStopListSettings.Binding
				.AddBinding(ViewModel, vm => vm.CanSaveDriversStopListProperties, b => b.Sensitive)
				.InitializeFromSource();

			ytableStopListProp.Binding
				.AddBinding(ViewModel, vm => vm.CanSaveDriversStopListProperties, t => t.Sensitive)
				.InitializeFromSource();

			frameSecondOrderDiscount.Sensitive = ViewModel.CanSaveSecondOrderDiscountAvailability;

			ycheckIsSecondOrderDiscountAvailable.Binding
				.AddBinding(ViewModel, vm => vm.IsClientsSecondOrderDiscountActive, v => v.Active)
				.InitializeFromSource();

			ybuttonSaveIsSecondOrderDiscountAvailable.Clicked += (sender, args) => ViewModel.SaveSecondOrderDiscountAvailabilityCommand.Execute();
		}
	}
}
