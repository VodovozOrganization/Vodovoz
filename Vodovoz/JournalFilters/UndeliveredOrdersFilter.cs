using System;
using System.ComponentModel.DataAnnotations;
using Gamma.Widgets;
using QS.Dialog;
using QS.DomainModel.UoW;
using QSOrmProject;
using QSOrmProject.RepresentationModel;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Orders;
using Vodovoz.ViewModel;
using Vodovoz.Filters.ViewModels;
using QS.Project.Services;
using QS.Project.Journal.EntitySelector;
using Vodovoz.EntityRepositories.Subdivisions;
using Vodovoz.Parameters;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Journals.JournalViewModels.Client;

namespace Vodovoz.JournalFilters
{
	[OrmDefaultIsFiltered(true)]
	[System.ComponentModel.ToolboxItem(true)]
	public partial class UndeliveredOrdersFilter : RepresentationFilterBase<UndeliveredOrdersFilter>, ISingleUoWDialog
	{
		protected override void ConfigureWithUow()
		{
			yEnumCMBGuilty.ItemsEnum = typeof(GuiltyTypes);
			enumCMBUndeliveryStatus.ItemsEnum = typeof(UndeliveryStatus);
			enumCMBUndeliveryStatus.SelectedItem = UndeliveryStatus.InProcess;
			yEnumCMBActionWithInvoice.ItemsEnum = typeof(ActionsWithInvoice);
			ySpecCMBinProcessAt.ItemsList = ySpecCMBGuiltyDep.ItemsList =
				new SubdivisionRepository(new ParametersProvider()).GetAllDepartmentsOrderedByName(UoW);

			refOldOrder.RepresentationModel = new OrdersVM(new OrdersFilter(UoW));
			refOldOrder.CanEditReference = ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("can_delete");

			var driversFilter = new EmployeeRepresentationFilterViewModel();
			driversFilter.SetAndRefilterAtOnce(
				x => x.RestrictCategory = EmployeeCategory.driver,
				x => x.Status = EmployeeStatus.IsWorking
			);
			refDriver.RepresentationModel = new EmployeesVM(driversFilter);

			refClient.RepresentationModel = new CounterpartyVM(new CounterpartyFilter(UoW));
			entityVMEntryDeliveryPoint.SetEntityAutocompleteSelectorFactory(
				new DefaultEntityAutocompleteSelectorFactory<DeliveryPoint, DeliveryPointJournalViewModel, DeliveryPointJournalFilterViewModel>(ServicesConfig.CommonServices));

			var authorsFilter = new EmployeeRepresentationFilterViewModel();
			authorsFilter.SetAndRefilterAtOnce(
				x => x.RestrictCategory = EmployeeCategory.office,
				x => x.Status = EmployeeStatus.IsWorking
			);
			refOldOrderAuthor.RepresentationModel = refUndeliveryAuthor.RepresentationModel = new EmployeesVM(authorsFilter);

			dateperiodOldOrderDate.StartDateOrNull = DateTime.Today.AddMonths(-1);
			dateperiodOldOrderDate.EndDateOrNull = DateTime.Today.AddMonths(1);
			chkProblematicCases.Toggled += (sender, e) => {
				if(chkProblematicCases.Active) {
					yEnumCMBGuilty.SelectedItemOrNull = null;
					ySpecCMBGuiltyDep.Visible = lblGuiltyDep.Visible = false;
				}
				yEnumCMBGuilty.Sensitive = !chkProblematicCases.Active;
				OnRefiltered();
			};
			
			//Подразделение
			var employeeSelectorFactory = new EmployeeJournalFactory().CreateEmployeeAutocompleteSelectorFactory();
			var subdivisionSelectorFactory =
				new SubdivisionJournalFactory().CreateDefaultSubdivisionAutocompleteSelectorFactory(employeeSelectorFactory);
			
			AuthorSubdivisionEntityviewmodelentry.SetEntityAutocompleteSelectorFactory(subdivisionSelectorFactory);
			
			AuthorSubdivisionEntityviewmodelentry.Changed += AuthorSubdivisionEntityviewmodelentryOnChanged;
		}

