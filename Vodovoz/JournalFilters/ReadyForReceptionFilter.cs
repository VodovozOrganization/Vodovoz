using System;
using System.Linq;
using QS.Dialog;
using QS.DomainModel.UoW;
using QSOrmProject.RepresentationModel;
using Vodovoz.Additions.Store;
using Vodovoz.Domain.Store;
using Vodovoz.Infrastructure.Permissions;
using Vodovoz.TempAdapters;

namespace Vodovoz
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class ReadyForReceptionFilter : RepresentationFilterBase<ReadyForReceptionFilter>, ISingleUoWDialog
	{
        public Warehouse RestrictWarehouse { get; set; }

        protected override void ConfigureWithUow()
		{
            var warehousesList = StoreDocumentHelper.GetRestrictedWarehousesList(UoW, new[] { WarehousePermissions.WarehouseView })
                                    .OrderBy(w => w.Name).ToList();
            if (warehousesList.Count > 5)
            {
                entryWarehouses.Subject = CurrentUserSettings.Settings.DefaultWarehouse ?? null;
                entryWarehouses.SetEntityAutocompleteSelectorFactory(new WarehouseSelectorFactory());

                entryWarehouses.Visible = true;
                comboWarehouses.Visible = false;
            }
            else
            {
                comboWarehouses.ItemsList = warehousesList;
                comboWarehouses.SelectedItem = CurrentUserSettings.Settings.DefaultWarehouse ?? null;

                entryWarehouses.Visible = false;
                comboWarehouses.Visible = true;
            }
        }

		public ReadyForReceptionFilter(IUnitOfWork uow) : this()
		{
			UoW = uow;
		}
		public ReadyForReceptionFilter()
		{
			this.Build();
		}

		void UpdateCreteria()
		{
			OnRefiltered();
		}
	

		[System.ComponentModel.Browsable(false)]
		public bool RestrictWithoutUnload {
			get { return checkWithoutUnload.Active; }
			set {
				checkWithoutUnload.Active = value;
				checkWithoutUnload.Sensitive = false;
			}
		}

		protected void OnCheckWithoutUnloadToggled(object sender, EventArgs e)
		{
			UpdateCreteria();
		}

        protected void OnEntryWarehousesChangedByUser(object sender, EventArgs e)
        {
            RestrictWarehouse = entryWarehouses.Subject as Warehouse;
            UpdateCreteria();
        }

        protected void OnComboWarehousesItemSelected(object sender, Gamma.Widgets.ItemSelectedEventArgs e)
        {
            RestrictWarehouse = e.SelectedItem as Warehouse;
            UpdateCreteria();
        }
    }
}

