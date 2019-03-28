using System;
using System.Collections.Generic;
using System.Linq;
using QS.Project.Dialogs;
using QS.Project.Dialogs.GtkUI;
using QS.RepresentationModel.GtkUI;
using QSProjectsLib;
using QSValidation;
using Vodovoz.Domain.Cash;
using Vodovoz.Domain.Cash.CashTransfer;
using Vodovoz.Domain.Employees;
using Vodovoz.Repositories.HumanResources;
using Vodovoz.ViewModelBased;
using Vodovoz.JournalFilters.QueryFilterViews;
using NHibernate.Criterion;
using QS.DomainModel.Config;
using Vodovoz.Domain.Logistic;
using Vodovoz.Repository.Cash;

namespace Vodovoz.Dialogs.Cash.CashTransfer
{
	public class IncomeCashTransferDocumentViewModel : ViewModel<IncomeCashTransferDocument>
	{
		private IEnumerable<Subdivision> cashSubdivisions;
		private IEnumerable<Subdivision> availableSubdivisionsForUser;

		public IncomeCashTransferDocumentViewModel(IEntityOpenOption entityOpenOption) : base(entityOpenOption)
		{
			if(entityOpenOption.NeedCreateNew) {
				Entity.CreationDate = DateTime.Now;
				Entity.Author = Cashier;
			}
			CreateCommands();
			UpdateCashSubdivisions();
			UpdateIncomeCategories();
			UpdateExpenseCategories();
			View = new IncomeCashTransferDlg(this);

			Entity.PropertyChanged += Entity_PropertyChanged;

			ConfigEntityUpdateSubscribes();
		}

		private Employee cashier;
		public Employee Cashier {
			get {
				if(cashier == null) {
					cashier = EmployeeRepository.GetEmployeeForCurrentUser(UoW);
				}
				return cashier;
			}
		}

		private bool incomesSelected;
		public virtual bool IncomesSelected {
			get => incomesSelected;
			set => SetField(ref incomesSelected, value, () => IncomesSelected);
		}

		private bool expensesSelected;
		public virtual bool ExpensesSelected {
			get => expensesSelected;
			set => SetField(ref expensesSelected, value, () => ExpensesSelected);
		}

		public bool CanEdit => Entity.Status == CashTransferDocumentStatuses.New;

		public override bool Save()
		{
			var valid = new QSValidator<IncomeCashTransferDocument>(Entity, new Dictionary<object, object>());

			if(valid.RunDlgIfNotValid()) {
				return false;
			}

			return base.Save();
		}

		protected override void ConfigurePropertyChangingRelations()
		{
			SetPropertyChangeRelation(e => e.Status,
				() => CanEdit
			);
		}

