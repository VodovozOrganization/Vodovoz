using System;
using QSOrmProject;
using QSOrmProject.RepresentationModel;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Store;
using Vodovoz.Domain.Employees;
using QSProjectsLib;
using Vodovoz.Repository.Store;

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
				yentryrefWarehouse.ItemsQuery = Repository.Store.WarehouseRepository.ActiveWarehouseQuery();
				if (CurrentUserSettings.Settings.DefaultWarehouse != null)
					yentryrefWarehouse.Subject = uow.GetById<Warehouse>(CurrentUserSettings.Settings.DefaultWarehouse.Id) ;
				yentryrefDriver.ItemsQuery = Repository.EmployeeRepository.DriversQuery();
				if(QSMain.User.Permissions["production"])
					IfUserTypeProduction();
			}
		}

		public StockDocumentsFilter (IUnitOfWork uow) : this()
		{
			UoW = uow;
		}

		public StockDocumentsFilter ()
		{
			this.Build ();
			dateperiodDocs.StartDate = DateTime.Today.AddDays(-7); 
			dateperiodDocs.EndDate = DateTime.Today.AddDays(1);
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

		public Warehouse RestrictWarehouse {
			get { return yentryrefWarehouse.Subject as Warehouse;}
			set { yentryrefWarehouse.Subject = value;
				yentryrefWarehouse.Sensitive = false;
			}
		}

		public Employee RestrictDriver {
			get { return yentryrefDriver.Subject as Employee;}
			set { yentryrefDriver.Subject = value;
				yentryrefDriver.Sensitive = false;
			}
		}

		public DeliveryPoint RestrictDeliveryPoint {
			get { return entryreferencePoint.Subject as DeliveryPoint;}
			set { entryreferencePoint.Subject = value;
				entryreferencePoint.Sensitive = false;
			}
		}

		public DateTime? RestrictStartDate {
			get { return dateperiodDocs.StartDateOrNull; }
			set {
				dateperiodDocs.StartDateOrNull = value;
				dateperiodDocs.Sensitive = false;
			}
		}

		public DateTime? RestrictEndDate {
			get { return dateperiodDocs.EndDateOrNull; }
			set {
				dateperiodDocs.EndDateOrNull = value;
				dateperiodDocs.Sensitive = false;
			}
		}

		protected void OnEntryreferencePointChanged (object sender, EventArgs e)
		{
			OnRefiltered ();
		}

		protected void OnEnumcomboDocumentTypeChanged (object sender, EventArgs e)
		{
			OnRefiltered ();
		}

		protected void OnYentryrefWarehouseChangedByUser(object sender, EventArgs e)
		{
			OnRefiltered ();
		}

		protected void OnYentryrefDriverChangedByUser(object sender, EventArgs e)
		{
			OnRefiltered ();
		}

		protected void OnDateperiodDocsPeriodChanged(object sender, EventArgs e)
		{
			OnRefiltered ();
		}

		protected void IfUserTypeProduction()
		{
			DocumentType[] filteredDoctypeList = { DocumentType.CarLoadDocument, DocumentType.CarUnloadDocument, DocumentType.SelfDeliveryDocument };

			object[] fDoctypeList = Array.ConvertAll(filteredDoctypeList, x => (object)x);

			Warehouse productionWarehouse = WarehouseRepository.DefaultWarehouseForProduction(uow);

			enumcomboDocumentType.AddEnumToHideList(fDoctypeList);
			enumcomboDocumentType.ShowSpecialStateAll = false;
			enumcomboDocumentType.SelectedItem = DocumentType.MovementDocument;

			if(productionWarehouse != null) {
				yentryrefWarehouse.Subject = uow.GetById<Warehouse>(WarehouseRepository.DefaultWarehouseForProduction(uow).Id);
				yentryrefWarehouse.Sensitive = false;
			}

		}
	}
}

