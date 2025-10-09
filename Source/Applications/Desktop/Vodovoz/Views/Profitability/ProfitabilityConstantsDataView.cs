using System;
using Gamma.GtkWidgets;
using Gdk;
using Gtk;
using QS.Views.GtkUI;
using Vodovoz.ViewModels.ViewModels.Profitability;
using Vodovoz.ViewModels.Widgets;
using Vodovoz.ViewWidgets.Profitability;

namespace Vodovoz.Views.Profitability
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class ProfitabilityConstantsDataView : WidgetViewBase<ProfitabilityConstantsDataViewModel>
	{
		private SelectableParametersFilterView _filterView;
		
		public ProfitabilityConstantsDataView(ProfitabilityConstantsDataViewModel viewModel) : base(viewModel)
		{
			Build();
			Configure();
		}

		private void Configure()
		{
			#region Административные расходы

			spinBtnAdministrativeExpenses.Binding
				.AddBinding(ViewModel.Entity, e => e.AdministrativeExpenses, w => w.ValueAsInt)
				.InitializeFromSource();
			spinBtnAdministrativeTotalShipped.Binding
				.AddBinding(ViewModel.Entity, e => e.AdministrativeTotalShipped, w => w.ValueAsInt)
				.InitializeFromSource();
			spinBtnAdministrativeTotalShipped.IsEditable = false;
			spinBtnAdministrativeTotalShipped.ClimbRate = 0;
			spinBtnAdministrativeTotalShipped.Adjustment.StepIncrement = 0;
			spinBtnAdministrativeExpensesPerKg.Binding
				.AddBinding(ViewModel.Entity, e => e.AdministrativeExpensesPerKg, w => w.ValueAsDecimal)
				.InitializeFromSource();
			spinBtnAdministrativeExpensesPerKg.IsEditable = false;
			spinBtnAdministrativeExpensesPerKg.ClimbRate = 0;
			spinBtnAdministrativeExpensesPerKg.Adjustment.StepIncrement = 0;
			
			chkAdministrativeProductGroupsFilter.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.IsAdministrativeExpensesProductGroupsFilterActive, w => w.Active)
				.InitializeFromSource();
			chkAdministrativeProductGroupsFilter.Pressed += OnAnyFilterPressed;
			chkAdministartiveWarehousesFilter.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.IsAdministrativeExpensesWarehousesFilterActive, w => w.Active)
				.InitializeFromSource();
			chkAdministartiveWarehousesFilter.Pressed += OnAnyFilterPressed;
			
			#endregion

			#region Складские расходы

			spinBtnWarehouseExpenses.Binding
				.AddBinding(ViewModel.Entity, e => e.WarehouseExpenses, w => w.ValueAsInt)
				.InitializeFromSource();
			spinBtnWarehousesTotalShipped.Binding
				.AddBinding(ViewModel.Entity, e => e.WarehousesTotalShipped, w => w.ValueAsInt)
				.InitializeFromSource();
			spinBtnWarehousesTotalShipped.IsEditable = false;
			spinBtnWarehousesTotalShipped.ClimbRate = 0;
			spinBtnWarehousesTotalShipped.Adjustment.StepIncrement = 0;
			spinBtnWarehouseExpensesPerKg.Binding
				.AddBinding(ViewModel.Entity, e => e.WarehouseExpensesPerKg, w => w.ValueAsDecimal)
				.InitializeFromSource();
			spinBtnWarehouseExpensesPerKg.IsEditable = false;
			spinBtnWarehouseExpensesPerKg.ClimbRate = 0;
			spinBtnWarehouseExpensesPerKg.Adjustment.StepIncrement = 0;
				
			chkWarehouseExpensesProductGroupsFilter.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.IsWarehousesExpensesProductGroupsFilterActive, w => w.Active)
				.InitializeFromSource();
			chkWarehouseExpensesProductGroupsFilter.Pressed += OnAnyFilterPressed;
			chkWarehouseExpensesWarehousesFilter.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.IsWarehousesExpensesWarehousesFilterActive, w => w.Active)
				.InitializeFromSource();
			chkWarehouseExpensesWarehousesFilter.Pressed += OnAnyFilterPressed;

			#endregion

			#region Амортизация авто

			spinBtnDecreaseGazelleCostFor3Year.Binding
				.AddBinding(ViewModel.Entity, e => e.DecreaseGazelleCostFor3Year, w => w.ValueAsInt)
				.InitializeFromSource();

			spinBtnDecreaseLargusCostFor3Year.Binding
				.AddBinding(ViewModel.Entity, e => e.DecreaseLargusCostFor3Year, w => w.ValueAsInt)
				.InitializeFromSource();

			spinBtnDecreaseMinivanCostFor3Year.Binding
				.AddBinding(ViewModel.Entity, e => e.DecreaseMinivanCostFor3Year, w => w.ValueAsInt)
				.InitializeFromSource();

			spinBtnDecreaseTruckCostFor3Year.Binding
				.AddBinding(ViewModel.Entity, e => e.DecreaseTruckCostFor3Year, w => w.ValueAsInt)
				.InitializeFromSource();

			spinBtnGazelleAverageMileage.Binding
				.AddBinding(ViewModel.Entity, e => e.GazelleAverageMileage, w => w.ValueAsInt)
				.InitializeFromSource();

			spinBtnGazelleAverageMileage.IsEditable = false;
			spinBtnGazelleAverageMileage.ClimbRate = 0;
			spinBtnGazelleAverageMileage.Adjustment.StepIncrement = 0;

			spinBtnLargusAverageMileage.Binding
				.AddBinding(ViewModel.Entity, e => e.LargusAverageMileage, w => w.ValueAsInt)
				.InitializeFromSource();

			spinBtnLargusAverageMileage.IsEditable = false;
			spinBtnLargusAverageMileage.ClimbRate = 0;
			spinBtnLargusAverageMileage.Adjustment.StepIncrement = 0;

			spinBtnMinivanAverageMileage.Binding
				.AddBinding(ViewModel.Entity, e => e.MinivanAverageMileage, w => w.ValueAsInt)
				.InitializeFromSource();

			spinBtnMinivanAverageMileage.IsEditable = false;
			spinBtnMinivanAverageMileage.ClimbRate = 0;
			spinBtnMinivanAverageMileage.Adjustment.StepIncrement = 0;

			spinBtnTruckAverageMileage.Binding
				.AddBinding(ViewModel.Entity, e => e.TruckAverageMileage, w => w.ValueAsInt)
				.InitializeFromSource();

			spinBtnTruckAverageMileage.IsEditable = false;
			spinBtnTruckAverageMileage.ClimbRate = 0;
			spinBtnTruckAverageMileage.Adjustment.StepIncrement = 0;

			spinBtnGazelleAmortisation.Binding
				.AddBinding(ViewModel.Entity, e => e.GazelleAmortisationPerKm, e => e.ValueAsDecimal)
				.InitializeFromSource();

			spinBtnGazelleAmortisation.IsEditable = false;
			spinBtnGazelleAmortisation.ClimbRate = 0;
			spinBtnGazelleAmortisation.Adjustment.StepIncrement = 0;

			spinBtnLargusAmortisation.Binding
				.AddBinding(ViewModel.Entity, e => e.LargusAmortisationPerKm, e => e.ValueAsDecimal)
				.InitializeFromSource();

			spinBtnLargusAmortisation.IsEditable = false;
			spinBtnLargusAmortisation.ClimbRate = 0;
			spinBtnLargusAmortisation.Adjustment.StepIncrement = 0;

			spinBtnMinivanAmortisation.Binding
				.AddBinding(ViewModel.Entity, e => e.MinivanAmortisationPerKm, e => e.ValueAsDecimal)
				.InitializeFromSource();

			spinBtnMinivanAmortisation.IsEditable = false;
			spinBtnMinivanAmortisation.ClimbRate = 0;
			spinBtnMinivanAmortisation.Adjustment.StepIncrement = 0;

			spinBtnTruckAmortisation.Binding
				.AddBinding(ViewModel.Entity, e => e.TruckAmortisationPerKm, e => e.ValueAsDecimal)
				.InitializeFromSource();

			spinBtnTruckAmortisation.IsEditable = false;
			spinBtnTruckAmortisation.ClimbRate = 0;
			spinBtnTruckAmortisation.Adjustment.StepIncrement = 0;

			#endregion

			#region Стоимость ремонта авто

			spinBtnOperatingExpensesAllGazelles.Binding
				.AddBinding(ViewModel.Entity, e => e.OperatingExpensesAllGazelles, w => w.ValueAsInt)
				.InitializeFromSource();

			spinBtnOperatingExpensesAllGazelles.IsEditable = false;
			spinBtnOperatingExpensesAllGazelles.ClimbRate = 0;
			spinBtnOperatingExpensesAllGazelles.Adjustment.StepIncrement = 0;

			spinBtnOperatingExpensesAllLarguses.Binding
				.AddBinding(ViewModel.Entity, e => e.OperatingExpensesAllLarguses, w => w.ValueAsInt)
				.InitializeFromSource();

			spinBtnOperatingExpensesAllLarguses.IsEditable = false;
			spinBtnOperatingExpensesAllLarguses.ClimbRate = 0;
			spinBtnOperatingExpensesAllLarguses.Adjustment.StepIncrement = 0;

			spinBtnOperatingExpensesAllMinivans.Binding
				.AddBinding(ViewModel.Entity, e => e.OperatingExpensesAllMinivans, w => w.ValueAsInt)
				.InitializeFromSource();

			spinBtnOperatingExpensesAllMinivans.IsEditable = false;
			spinBtnOperatingExpensesAllMinivans.ClimbRate = 0;
			spinBtnOperatingExpensesAllMinivans.Adjustment.StepIncrement = 0;

			spinBtnOperatingExpensesAllTrucks.Binding
				.AddBinding(ViewModel.Entity, e => e.OperatingExpensesAllTrucks, w => w.ValueAsInt)
				.InitializeFromSource();

			spinBtnOperatingExpensesAllTrucks.IsEditable = false;
			spinBtnOperatingExpensesAllTrucks.ClimbRate = 0;
			spinBtnOperatingExpensesAllTrucks.Adjustment.StepIncrement = 0;

			spinBtnAverageMileageAllGazelles.Binding
				.AddBinding(ViewModel.Entity, e => e.AverageMileageAllGazelles, w => w.ValueAsInt)
				.InitializeFromSource();

			spinBtnAverageMileageAllGazelles.IsEditable = false;
			spinBtnAverageMileageAllGazelles.ClimbRate = 0;
			spinBtnAverageMileageAllGazelles.Adjustment.StepIncrement = 0;

			spinBtnAverageMileageAllLarguses.Binding
				.AddBinding(ViewModel.Entity, e => e.AverageMileageAllLarguses, w => w.ValueAsInt)
				.InitializeFromSource();

			spinBtnAverageMileageAllLarguses.IsEditable = false;
			spinBtnAverageMileageAllLarguses.ClimbRate = 0;
			spinBtnAverageMileageAllLarguses.Adjustment.StepIncrement = 0;

			spinBtnAverageMileageAllMinivans.Binding
				.AddBinding(ViewModel.Entity, e => e.AverageMileageAllMinivans, w => w.ValueAsInt)
				.InitializeFromSource();

			spinBtnAverageMileageAllMinivans.IsEditable = false;
			spinBtnAverageMileageAllMinivans.ClimbRate = 0;
			spinBtnAverageMileageAllMinivans.Adjustment.StepIncrement = 0;

			spinBtnAverageMileageAllTrucks.Binding
				.AddBinding(ViewModel.Entity, e => e.AverageMileageAllTrucks, w => w.ValueAsInt)
				.InitializeFromSource();

			spinBtnAverageMileageAllTrucks.IsEditable = false;
			spinBtnAverageMileageAllTrucks.ClimbRate = 0;
			spinBtnAverageMileageAllTrucks.Adjustment.StepIncrement = 0;
			spinBtnGazellesRepairCost.Binding

				.AddBinding(ViewModel.Entity, e => e.GazelleRepairCostPerKm, w => w.ValueAsDecimal)
				.InitializeFromSource();

			spinBtnGazellesRepairCost.IsEditable = false;
			spinBtnGazellesRepairCost.ClimbRate = 0;
			spinBtnGazellesRepairCost.Adjustment.StepIncrement = 0;

			spinBtnLargusesRepairCost.Binding
				.AddBinding(ViewModel.Entity, e => e.LargusRepairCostPerKm, w => w.ValueAsDecimal)
				.InitializeFromSource();

			spinBtnLargusesRepairCost.IsEditable = false;
			spinBtnLargusesRepairCost.ClimbRate = 0;
			spinBtnLargusesRepairCost.Adjustment.StepIncrement = 0;

			spinBtnMinivansRepairCost.Binding
				.AddBinding(ViewModel.Entity, e => e.MinivanRepairCostPerKm, w => w.ValueAsDecimal)
				.InitializeFromSource();

			spinBtnMinivansRepairCost.IsEditable = false;
			spinBtnMinivansRepairCost.ClimbRate = 0;
			spinBtnMinivansRepairCost.Adjustment.StepIncrement = 0;

			spinBtnTrucksRepairCost.Binding
				.AddBinding(ViewModel.Entity, e => e.TruckRepairCostPerKm, w => w.ValueAsDecimal)
				.InitializeFromSource();

			spinBtnTrucksRepairCost.IsEditable = false;
			spinBtnTrucksRepairCost.ClimbRate = 0;
			spinBtnTrucksRepairCost.Adjustment.StepIncrement = 0;
			
			chkCarEventsFilter.Binding
				.AddBinding(ViewModel, vm => vm.IsCarEventTypesFilterActive, w => w.Active)
				.InitializeFromSource();

			chkCarEventsFilter.Pressed += OnAnyFilterPressed;

			#endregion
			
			lblCalculationSaved.Binding
				.AddBinding(ViewModel, vm => vm.IsCalculationDateAndAuthorActive, w => w.Visible)
				.InitializeFromSource();
			lblCalculationSaveTimeAndAuthor.Binding
				.AddBinding(ViewModel.Entity, e => e.CalculationDateAndAuthor, w => w.Text)
				.AddBinding(ViewModel, vm => vm.IsCalculationDateAndAuthorActive, w => w.Visible)
				.InitializeFromSource();

			var hboxProfitabilityFilters = new HBox();
			ViewModel.UpdateActiveFilterViewModel(ViewModel.AdministrativeExpensesProductGroupsFilterViewModel);
			_filterView = new SelectableParametersFilterView(ViewModel.ActiveFilterViewModel);
			
			_filterView.Show();
			hboxProfitabilityFilters.Add(_filterView);
			panelFilters.Panel = hboxProfitabilityFilters;
			panelFilters.IsHided = true;
			panelFilters.WidthRequest = 350;

			ViewModel.ProgressBarDisplayable = progressWidget;
		}

		private void OnAnyFilterPressed(object sender, EventArgs e)
		{
			if(!(sender is yCheckButton chkBtn))
			{
				return;
			}

			/* Не хочу делать через Gtk.Application.Invoke
			 * поэтому здесь нет инвертированного условия, т.к. прилетает Active = false, при активации
			 * и наоборот Active = true, при сбросе
			*/
			panelFilters.IsHided = chkBtn.Active;

			if(chkBtn.Active)
			{
				return;
			}

			switch(chkBtn.Name)
			{
				case nameof(chkAdministrativeProductGroupsFilter):
					UpdateFilterViewModel(ViewModel.AdministrativeExpensesProductGroupsFilterViewModel);
					break;
				case nameof(chkAdministartiveWarehousesFilter):
					UpdateFilterViewModel(ViewModel.AdministrativeExpensesWarehousesFilterViewModel);
					break;
				case nameof(chkWarehouseExpensesProductGroupsFilter):
					UpdateFilterViewModel(ViewModel.WarehouseExpensesProductGroupsFilterViewModel);
					break;
				case nameof(chkWarehouseExpensesWarehousesFilter):
					UpdateFilterViewModel(ViewModel.WarehouseExpensesWarehousesFilterViewModel);
					break;
				case nameof(chkCarEventsFilter):
					UpdateFilterViewModel(ViewModel.CarEventsFilterViewModel);
					break;
			}
		}

		private void UpdateFilterViewModel(SelectableParametersFilterViewModel filterViewModel)
		{
			if(ViewModel.UpdateActiveFilterViewModel(filterViewModel))
			{
				_filterView.UpdateViewModel(ViewModel.ActiveFilterViewModel);
			}
		}
	}
}
