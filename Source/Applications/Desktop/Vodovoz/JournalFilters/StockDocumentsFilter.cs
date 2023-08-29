using System;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Project.Services;
using QSOrmProject;
using QSOrmProject.RepresentationModel;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Documents.MovementDocuments;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Store;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Journals.FilterViewModels.Employees;
using Vodovoz.ViewModels.Journals.JournalFactories;

namespace Vodovoz
{
	[OrmDefaultIsFiltered(true)]
	public partial class StockDocumentsFilter : RepresentationFilterBase<StockDocumentsFilter>, ISingleUoWDialog
	{
		protected override void ConfigureWithUow()
		{
			enumcomboDocumentType.ItemsEnum = typeof(DocumentType);
			enumcomboDocumentType.HiddenItems = new[] { DocumentType.DeliveryDocument as object };
			var warehouseJournalFactory = new WarehouseJournalFactory();

			evmeWarehouse.SetEntityAutocompleteSelectorFactory(warehouseJournalFactory.CreateSelectorFactory());

			if(ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("user_have_access_only_to_warehouse_and_complaints")
			   && !ServicesConfig.CommonServices.UserService.GetCurrentUser().IsAdmin)
			{
				evmeWarehouse.Sensitive = evmeWarehouse.CanEditReference = false;
			}
			
			if(CurrentUserSettings.Settings.DefaultWarehouse != null)
			{
				evmeWarehouse.Subject = UoW.GetById<Warehouse>(CurrentUserSettings.Settings.DefaultWarehouse.Id);
			}

			var filter = new EmployeeFilterViewModel();
			filter.SetAndRefilterAtOnce(
				x => x.RestrictCategory = EmployeeCategory.driver,
				x => x.Status = EmployeeStatus.IsWorking
			);
			var driverFactory = new EmployeeJournalFactory(filter);
			evmeDriver.SetEntityAutocompleteSelectorFactory(driverFactory.CreateEmployeeAutocompleteSelectorFactory());
			dateperiodDocs.StartDate = DateTime.Today.AddDays(-7);
			dateperiodDocs.EndDate = DateTime.Today.AddDays(1);

			comboMovementStatus.ItemsEnum = typeof(MovementDocumentStatus);
			evmeDriver.Changed += (sender, args) => OnRefiltered();
			evmeWarehouse.Changed += (sender, args) => OnRefiltered();
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
			get => enumcomboDocumentType.SelectedItem as DocumentType?;
			set
			{
				enumcomboDocumentType.SelectedItem = value;
				enumcomboDocumentType.Sensitive = false;
			}
		}

		public MovementDocumentStatus? RestrictMovementStatus {
			get => comboMovementStatus.SelectedItem as MovementDocumentStatus?;
			set
			{
				comboMovementStatus.SelectedItem = value;
				comboMovementStatus.Sensitive = false;
			}
		}

		public Warehouse RestrictWarehouse {
			get => evmeWarehouse.Subject as Warehouse;
			set
			{
				evmeWarehouse.Subject = value;
				evmeWarehouse.Sensitive = false;
			}
		}

		public Employee RestrictDriver {
			get => evmeDriver.Subject as Employee;
			set
			{
				evmeDriver.Subject = value;
				evmeDriver.Sensitive = false;
			}
		}

		public DateTime? RestrictStartDate {
			get => dateperiodDocs.StartDateOrNull;
			set
			{
				dateperiodDocs.StartDateOrNull = value;
				dateperiodDocs.Sensitive = false;
			}
		}

		public DateTime? RestrictEndDate {
			get => dateperiodDocs.EndDateOrNull;
			set
			{
				dateperiodDocs.EndDateOrNull = value;
				dateperiodDocs.Sensitive = false;
			}
		}

		protected void OnEnumcomboDocumentTypeChanged(object sender, EventArgs e)
		{
			OnRefiltered();
			ylabelMovementStatus.Visible = RestrictDocumentType == DocumentType.MovementDocument;
			comboMovementStatus.Visible = RestrictDocumentType == DocumentType.MovementDocument;
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
