using System;
using System.Collections.Generic;
using System.Linq;
using Gamma.GtkWidgets;
using Gamma.Widgets;
using QS.Dialog.GtkUI;
using QS.DomainModel.Entity.EntityPermissions.EntityExtendedPermission;
using QS.DomainModel.NotifyChange;
using QS.DomainModel.UoW;
using QSOrmProject;
using QS.Validation;
using Vodovoz.Domain.Cash;
using Vodovoz.Domain.Logistic;
using Vodovoz.EntityRepositories.Employees;
using QS.Project.Services;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.Parameters;
using Vodovoz.PermissionExtensions;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Organizations;
using Vodovoz.EntityRepositories.Cash;
using Vodovoz.EntityRepositories.Counterparties;
using Vodovoz.TempAdapters;
using Vodovoz.EntityRepositories.Subdivisions;
using QS.Services;
using Gtk;

namespace Vodovoz
{
	public partial class CashIncomeDlg : QS.Dialog.Gtk.EntityDialogBase<Income>
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
		private static IParametersProvider _parametersProvider = new ParametersProvider();

		//Блокируем возможность выбора категории приходаЖ самовывоз - старый
		private const int excludeIncomeCategoryId = 3;
		private bool _canEdit = true;
		private readonly bool _canCreate;
		private readonly bool canEditRectroactively;
		private readonly bool canEditDate
			= ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("can_edit_cash_income_expense_date");

		private readonly IEmployeeRepository _employeeRepository = new EmployeeRepository();
		private readonly ICategoryRepository _categoryRepository = new CategoryRepository(_parametersProvider);
		private readonly IAccountableDebtsRepository _accountableDebtsRepository = new AccountableDebtsRepository();
		private readonly ICounterpartyRepository _counterpartyRepository = new CounterpartyRepository();
		private readonly ISubdivisionRepository _subdivisionsRepository = new SubdivisionRepository(new ParametersProvider());
		private readonly IUserService _userService = ServicesConfig.UserService;

		private RouteListCashOrganisationDistributor routeListCashOrganisationDistributor = 
			new RouteListCashOrganisationDistributor(
				new CashDistributionCommonOrganisationProvider(
					new OrganizationParametersProvider(_parametersProvider)),
				new RouteListItemCashDistributionDocumentRepository(),
				new OrderRepository());
		
		private IncomeCashOrganisationDistributor incomeCashOrganisationDistributor = 
			new IncomeCashOrganisationDistributor(
				new CashDistributionCommonOrganisationProvider(
					new OrganizationParametersProvider(_parametersProvider)));
		
		private List<Selectable<Expense>> _selectableAdvances = new List<Selectable<Expense>>();

		public CashIncomeDlg()
		{
			Build();
			UoWGeneric = UnitOfWorkFactory.CreateWithNewRoot<Income>();
			Entity.Casher = _employeeRepository.GetEmployeeForCurrentUser (UoW);
			if(Entity.Casher == null)
			{
				MessageDialogHelper.RunErrorDialog ("Ваш пользователь не привязан к действующему сотруднику, вы не можете создавать кассовые документы, так как некого указывать в качестве кассира.");
				FailInitialize = true;
				return;
			}

			_canCreate = permissionResult.CanCreate;
			if(!_canCreate) 
			{
				MessageDialogHelper.RunErrorDialog("Отсутствуют права на создание приходного ордера");
				FailInitialize = true;
				return;
			}
			if(!accessfilteredsubdivisionselectorwidget.Configure(UoW, false, typeof(Income))) {

				MessageDialogHelper.RunErrorDialog(accessfilteredsubdivisionselectorwidget.ValidationErrorMessage);
				FailInitialize = true;
				return;
			}

			Entity.Date = DateTime.Now;
			ConfigureDlg();
		}

