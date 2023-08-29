using System;
using System.Linq;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Project.Services;
using QSOrmProject.RepresentationModel;
using Vodovoz.Domain.Permissions.Warehouses;
using Vodovoz.Domain.Store;
using Vodovoz.Tools.Store;
using Vodovoz.ViewModels.Journals.FilterViewModels.Store;
using Vodovoz.ViewModels.Journals.JournalFactories;

namespace Vodovoz
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class ReadyForShipmentFilter : RepresentationFilterBase<ReadyForShipmentFilter>, ISingleUoWDialog
	{
        public Warehouse RestrictWarehouse { get; set; }

        protected override void ConfigureWithUow()
		{
            var warehousesList = new StoreDocumentHelper(new UserSettingsGetter())
	            .GetRestrictedWarehousesList(UoW, WarehousePermissionsType.WarehouseView)
				.OrderBy(w => w.Name).ToList();

            bool accessToWarehouseAndComplaints =
	            ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("user_have_access_only_to_warehouse_and_complaints")
	            && !ServicesConfig.CommonServices.UserService.GetCurrentUser().IsAdmin;
            
            if (warehousesList.Count > 5)
            {
                entryWarehouses.Subject = CurrentUserSettings.Settings.DefaultWarehouse ?? null;
				Action<WarehouseJournalFilterViewModel> filterParams = f => f.IncludeWarehouseIds = warehousesList.Select(x => x.Id);

				var warehouseJournalFactory = new WarehouseJournalFactory();					 

                entryWarehouses.SetEntityAutocompleteSelectorFactory(warehouseJournalFactory.CreateSelectorFactory(filterParams));

                entryWarehouses.Visible = true;
                yspeccomboWarehouse.Visible = false;
            }
            else
            {
                yspeccomboWarehouse.ItemsList = warehousesList;
                yspeccomboWarehouse.SelectedItem = CurrentUserSettings.Settings.DefaultWarehouse ?? null;

                entryWarehouses.Visible = false;
                yspeccomboWarehouse.Visible = true;
            }

            if(accessToWarehouseAndComplaints)
            {
	            entryWarehouses.Sensitive = yspeccomboWarehouse.Sensitive = false;
            }
		}

		public ReadyForShipmentFilter(IUnitOfWork uow) : this()
		{
			UoW = uow;
		}

		public ReadyForShipmentFilter()
		{
			this.Build();
		}

		void UpdateCreteria()
		{
			OnRefiltered();
		}

		protected void OnYspeccomboWarehouseItemSelected(object sender, Gamma.Widgets.ItemSelectedEventArgs e)
		{
            RestrictWarehouse = e.SelectedItem as Warehouse;
            UpdateCreteria();
		}

        protected void OnEntryWarehousesChangedByUser(object sender, System.EventArgs e)
        {
            RestrictWarehouse = entryWarehouses.Subject as Warehouse;
            UpdateCreteria();
        }
    }
}

