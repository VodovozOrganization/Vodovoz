﻿using System;
using System.Linq;
using Autofac;
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
	public partial class ReadyForReceptionFilter : RepresentationFilterBase<ReadyForReceptionFilter>, ISingleUoWDialog
	{
		private ILifetimeScope _lifetimeScope = Startup.AppDIContainer.BeginLifetimeScope();
		
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
				
				var warehouseJournalFactory = new WarehouseJournalFactory(ServicesConfig.UnitOfWorkFactory);

				entryWarehouses.SetEntityAutocompleteSelectorFactory(warehouseJournalFactory.CreateSelectorFactory(_lifetimeScope, filterParams));

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
            
            if(accessToWarehouseAndComplaints)
            {
	            entryWarehouses.Sensitive = comboWarehouses.Sensitive = false;
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
		
		protected override void OnDestroyed()
		{
			if(_lifetimeScope != null)
			{
				_lifetimeScope.Dispose();
				_lifetimeScope = null;
			}
			base.OnDestroyed();
		}
    }
}

