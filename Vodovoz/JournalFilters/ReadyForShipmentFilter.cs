using System.Linq;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Project.Services;
using QSOrmProject.RepresentationModel;
using Vodovoz.Additions.Store;
using Vodovoz.Domain.Permissions.Warehouses;
using Vodovoz.Domain.Store;
using Vodovoz.Infrastructure.Permissions;
using Vodovoz.JournalViewModels;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Journals.JournalFactories;
using Vodovoz.ViewModels.Journals.JournalViewModels.Store;

namespace Vodovoz
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class ReadyForShipmentFilter : RepresentationFilterBase<ReadyForShipmentFilter>, ISingleUoWDialog
	{
        public Warehouse RestrictWarehouse { get; set; }

        protected override void ConfigureWithUow()
		{
            var warehousesList = StoreDocumentHelper.GetRestrictedWarehousesList(UoW, new[] { WarehousePermissions.WarehouseView })
                                    .OrderBy(w => w.Name).ToList();

            bool accessToWarehouseAndComplaints =
	            ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("user_have_access_only_to_warehouse_and_complaints")
	            && !ServicesConfig.CommonServices.UserService.GetCurrentUser(UoW).IsAdmin;
            
            if (warehousesList.Count > 5)
            {
                entryWarehouses.Subject = CurrentUserSettings.Settings.DefaultWarehouse ?? null;
				var warehouseFilter = new WarehouseJournalFilterViewModel
				{
					IncludeWarehouseIds = warehousesList.Select(x => x.Id)
				};
                entryWarehouses.SetEntityAutocompleteSelectorFactory(new WarehouseSelectorFactory(warehouseFilter));

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

