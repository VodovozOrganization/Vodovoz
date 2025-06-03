using Gamma.ColumnConfig;
using QS.DomainModel.Entity;
using QS.Views;
using System;
using Vodovoz.Domain.Goods;
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
			yradiobuttonAccounting.Toggled += OnNotepadRadiobuttonToggled;

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

			ConfigureInsuranceNotificationsSettings();

			ConfigureCarTechnicalCheckupSettings();

			ConfigureFastDeliveryLates();

			ConfigureMaxDailyFuelLimits();

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

			ConfigureEmployeesFixedPrices();

			recomendationsettingsview1.ViewModel = ViewModel.RecomendationSettingsViewModel;

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

			#region Вкладка Бухгалтерия

			ConfigureAccountingSettings();

			#endregion Вкладка Бухгалтерия
		}

		private void ConfigureEmployeesFixedPrices()
		{
			//Чтобы помещалось 4 строчки без полосы прокрутки
			frameVodovozEmployeeFixedPrices.HeightRequest = 200;
			btnSaveEmployeesFixedPrices.BindCommand(ViewModel.EmployeeFixedPricesViewModel.SaveEmployeesFixedPricesCommand);
			btnSaveEmployeesFixedPrices.Binding
				.AddSource(ViewModel.EmployeeFixedPricesViewModel)
				.AddBinding(vm => vm.CanChangeEmployeesFixedPrices, w => w.Sensitive)
				.InitializeFromSource();

			treeNomenclatures.ColumnsConfig = FluentColumnsConfig<INamedDomainObject>.Create()
				.AddColumn("Номенклатура").AddTextRenderer(n => n.Name)
				.Finish();

			treeNomenclatures.Binding
				.AddSource(ViewModel.EmployeeFixedPricesViewModel)
				.AddBinding(vm => vm.SelectedNomenclature, w => w.SelectedRow)
				.AddBinding(vm => vm.Nomenclatures, w => w.ItemsDataSource)
				.InitializeFromSource();
			treeNomenclatures.Selection.Changed += OnNomenclaturesSelectionChanged;

			btnAddNomenclatureFixedPrice.BindCommand(ViewModel.EmployeeFixedPricesViewModel.AddNomenclatureForFixedPriceCommand);
			btnAddNomenclatureFixedPrice.Binding
				.AddSource(ViewModel.EmployeeFixedPricesViewModel)
				.AddBinding(vm => vm.CanChangeEmployeesFixedPrices, w => w.Sensitive)
				.InitializeFromSource();

			btnRemoveNomenclatureFixedPrice.BindCommand(ViewModel.EmployeeFixedPricesViewModel.RemoveNomenclatureForFixedPriceCommand);
			btnRemoveNomenclatureFixedPrice.Binding
				.AddSource(ViewModel.EmployeeFixedPricesViewModel)
				.AddBinding(vm => vm.CanRemoveNomenclature, w => w.Sensitive)
				.InitializeFromSource();

			treeFixedPrices.ColumnsConfig = FluentColumnsConfig<NomenclatureFixedPrice>.Create()
				.AddColumn("Минимальное\nколичество")
					.AddNumericRenderer(n => n.MinCount).Editing(ViewModel.EmployeeFixedPricesViewModel.CanChangeEmployeesFixedPrices)
					.Adjustment(new Gtk.Adjustment(0, 0, 1e6, 1, 10, 10))
				.AddColumn("Фиксированная\nцена")
					.AddNumericRenderer(n => n.Price).Editing(ViewModel.EmployeeFixedPricesViewModel.CanChangeEmployeesFixedPrices)
					.Adjustment(new Gtk.Adjustment(0, 0, 1e6, 1, 10, 10))
				.Finish();

			treeFixedPrices.Binding
				.AddSource(ViewModel.EmployeeFixedPricesViewModel)
				.AddBinding(vm => vm.SelectedFixedPrice, w => w.SelectedRow)
				.InitializeFromSource();

			btnAddFixedPrice.BindCommand(ViewModel.EmployeeFixedPricesViewModel.AddFixedPriceCommand);
			btnAddFixedPrice.Binding
				.AddSource(ViewModel.EmployeeFixedPricesViewModel)
				.AddBinding(vm => vm.CanAddFixedPrice, w => w.Sensitive)
				.InitializeFromSource();

			btnRemoveFixedPrice.BindCommand(ViewModel.EmployeeFixedPricesViewModel.RemoveFixedPriceCommand);
			btnRemoveFixedPrice.Binding
				.AddSource(ViewModel.EmployeeFixedPricesViewModel)
				.AddBinding(vm => vm.CanRemoveFixedPrice, w => w.Sensitive)
				.InitializeFromSource();
		}

		private void OnNomenclaturesSelectionChanged(object sender, EventArgs e)
		{
			var selectedNomenclature = ViewModel.EmployeeFixedPricesViewModel.SelectedNomenclature;
			if(selectedNomenclature is null)
			{
				return;
			}

			if(ViewModel.EmployeeFixedPricesViewModel.FixedPrices.TryGetValue(selectedNomenclature.Id, out var fixedPrices))
			{
				//Вызываем отложенную инициализацию списка фиксы для номенклатуры, чтобы при изменении любого параметра у первой и перехода
				//к другой номенклатуре это значение не применилось к последней
				Gtk.Application.Invoke((s, args) => treeFixedPrices.ItemsDataSource = fixedPrices);
			}
		}

		private void ConfigureFastDeliveryLates()
		{
			frameFastDeliveryIntervalFrom.Sensitive = ViewModel.CanEditFastDeliveryIntervalFromSetting;

			yrbtnFastDeliveryIntervalFromOrderCreated.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.IsIntervalFromOrderCreated, w => w.Active)
				.InitializeFromSource();

			yrbtnFastDeliveryIntervalFromAddedInFirstRouteList.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.IsIntervalFromAddedInFirstRouteList, w => w.Active)
				.InitializeFromSource();

			yrbtnFastDeliveryIntervalFromRouteListItemTransfered.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.IsIntervalFromRouteListItemTransfered, w => w.Active)
				.InitializeFromSource();

			ybuttonSaveFastDeliveryIntervalFrom.Clicked += (s, e) => ViewModel.SaveFastDeliveryIntervalFromCommand.Execute();

			ytableframeFastDeliveryMaximumPermissibleLate.Sensitive = ViewModel.CanEditFastDeliveryIntervalFromSetting;

			yspinbuttonFastDeliveryMaximumPermissibleLate.Binding
				.AddBinding(ViewModel, vm => vm.FastDeliveryMaximumPermissibleLateMinutes, w => w.ValueAsInt)
				.InitializeFromSource();

			ybuttonSaveFastDeliveryMaximumPermissibleLate.Clicked += (s, e) => ViewModel.SaveFastDeliveryMaximumPermissibleLateCommand.Execute();
		}

		private void ConfigureMaxDailyFuelLimits()
		{
			frameMaxDailyFuelLimits.Sensitive = ViewModel.CanEditDailyFuelLimitsSetting;

			yspinbuttonLargusMaxDailyFuelLimit.Binding
				.AddBinding(ViewModel, vm => vm.LargusMaxDailyFuelLimit, w => w.ValueAsInt)
				.InitializeFromSource();

			yspinbuttonGazelleMaxDailyFuelLimit.Binding
				.AddBinding(ViewModel, vm => vm.GazelleMaxDailyFuelLimit, w => w.ValueAsInt)
				.InitializeFromSource();

			yspinbuttonTruckMaxDailyFuelLimit.Binding
				.AddBinding(ViewModel, vm => vm.TruckMaxDailyFuelLimit, w => w.ValueAsInt)
				.InitializeFromSource();

			yspinbuttonLoaderMaxDailyFuelLimit.Binding
				.AddBinding(ViewModel, vm => vm.LoaderMaxDailyFuelLimit, w => w.ValueAsInt)
				.InitializeFromSource();

			ybuttonSaveMaxDailyFuelLimits.BindCommand(ViewModel.SaveDailyFuelLimitsCommand);
		}

		private void ConfigureInsuranceNotificationsSettings()
		{
			frameInsurancesNotificationsSettings.Sensitive = ViewModel.CanEditInsuranceNotificationsSettings;

			yspinbuttonKaskoNotificationDays.Binding
				.AddBinding(ViewModel, vm => vm.KaskoEndingNotifyDaysBefore, w => w.ValueAsInt)
				.InitializeFromSource();

			yspinbuttonOsagoNotificationDays.Binding
				.AddBinding(ViewModel, vm => vm.OsagoEndingNotifyDaysBefore, w => w.ValueAsInt)
				.InitializeFromSource();

			ybuttonSaveInsurancesNotificationsSettings.BindCommand(ViewModel.SaveInsuranceNotificationsSettingsCommand);
		}

		private void ConfigureCarTechnicalCheckupSettings()
		{
			frameCarTechnicalCheckup.Sensitive = ViewModel.CanEditCarTechnicalCheckupNotificationsSettings;

			yspinbuttonCarTechnicalCheckupNotificationDays.Binding
				.AddBinding(ViewModel, vm => vm.CarTechnicalCheckupEndingNotifyDaysBefore, w => w.ValueAsInt)
				.InitializeFromSource();

			ybuttonSaveCarTechnicalCheckupNotificationDays.BindCommand(ViewModel.SaveCarTechnicalCheckupSettingsCommand);
		}

		private void ConfigureAccountingSettings()
		{
			paymentWriteOffFinancialExpenseCatogories.ViewModel = ViewModel.PaymentWriteOffAllowedFinancialExpenseCategoriesViewModel;
		}

		private void OnNotepadRadiobuttonToggled(object sender, EventArgs e)
		{
			if(yradiobuttonLogistics.Active)
			{
				ynotebookData.CurrentPage = 0;
				return;
			}

			if(yradiobuttonComplaints.Active)
			{
				ynotebookData.CurrentPage = 1;
				return;
			}

			if(yradiobuttonOrders.Active)
			{
				ynotebookData.CurrentPage = 2;
				return;
			}

			if(yradiobuttonWarehouse.Active)
			{
				ynotebookData.CurrentPage = 3;
				return;
			}

			if(yradiobuttonAccounting.Active)
			{
				ynotebookData.CurrentPage = 4;
				return;
			}
		}

		public override void Destroy()
		{
			treeNomenclatures.Selection.Changed -= OnNomenclaturesSelectionChanged;
			base.Destroy();
		}
	}
}
