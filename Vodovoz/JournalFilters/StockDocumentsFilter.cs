using System;
using QSOrmProject;
using QSOrmProject.RepresentationModel;
using Vodovoz.Domain;
using Vodovoz.Domain.Documents;

namespace Vodovoz
{
	[OrmDefaultIsFiltered (true)]
	public partial class StockDocumentsFilter : Gtk.Bin, IRepresentationFilter
	{
		IUnitOfWork uow;

		public IUnitOfWork UoW {
			get {
				return uow;
			}
			set {
				uow = value;
				enumcomboDocumentType.ItemsEnum = typeof(DocumentType);
				//entryreferenceClient.RepresentationModel = new ViewModel.CounterpartyVM (uow);
			}
		}

		public StockDocumentsFilter (IUnitOfWork uow) : this()
		{
			UoW = uow;
		}

		public StockDocumentsFilter ()
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

		public DocumentType? RestrictDocumentType {
			get { return enumcomboDocumentType.SelectedItem as DocumentType?;}
			set { enumcomboDocumentType.SelectedItem = value;
				enumcomboDocumentType.Sensitive = false;
			}
		
		}

		public DeliveryPoint RestrictDeliveryPoint {
			get { return entryreferencePoint.Subject as DeliveryPoint;}
			set { entryreferencePoint.Subject = value;
				entryreferencePoint.Sensitive = false;
			}

		}

		protected void OnEntryreferencePointChanged (object sender, EventArgs e)
		{
			OnRefiltered ();
		}

		protected void OnEnumcomboDocumentTypeEnumItemSelected (object sender, EnumItemClickedEventArgs e)
		{
			OnRefiltered ();
		}
	}
}

