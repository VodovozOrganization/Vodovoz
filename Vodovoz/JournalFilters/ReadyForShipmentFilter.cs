using System;
using QSOrmProject;
using QSOrmProject.RepresentationModel;
using Vodovoz.Domain.Store;

namespace Vodovoz
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class ReadyForShipmentFilter : Gtk.Bin, IRepresentationFilter
	{
		IUnitOfWork uow;

		public IUnitOfWork UoW {
			get {
				return uow;
			}
			set {
				uow = value;
				yspeccomboWarehouse.ItemsList = Repository.Store.WarehouseRepository.GetActiveWarehouse (UoW);
				if (CurrentUserSettings.Settings.DefaultWarehouse != null)
					yspeccomboWarehouse.SelectedItem = CurrentUserSettings.Settings.DefaultWarehouse;
			}
		}

		public ReadyForShipmentFilter (IUnitOfWork uow) : this ()
		{
			UoW = uow;
		}

		public ReadyForShipmentFilter ()
		{
			this.Build ();
		}

		#region IReferenceFilter implementation

		public event EventHandler Refiltered;

		void OnRefiltered ()
		{
			if (Refiltered != null)
				Refiltered (this, new EventArgs ());
		}

		#endregion

		void UpdateCreteria ()
		{
			OnRefiltered ();
		}

		public Warehouse RestrictWarehouse {
			get { return yspeccomboWarehouse.SelectedItem as Warehouse; }
			set {
				yspeccomboWarehouse.SelectedItem = value;
				yspeccomboWarehouse.Sensitive = false;
			}
		}

		protected void OnYspeccomboWarehouseItemSelected (object sender, Gamma.Widgets.ItemSelectedEventArgs e)
		{
			UpdateCreteria ();
		}
	}
}

