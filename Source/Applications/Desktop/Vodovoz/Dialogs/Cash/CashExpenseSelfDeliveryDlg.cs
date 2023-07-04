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
using Vodovoz.Domain.Orders;
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
using Vodovoz.Parameters;
using Vodovoz.TempAdapters;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Dialogs.Cash
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class CashExpenseSelfDeliveryDlg : EntityDialogBase<Expense>
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
		private bool _canEdit = true;
		private readonly bool _canCreate;
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
						ErrorReporter.Instance);
				}
				return callTaskWorker;
			}
			set { callTaskWorker = value; }
		}

		public CashExpenseSelfDeliveryDlg()
		{
			Build();
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

			_canCreate = permissionResult.CanCreate;
			if(!_canCreate) {
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

		public CashExpenseSelfDeliveryDlg(Order order) : this()
		{
			Entity.Order = UoW.GetById<Order>(order.Id);
		}

		public CashExpenseSelfDeliveryDlg(int id)
		{
			Build();
			UoWGeneric = UnitOfWorkFactory.CreateForRoot<Expense>(id);

			if(!accessfilteredsubdivisionselectorwidget.Configure(UoW, false, typeof(Expense))) {

				MessageDialogHelper.RunErrorDialog(accessfilteredsubdivisionselectorwidget.ValidationErrorMessage);
				FailInitialize = true;
				return;
			}
			accessfilteredsubdivisionselectorwidget.OnSelected += Accessfilteredsubdivisionselectorwidget_OnSelected;

			_canEdit = permissionResult.CanUpdate;
			
			var permmissionValidator =
				new EntityExtendedPermissionValidator(PermissionExtensionSingletonStore.GetInstance(), _employeeRepository);
			
			canEditRectroactively =
				permmissionValidator.Validate(typeof(Expense), ServicesConfig.UserService.CurrentUserId, nameof(RetroactivelyClosePermission));

			ConfigureDlg();
		}

		public CashExpenseSelfDeliveryDlg(Expense sub) : this(sub.Id) { }

		private bool CanEdit => (UoW.IsNew && _canCreate) ||
		                        (_canEdit && Entity.Date.Date == DateTime.Now.Date) ||
		                        canEditRectroactively;

		void ConfigureDlg()
		{
			TabName = "Расходный кассовый ордер самовывоза";

			Entity.TypeDocument = ExpenseInvoiceDocumentType.ExpenseInvoiceSelfDelivery;

			enumcomboOperation.ItemsEnum = typeof(ExpenseType);
			enumcomboOperation.Binding.AddBinding(Entity, s => s.TypeOperation, w => w.SelectedItem).InitializeFromSource();
			enumcomboOperation.Sensitive = false;
			Entity.TypeOperation = ExpenseType.ExpenseSelfDelivery;

			var orderFactory = new OrderSelectorFactory();
			evmeOrder.SetEntityAutocompleteSelectorFactory(orderFactory.CreateCashSelfDeliveryOrderAutocompleteSelector());
			evmeOrder.Binding.AddBinding(Entity, x => x.Order, x => x.Subject).InitializeFromSource();
			evmeOrder.Changed += OnYentryOrderChanged;

			var employeeFactory = new EmployeeJournalFactory();
			evmeCashier.SetEntityAutocompleteSelectorFactory(employeeFactory.CreateEmployeeAutocompleteSelectorFactory());
			evmeCashier.Binding.AddBinding(Entity, s => s.Casher, w => w.Subject).InitializeFromSource();
			evmeCashier.Sensitive = false;

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
			var contextItems = new Dictionary<object, object> { { "IsSelfDelivery", true } };
			var context = new ValidationContext(Entity, null, contextItems);
			var validator = new ObjectValidator(new GtkValidationViewFactory());
			if(!validator.Validate(Entity, context))
			{
				return false;
			}

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