		public CashIncomeDlg(int id)
		{
			Build();
			UoWGeneric = UnitOfWorkFactory.CreateForRoot<Income>(id);

			if(!accessfilteredsubdivisionselectorwidget.Configure(UoW, false, typeof(Income))) {

				MessageDialogHelper.RunErrorDialog(accessfilteredsubdivisionselectorwidget.ValidationErrorMessage);
				FailInitialize = true;
				return;
			}
			
			_canEdit = permissionResult.CanUpdate;

			var permissionValidator =
				new EntityExtendedPermissionValidator(PermissionExtensionSingletonStore.GetInstance(), _employeeRepository);
			canEditRectroactively =
				permissionValidator.Validate(
					typeof(Income), ServicesConfig.UserService.CurrentUserId, nameof(RetroactivelyClosePermission));
			
			ConfigureDlg();
		}

		public CashIncomeDlg(Expense advance) : this()
		{
			if(advance.Employee == null)
			{
				logger.Error("Аванс без сотрудника. Для него нельзя открыть диалог возврата.");
				FailInitialize = true;
				return;
			}

			Entity.TypeOperation = IncomeType.Return;
			Entity.ExpenseCategory = advance.ExpenseCategory;
			Entity.Employee = advance.Employee;
			Entity.Organisation = advance.Organisation;
			_selectableAdvances.Find(x => x.Value.Id == advance.Id).Selected = true;
		}

		public CashIncomeDlg(Income sub) : this(sub.Id) {}

		private bool CanEdit => (UoW.IsNew && _canCreate) ||
		                        (_canEdit && Entity.Date.Date == DateTime.Now.Date) ||
		                        canEditRectroactively;
		