		public void ResetFilter(){
			enumCMBUndeliveryStatus.SelectedItem = SpecialComboState.All;
			dateperiodOldOrderDate.StartDateOrNull = null;
			dateperiodOldOrderDate.EndDateOrNull = null;
		}

		public UndeliveredOrdersFilter(IUnitOfWork uow) : this()
		{
			UoW = uow;
		}

		public UndeliveredOrdersFilter()
		{
			this.Build();
		}

		public bool IsProblematicCasesChkActive => chkProblematicCases.Active;
		public GuiltyTypes[] ExcludingGuiltiesForProblematicCases => new GuiltyTypes[] { GuiltyTypes.Client, GuiltyTypes.None };

		public Order RestrictOldOrder {
			get => refOldOrder.Subject as Order;
			set {
				refOldOrder.Subject = value;
				refOldOrder.Sensitive = false;
			}
		}

		public Employee RestrictDriver {
			get => refDriver.Subject as Employee;
			set {
				refDriver.Subject = value;
				refDriver.Sensitive = false;
			}
		}
		
		public Subdivision AuthorSubdivision {
			get => AuthorSubdivisionEntityviewmodelentry.Subject as Subdivision;
			set {
				AuthorSubdivisionEntityviewmodelentry.Subject = value;
				AuthorSubdivisionEntityviewmodelentry.Sensitive = false;
			}
		}

		public Counterparty RestrictClient {
			get => refClient.Subject as Counterparty;
			set {
				refClient.Subject = value;
				refClient.Sensitive = false;
			}
		}

		public DeliveryPoint RestrictAddress {
			get => entityVMEntryDeliveryPoint.Subject as DeliveryPoint;
			set {
				entityVMEntryDeliveryPoint.Subject = value;
				entityVMEntryDeliveryPoint.Sensitive = false;
			}
		}

		public Employee RestrictOldOrderAuthor {
			get => refOldOrderAuthor.Subject as Employee;
			set {
				refOldOrderAuthor.Subject = value;
				refOldOrderAuthor.Sensitive = false;
			}
		}

		public DateTime? RestrictOldOrderStartDate {
			get => dateperiodOldOrderDate.StartDateOrNull;
			set {
				dateperiodOldOrderDate.StartDateOrNull = value;
				dateperiodOldOrderDate.Sensitive = false;
			}
		}

		public DateTime? RestrictOldOrderEndDate {
			get => dateperiodOldOrderDate.EndDateOrNull;
			set {
				dateperiodOldOrderDate.EndDateOrNull = value;
				dateperiodOldOrderDate.Sensitive = false;
			}
		}

		public DateTime? RestrictNewOrderStartDate {
			get => dateperiodNewOrderDate.StartDateOrNull;
			set {
				dateperiodNewOrderDate.StartDateOrNull = value;
				dateperiodNewOrderDate.Sensitive = false;
			}
		}

		public DateTime? RestrictNewOrderEndDate {
			get => dateperiodNewOrderDate.EndDateOrNull;
			set {
				dateperiodNewOrderDate.EndDateOrNull = value;
				dateperiodNewOrderDate.Sensitive = false;
			}
		}

		public GuiltyTypes? RestrictGuiltySide {
			get => yEnumCMBGuilty.SelectedItem as GuiltyTypes?;
			set {
				yEnumCMBGuilty.SelectedItem = value;
				yEnumCMBGuilty.Sensitive = false;
			}
		}

		public Subdivision RestrictGuiltyDepartment {
			get => ySpecCMBGuiltyDep.SelectedItem as Subdivision;
			set {
				ySpecCMBGuiltyDep.SelectedItem = value;
				ySpecCMBGuiltyDep.Sensitive = false;
			}
		}

