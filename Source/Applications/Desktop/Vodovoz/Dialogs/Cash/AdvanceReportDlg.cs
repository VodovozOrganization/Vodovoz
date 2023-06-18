using System;
using System.Collections.Generic;
using System.Linq;
using Gamma.GtkWidgets;
using QS.Dialog.GtkUI;
using QS.DomainModel.Entity.EntityPermissions.EntityExtendedPermission;
using QS.DomainModel.UoW;
using QSProjectsLib;
using QS.Validation;
using Vodovoz.Domain.Cash;
using Vodovoz.Domain.Employees;
using Vodovoz.EntityRepositories.Employees;
using QS.DomainModel.NotifyChange;
using QS.Project.Services;
using Vodovoz.PermissionExtensions;
using Vodovoz.Domain.Organizations;
using Vodovoz.EntityRepositories.Cash;
using Vodovoz.Parameters;
using Vodovoz.TempAdapters;
using Gtk;

namespace Vodovoz
{
	public partial class AdvanceReportDlg : QS.Dialog.Gtk.EntityDialogBase<AdvanceReport>
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		decimal debt = 0;
		decimal Balance = 0;
		decimal closingSum = 0;

		List<RecivedAdvance> advanceList;
		private bool _canEdit = true;
		private readonly bool _canCreate;
		private readonly bool canEditRectroactively;
		private readonly AdvanceCashOrganisationDistributor distributor = new AdvanceCashOrganisationDistributor();
		private readonly IEmployeeRepository _employeeRepository = new EmployeeRepository();
		private readonly ICategoryRepository _categoryRepository = new CategoryRepository(new ParametersProvider());
		private readonly IAccountableDebtsRepository _accountableDebtsRepository = new AccountableDebtsRepository();

		protected decimal Debt {
			get {
				return debt;
			}
			set {
				debt = value;
				labelCurrentDebt.LabelProp = String.Format("{0:C}", debt);
			}
		}

		protected decimal ClosingSum {
			get {
				return closingSum;
			}
			set {
				closingSum = value;
				labelClosingSum.LabelProp = String.Format("{0:C}", closingSum);
				CalculateBalance();
			}
		}

		public AdvanceReportDlg(Expense advance) : this(advance.Employee, advance.ExpenseCategory, advance.UnclosedMoney)
		{
			if(advance.Employee == null) {
				logger.Error("Аванс без сотрудника. Для него нельзя открыть диалог возврата.");
				base.FailInitialize = true;
				return;
			}
			advanceList.Find(x => x.Advance.Id == advance.Id).Selected = true;
		}

		public AdvanceReportDlg(Employee accountable, ExpenseCategory expenseCategory, decimal money) : this()
		{
			Entity.Accountable = accountable;
			Entity.ExpenseCategory = expenseCategory;
			Entity.Money = money;
		}

		public AdvanceReportDlg()
		{
			Build();
			UoWGeneric = UnitOfWorkFactory.CreateWithNewRoot<AdvanceReport>();
			Entity.Casher = _employeeRepository.GetEmployeeForCurrentUser(UoW);
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

			if(!accessfilteredsubdivisionselectorwidget.Configure(UoW, false, typeof(AdvanceReport)))
			{
				MessageDialogHelper.RunErrorDialog(accessfilteredsubdivisionselectorwidget.ValidationErrorMessage);
				FailInitialize = true;
				return;
			}

			Entity.Date = DateTime.Now;
			ConfigureDlg();
			FillDebt();
		}

