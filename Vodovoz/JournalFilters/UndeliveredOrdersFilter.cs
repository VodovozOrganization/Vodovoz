using System;
using System.ComponentModel.DataAnnotations;
using QS.Dialog;
using QS.DomainModel.UoW;
using QSOrmProject;
using QSOrmProject.RepresentationModel;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Orders;
using QS.Project.Services;
using Vodovoz.EntityRepositories.Subdivisions;
using Vodovoz.Parameters;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Journals.FilterViewModels.Employees;

namespace Vodovoz.JournalFilters
{
	[OrmDefaultIsFiltered(true)]
	[System.ComponentModel.ToolboxItem(true)]
	[Obsolete("Похоже, что старый журнал UndeliveredOrdersVM нигде не используется и фильтр с журналом можно удалить")]
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

			var orderFactory = new OrderSelectorFactory();
			evmeOldOrder.SetEntityAutocompleteSelectorFactory(orderFactory.CreateOrderAutocompleteSelectorFactory());
			evmeOldOrder.CanEditReference = ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("can_delete");

			var driversFilter = new EmployeeFilterViewModel();
			driversFilter.SetAndRefilterAtOnce(
				x => x.RestrictCategory = EmployeeCategory.driver,
				x => x.Status = EmployeeStatus.IsWorking
			);
			var driverFactory = new EmployeeJournalFactory(driversFilter);
			evmeDriver.SetEntityAutocompleteSelectorFactory(driverFactory.CreateEmployeeAutocompleteSelectorFactory());

			var clientFactory = new CounterpartyJournalFactory();
			evmeClient.SetEntityAutocompleteSelectorFactory(clientFactory.CreateCounterpartyAutocompleteSelectorFactory());
			var dpFactory = new DeliveryPointJournalFactory();
			entityVMEntryDeliveryPoint.SetEntityAutocompleteSelectorFactory(dpFactory.CreateDeliveryPointAutocompleteSelectorFactory());

			var oldAuthorsFilter = new EmployeeFilterViewModel();
			oldAuthorsFilter.SetAndRefilterAtOnce(
				x => x.RestrictCategory = EmployeeCategory.office,
				x => x.Status = EmployeeStatus.IsWorking
			);
			var oldAuthorFactory = new EmployeeJournalFactory(oldAuthorsFilter);
			evmeOldOrder.SetEntityAutocompleteSelectorFactory(oldAuthorFactory.CreateEmployeeAutocompleteSelectorFactory());
			var undeliveryAuthorFilter = new EmployeeFilterViewModel();
			undeliveryAuthorFilter.SetAndRefilterAtOnce(
				x => x.RestrictCategory = EmployeeCategory.office,
				x => x.Status = EmployeeStatus.IsWorking
			);
			var undeliveryAuthorFactory = new EmployeeJournalFactory(undeliveryAuthorFilter);
			evmeUndeliveryAuthor.SetEntityAutocompleteSelectorFactory(undeliveryAuthorFactory.CreateEmployeeAutocompleteSelectorFactory());

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
			
			AuthorSubdivisionEntityviewmodelentry.Changed += (sender, args) => OnRefiltered();
			evmeUndeliveryAuthor.Changed += (sender, args) => OnRefiltered();
			evmeOrderAuthor.Changed += (sender, args) => OnRefiltered();
			evmeClient.Changed += (sender, args) => OnRefiltered();
			evmeDriver.Changed += (sender, args) => OnRefiltered();
			evmeOldOrder.Changed += (sender, args) => OnRefiltered();

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
			get => evmeOldOrder.Subject as Order;
			set {
				evmeOldOrder.Subject = value;
				evmeOldOrder.Sensitive = false;
			}
		}

		public Employee RestrictDriver {
			get => evmeDriver.Subject as Employee;
			set {
				evmeDriver.Subject = value;
				evmeDriver.Sensitive = false;
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
			get => evmeClient.Subject as Counterparty;
			set {
				evmeClient.Subject = value;
				evmeClient.Sensitive = false;
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
			get => evmeOrderAuthor.Subject as Employee;
			set {
				evmeOrderAuthor.Subject = value;
				evmeOrderAuthor.Sensitive = false;
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

		public UndeliveryStatus? RestrictUndeliveryStatus {
			get => enumCMBUndeliveryStatus.SelectedItem as UndeliveryStatus?;
			set {
				enumCMBUndeliveryStatus.SelectedItem = value;
				enumCMBUndeliveryStatus.Sensitive = false;
			}
		}

		public Employee RestrictUndeliveryAuthor {
			get => evmeUndeliveryAuthor.Subject as Employee;
			set {
				evmeUndeliveryAuthor.Subject = value;
				evmeUndeliveryAuthor.Sensitive = false;
			}
		}

		public bool? NewInvoiceCreated { get; set; }

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

		protected void OnYSpecCMBinProcessAtItemSelected(object sender, Gamma.Widgets.ItemSelectedEventArgs e)
		{
			OnRefiltered();
		}

		protected void OnEntityVMEntryDeliveryPointChanged(object sender, EventArgs e)
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
