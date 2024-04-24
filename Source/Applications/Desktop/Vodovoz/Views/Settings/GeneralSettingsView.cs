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
			ynotebookData.ShowTabs = false;

			yradiobuttonLogistics.Toggled += OnNotepadRadiobuttonToggled;
			yradiobuttonComplaints.Toggled += OnNotepadRadiobuttonToggled;
			yradiobuttonOrders.Toggled += OnNotepadRadiobuttonToggled;
			yradiobuttonWarehouse.Toggled += OnNotepadRadiobuttonToggled;

			#region Вкладка Логистика
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

			frameWaitUntil.Sensitive = ViewModel.CanEditOrderWaitUntilSetting;

			ycheckWaitUntil.Binding
				.AddBinding(ViewModel, vm => vm.IsOrderWaitUntilActive, v => v.Active)
				.InitializeFromSource();

			ybuttonSaveWaitUntil.Clicked += (sender, args) => ViewModel.SaveOrderWaitUntilActiveCommand.Execute();

			frameFastDeliveryBottlesLimit.Sensitive = ViewModel.CanEditFastDelivery19LBottlesLimitSetting;

			ycheckFastDeliveryBottlesLimit.Binding
				.AddBinding(ViewModel, vm => vm.IsFastDelivery19LBottlesLimitActive, v => v.Active)
				.InitializeFromSource();

			ybuttonSaveFastDeliveryBottlesLimit.Clicked += (sender, args) => ViewModel.SaveFastDelivery19LBottlesLimitActiveCommand.Execute();

			yspinbuttonFastDeliveryBottlesLimit.Binding
				.AddBinding(ViewModel, vm => vm.FastDelivery19LBottlesLimitCount, w => w.ValueAsInt)
				.InitializeFromSource();

			yentryBillAdditionalinfo.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.BillAdditionalInfo, w => w.Text)
				.AddBinding(vm => vm.CanSaveBillAdditionalInfo, w => w.Sensitive)
				.InitializeFromSource();

			ybuttonSaveBillAdditionaInfo.Sensitive = ViewModel.CanSaveBillAdditionalInfo;
			ybuttonSaveBillAdditionaInfo.Clicked += (s, e) => ViewModel.SaveBillAdditionalInfoCommand.Execute();

			yspinbuttonTechInspectOurCars.Binding
				.AddBinding(ViewModel, vm => vm.UpcomingTechInspectForOurCars, w => w.ValueAsInt)
				.InitializeFromSource();

			yspinbuttonTechInspectRaskatCars.Binding
				.AddBinding(ViewModel, vm => vm.UpcomingTechInspectForRaskatCars, w => w.ValueAsInt)
				.InitializeFromSource();

			frameTechInspect.Sensitive = ViewModel.CanEditUpcomingTechInspectSetting;
			ybuttonSaveUpcomingTechInspectSettings.Clicked += (s, e) => ViewModel.SaveUpcomingTechInspectCommand.Execute();

			#endregion Вкладка Логистика

			#region Вкладка Рекламации
			complaintSubdivisionsView.ViewModel = ViewModel.ComplaintsSubdivisionSettingsViewModel;
			#endregion Вкладка Рекламации

			#region Вкладка Заказы
			roboatssettingsview1.ViewModel = ViewModel.RoboatsSettingsViewModel;

			alternativePriceSubdivisionsView.ViewModel = ViewModel.AlternativePricesSubdivisionSettingsViewModel;

			btnOrderAutoCommentInfo.Clicked += (sender, args) => ViewModel.ShowAutoCommentInfoCommand.Execute();

			btnSaveOrderAutoComment.Clicked += (sender, args) => ViewModel.SaveOrderAutoCommentCommand.Execute();
			btnSaveOrderAutoComment.Binding.AddBinding(ViewModel, vm => vm.CanEditOrderAutoComment, w => w.Sensitive).InitializeFromSource();

			entryOrderAutoComment.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.OrderAutoComment, w => w.Text)
				.AddBinding(vm => vm.CanEditOrderAutoComment, w => w.IsEditable)
				.InitializeFromSource();

			frameSecondOrderDiscount.Sensitive = ViewModel.CanSaveSecondOrderDiscountAvailability;

			ycheckIsSecondOrderDiscountAvailable.Binding
				.AddBinding(ViewModel, vm => vm.IsClientsSecondOrderDiscountActive, v => v.Active)
				.InitializeFromSource();

			ybuttonSaveIsSecondOrderDiscountAvailable.Clicked += (sender, args) => ViewModel.SaveSecondOrderDiscountAvailabilityCommand.Execute();
			#endregion Вкладка Заказы

			#region Вкладка Склад
			warehousesForPricesAndStocksIntegrationsView.ViewModel = ViewModel.WarehousesForPricesAndStocksIntegrationViewModel;

			yentryCarLoadDocumentInfoString.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.CarLoadDocumentInfoString, w => w.Text)
				.AddBinding(vm => vm.CanSaveCarLoadDocumentInfoString, w => w.Sensitive)
				.InitializeFromSource();

			ybuttonSaveCarLoadDocumentInfoString.Sensitive = ViewModel.CanSaveCarLoadDocumentInfoString;
			ybuttonSaveCarLoadDocumentInfoString.Clicked += (s, e) => ViewModel.SaveCarLoadDocumentInfoStringCommand.Execute();
			#endregion Вкладка Склад
		}

		private void OnNotepadRadiobuttonToggled(object sender, System.EventArgs e)
		{
			if(yradiobuttonLogistics.Active)
			{
				ynotebookData.CurrentPage = 0;
			}

			if(yradiobuttonComplaints.Active)
			{
				ynotebookData.CurrentPage = 1;
			}

			if(yradiobuttonOrders.Active)
			{
				ynotebookData.CurrentPage = 2;
			}

			if(yradiobuttonWarehouse.Active)
			{
				ynotebookData.CurrentPage = 3;
			}
		}
	}
}
