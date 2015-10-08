using System;
using QSOrmProject;
using QSOrmProject.RepresentationModel;
using Vodovoz.Domain.Cash;
using Gamma.Widgets;

namespace Vodovoz
{
	[OrmDefaultIsFiltered (true)]
	public partial class CashDocumentsFilter : Gtk.Bin, IRepresentationFilter
	{
		IUnitOfWork uow;

		public IUnitOfWork UoW {
			get {
				return uow;
			}
			set {
				uow = value;
				enumcomboDocumentType.ItemsEnum = typeof(CashDocumentType);
			}
		}

		public CashDocumentsFilter (IUnitOfWork uow) : this()
		{
			UoW = uow;
		}

		public CashDocumentsFilter ()
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

		public CashDocumentType? RestrictDocumentType {
			get { return enumcomboDocumentType.SelectedItem as CashDocumentType?;}
			set { enumcomboDocumentType.SelectedItem = value;
				enumcomboDocumentType.Sensitive = false;
			}
		
		}

		protected void OnEnumcomboDocumentTypeEnumItemSelected (object sender, ItemSelectedEventArgs e)
		{
			OnRefiltered ();
		}
	}
}

