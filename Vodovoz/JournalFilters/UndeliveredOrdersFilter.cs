using System;
using System.ComponentModel.DataAnnotations;
using QSOrmProject;
using QSOrmProject.RepresentationModel;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Orders;
using Vodovoz.ViewModel;

namespace Vodovoz.JournalFilters
{
	[OrmDefaultIsFiltered(true)]
	[System.ComponentModel.ToolboxItem(true)]
	public partial class UndeliveredOrdersFilter : RepresentationFilterBase<UndeliveredOrdersFilter>
	{
		protected override void OnUoWSet()
		{
			Configure();
		}

		void Configure(){
			yEnumCMBGuilty.ItemsEnum = typeof(GuiltyTypes);
			enumCMBUndeliveryStatus.ItemsEnum = typeof(UndeliveryStatus);
			enumCMBUndeliveryStatus.SelectedItem = UndeliveryStatus.InProcess;
			yEnumCMBActionWithInvoice.ItemsEnum = typeof(ActionsWithInvoice);
			ySpecCMBGuiltyDep.ItemsList = Repository.EmployeeRepository.Subdivisions(UoW);

			refOldOrder.RepresentationModel = new OrdersVM(new OrdersFilter(UoW));

			var DriversFilter = new EmployeeFilter(UoW);
			DriversFilter.RestrictCategory = EmployeeCategory.driver;
			refDriver.RepresentationModel = new EmployeesVM(DriversFilter);

			refClient.RepresentationModel = new CounterpartyVM(new CounterpartyFilter(UoW));
			refDeliveryPoint.RepresentationModel = new DeliveryPointsVM(new DeliveryPointFilter(UoW));

			var AuthorsFilter = new EmployeeFilter(UoW);
			AuthorsFilter.RestrictCategory = EmployeeCategory.office;
			refOldOrderAuthor.RepresentationModel = refUndeliveryAuthor.RepresentationModel = new EmployeesVM(AuthorsFilter);

			dateperiodOldOrderDate.StartDateOrNull = DateTime.Today.AddMonths(-1);
			dateperiodOldOrderDate.EndDateOrNull = DateTime.Today.AddMonths(1);
		}

		public UndeliveredOrdersFilter(IUnitOfWork uow) : this()
		{
			UoW = uow;
		}

		public UndeliveredOrdersFilter()
		{
			this.Build();
		}

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

		public Counterparty RestrictClient{
			get => refClient.Subject as Counterparty;
			set {
				refClient.Subject = value;
				refClient.Sensitive = false;
			}
		}

		public DeliveryPoint RestrictAddress{
			get => refDeliveryPoint.Subject as DeliveryPoint;
			set {
				refDeliveryPoint.Subject = value;
				refDeliveryPoint.Sensitive = false;
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
		}

		protected void OnYEnumCMBGuiltyEnumItemSelected(object sender, Gamma.Widgets.ItemSelectedEventArgs e)
		{
			switch(e.SelectedItem){
				case GuiltyTypes.Department:
					ySpecCMBGuiltyDep.Visible = lblGuiltyDep.Visible = true;
					break;
				default:
					ySpecCMBGuiltyDep.Visible = lblGuiltyDep.Visible = false;
					ySpecCMBGuiltyDep.SelectedItem = null;
					break;
			}
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
