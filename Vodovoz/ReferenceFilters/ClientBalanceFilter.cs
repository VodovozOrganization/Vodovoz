using System;
using QSOrmProject;
using QSOrmProject.RepresentationModel;
using Vodovoz.Domain;

namespace Vodovoz
{
	[OrmDefaultIsFiltered (false)]
	[System.ComponentModel.ToolboxItem (true)]
	public partial class ClientBalanceFilter : Gtk.Bin, IRepresentationFilter
	{
		IUnitOfWork uow;

		public IUnitOfWork UoW {
			get {
				return uow;
			}
			set {
				uow = value;
				entryreferenceClient.RepresentationModel = new ViewModel.CounterpartyVM (uow);
				//speccomboStock.ItemsDataSource = Repository.WarehouseRepository.GetActiveWarehouse (uow);
			}
		}

		public ClientBalanceFilter (IUnitOfWork uow) : this()
		{
			UoW = uow;
		}

		public ClientBalanceFilter ()
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

		public Counterparty RestrictCounterparty {
			get {
				if ( entryreferenceClient.Subject is Counterparty)
					return entryreferenceClient.Subject as Counterparty;
				else
					return null;
			}
			set { entryreferenceClient.Subject = value;
				entryreferenceClient.Sensitive = false;
			}
		
		}

		protected void OnSpeccomboStockItemSelected (object sender, EnumItemClickedEventArgs e)
		{
			OnRefiltered ();
		}
	}
}

