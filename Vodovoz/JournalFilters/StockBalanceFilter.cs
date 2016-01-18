using System;
using QSOrmProject;
using QSOrmProject.RepresentationModel;
using Vodovoz.Domain.Store;

namespace Vodovoz
{
	[OrmDefaultIsFiltered (false)]
	[System.ComponentModel.ToolboxItem (true)]
	public partial class StockBalanceFilter : Gtk.Bin, IRepresentationFilter
	{
		IUnitOfWork uow;

		public IUnitOfWork UoW {
			get {
				return uow;
			}
			set {
				uow = value;
				speccomboStock.ItemsDataSource = Repository.Store.WarehouseRepository.GetActiveWarehouse (uow);
			}
		}

		public StockBalanceFilter (IUnitOfWork uow) : this()
		{
			UoW = uow;
		}

		public StockBalanceFilter ()
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

		protected void OnEnumcomboTypeEnumItemSelected (object sender, EnumItemClickedEventArgs e)
		{
			OnRefiltered ();
		}

		public Warehouse RestrictWarehouse {
			get {
				if (speccomboStock.SelectedItem is Warehouse)
					return speccomboStock.SelectedItem as Warehouse;
				else
					return null;
			}
			set { speccomboStock.SelectedItem = value;
				speccomboStock.Sensitive = false;
			}
		}

		protected void OnSpeccomboStockItemSelected (object sender, EnumItemClickedEventArgs e)
		{
			OnRefiltered ();
		}
	}
}