		void ConfigureDlg()
		{
			if (!UoW.IsNew) {
				enumcomboOperation.Sensitive = false;
				specialListCmbOrganisation.Sensitive = false;
			}
			
			accessfilteredsubdivisionselectorwidget.OnSelected += Accessfilteredsubdivisionselectorwidget_OnSelected;
			if(Entity.RelatedToSubdivision != null) {
				accessfilteredsubdivisionselectorwidget.SelectIfPossible(Entity.RelatedToSubdivision);
			}

			enumcomboOperation.ItemsEnum = typeof(IncomeType);
			enumcomboOperation.Binding.AddBinding (Entity, s => s.TypeOperation, w => w.SelectedItem).InitializeFromSource ();

			var employeeFactory = new EmployeeJournalFactory();
			evmeCashier.SetEntityAutocompleteSelectorFactory(employeeFactory.CreateEmployeeAutocompleteSelectorFactory());
			evmeCashier.Binding.AddBinding(Entity, s => s.Casher, w => w.Subject).InitializeFromSource();

			evmeEmployee.SetEntityAutocompleteSelectorFactory(employeeFactory.CreateWorkingEmployeeAutocompleteSelectorFactory());
			evmeEmployee.Binding.AddBinding(Entity, s => s.Employee, w => w.Subject).InitializeFromSource();
			evmeEmployee.Changed += (sender, e) => FillDebts();

			var filterRL = new RouteListsFilter(UoW) {
				OnlyStatuses = new[] {RouteListStatus.EnRoute, RouteListStatus.OnClosing}
			};
			yEntryRouteList.RepresentationModel = new ViewModel.RouteListsVM(filterRL);
			yEntryRouteList.Binding.AddBinding(Entity, s => s.RouteListClosing, w => w.Subject).InitializeFromSource();
			yEntryRouteList.CanEditReference = ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("can_delete");

			yEntryRouteList.Hidden += YEntryRouteList_ValueOrVisibilityChanged;
			yEntryRouteList.Shown += YEntryRouteList_ValueOrVisibilityChanged;
			yEntryRouteList.ChangedByUser += YEntryRouteList_ValueOrVisibilityChanged;

			yentryClient.ItemsQuery = _counterpartyRepository.ActiveClientsQuery ();
			yentryClient.Binding.AddBinding (Entity, s => s.Customer, w => w.Subject).InitializeFromSource ();

			ydateDocument.Binding.AddBinding(Entity, s => s.Date, w => w.Date).InitializeFromSource();
			ydateDocument.Sensitive = canEditDate;

			NotifyConfiguration.Instance.BatchSubscribeOnEntity<ExpenseCategory>(
				s => 
					comboExpense.ItemsList = _categoryRepository.ExpenseCategories(UoW).Where(x => 
						x.ExpenseDocumentType != ExpenseInvoiceDocumentType.ExpenseInvoiceSelfDelivery)
			);
			comboExpense.ItemsList = 
				_categoryRepository.ExpenseCategories(UoW).Where(x => 
					x.ExpenseDocumentType != ExpenseInvoiceDocumentType.ExpenseInvoiceSelfDelivery);
			comboExpense.Binding.AddBinding (Entity, s => s.ExpenseCategory, w => w.SelectedItem).InitializeFromSource ();

			NotifyConfiguration.Instance.BatchSubscribeOnEntity<IncomeCategory>(
				s => 
					comboCategory.ItemsList = _categoryRepository.IncomeCategories(UoW).Where(x =>
						x.IncomeDocumentType != IncomeInvoiceDocumentType.IncomeInvoiceSelfDelivery && x.Id != excludeIncomeCategoryId)
			); 
			comboCategory.ItemsList = _categoryRepository.IncomeCategories(UoW).Where(x =>
				x.IncomeDocumentType != IncomeInvoiceDocumentType.IncomeInvoiceSelfDelivery && x.Id != excludeIncomeCategoryId);
			comboCategory.Binding.AddBinding (Entity, s => s.IncomeCategory, w => w.SelectedItem).InitializeFromSource ();

			specialListCmbOrganisation.ShowSpecialStateNot = true;
			specialListCmbOrganisation.ItemsList = UoW.GetAll<Organization>();
			specialListCmbOrganisation.Binding.AddBinding(Entity, e => e.Organisation, w => w.SelectedItem).InitializeFromSource();
			specialListCmbOrganisation.ItemSelected += SpecialListCmbOrganisationOnItemSelected;
			
			checkNoClose.Binding.AddBinding(Entity, e => e.NoFullCloseMode, w => w.Active);

			yspinMoney.Binding.AddBinding (Entity, s => s.Money, w => w.ValueAsDecimal).InitializeFromSource ();

			ytextviewDescription.Binding.AddBinding (Entity, s => s.Description, w => w.Buffer.Text).InitializeFromSource ();

			ytreeviewDebts.ColumnsConfig = ColumnsConfigFactory.Create<Selectable<Expense>> ()
				.AddColumn ("Закрыть").AddToggleRenderer (a => a.Selected).Editing()
				.AddColumn ("Дата").AddTextRenderer (a => a.Value.Date.ToString ())
				.AddColumn ("Получено").AddTextRenderer (a => a.Value.Money.ToString ("C"))
				.AddColumn ("Непогашено").AddTextRenderer (a => a.Value.UnclosedMoney.ToString ("C"))
				.AddColumn ("Статья").AddTextRenderer (a => a.Value.ExpenseCategory.Name)
				.AddColumn ("Основание").AddTextRenderer (a => a.Value.Description)
				.RowCells().AddSetter<CellRenderer>(
					(cell, node) =>
					{
						cell.Sensitive =
							node.Value.RouteListClosing == Entity.RouteListClosing
							|| _selectableAdvances.Count(s => s.Selected) == 0;
					})
				.Finish();
			UpdateSubdivision();

			if (!CanEdit)
			{
				table1.Sensitive = false;
				ytreeviewDebts.Sensitive = false;
				ytextviewDescription.Sensitive = false;
				buttonSave.Sensitive = false;
				accessfilteredsubdivisionselectorwidget.Sensitive = false;
			}
		}

		private void SpecialListCmbOrganisationOnItemSelected(object sender, ItemSelectedEventArgs e)
		{
			FillDebts();
		}

