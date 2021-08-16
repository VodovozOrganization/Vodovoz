using System;
using QS.DomainModel.UoW;
using QS.Project.Services;
using QSOrmProject;
using QSOrmProject.RepresentationModel;
using Vodovoz.Additions.Store;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Store;
using Vodovoz.Filters.ViewModels;
using Vodovoz.JournalFilters;
using Vodovoz.ViewModel;
using Vodovoz.ViewModels.Journals.FilterViewModels.Employees;

namespace Vodovoz
{
	[OrmDefaultIsFiltered(true)]
	public partial class StockDocumentsFilter : RepresentationFilterBase<StockDocumentsFilter>
	{
		protected override void ConfigureWithUow()
		{
			enumcomboDocumentType.ItemsEnum = typeof(DocumentType);
			enumcomboDocumentType.HiddenItems = new[] { DocumentType.DeliveryDocument as object };

			yentryrefWarehouse.ItemsQuery = StoreDocumentHelper.GetRestrictedWarehouseQuery();

			if(ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("user_have_access_only_to_warehouse_and_complaints")
			   && !ServicesConfig.CommonServices.UserService.GetCurrentUser(UoW).IsAdmin)
			{
				yentryrefWarehouse.Sensitive = yentryrefWarehouse.CanEditReference = false;
			}
			
			if(CurrentUserSettings.Settings.DefaultWarehouse != null)
			{
				yentryrefWarehouse.Subject = UoW.GetById<Warehouse>(CurrentUserSettings.Settings.DefaultWarehouse.Id);
			}

			var filter = new EmployeeRepresentationFilterViewModel();
			filter.SetAndRefilterAtOnce(
				x => x.RestrictCategory = EmployeeCategory.driver,
				x => x.Status = EmployeeStatus.IsWorking
			);
			yentryrefDriver.RepresentationModel = new EmployeesVM(filter);
			dateperiodDocs.StartDate = DateTime.Today.AddDays(-7);
			dateperiodDocs.EndDate = DateTime.Today.AddDays(1);

			comboMovementStatus.ItemsEnum = typeof(MovementDocumentStatus);
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

		public MovementDocumentStatus? RestrictMovementStatus {
			get { return comboMovementStatus.SelectedItem as MovementDocumentStatus?; }
			set {
				comboMovementStatus.SelectedItem = value;
				comboMovementStatus.Sensitive = false;
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
			ylabelMovementStatus.Visible = RestrictDocumentType == DocumentType.MovementDocument;
			comboMovementStatus.Visible = RestrictDocumentType == DocumentType.MovementDocument;
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

		protected void OnComboMovementStatusChanged(object sender, EventArgs e)
		{
			OnRefiltered();
		}
	}
}

