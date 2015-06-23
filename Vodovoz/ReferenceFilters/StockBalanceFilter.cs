using System;
using NHibernate;
using NHibernate.Criterion;
using QSOrmProject;
using Vodovoz.Domain;

namespace Vodovoz
{
	[OrmDefaultIsFiltered (false)]
	[System.ComponentModel.ToolboxItem (true)]
	public partial class StockBalanceFilter : Gtk.Bin, IReferenceFilter
	{
		IUnitOfWork uow;

		public IUnitOfWork Uow {
			get {
				return uow;
			}
			set {
				uow = value;
				speccomboStock.ItemsDataSource = Repository.WarehouseRepository.GetActiveWarehouse (uow);
			}
		}

		public StockBalanceFilter (IUnitOfWork uow) : base()
		{
			Uow = uow;
		}

		public StockBalanceFilter ()
		{
			this.Build ();
			IsFiltred = false;
		}

		#region IReferenceFilter implementation

		public event EventHandler Refiltered;

		public ISession Session { get; set; }

		public ICriteria BaseCriteria { get; set; }

		ICriteria filtredCriteria;

		public ICriteria FiltredCriteria {
			private set { filtredCriteria = value; }
			get {
				if (filtredCriteria == null)
					UpdateCreteria ();
				return filtredCriteria;
			}		
		}

		void OnRefiltered ()
		{
			if (Refiltered != null)
				Refiltered (this, new EventArgs ());
		}

		public bool IsFiltred { get; private set; }

		#endregion

		void UpdateCreteria ()
		{
/*			IsFiltred = false;
			if (BaseCriteria == null)
				return;
			FiltredCriteria = (ICriteria)BaseCriteria.Clone ();
			if (enumcombo.SelectedItem is NomenclatureCategory) {
				FiltredCriteria.Add (Restrictions.Eq ("Category", enumcombo.SelectedItem));
				IsFiltred = true;
			} else
				FiltredCriteria.AddOrder (NHibernate.Criterion.Order.Asc ("Category"));
*/
			OnRefiltered ();
		}

		protected void OnEnumcomboTypeEnumItemSelected (object sender, EnumItemClickedEventArgs e)
		{
			UpdateCreteria ();
		}

		public Warehouse RestrictWarehouse {
			get {
				if (speccomboStock.SelectedItem is Warehouse)
					return speccomboStock.SelectedItem as Warehouse;
				else
					return null;
			}
		}

		protected void OnSpeccomboStockItemSelected (object sender, EnumItemClickedEventArgs e)
		{
			OnRefiltered ();
		}
	}
}