		public void FillForRoutelist(int routelistId)
		{
			var cashier = _employeeRepository.GetEmployeeForCurrentUser(UoW);
			if(cashier == null) {
				MessageDialogHelper.RunErrorDialog("Ваш пользователь не привязан к действующему сотруднику, вы не можете закрыть МЛ, так как некого указывать в качестве кассира.");
				return;
			}

			var rl = UoW.GetById<RouteList>(routelistId);

			Entity.IncomeCategory = _categoryRepository.RouteListClosingIncomeCategory(UoW);
			Entity.TypeOperation = IncomeType.DriverReport;
			Entity.Date = DateTime.Now;
			Entity.Casher = cashier;
			Entity.Employee = rl.Driver;
			Entity.Description = $"Закрытие МЛ №{rl.Id} от {rl.Date:d}";
			Entity.RouteListClosing = rl;
			Entity.RelatedToSubdivision = GetSubdivision(rl);
		}

		private Subdivision GetSubdivision(RouteList routeList)
		{
			var user = _userService.GetCurrentUser(UoW);
			var employee = _employeeRepository.GetEmployeeForCurrentUser(UoW);
			var subdivisions = _subdivisionsRepository.GetCashSubdivisionsAvailableForUser(UoW, user).ToList();
			if(subdivisions.Any(x => x.Id == employee.Subdivision.Id))
			{
				return employee.Subdivision;
			}
			if(routeList.ClosingSubdivision != null)
			{
				return routeList.ClosingSubdivision;
			}

			throw new InvalidOperationException("Невозможно подобрать подразделение кассы. " +
				"Возможно документ сохраняет не кассир или не правильно заполнены части города в МЛ.");
		}

		void Accessfilteredsubdivisionselectorwidget_OnSelected(object sender, EventArgs e)
		{
			UpdateSubdivision();
		}

		private void UpdateSubdivision()
		{
			if(accessfilteredsubdivisionselectorwidget.SelectedSubdivision != null && accessfilteredsubdivisionselectorwidget.NeedChooseSubdivision) {
				Entity.RelatedToSubdivision = accessfilteredsubdivisionselectorwidget.SelectedSubdivision;
			}
		}

		private void UpdateRouteListInfo()
		{
			SetRouteListControlsVisibility();
			SetRouteListReference();

			ytreeviewDebts.ItemsDataSource = _selectableAdvances;
		}

		private void SetRouteListControlsVisibility()
		{
			lblRouteList.Visible = _allowedToSpecifyRouteList;
			yEntryRouteList.Visible = _allowedToSpecifyRouteList;

			yEntryRouteList.Sensitive =
				Entity.TypeOperation == IncomeType.DriverReport;
		}

		private void SetRouteListReference()
		{
			if(!_allowedToSpecifyRouteList)
			{
				Entity.RouteListClosing = null;
				return;
			}

			var selectedAdvances = _selectableAdvances
				.Where(expense => expense.Selected)
				.Select(e => e.Value.RouteListClosing)
				.ToList();

			var selectedRouteListsCount = selectedAdvances.GroupBy(rl => rl?.Id).Count();
			if(selectedRouteListsCount != 1)
			{
				Entity.RouteListClosing = null;
				return;
			}

			Entity.RouteListClosing = selectedAdvances.FirstOrDefault();
		}

		private bool _allowedToSpecifyRouteList => 
			Entity.TypeOperation == IncomeType.DriverReport
			|| Entity.TypeOperation == IncomeType.Return;


		public override bool Save ()
		{
			if (Entity.TypeOperation == IncomeType.Return && UoW.IsNew && _selectableAdvances.Count > 0)
				Entity.PrepareCloseAdvance(_selectableAdvances.Where(x => x.Selected).Select(x => x.Value).ToList());

			var valid = new QSValidator<Income> (UoWGeneric.Root);
			if (valid.RunDlgIfNotValid ((Gtk.Window)this.Toplevel))
				return false;

			logger.Info ("Сохраняем Приходный ордер..."); 
			if (Entity.TypeOperation == IncomeType.Return && UoW.IsNew) {
				logger.Info ("Закрываем авансы...");
				Entity.CloseAdvances(UoW);
			}

			if (UoW.IsNew) {
				DistributeCash();
			}
			else {
				UpdateCashDistributionsDocuments();
			}

			UoWGeneric.Save();
			logger.Info ("Ok");

			if(Entity.RouteListClosing != null)
			{
				logger.Info("Обновляем сумму долга по МЛ...");
				UpdateRouteListDebt();
				logger.Info("Ok");
			}
			return true;
		}

