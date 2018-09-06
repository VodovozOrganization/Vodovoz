using System;
using QSOrmProject;
using QSOrmProject.RepresentationModel;
using Vodovoz.Additions.Store;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Store;
using Vodovoz.ViewModel;

namespace Vodovoz
{
	[OrmDefaultIsFiltered(true)]
	public partial class StockDocumentsFilter : RepresentationFilterBase<StockDocumentsFilter>
	{
		protected override void ConfigureFilter()
		{
			enumcomboDocumentType.ItemsEnum = typeof(DocumentType);

			yentryrefWarehouse.ItemsQuery = StoreDocumentHelper.GetRestrictedWarehouseQuery();
			if(CurrentUserSettings.Settings.DefaultWarehouse != null)
				yentryrefWarehouse.Subject = UoW.GetById<Warehouse>(CurrentUserSettings.Settings.DefaultWarehouse.Id);
			var filter = new EmployeeFilter(UoW);
			filter.RestrictAtOnce(x => x.RestrictCategory = EmployeeCategory.driver);
			yentryrefDriver.RepresentationModel = new EmployeesVM(filter);
			dateperiodDocs.StartDate = DateTime.Today.AddDays(-7);
			dateperiodDocs.EndDate = DateTime.Today.AddDays(1);
		}

		public StockDocumentsFilter(IUnitOfWork uow) : this()
		{
			UoW = uow;
		}

		public StockDocumentsFilter()
		{
			this.Build();
		}

		public DocumentType? RestrictDocumentType {
			get { return enumcomboDocumentType.SelectedItem as DocumentType?; }
			set {
				enumcomboDocumentType.SelectedItem = value;
				enumcomboDocumentType.Sensitive = false;
			}

		}

		public Warehouse RestrictWarehouse {
			get { return yentryrefWarehouse.Subject as Warehouse; }
			set {
				yentryrefWarehouse.Subject = value;
				yentryrefWarehouse.Sensitive = false;
			}
		}

		public Employee RestrictDriver {
			get { return yentryrefDriver.Subject as Employee; }
			set {
				yentryrefDriver.Subject = value;
				yentryrefDriver.Sensitive = false;
			}
		}

		public DeliveryPoint RestrictDeliveryPoint {
			get { return entryreferencePoint.Subject as DeliveryPoint; }
			set {
				entryreferencePoint.Subject = value;
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

		protected void OnEntryreferencePointChanged(object sender, EventArgs e)
		{
			OnRefiltered();
		}

		protected void OnEnumcomboDocumentTypeChanged(object sender, EventArgs e)
		{
			OnRefiltered();
		}

		protected void OnYentryrefWarehouseChangedByUser(object sender, EventArgs e)
		{
			OnRefiltered();
		}

		protected void OnYentryrefDriverChangedByUser(object sender, EventArgs e)
		{
			OnRefiltered();
		}

		protected void OnDateperiodDocsPeriodChanged(object sender, EventArgs e)
		{
			OnRefiltered();
		}
	}
}

