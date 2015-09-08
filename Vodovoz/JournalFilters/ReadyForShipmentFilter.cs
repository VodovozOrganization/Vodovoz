using System;
using QSOrmProject.RepresentationModel;
using QSOrmProject;
using Vodovoz.ViewModel;
using Vodovoz.Domain;

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
				enumDocType.ItemsEnum = typeof(ShipmentDocumentType);
				yspeccomboWarehouse.ItemsList = Repository.WarehouseRepository.GetActiveWarehouse (UoW);
			}
		}

		public ReadyForShipmentFilter (IUnitOfWork uow) : this ()
		{
			UoW = uow;
		}

		public ReadyForShipmentFilter ()
		{
			this.Build ();
			IsFiltred = false;
		}

		#region IReferenceFilter implementation

		public event EventHandler Refiltered;

		void OnRefiltered ()
		{
			if (Refiltered != null)
				Refiltered (this, new EventArgs ());
		}

		public bool IsFiltred { get; private set; }

		#endregion

		void UpdateCreteria ()
		{
			OnRefiltered ();
		}

		public ShipmentDocumentType? RestrictDocumentType {
			get { return enumDocType.SelectedItem as ShipmentDocumentType?; }
			set {
				enumDocType.SelectedItem = value;
				enumDocType.Sensitive = false;
			}
		}

		public Warehouse RestrictWarehouse {
			get { return yspeccomboWarehouse.SelectedItem as Warehouse; }
			set {
				yspeccomboWarehouse.SelectedItem = value;
				yspeccomboWarehouse.Sensitive = false;
			}
		}

		protected void OnEnumDocTypeEnumItemSelected (object sender, EnumItemClickedEventArgs e)
		{
			UpdateCreteria ();
		}

		protected void OnYspeccomboWarehouseItemSelected (object sender, Gamma.Widgets.ItemSelectedEventArgs e)
		{
			UpdateCreteria ();
		}
	}
}