		private void UpdateRouteListDebt()
		{
			if(Entity.RouteListClosing == null)
			{
				return;
			}

			var routeListDebt = Entity.RouteListClosing.CalculateRouteListDebt();

			if(Entity.RouteListClosing.RouteListDebt == routeListDebt)
			{
				logger.Info("Обновление суммы долга по МЛ не требуется...");
				return;
			}

			using(var uow =
				UnitOfWorkFactory.CreateForRoot<RouteList>(Entity.RouteListClosing.Id, "Обновление суммы долга по МЛ"))
			{
				uow.Root.RouteListDebt = routeListDebt;
				uow.Save();
			}
		}

		private void DistributeCash()
		{
			if (Entity.TypeOperation == IncomeType.DriverReport && 
			    Entity.IncomeCategory.Id == _categoryRepository.RouteListClosingIncomeCategory(UoW)?.Id) {
				routeListCashOrganisationDistributor.DistributeIncomeCash(UoW, Entity.RouteListClosing, Entity, Entity.Money);
			}
			else if (Entity.TypeOperation == IncomeType.Return) {
				incomeCashOrganisationDistributor.DistributeCashForIncome(UoW, Entity, Entity.Organisation);
			}
			else {
				incomeCashOrganisationDistributor.DistributeCashForIncome(UoW, Entity);
			}
		}
		
		private void UpdateCashDistributionsDocuments()
		{
			var editor = _employeeRepository.GetEmployeeForCurrentUser(UoW);
			var document = UoW.Session.QueryOver<CashOrganisationDistributionDocument>()
				.Where(x => x.Income.Id == Entity.Id).List().FirstOrDefault();

			if (document != null)
			{
				switch (document.Type)
				{
					case CashOrganisationDistributionDocType.IncomeCashDistributionDoc:
						incomeCashOrganisationDistributor.UpdateRecords(UoW, (IncomeCashDistributionDocument)document, Entity, editor);
						break;
					case CashOrganisationDistributionDocType.RouteListItemCashDistributionDoc:
						routeListCashOrganisationDistributor.UpdateIncomeCash(UoW, Entity.RouteListClosing, Entity, Entity.Money);
						break;
				}
			}
		}

		protected void OnButtonPrintClicked (object sender, EventArgs e)
		{
			if (UoWGeneric.HasChanges && CommonDialogs.SaveBeforePrint (typeof(Expense), "квитанции"))
				Save ();

			var reportInfo = new QS.Report.ReportInfo {
				Title = String.Format ("Квитанция №{0} от {1:d}", Entity.Id, Entity.Date),
				Identifier = "Cash.ReturnTicket",
				Parameters = new Dictionary<string, object> {
					{ "id",  Entity.Id }
				}
			};

			var report = new QSReport.ReportViewDlg (reportInfo);
			TabParent.AddTab (report, this, false);
		}

		protected void OnEnumcomboOperationEnumItemSelected (object sender, Gamma.Widgets.ItemSelectedEventArgs e)
		{
			buttonPrint.Sensitive = Entity.TypeOperation == IncomeType.Return;
			labelExpenseTitle.Visible = comboExpense.Visible = 
				ylabel1.Visible = specialListCmbOrganisation.Visible = Entity.TypeOperation == IncomeType.Return;
			labelIncomeTitle.Visible = comboCategory.Visible = Entity.TypeOperation != IncomeType.Return;

			labelClientTitle.Visible = yentryClient.Visible = Entity.TypeOperation == IncomeType.Payment;

			vboxDebts.Visible = checkNoClose.Visible = Entity.TypeOperation == IncomeType.Return && UoW.IsNew;
			yspinMoney.Sensitive = Entity.TypeOperation != IncomeType.Return;
			yspinMoney.ValueAsDecimal = 0;

			Entity.RouteListClosing = null;

			FillDebts();
			CheckOperation((IncomeType)e.SelectedItem);
			UpdateRouteListInfo();
		}