		public AdvanceReportDlg(int id)
		{
			Build();
			UoWGeneric = UnitOfWorkFactory.CreateForRoot<AdvanceReport>(id);

			if(!accessfilteredsubdivisionselectorwidget.Configure(UoW, false, typeof(Income))) {

				MessageDialogHelper.RunErrorDialog(accessfilteredsubdivisionselectorwidget.ValidationErrorMessage);
				FailInitialize = true;
				return;
			}
			
			_canEdit = permissionResult.CanUpdate;

			var permmissionValidator =
				new EntityExtendedPermissionValidator(PermissionExtensionSingletonStore.GetInstance(), _employeeRepository);
			canEditRectroactively =
				permmissionValidator.Validate(
					typeof(AdvanceReport), ServicesConfig.UserService.CurrentUserId, nameof(RetroactivelyClosePermission));

			//Отключаем отображение ненужных элементов.
			labelDebtTitle.Visible = labelTableTitle.Visible = hboxDebt.Visible = GtkScrolledWindow1.Visible = labelCreating.Visible = false;

			comboExpense.Sensitive = yspinMoney.Sensitive = evmeEmployee.Sensitive = specialListCmbOrganisation.Sensitive = false;

			ConfigureDlg();
		}

		public AdvanceReportDlg(AdvanceReport sub) : this(sub.Id) { }

		private bool CanEdit => (UoW.IsNew && _canCreate) ||
		                        (_canEdit && Entity.Date.Date == DateTime.Now.Date) ||
		                        canEditRectroactively;
		
		void ConfigureDlg()
		{
			accessfilteredsubdivisionselectorwidget.OnSelected += Accessfilteredsubdivisionselectorwidget_OnSelected;
			if(Entity.RelatedToSubdivision != null) {
				accessfilteredsubdivisionselectorwidget.SelectIfPossible(Entity.RelatedToSubdivision);
			}

			var employeeFactory = new EmployeeJournalFactory();
			evmeEmployee.SetEntityAutocompleteSelectorFactory(employeeFactory.CreateWorkingEmployeeAutocompleteSelectorFactory());
			evmeEmployee.Binding.AddBinding(Entity, e => e.Accountable, w => w.Subject).InitializeFromSource();
			evmeEmployee.Changed += (sender, e) => FillDebt();

			ytextviewRouteList.Sensitive = false;
			ytextviewRouteList.Visible = false;

			evmeCashier.Binding.AddBinding(Entity, e => e.Casher, w => w.Subject).InitializeFromSource();
			evmeCashier.SetEntityAutocompleteSelectorFactory(employeeFactory.CreateEmployeeAutocompleteSelectorFactory());

			ydateDocument.Binding.AddBinding(Entity, s => s.Date, w => w.Date).InitializeFromSource();

			NotifyConfiguration.Instance.BatchSubscribeOnEntity<ExpenseCategory>(HandleBatchEntityChangeHandler);
			UpdateExpenseCategories();

			comboExpense.Binding.AddBinding(Entity, s => s.ExpenseCategory, w => w.SelectedItem).InitializeFromSource();

			yspinMoney.Binding.AddBinding(Entity, s => s.Money, w => w.ValueAsDecimal).InitializeFromSource();

			specialListCmbOrganisation.ShowSpecialStateNot = true;
			specialListCmbOrganisation.ItemsList = UoW.GetAll<Organization>();
			specialListCmbOrganisation.Binding.AddBinding(Entity, e => e.Organisation, w => w.SelectedItem).InitializeFromSource();

			ytextviewDescription.Binding.AddBinding(Entity, s => s.Description, w => w.Buffer.Text).InitializeFromSource();

			ytreeviewDebts.ColumnsConfig = ColumnsConfigFactory.Create<RecivedAdvance>()
				.AddColumn("Закрыть").AddToggleRenderer(a => a.Selected).Editing()
				.AddColumn("Дата").AddTextRenderer(a => a.Advance.Date.ToString())
				.AddColumn("Получено").AddTextRenderer(a => a.Advance.Money.ToString("C"))
				.AddColumn("Непогашено").AddTextRenderer(a => a.Advance.UnclosedMoney.ToString("C"))
				.AddColumn("Статья").AddTextRenderer(a => a.Advance.ExpenseCategory.Name)
				.AddColumn("Основание").AddTextRenderer(a => a.Advance.Description)
				.RowCells().AddSetter<CellRenderer>(
					(cell, node) =>
					{
						cell.Sensitive =
							node.Advance.RouteListClosing == Entity.RouteList
							|| advanceList.Count(s => s.Selected) == 0;
					})
				.Finish();
			UpdateSubdivision();

			if(!CanEdit) {
				table1.Sensitive = false;
				accessfilteredsubdivisionselectorwidget.Sensitive = false;
				buttonSave.Sensitive = false;
				ytreeviewDebts.Sensitive = false;
				ytextviewDescription.Editable = false;
			}
		}

