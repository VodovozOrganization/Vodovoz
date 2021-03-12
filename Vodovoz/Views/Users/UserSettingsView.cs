using System.Linq;
using QS.DomainModel.UoW;
using QS.Project.Journal.EntitySelector;
using QS.Project.Services;
using QS.Services;
using QS.Views.GtkUI;
using Vodovoz.Additions.Store;
using Vodovoz.Domain.Complaints;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Filters.ViewModels;
using Vodovoz.FilterViewModels.Organization;
using Vodovoz.Journals.JournalViewModels.Organization;
using Vodovoz.JournalViewModels;
using Vodovoz.ViewModels.Users;

namespace Vodovoz.Views.Users
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class UserSettingsView : TabViewBase<UserSettingsViewModel>
	{
		public UserSettingsView(UserSettingsViewModel viewModel) : base(viewModel) {
			this.Build();
			ConfigureDlg();
		}

		void ConfigureDlg()
		{
			yentryrefWarehouse.ItemsQuery = StoreDocumentHelper.GetWarehouseQuery();
			yentryrefWarehouse.Binding.AddBinding(ViewModel.Entity, e => e.DefaultWarehouse, w => w.Subject).InitializeFromSource();

			yenumcomboDefaultCategory.ItemsEnum = typeof(NomenclatureCategory);
			var itemsToHide = Nomenclature.GetAllCategories().Except(Nomenclature.GetCategoriesForSaleToOrder()).Cast<object>().ToArray();
			yenumcomboDefaultCategory.AddEnumToHideList(itemsToHide);
			yenumcomboDefaultCategory.Binding.AddBinding(ViewModel.Entity, e => e.DefaultSaleCategory, w => w.SelectedItemOrNull).InitializeFromSource();

			buttonSave.Clicked += (sender, e) => { ViewModel.SaveAndClose(); };
			buttonCancel.Clicked += (sender, e) => { ViewModel.Close(false, QS.Navigation.CloseSource.Cancel); };

			ycheckbuttonDelivery.Binding.AddBinding(ViewModel.Entity, e => e.LogisticDeliveryOrders, w => w.Active).InitializeFromSource();
			ycheckbuttonService.Binding.AddBinding(ViewModel.Entity, e => e.LogisticServiceOrders, w => w.Active).InitializeFromSource();
			ycheckbuttonChainStore.Binding.AddBinding(ViewModel.Entity, e => e.LogisticChainStoreOrders, w => w.Active).InitializeFromSource();

            yenumcomboStatus.ShowSpecialStateAll = true;
            yenumcomboStatus.ItemsEnum = typeof(ComplaintStatuses);
            yenumcomboStatus.Binding.AddBinding(ViewModel.Entity, e => e.DefaultComplaintStatus, w => w.SelectedItemOrNull).InitializeFromSource();

            ycheckbuttonUse.Binding.AddBinding(ViewModel.Entity, e => e.UseEmployeeSubdivision, w => w.Active).InitializeFromSource();

            if (ViewModel.IsUserFromOkk)
            {
                complaintsFrame.Sensitive = false;
            }
            else
            {
                yentrySubdivision.Sensitive = !ViewModel.Entity.UseEmployeeSubdivision;

                ycheckbuttonUse.Toggled += (sender, e) =>
                {
                    bool useEmployeeSubdivision = ViewModel.Entity.UseEmployeeSubdivision;
                    yentrySubdivision.Sensitive = !useEmployeeSubdivision;

                    if (useEmployeeSubdivision)
                    {
                        yentrySubdivision.Subject = null;
                    }

                };

                #region SubdivisionEntityviewmodelentry
                //Это создается тут, а не в ViewModel потому что EmployeesJournalViewModel и EmployeeFilterViewModel нет в ViewModels

                var employeeSelectorFactory =
                new DefaultEntityAutocompleteSelectorFactory
                <Employee, EmployeesJournalViewModel, EmployeeFilterViewModel>(ServicesConfig.CommonServices);

                var filter = new SubdivisionFilterViewModel() { SubdivisionType = SubdivisionType.Default };

                yentrySubdivision.SetEntityAutocompleteSelectorFactory(
                    new EntityAutocompleteSelectorFactory<SubdivisionsJournalViewModel>(typeof(Subdivision), () => new SubdivisionsJournalViewModel(
                        filter,
                        UnitOfWorkFactory.GetDefaultFactory,
                        ServicesConfig.CommonServices,
                        employeeSelectorFactory
                        )
                    )
                );

                yentrySubdivision.Binding.AddBinding(ViewModel.Entity, s => s.DefaultSubdivision, w => w.Subject).InitializeFromSource();

                #endregion
            }
        }
	}
}