		void CheckOperation(IncomeType incomeType)
		{
			if(incomeType == IncomeType.DriverReport){
				Entity.IncomeCategory = UoW.GetById<IncomeCategory>(1);
			}
		}

		protected void OnComboExpenseItemSelected (object sender, Gamma.Widgets.ItemSelectedEventArgs e)
		{
			FillDebts ();
			UpdateRouteListInfo();
		}

		protected void FillDebts()
		{
			if (Entity.TypeOperation == IncomeType.Return && Entity.Employee != null)
			{
				var advances = _accountableDebtsRepository
					.UnclosedAdvance(UoW, Entity.Employee, Entity.ExpenseCategory, Entity.Organisation?.Id);
				_selectableAdvances = advances.Select (advance => new Selectable<Expense> (advance))
				.ToList ();
				_selectableAdvances.ForEach (advance => advance.SelectChanged += OnAdvanceSelectionChanged);
				ytreeviewDebts.ItemsDataSource = _selectableAdvances;
				return;
			}
			_selectableAdvances = new List<Selectable<Expense>>();
			ytreeviewDebts.ItemsDataSource = _selectableAdvances;
		}

		protected void OnAdvanceSelectionChanged(object sender, EventArgs args)
		{
			var selectedExpense = sender as Selectable<Expense>;

			if(checkNoClose.Active && selectedExpense.Selected)
			{
				_selectableAdvances
					.Where(x => x != selectedExpense)
					.ToList()
					.ForEach(x => x.SilentUnselect());
			}

			_selectableAdvances
					.Where(x => x.Value.RouteListClosing != selectedExpense.Value.RouteListClosing)
					.ToList()
					.ForEach(x => x.SilentUnselect());

			if(checkNoClose.Active)
				return;

			Entity.Money = _selectableAdvances.
				Where(expense=>expense.Selected)
				.Sum(se => se.Value.UnclosedMoney);

			UpdateRouteListInfo();
		}
			
		protected void OnCheckNoCloseToggled(object sender, EventArgs e)
		{
			if (_selectableAdvances == null)
				return;
			if(checkNoClose.Active && _selectableAdvances.Count(x => x.Selected) > 1)
			{
				MessageDialogHelper.RunWarningDialog("Частично вернуть можно только один аванс.");
				checkNoClose.Active = false;
				return;
			}
			yspinMoney.Sensitive = checkNoClose.Active;
			if(!checkNoClose.Active)
			{
				yspinMoney.ValueAsDecimal = _selectableAdvances.Where(x => x.Selected).Sum(x => x.Value.UnclosedMoney);
			}
		}

		void YEntryRouteList_ValueOrVisibilityChanged(object sender, EventArgs e)
		{
			if(yEntryRouteList.Visible && Entity.RouteListClosing != null){
				Entity.Description = $"Приход по МЛ №{Entity.RouteListClosing.Id} от {Entity.RouteListClosing.Date:d}";
				Entity.Employee = Entity.RouteListClosing.Driver;
			} else {
				Entity.Description = "";
				Entity.RouteListClosing = null;
				Entity.Employee = null;
			}
		}
		
		public override void Destroy()
		{
			NotifyConfiguration.Instance.UnsubscribeAll(this);
			base.Destroy();
		}
	}

	public class Selectable<T> {

		private bool selected;

		public bool Selected {
			get { return selected;}
			set{ selected = value;
				SelectChanged?.Invoke (this, EventArgs.Empty);
			}
		}

		public event EventHandler SelectChanged;

		public void SilentUnselect()
		{
			selected = false;
		}

		public T Value { get; set;}

		public Selectable(T obj)
		{
			Value = obj;
			Selected = false;
		}
	}
}