		public Subdivision RestrictInProcessAtDepartment {
			get => ySpecCMBinProcessAt.SelectedItem as Subdivision;
			set {
				ySpecCMBinProcessAt.SelectedItem = value;
				ySpecCMBinProcessAt.Sensitive = false;
			}
		}

		public ActionsWithInvoice? RestrictActionsWithInvoice {
			get => yEnumCMBActionWithInvoice.SelectedItem as ActionsWithInvoice?;
			set {
				yEnumCMBActionWithInvoice.SelectedItem = value;
				yEnumCMBActionWithInvoice.Sensitive = false;
			}
		}

		public UndeliveryStatus? RestrictUndeliveryStatus {
			get => enumCMBUndeliveryStatus.SelectedItem as UndeliveryStatus?;
			set {
				enumCMBUndeliveryStatus.SelectedItem = value;
				enumCMBUndeliveryStatus.Sensitive = false;
			}
		}

		public Employee RestrictUndeliveryAuthor {
			get => refUndeliveryAuthor.Subject as Employee;
			set {
				refUndeliveryAuthor.Subject = value;
				refUndeliveryAuthor.Sensitive = false;
			}
		}

		public bool? NewInvoiceCreated { get; set; }

		protected void OnRefOldOrderChanged(object sender, EventArgs e)
		{
			OnRefiltered();
		}

		protected void OnRefDriverChanged(object sender, EventArgs e)
		{
			OnRefiltered();
		}

		protected void OnRefClientChanged(object sender, EventArgs e)
		{
			OnRefiltered();
		}

		protected void OnRefOldOrderAuthorChanged(object sender, EventArgs e)
		{
			OnRefiltered();
		}

		protected void OnYEnumCMBActionWithInvoiceEnumItemSelected(object sender, Gamma.Widgets.ItemSelectedEventArgs e)
		{
			switch(e.SelectedItem) {
				case ActionsWithInvoice.createdNew:
					NewInvoiceCreated = true;
					break;
				case ActionsWithInvoice.notCreated:
					NewInvoiceCreated = false;
					break;
				default:
					NewInvoiceCreated = null;
					break;
			}
			OnRefiltered();
		}

		protected void OnEnumCMBUndeliveryStatusEnumItemSelected(object sender, Gamma.Widgets.ItemSelectedEventArgs e)
		{
			OnRefiltered();
		}

		protected void OnYEnumCMBGuiltyEnumItemSelected(object sender, Gamma.Widgets.ItemSelectedEventArgs e)
		{
			switch(e.SelectedItem) {
				case GuiltyTypes.Department:
					ySpecCMBGuiltyDep.Visible = lblGuiltyDep.Visible = true;
					break;
				default:
					ySpecCMBGuiltyDep.Visible = lblGuiltyDep.Visible = false;
					ySpecCMBGuiltyDep.SelectedItem = null;
					break;
			}
			OnRefiltered();
		}

		protected void OnYSpecCMBGuiltyDepItemSelected(object sender, Gamma.Widgets.ItemSelectedEventArgs e)
		{
			OnRefiltered();
		}

		protected void OnDateperiodOldOrderDatePeriodChanged(object sender, EventArgs e)
		{
			OnRefiltered();
		}

		protected void OnDateperiodNewOrderDatePeriodChanged(object sender, EventArgs e)
		{
			OnRefiltered();
		}

		protected void OnRefUndeliveryAuthorChanged(object sender, EventArgs e)
		{
			OnRefiltered();
		}

		protected void OnYSpecCMBinProcessAtItemSelected(object sender, Gamma.Widgets.ItemSelectedEventArgs e)
		{
			OnRefiltered();
		}

		protected void OnEntityVMEntryDeliveryPointChanged(object sender, EventArgs e)
		{
			OnRefiltered();
		}
		
		private void AuthorSubdivisionEntityviewmodelentryOnChanged(object sender, EventArgs e)
		{
			OnRefiltered();
		}
	}

	public enum ActionsWithInvoice
	{
		[Display(Name = "В раскладке")]
		createdNew,
		[Display(Name = "Удалена")]
		notCreated
	}
}
