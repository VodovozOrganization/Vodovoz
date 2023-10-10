﻿using QS.Navigation;
using QS.Views.GtkUI;
using Vodovoz.Domain;
using Vodovoz.Domain.Goods.NomenclaturesOnlineParameters;
using Vodovoz.ViewModels.ViewModels.Rent;

namespace Vodovoz.Views.Rent
{
    public partial class FreeRentPackageView : TabViewBase<FreeRentPackageViewModel>
    {
        public FreeRentPackageView(FreeRentPackageViewModel viewModel) : base(viewModel)
        {
            Build();
            Configure();
        }

        private void Configure()
        {
			notebook.ShowTabs = false;
			notebook.Binding
				.AddBinding(ViewModel, vm => vm.CurrentPage, w => w.CurrentPage)
				.InitializeFromSource();
			
			radioBtnInformation.Binding
				.AddBinding(ViewModel, vm => vm.InformationTabActive, w => w.Active)
				.InitializeFromSource();
			radioBtnSitesAndApps.Binding
				.AddBinding(ViewModel, vm => vm.SitesAndAppsTabActive, w => w.Active)
				.InitializeFromSource();

            buttonSave.Clicked += (sender, e) => ViewModel.SaveAndClose();
            buttonCancel.Clicked += (sender, e) => ViewModel.Close(false, CloseSource.Cancel);

			ConfigureInformationTab();
			ConfigureSitesAndAppsTab();
		}

		private void ConfigureInformationTab()
		{
			dataentryName.Binding
				.AddBinding(ViewModel.Entity, e => e.Name, w => w.Text)
				.InitializeFromSource();
			spinDeposit.Binding
				.AddBinding(ViewModel.Entity, e => e.Deposit, w => w.ValueAsDecimal)
				.InitializeFromSource();
			spinMinWaterAmount.Binding
				.AddBinding(ViewModel.Entity, e => e.MinWaterAmount, w => w.ValueAsInt)
				.InitializeFromSource();

			entryDepositService.SetEntityAutocompleteSelectorFactory(ViewModel.DepositServiceSelectorFactory);
			entryDepositService.Binding
				.AddBinding(ViewModel.Entity, e => e.DepositService, w => w.Subject)
				.InitializeFromSource();

			referenceEquipmentType.SubjectType = typeof(EquipmentKind);
			referenceEquipmentType.Binding
				.AddBinding(ViewModel.Entity, e => e.EquipmentKind, w => w.Subject)
				.InitializeFromSource();
		}

		private void ConfigureSitesAndAppsTab()
		{
			lblErpIdTitle.Binding
				.AddFuncBinding(ViewModel, vm => !vm.IsNewEntity, w => w.Visible)
				.InitializeFromSource();
			lblErpId.Binding
				.AddFuncBinding(ViewModel, vm => !vm.IsNewEntity, w => w.Visible)
				.AddBinding(ViewModel, e => e.IdString, w => w.LabelProp)
				.InitializeFromSource();

			entryOnlineName.MaxLength = 0;
			entryOnlineName.Binding
				.AddBinding(ViewModel.Entity, e => e.OnlineName, w => w.Text)
				.InitializeFromSource();

			enumCmbOnlineAvailabilityMobileApp.ShowSpecialStateNot = true;
			enumCmbOnlineAvailabilityMobileApp.ItemsEnum = typeof(NomenclatureOnlineAvailability);
			enumCmbOnlineAvailabilityMobileApp.Binding
				.AddBinding(ViewModel.MobileAppFreeRentPackageOnlineParameters, p => p.PackageOnlineAvailability, w => w.SelectedItemOrNull)
				.InitializeFromSource();
			
			enumCmbOnlineAvailabilityVodovozWebSite.ShowSpecialStateNot = true;
			enumCmbOnlineAvailabilityVodovozWebSite.ItemsEnum = typeof(NomenclatureOnlineAvailability);
			enumCmbOnlineAvailabilityVodovozWebSite.Binding
				.AddBinding(ViewModel.VodovozWebSiteFreeRentPackageOnlineParameters, p => p.PackageOnlineAvailability, w => w.SelectedItemOrNull)
				.InitializeFromSource();
			
			enumCmbOnlineAvailabilityKulerSaleWebSite.ShowSpecialStateNot = true;
			enumCmbOnlineAvailabilityKulerSaleWebSite.ItemsEnum = typeof(NomenclatureOnlineAvailability);
			enumCmbOnlineAvailabilityKulerSaleWebSite.Binding
				.AddBinding(ViewModel.KulerSaleWebSiteFreeRentPackageOnlineParameters, p => p.PackageOnlineAvailability, w => w.SelectedItemOrNull)
				.InitializeFromSource();
		}
	}
}
