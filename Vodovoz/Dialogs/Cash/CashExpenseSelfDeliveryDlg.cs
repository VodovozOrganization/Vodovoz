using System;
using System.Collections.Generic;
using QS.Dialog.Gtk;
using QS.Dialog.GtkUI;
using QS.DomainModel.Entity.EntityPermissions.EntityExtendedPermission;
using QS.DomainModel.NotifyChange;
using QS.DomainModel.UoW;
using QSOrmProject;
using QS.Validation;
using Vodovoz.Domain.Cash;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders;
using Vodovoz.ViewModel;
using QS.Services;
using Vodovoz.Tools.CallTasks;
using Vodovoz.EntityRepositories.CallTasks;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.Core.DataService;
using QS.Project.Services;
using Vodovoz.EntityRepositories.Documents;
using Vodovoz.PermissionExtensions;
using Vodovoz.Tools;
using System.Linq;
using Vodovoz.EntityRepositories.Cash;
using Vodovoz.JournalFilters;
using Vodovoz.Parameters;

namespace Vodovoz.Dialogs.Cash
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class CashExpenseSelfDeliveryDlg : EntityDialogBase<Expense>
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
		private bool canEdit = true;
		private readonly bool canCreate;
		private readonly bool canEditRectroactively;
		private readonly IEmployeeRepository _employeeRepository = new EmployeeRepository();
		private readonly ICategoryRepository _categoryRepository = new CategoryRepository(new ParametersProvider());
		private readonly ICashRepository _cashRepository = new CashRepository();
        private List<ExpenseCategory> expenseCategoryList = new List<ExpenseCategory>();
		private SelfDeliveryCashOrganisationDistributor selfDeliveryCashOrganisationDistributor = 
			new SelfDeliveryCashOrganisationDistributor(new SelfDeliveryCashDistributionDocumentRepository());
		
		private CallTaskWorker callTaskWorker;
		public virtual CallTaskWorker CallTaskWorker {
			get {
				if(callTaskWorker == null) {
					callTaskWorker = new CallTaskWorker(
						CallTaskSingletonFactory.GetInstance(),
						new CallTaskRepository(),
						new OrderRepository(),
						_employeeRepository,
						new BaseParametersProvider(new ParametersProvider()),
						ServicesConfig.CommonServices.UserService,
						SingletonErrorReporter.Instance);
				}
				return callTaskWorker;
			}
			set { callTaskWorker = value; }
		}

		public CashExpenseSelfDeliveryDlg(IPermissionService permissionService)
		{
			this.Build();
			UoWGeneric = UnitOfWorkFactory.CreateWithNewRoot<Expense>();
			Entity.Casher = _employeeRepository.GetEmployeeForCurrentUser(UoW);
            expenseCategoryList.AddRange(_categoryRepository.ExpenseSelfDeliveryCategories(UoW));
            if (Entity.Id == 0){
                Entity.ExpenseCategory = expenseCategoryList.FirstOrDefault();
            }
			if(Entity.Casher == null) {
				MessageDialogHelper.RunErrorDialog("Ваш пользователь не привязан к действующему сотруднику, вы не можете создавать кассовые документы, так как некого указывать в качестве кассира.");
				FailInitialize = true;
				return;
			}

			var userPermission = permissionService.ValidateUserPermission(typeof(Expense), ServicesConfig.UserService.CurrentUserId);
			canCreate = userPermission.CanCreate;
			if(!userPermission.CanCreate) {
				MessageDialogHelper.RunErrorDialog("Отсутствуют права на создание приходного ордера");
				FailInitialize = true;
				return;
			}

			if(!accessfilteredsubdivisionselectorwidget.Configure(UoW, false, typeof(Expense))) {

				MessageDialogHelper.RunErrorDialog(accessfilteredsubdivisionselectorwidget.ValidationErrorMessage);
				FailInitialize = true;
				return;
			}
			accessfilteredsubdivisionselectorwidget.OnSelected += Accessfilteredsubdivisionselectorwidget_OnSelected;

			Entity.Date = DateTime.Now;
			ConfigureDlg();
		}

		public CashExpenseSelfDeliveryDlg(Order order, IPermissionService permissionService) : this(permissionService)
		{
			Entity.Order = UoW.GetById<Order>(order.Id);
		}

		public CashExpenseSelfDeliveryDlg(int id, IPermissionService permissionService)
		{
			this.Build();
			UoWGeneric = UnitOfWorkFactory.CreateForRoot<Expense>(id);
			var userPermission = permissionService.ValidateUserPermission(typeof(Expense), ServicesConfig.UserService.CurrentUserId);
			if(!userPermission.CanRead) {
				MessageDialogHelper.RunErrorDialog("Отсутствуют права на просмотр приходного ордера");
				FailInitialize = true;
				return;
			}

			if(!accessfilteredsubdivisionselectorwidget.Configure(UoW, false, typeof(Expense))) {

				MessageDialogHelper.RunErrorDialog(accessfilteredsubdivisionselectorwidget.ValidationErrorMessage);
				FailInitialize = true;
				return;
			}
			accessfilteredsubdivisionselectorwidget.OnSelected += Accessfilteredsubdivisionselectorwidget_OnSelected;

			canEdit = userPermission.CanUpdate;
			
			var permmissionValidator =
				new EntityExtendedPermissionValidator(PermissionExtensionSingletonStore.GetInstance(), _employeeRepository);
			
			canEditRectroactively =
				permmissionValidator.Validate(typeof(Expense), ServicesConfig.UserService.CurrentUserId, nameof(RetroactivelyClosePermission));

			ConfigureDlg();
		}

		public CashExpenseSelfDeliveryDlg(Expense sub, IPermissionService permissionService) : this(sub.Id, permissionService) { }

		private bool CanEdit => (UoW.IsNew && canCreate) ||
		                        (canEdit && Entity.Date.Date == DateTime.Now.Date) ||
		                        canEditRectroactively;

		void ConfigureDlg()
		{
			TabName = "Расходный кассовый ордер самовывоза";

			Entity.TypeDocument = ExpenseInvoiceDocumentType.ExpenseInvoiceSelfDelivery;

			enumcomboOperation.ItemsEnum = typeof(ExpenseType);
			enumcomboOperation.Binding.AddBinding(Entity, s => s.TypeOperation, w => w.SelectedItem).InitializeFromSource();
			enumcomboOperation.Sensitive = false;
			Entity.TypeOperation = ExpenseType.ExpenseSelfDelivery;

			var filterOrders = new OrdersFilter(UoW);
			filterOrders.SetAndRefilterAtOnce(
				x => x.RestrictStatus = OrderStatus.WaitForPayment,
				x => x.AllowPaymentTypes = new PaymentType[] { PaymentType.cash, PaymentType.BeveragesWorld },
				x => x.RestrictSelfDelivery = true,
				x => x.RestrictWithoutSelfDelivery = false,
				x => x.RestrictHideService = true,
				x => x.RestrictOnlyService = false
			);
			yentryOrder.RepresentationModel = new OrdersVM(filterOrders);
			yentryOrder.Binding.AddBinding(Entity, x => x.Order, x => x.Subject).InitializeFromSource();

			var filterCasher = new EmployeeRepresentationFilterViewModel();
			filterCasher.Status = Domain.Employees.EmployeeStatus.IsWorking;
			yentryCasher.RepresentationModel = new EmployeesVM(filterCasher);
			yentryCasher.Binding.AddBinding(Entity, s => s.Casher, w => w.Subject).InitializeFromSource();
			yentryCasher.Sensitive = false;

			ydateDocument.Binding.AddBinding(Entity, s => s.Date, w => w.Date).InitializeFromSource();

			NotifyConfiguration.Instance.BatchSubscribeOnEntity<ExpenseCategory>(
				s => comboExpense.ItemsList = _categoryRepository.ExpenseSelfDeliveryCategories(UoW)
			);
			comboExpense.ItemsList = expenseCategoryList;
			comboExpense.Binding.AddBinding(Entity, s => s.ExpenseCategory, w => w.SelectedItem).InitializeFromSource();

			yspinMoney.Binding.AddBinding(Entity, s => s.Money, w => w.ValueAsDecimal).InitializeFromSource();

			ytextviewDescription.Binding.AddBinding(Entity, s => s.Description, w => w.Buffer.Text).InitializeFromSource();

			UpdateSubdivision();

			if(!CanEdit) {
				table1.Sensitive = false;
				accessfilteredsubdivisionselectorwidget.Sensitive = false;
				buttonSave.Sensitive = false;
				ytextviewDescription.Editable = false;
			}
		}

		void Accessfilteredsubdivisionselectorwidget_OnSelected(object sender, EventArgs e)
		{
			UpdateSubdivision();
		}

		private void UpdateSubdivision()
		{
			Entity.RelatedToSubdivision = accessfilteredsubdivisionselectorwidget.SelectedSubdivision;
		}

		public override bool Save()
		{
			var validationContext = new Dictionary<object, object> {{"IsSelfDelivery", true}};
			var valid = new QSValidator<Expense>(UoWGeneric.Root, validationContext);
			if(valid.RunDlgIfNotValid((Gtk.Window)this.Toplevel))
				return false;

			Entity.AcceptSelfDeliveryPaid(CallTaskWorker);
			
			if (UoW.IsNew)
			{
				logger.Info("Создаем документ распределения налички по юр лицу...");
				selfDeliveryCashOrganisationDistributor.DistributeExpenseCash(UoW, Entity.Order, Entity);
			}
			else
			{ 
				logger.Info("Меняем документ распределения налички по юр лицу...");
				selfDeliveryCashOrganisationDistributor.UpdateRecords(UoW, Entity.Order, Entity,
					_employeeRepository.GetEmployeeForCurrentUser(UoW));
			}

			logger.Info("Сохраняем расходный ордер...");
			UoWGeneric.Save();
			logger.Info("Ok");
			return true;
		}

		protected void OnButtonPrintClicked(object sender, EventArgs e)
		{
			if(UoWGeneric.HasChanges && CommonDialogs.SaveBeforePrint(typeof(Expense), "квитанции"))
				Save();

			var reportInfo = new QS.Report.ReportInfo {
				Title = String.Format("Квитанция №{0} от {1:d}", Entity.Id, Entity.Date),
				Identifier = "Cash.Expense",
				Parameters = new Dictionary<string, object> {
					{ "id",  Entity.Id }
				}
			};

			var report = new QSReport.ReportViewDlg(reportInfo);
			TabParent.AddTab(report, this, false);
		}

		protected void OnYentryOrderChanged(object sender, EventArgs e)
		{
			if (Entity.Order != null)
			{
				Entity.FillFromOrder(UoW, _cashRepository);
			}
		}
		
		public override void Destroy()
		{
			NotifyConfiguration.Instance.UnsubscribeAll(this);
			base.Destroy();
		}
	}
}