		private void Entity_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof(Entity.CashSubdivisionFrom)) {
				UpdateSubdivisionsTo();
				Entity.ObservableCashTransferDocumentIncomeItems.Clear();
				Entity.ObservableCashTransferDocumentExpenseItems.Clear();
			}
			if(e.PropertyName == nameof(Entity.CashSubdivisionTo)) {
				UpdateSubdivisionsFrom();
			}
		}

		#region Подписки на внешние измнения сущностей

		private void ConfigEntityUpdateSubscribes()
		{
			IEntityConfig routelistEntityConfig = DomainConfiguration.GetEntityConfig(typeof(RouteList));
			routelistEntityConfig.EntityUpdated += RoutelistEntityConfig_EntityUpdated;

			IEntityConfig incomeCategoryEntityConfig = DomainConfiguration.GetEntityConfig(typeof(IncomeCategory));
			incomeCategoryEntityConfig.EntityUpdated += IncomeCategoryEntityConfig_EntityUpdated;

			IEntityConfig expenseCategoryEntityConfig = DomainConfiguration.GetEntityConfig(typeof(ExpenseCategory));
			expenseCategoryEntityConfig.EntityUpdated += ExpenseCategoryEntityConfig_EntityUpdated; ;
		}

		private void RoutelistEntityConfig_EntityUpdated(object sender, EntityUpdatedEventArgs e)
		{
			foreach(var updatedItem in e.UpdatedSubjects) {
				RouteList updatedRouteList = updatedItem as RouteList;
				if(updatedRouteList != null) {
					var foundRouteList = Entity.CashTransferDocumentIncomeItems
						.Where(x => x.Income != null)
						.Where(x => x.Income.RouteListClosing != null)
						.Select(x => x.Income.RouteListClosing)
						.FirstOrDefault(x => x.Id == updatedRouteList.Id);
					if(foundRouteList != null) {
						UoW.Session.Refresh(foundRouteList);
					}
				}
			}
		}

		private void IncomeCategoryEntityConfig_EntityUpdated(object sender, EntityUpdatedEventArgs e)
		{
			foreach(var updatedItem in e.UpdatedSubjects) {
				IncomeCategory updatedIncomeCategory = updatedItem as IncomeCategory;
				//Если хотябы одна необходимая статья обновилась можем обнлять весь список.
				if(updatedIncomeCategory != null && updatedIncomeCategory.IncomeDocumentType == IncomeInvoiceDocumentType.IncomeTransferDocument) {
					UpdateIncomeCategories();
					return;
				}
			}
		}

		private void ExpenseCategoryEntityConfig_EntityUpdated(object sender, EntityUpdatedEventArgs e)
		{
			foreach(var updatedItem in e.UpdatedSubjects) {
				ExpenseCategory updatedExpenseCategory = updatedItem as ExpenseCategory;
				//Если хотябы одна необходимая статья обновилась можем обнлять весь список.
				if(updatedExpenseCategory != null && updatedExpenseCategory.ExpenseDocumentType == ExpenseInvoiceDocumentType.ExpenseTransferDocument) {
					UpdateExpenseCategories();
					return;
				}
			}
		}

		#endregion Подписки на внешние измнения сущностей

		#region Commands

		public DelegateCommand<IEnumerable<IncomeCashTransferedItem>> DeleteIncomesCommand { get; private set; }
		public DelegateCommand<IEnumerable<ExpenseCashTransferedItem>> DeleteExpensesCommand { get; private set; }
		public DelegateCommand<Income> OpenRouteListCommand { get; private set; }
		public DelegateCommand SendCommand { get; private set; }
		public DelegateCommand ReceiveCommand { get; private set; }
		public DelegateCommand AddIncomesCommand { get; private set; }
		public DelegateCommand AddExpensesCommand { get; private set; }
		public DelegateCommand PrintCommand { get; private set; }

		private void CreateCommands()
		{
			DeleteIncomesCommand = new DelegateCommand<IEnumerable<IncomeCashTransferedItem>>(
				Entity.DeleteTransferedIncomes,
				(IEnumerable<IncomeCashTransferedItem> parameter) => {
					return parameter != null 
						&& parameter.Any()
						&& CanEdit;
				}
			);
			DeleteIncomesCommand.CanExecuteChangedWith(this, x => x.IncomesSelected);
			DeleteIncomesCommand.CanExecuteChangedWith(Entity, x => x.Status);

			DeleteExpensesCommand = new DelegateCommand<IEnumerable<ExpenseCashTransferedItem>>(
				Entity.DeleteTransferedExpenses,
				(IEnumerable<ExpenseCashTransferedItem> parameter) => {
					return parameter != null 
						&& parameter.Any()
						&& CanEdit;
				}
			);
			DeleteExpensesCommand.CanExecuteChangedWith(this, x => x.ExpensesSelected);
			DeleteExpensesCommand.CanExecuteChangedWith(Entity, x => x.Status);

			OpenRouteListCommand = new DelegateCommand<Income>(
				(Income parameter) => {
					View.TabParent.OpenTab<RouteListClosingDlg, int>(parameter.RouteListClosing.Id);
				},
				(Income parameter) => { return parameter != null && parameter.RouteListClosing != null; }
			);

			SendCommand = new DelegateCommand(
				() => {
					var valid = new QSValidator<IncomeCashTransferDocument>(Entity, new Dictionary<object, object>());
					if(valid.RunDlgIfNotValid()) {
						return;
					}
					Entity.Send(Cashier);
				},
				() => {
					return Cashier != null
						&& Entity.Status == CashTransferDocumentStatuses.New
						&& Entity.Driver != null
						&& Entity.Car != null
						&& Entity.CashSubdivisionFrom != null
						&& Entity.CashSubdivisionTo != null
						&& Entity.ExpenseCategory != null
						&& Entity.IncomeCategory != null
						&& Entity.TransferedSum > 0;
				}
			);
			SendCommand.CanExecuteChangedWith(Entity,
				x => x.Status,
				x => x.Driver,
				x => x.Car,
				x => x.CashSubdivisionFrom,
				x => x.CashSubdivisionTo,
				x => x.TransferedSum,
				x => x.ExpenseCategory,
				x => x.IncomeCategory
			);

			ReceiveCommand = new DelegateCommand(
				() => {
					Entity.Receive(Cashier);
				},
				() => {
					return Cashier != null
						&& Entity.Status == CashTransferDocumentStatuses.Sent
						&& availableSubdivisionsForUser.Contains(Entity.CashSubdivisionTo)
						&& Entity.Id != 0;
				}
			);
			ReceiveCommand.CanExecuteChangedWith(Entity,
				x => x.Status,
				x => x.Id
			);

			AddIncomesCommand = new DelegateCommand(
				() => {
					var existsIncomes = Entity.CashTransferDocumentIncomeItems.Select(x => x.Income.Id).ToArray();

					//скрываем уже выбранные приходники и отображаем расходники только выбранного подразделения
					var restriction = Restrictions.On<Income>(x => x.Id).Not.IsIn(existsIncomes);
					restriction = Restrictions.And(restriction, Restrictions.Where<Income>(x => x.RouteListClosing != null));
					if(Entity.CashSubdivisionFrom != null) {
						restriction = Restrictions.And(restriction, Restrictions.Where<Income>(x => x.RelatedToSubdivision == Entity.CashSubdivisionFrom));
					}
					//скрываем приходники выбранные в других документах перемещения
					restriction = Restrictions.And(restriction, Restrictions.Where<Income>(x => x.TransferedBy == null));


					var filter = new CashIncomeFilterView();
					var incomesVM = new EntityCommonRepresentationModelConstructor<Income>(UoW)
						.AddColumn("№", x => x.Id.ToString())
						.AddColumn("Дата", x => x.Date.ToShortDateString())
						.AddColumn("Сотрудник", x => x.Employee != null ? x.Employee.GetPersonNameWithInitials() : "")
						.AddColumn("Статья", x => x.IncomeCategory != null ? x.IncomeCategory.Name : "")
						.AddColumn("Сумма", x => CurrencyWorks.GetShortCurrencyString(x.Money))
						.AddColumn("Кассир", x => x.Casher != null ? x.Casher.GetPersonNameWithInitials() : "")
						.AddColumn("Основание", x => x.Description)
						.OrderByDesc(x => x.Date)
						.SetFixedRestriction(restriction)
						.SetQueryFilter(filter)
						.Finish();
					var incomesSelectDlg = new RepresentationJournalDialog(incomesVM);
					incomesSelectDlg.Mode = JournalSelectMode.Multiple;
					incomesSelectDlg.ObjectSelected += IncomesSelectDlg_ObjectSelected;
					View.TabParent.AddSlaveTab(View, incomesSelectDlg);
				},
				() => {
					return Entity.Status == CashTransferDocumentStatuses.New 
						&& Entity.CashSubdivisionTo != null
						&& Entity.CashSubdivisionFrom != null;
				}
			);
			AddIncomesCommand.CanExecuteChangedWith(Entity,
				x => x.Status,
				x => x.CashSubdivisionTo,
				x => x.CashSubdivisionFrom
			);

			AddExpensesCommand = new DelegateCommand(
				() => {
					var existsExpenses = Entity.CashTransferDocumentExpenseItems.Select(x => x.Expense.Id).ToArray();

					//скрываем уже выбранные расходники и отображаем расходники только выбранного подразделения
					var restriction = Restrictions.On<Expense>(x => x.Id).Not.IsIn(existsExpenses);
					if(Entity.CashSubdivisionFrom != null) {
						restriction = Restrictions.And(restriction, Restrictions.Where<Expense>(x => x.RelatedToSubdivision == Entity.CashSubdivisionFrom));
					}
					//скрываем расходники выбранные в других документах перемещения
					restriction = Restrictions.And(restriction, Restrictions.Where<Expense>(x => x.TransferedBy == null));

					var filter = new CashExpenseFilterView();
					var expensesVM = new EntityCommonRepresentationModelConstructor<Expense>(UoW)
						.AddColumn("№", x => x.Id.ToString())
						.AddColumn("Дата", x => x.Date.ToShortDateString())
						.AddColumn("Сотрудник", x => x.Employee != null ? x.Employee.GetPersonNameWithInitials() : "")
						.AddColumn("Статья", x => x.ExpenseCategory != null ? x.ExpenseCategory.Name : "")
						.AddColumn("Сумма", x => CurrencyWorks.GetShortCurrencyString(x.Money))
						.AddColumn("Кассир", x => x.Casher != null ? x.Casher.GetPersonNameWithInitials() : "")
						.AddColumn("Основание", x => x.Description)
						.OrderByDesc(x => x.Date)
						.SetFixedRestriction(restriction)
						.SetQueryFilter(filter)
						.Finish();
					var expensesSelectDlg = new RepresentationJournalDialog(expensesVM);
					expensesSelectDlg.Mode = JournalSelectMode.Multiple;
					expensesSelectDlg.ObjectSelected += ExpensesSelectDlg_ObjectSelected;;
					View.TabParent.AddSlaveTab(View, expensesSelectDlg);
				},
				() => {
					return Entity.Status == CashTransferDocumentStatuses.New
						&& Entity.CashSubdivisionTo != null
						&& Entity.CashSubdivisionFrom != null;
				}
			);
			AddExpensesCommand.CanExecuteChangedWith(Entity,
				x => x.Status,
				x => x.CashSubdivisionTo,
				x => x.CashSubdivisionFrom
			);

			PrintCommand = new DelegateCommand(
				() => {
					var reportInfo = new QS.Report.ReportInfo {
						Title = String.Format($"Документ перемещения №{Entity.Id} от {Entity.CreationDate:d}"),
						Identifier = "Documents.IncomeCashTransfer",
						Parameters = new Dictionary<string, object> { { "transfer_document_id",  Entity.Id } }
					};

					var report = new QSReport.ReportViewDlg(reportInfo);
					View.TabParent.AddTab(report, View, false);
				},
				() => Entity.Id != 0
			);
		}

		void IncomesSelectDlg_ObjectSelected(object sender, JournalObjectSelectedEventArgs e)
		{
			if(!e.Selected.Any()) {
				return;
			}

			foreach(var item in e.Selected) {
				var incomeItem = item as Income;

				if(incomeItem != null) {
					Entity.AddIncomeItem(incomeItem);
				}
			}
		}

		void ExpensesSelectDlg_ObjectSelected(object sender, JournalObjectSelectedEventArgs e)
		{
			if(!e.Selected.Any()) {
				return;
			}

			foreach(var item in e.Selected) {
				var expenseItem = item as Expense;

				if(expenseItem != null) {
					Entity.AddExpenseItem(expenseItem);
				}
			}
		}

		#endregion Commands

		#region Настройка списков статей дохода и прихода

		private IList<IncomeCategory> incomeCategories;
		public virtual IList<IncomeCategory> IncomeCategories {
			get => incomeCategories;
			set => SetField(ref incomeCategories, value, () => IncomeCategories);
		}

		private IList<ExpenseCategory> expenseCategories;
		public virtual IList<ExpenseCategory> ExpenseCategories {
			get => expenseCategories;
			set => SetField(ref expenseCategories, value, () => ExpenseCategories);
		}

		private void UpdateIncomeCategories()
		{
			if(!CanEdit) {
				return;
			}
			var currentSelectedCategory = Entity.IncomeCategory;
			IncomeCategories = CategoryRepository.IncomeCategories(UoW).Where(x => x.IncomeDocumentType == IncomeInvoiceDocumentType.IncomeTransferDocument).ToList();
			if(IncomeCategories.Contains(currentSelectedCategory)) {
				Entity.IncomeCategory = currentSelectedCategory;
			}
		}

		private void UpdateExpenseCategories()
		{
			if(!CanEdit) {
				return;
			}
			var currentSelectedCategory = Entity.ExpenseCategory;
			ExpenseCategories = CategoryRepository.ExpenseCategories(UoW).Where(x => x.ExpenseDocumentType == ExpenseInvoiceDocumentType.ExpenseTransferDocument).ToList();
			if(ExpenseCategories.Contains(currentSelectedCategory)) {
				Entity.ExpenseCategory = currentSelectedCategory;
			}
		}

		#endregion Настройка списков статей дохода и прихода

		#region Настройка списков доступных подразделений кассы

		private IEnumerable<Subdivision> subdivisionsFrom;
		public virtual IEnumerable<Subdivision> SubdivisionsFrom {
			get => subdivisionsFrom;
			set => SetField(ref subdivisionsFrom, value, () => SubdivisionsFrom);
		}

		private IEnumerable<Subdivision> subdivisionsTo;
		public virtual IEnumerable<Subdivision> SubdivisionsTo {
			get => subdivisionsTo;
			set => SetField(ref subdivisionsTo, value, () => SubdivisionsTo);
		}

		private void UpdateCashSubdivisions()
		{
			Type[] cashDocumentTypes = new Type[] { typeof(Income), typeof(Expense), typeof(AdvanceReport) };
			availableSubdivisionsForUser = SubdivisionsRepository.GetAvailableSubdivionsForUser(UoW, cashDocumentTypes);
			cashSubdivisions = SubdivisionsRepository.GetSubdivisionsForDocumentTypes(UoW, cashDocumentTypes).Distinct();
			SubdivisionsFrom = availableSubdivisionsForUser;
			SubdivisionsTo = cashSubdivisions;
		}


		private bool isUpdatingSubdivisions = false;

		private void UpdateSubdivisionsFrom()
		{
			if(isUpdatingSubdivisions) {
				return;
			}
			isUpdatingSubdivisions = true;
			var currentSubdivisonFrom = Entity.CashSubdivisionFrom;
			SubdivisionsFrom = availableSubdivisionsForUser.Where(x => x != Entity.CashSubdivisionTo);
			if(SubdivisionsTo.Contains(currentSubdivisonFrom)) {
				Entity.CashSubdivisionFrom = currentSubdivisonFrom;
			}
			isUpdatingSubdivisions = false;
		}

		private void UpdateSubdivisionsTo()
		{
			if(isUpdatingSubdivisions) {
				return;
			}
			isUpdatingSubdivisions = true;
			var currentSubdivisonTo = Entity.CashSubdivisionTo;
			SubdivisionsTo = cashSubdivisions.Where(x => x != Entity.CashSubdivisionFrom);
			if(SubdivisionsTo.Contains(currentSubdivisonTo)) {
				Entity.CashSubdivisionTo = currentSubdivisonTo;
			}
			isUpdatingSubdivisions = false;
		}

		#endregion Настройка списков доступных подразделений кассы
	}
}
