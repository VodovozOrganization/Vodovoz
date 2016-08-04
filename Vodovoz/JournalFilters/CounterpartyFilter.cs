using System;
using QSOrmProject;
using QSOrmProject.RepresentationModel;

namespace Vodovoz
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class CounterpartyFilter : Gtk.Bin, IRepresentationFilter
	{
		public CounterpartyFilter (IUnitOfWork uow) : this ()
		{
			UoW = uow;
		}

		public CounterpartyFilter ()
		{
			this.Build ();
		}


		#region IRepresentationFilter implementation

		public event EventHandler Refiltered;

		void OnRefiltered ()
		{
			if (Refiltered != null)
				Refiltered (this, new EventArgs ());
		}

		IUnitOfWork uow;

		public IUnitOfWork UoW {
			get {
				return uow;
			}
			set {
				uow = value;
			}
		}

		#endregion

		public bool RestrictIncludeCustomer {
			get { return checkCustomer.Active; }
			set {
				checkCustomer.Active = value;
				checkCustomer.Sensitive = false;
			}
		}

		public bool RestrictIncludeSupplier {
			get { return checkSupplier.Active; }
			set {
				checkSupplier.Active = value;
				checkSupplier.Sensitive = false;
			}
		}

		public bool RestrictIncludePartner {
			get { return checkPartner.Active; }
			set {
				checkPartner.Active = value;
				checkPartner.Sensitive = false;
			}
		}
			
		public bool RestrictIncludeArhive {
			get { return checkIncludeArhive.Active; }
			set {
				checkIncludeArhive.Active = value;
				checkIncludeArhive.Sensitive = false;
			}
		}

		protected void OnComboCounterpartyTypeEnumItemSelected (object sender, EnumItemClickedEventArgs e)
		{
			OnRefiltered ();
		}

		protected void OnCheckIncludeArhiveToggled(object sender, EventArgs e)
		{
			OnRefiltered ();
		}

		protected void OnCheckPartnerToggled(object sender, EventArgs e)
		{
			OnRefiltered ();
		}

		protected void OnCheckSupplierToggled(object sender, EventArgs e)
		{
			OnRefiltered ();
		}
			
		protected void OnCheckCustomerToggled(object sender, EventArgs e)
		{
			OnRefiltered ();
		}
	}
}

