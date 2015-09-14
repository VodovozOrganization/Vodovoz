using System;
using QSOrmProject.RepresentationModel;
using QSOrmProject;
using Vodovoz.Domain;

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
				comboCounterpartyType.ItemsEnum = typeof(CounterpartyType);
			}
		}

		#endregion

		public CounterpartyType? RestrictCounterpartyType {
			get { return comboCounterpartyType.SelectedItem as CounterpartyType?; }
			set {
				comboCounterpartyType.SelectedItem = value;
				comboCounterpartyType.Sensitive = false;
			}
		}

		protected void OnComboCounterpartyTypeEnumItemSelected (object sender, EnumItemClickedEventArgs e)
		{
			OnRefiltered ();
		}
	}
}