		void HandleBatchEntityChangeHandler(EntityChangeEvent[] changeEvents)
		{
			UpdateExpenseCategories();
		}

		private void UpdateExpenseCategories()
		{
			comboExpense.ItemsList = _categoryRepository.ExpenseCategories(UoW);
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

		public override bool Save()
		{
			var valid = new QSValidator<AdvanceReport>(UoWGeneric.Root);
			if(valid.RunDlgIfNotValid((Gtk.Window)this.Toplevel))
				return false;

			logger.Info("Сохраняем авансовый отчет...");
			Income newIncome;
			Expense newExpense;
			bool needClosing = UoWGeneric.IsNew;
			UoWGeneric.Save(); // Сохраняем сначала отчет, так как нужно получить Id.
			if(needClosing) {
				var closing = Entity.CloseAdvances(out newExpense, out newIncome,
					advanceList.Where(a => a.Selected).Select(a => a.Advance).ToList());

				if (newExpense != null) {
					UoWGeneric.Save(newExpense);
					logger.Info("Создаем документ распределения расхода налички по юр лицу...");
					distributor.DistributeCashForExpenseAdvance(UoW, newExpense, Entity);
				}

				if (newIncome != null) {
					UoWGeneric.Save(newIncome);
					logger.Info("Создаем документ распределения прихода налички по юр лицу...");
					distributor.DistributeCashForIncomeAdvance(UoW, newIncome, Entity);
				}

				advanceList.Where(a => a.Selected).Select(a => a.Advance).ToList().ForEach(a => UoWGeneric.Save(a));
				closing.ForEach(c => UoWGeneric.Save(c));

				UoWGeneric.Save();

				if(newIncome != null) {
					MessageDialogWorks.RunInfoDialog(String.Format("Дополнительно создан приходный ордер №{0}, на сумму {1:C}.\nНе забудьте получить сдачу от подотчетного лица!",
						newIncome.Id, newIncome.Money));
				}
				if(newExpense != null) {
					MessageDialogWorks.RunInfoDialog(String.Format("Дополнительно создан расходный ордер №{0}, на сумму {1:C}.\nНе забудьте доплатить подотчетному лицу!",
						newExpense.Id, newExpense.Money));
				}
			}
			logger.Info("Ok");

			if(Entity.RouteList != null)
			{
				logger.Info("Обновляем сумму долга по МЛ...");
				Entity.RouteList.UpdateRouteListDebt();
				logger.Info("Ok");
			}

			return true;
		}

		protected void CalculateBalance()
		{
			if(!UoW.IsNew)
				return;

			Balance = ClosingSum - Entity.Money;

			labelChangeSum.Visible = labelChangeType.Visible = true;

			if(ClosingSum == 0) {
				labelChangeSum.Visible = labelChangeType.Visible = false;
				labelCreating.Markup = "<span foreground=\"Cadet Blue\">Не выбранных авансов.</span>";
			} else if(Balance == 0) {
				labelChangeSum.Visible = labelChangeType.Visible = false;
				labelCreating.Markup = "<span foreground=\"green\">Аванс будет закрыт полностью.</span>";
			} else if(Balance < 0) {
				labelChangeType.LabelProp = "Доплата:";
				labelChangeSum.LabelProp = string.Format("<span foreground=\"red\">{0:C}</span>", Math.Abs(Balance));
				labelCreating.Markup = String.Format("<span foreground=\"blue\">Будет создан расходный ордер на сумму {0:C}, в качестве доплаты.</span>", Math.Abs(Balance));
			} else {
				labelChangeType.LabelProp = "Остаток:";
				labelChangeSum.LabelProp = string.Format("{0:C}", Balance);
				labelCreating.Markup = String.Format("<span foreground=\"blue\">Будет создан приходный ордер на сумму {0:C}, в качестве сдачи от подотчетного лица.</span>", Math.Abs(Balance));
			}
		}

		protected void OnYspinMoneyValueChanged(object sender, EventArgs e)
		{
			CalculateBalance();
			ylabel1.Visible = specialListCmbOrganisation.Visible = Entity.NeedValidateOrganisation = ClosingSum != Entity.Money;
		}

		void FillDebt()
		{
			if(!UoW.IsNew)
				return;

			if(Entity.Accountable == null) {
				Debt = 0;
				ytreeviewDebts.Model = null;
				return;
			}

			logger.Info("Получаем долг {0}...", Entity.Accountable.ShortName);
			//Debt = Repository.Cash.AccountableDebtsRepository.EmloyeeDebt (UoW, Entity.Accountable);

			var advances =
				_accountableDebtsRepository.UnclosedAdvance(UoW, Entity.Accountable, Entity.ExpenseCategory, Entity.Organisation?.Id);

			Debt = advances.Sum(a => a.UnclosedMoney);

			advanceList = new List<RecivedAdvance>();

			advances.ToList().ForEach(adv => advanceList.Add(new RecivedAdvance(adv)));
			advanceList.ForEach(i => i.SelectChanged += I_SelectChanged);
			ytreeviewDebts.ItemsDataSource = advanceList;

			CalculateBalance();
			logger.Info("Ok");
		}

		private void UpdateRouteListInfo()
		{
			SetRouteListReference();
			SetRouteListControlsVisibility();
		}

		private void SetRouteListReference()
		{
			if(!UoW.IsNew)
			{
				return;
			}

			var selectedAdvances = advanceList?
				.Where(a => a.Selected)
				.Select(a => a.Advance?.RouteListClosing)
				.ToList();

			var selectedRouteListsCount = selectedAdvances?.GroupBy(rl => rl?.Id).Count();
			if(selectedRouteListsCount != 1)
			{
				Entity.RouteList = null;
				return;
			}

			Entity.RouteList = selectedAdvances.FirstOrDefault();
		}

		private void SetRouteListControlsVisibility()
		{
			labelRouteList.Visible = Entity.RouteList != null;
			ytextviewRouteList.Visible = Entity.RouteList != null;
			ytextviewRouteList.Buffer.Text = Entity.RouteList?.Title;
		}

		void I_SelectChanged(object sender, EventArgs e)
		{
			var recivedAdvances = sender as RecivedAdvance;

			advanceList?
					.Where(x => x.Advance?.RouteListClosing != recivedAdvances?.Advance?.RouteListClosing)
					.ToList()
					.ForEach(x => x.SilentUnselect());

			UpdateRouteListInfo();

			ytreeviewDebts.ItemsDataSource = advanceList;

			ClosingSum = advanceList.Where(a => a.Selected).Sum(a => a.Advance.UnclosedMoney);
			Entity.Money = ClosingSum;
			ylabel1.Visible = specialListCmbOrganisation.Visible = Entity.NeedValidateOrganisation = ClosingSum != Entity.Money;
		}

		protected void OnComboExpenseChanged(object sender, EventArgs e)
		{
			FillDebt();
			UpdateRouteListInfo();
		}

		class RecivedAdvance
		{
			private bool selected;

			public bool Selected {
				get { return selected; }
				set {
					selected = value;
					SelectChanged?.Invoke(this, EventArgs.Empty);
				}
			}

			public event EventHandler SelectChanged;

			public void SilentUnselect()
			{
				selected = false;
			}

			public Expense Advance { get; set; }

			public RecivedAdvance(Expense exp)
			{
				Advance = exp;
				Selected = false;
			}
		}
	}
}
