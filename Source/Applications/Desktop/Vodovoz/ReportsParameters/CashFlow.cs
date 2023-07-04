using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Gtk;
using QS.Dialog.GtkUI;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Report;
using QS.Services;
using QSOrmProject;
using QSReport;
using Vodovoz.Domain.Cash;
using Vodovoz.Domain.Employees;
using Vodovoz.EntityRepositories.Subdivisions;
using System.Linq;
using Vodovoz.Domain.Organizations;
using Vodovoz.EntityRepositories.Cash;
using Vodovoz.Parameters;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Journals.FilterViewModels.Employees;
using QS.Project.Services;
using QS.Dialog;

namespace Vodovoz.Reports
{
	public partial class CashFlow : SingleUoWWidgetBase, IParametersWidget
	{
		private readonly ISubdivisionRepository _subdivisionRepository;
		private readonly ICommonServices _commonServices;

		private IEnumerable<Subdivision> UserSubdivisions { get; }
		private IEnumerable<Organization> Organisations { get; }

		ExpenseCategory allItem = new ExpenseCategory{
			Name = "Все"
		};

		public CashFlow (
			ISubdivisionRepository subdivisionRepository, ICommonServices commonServices, ICategoryRepository categoryRepository)
		{
			_subdivisionRepository = subdivisionRepository ?? throw new ArgumentNullException(nameof(subdivisionRepository));
			_commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));

			if(categoryRepository == null)
			{
				throw new ArgumentNullException(nameof(categoryRepository));
			}
			
			Build();
			
			UoW = UnitOfWorkFactory.CreateWithoutRoot ();
			comboPart.ItemsEnum = typeof(ReportParts);
			comboIncomeCategory.ItemsList = categoryRepository.IncomeCategories(UoW);
			comboExpenseCategory.Sensitive = comboIncomeCategory.Sensitive = false;
			var now = DateTime.Now;
			dateStart.Date = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0);
			dateEnd.Date = new DateTime(now.Year, now.Month, now.Day, 23, 59, 59);

			var officeFilter = new EmployeeFilterViewModel();
			officeFilter.SetAndRefilterAtOnce(
				x => x.Status = EmployeeStatus.IsWorking,
				x => x.RestrictCategory = EmployeeCategory.office);
			var employeeFactory = new EmployeeJournalFactory(officeFilter);
			evmeCashier.SetEntityAutocompleteSelectorFactory(employeeFactory.CreateWorkingOfficeEmployeeAutocompleteSelectorFactory());

			evmeEmployee.SetEntityAutocompleteSelectorFactory(employeeFactory.CreateWorkingEmployeeAutocompleteSelectorFactory());

			var recurciveConfig = OrmMain.GetObjectDescription<ExpenseCategory>().TableView.RecursiveTreeConfig;
			var list = categoryRepository.ExpenseCategories(UoW);
			list.Insert(0, allItem);
			var model = recurciveConfig.CreateModel((IList)list);
			comboExpenseCategory.Model = model.Adapter;
			comboExpenseCategory.PackStart(new CellRendererText(), true);
			comboExpenseCategory.SetCellDataFunc(comboExpenseCategory.Cells[0], HandleCellLayoutDataFunc);
			comboExpenseCategory.SetActiveIter(model.IterFromNode(allItem));

			UserSubdivisions = GetSubdivisionsForUser();
			specialListCmbCashSubdivisions.SetRenderTextFunc<Subdivision>(s => s.Name);
			specialListCmbCashSubdivisions.ItemsList = UserSubdivisions;

			ylblOrganisations.Visible = specialListCmbOrganisations.Visible = false;
			Organisations = UoW.GetAll<Organization>();
			specialListCmbOrganisations.SetRenderTextFunc<Organization>(s => s.Name);
			specialListCmbOrganisations.ItemsList = Organisations;
			
			int currentUserId = commonServices.UserService.CurrentUserId;
			bool canCreateCashReportsForOrganisations = 
				commonServices.PermissionService.ValidateUserPresetPermission("can_create_cash_reports_for_organisations", currentUserId);
			checkOrganisations.Visible = canCreateCashReportsForOrganisations;
			checkOrganisations.Toggled += CheckOrganisationsToggled;
		}

		void CheckOrganisationsToggled(object sender, EventArgs e)
		{
			if(checkOrganisations.Active) {
				ylblCashSubdivisions.Visible = specialListCmbCashSubdivisions.Visible = false;
				ylblOrganisations.Visible = specialListCmbOrganisations.Visible = true;
			}
			else {
				ylblCashSubdivisions.Visible = specialListCmbCashSubdivisions.Visible = true;
				ylblOrganisations.Visible = specialListCmbOrganisations.Visible = false;
			}
		}

		private IEnumerable<Subdivision> GetSubdivisionsForUser() => 
			_subdivisionRepository.GetCashSubdivisionsAvailableForUser(UoW, _commonServices.UserService.GetCurrentUser());

		void HandleCellLayoutDataFunc (Gtk.CellLayout cell_layout, CellRenderer cell, Gtk.TreeModel tree_model, Gtk.TreeIter iter)
		{
			string text = DomainHelper.GetTitle(tree_model.GetValue(iter, 0));
			(cell as CellRendererText).Text = text;
		}

		#region IParametersWidget implementation

		public string Title => "Доходы и расходы";

		public event EventHandler<LoadReportEventArgs> LoadReport;

		#endregion

		void OnUpdate(bool hide = false)
		{
			if(!UserSubdivisions.Any())
			{
				ServicesConfig.InteractiveService.ShowMessage(ImportanceLevel.Error, "Пользователь не имеет доступа к кассам. Отчет сформировать невозможно.");
				return;
			}

			LoadReport?.Invoke(this, new LoadReportEventArgs(GetReportInfo(), hide));
		}

		protected void OnButtonRunClicked (object sender, EventArgs e) => OnUpdate (true);

		private ReportInfo GetReportInfo ()
		{
			string ReportName;
			switch (checkOrganisations.Active)
			{
				case true:
					if (checkDetail.Active) {
						if (comboPart.SelectedItem.Equals (Gamma.Widgets.SpecialComboState.All))
							ReportName = "Cash.CashFlowDetailOrganisations";
						else if (comboPart.SelectedItem.Equals (ReportParts.IncomeAll))
							ReportName = "Cash.CashFlowDetailIncomeAllOrganisations";
						else if (comboPart.SelectedItem.Equals (ReportParts.Income))
							ReportName = "Cash.CashFlowDetailIncomeOrganisations";
						else if (comboPart.SelectedItem.Equals (ReportParts.IncomeReturn))
							ReportName = "Cash.CashFlowDetailIncomeReturnOrganisations";
						else if (comboPart.SelectedItem.Equals (ReportParts.ExpenseAll))
							ReportName = "Cash.CashFlowDetailExpenseAllOrganisations";
						else if (comboPart.SelectedItem.Equals (ReportParts.Expense))
							ReportName = "Cash.CashFlowDetailExpenseOrganisations";
						else if (comboPart.SelectedItem.Equals (ReportParts.Advance))
							ReportName = "Cash.CashFlowDetailAdvanceOrganisations";
						else if (comboPart.SelectedItem.Equals (ReportParts.AdvanceReport))
							ReportName = "Cash.CashFlowDetailAdvanceReportOrganisations";
						else if (comboPart.SelectedItem.Equals (ReportParts.UnclosedAdvance))
							ReportName = "Cash.CashFlowDetailUnclosedAdvanceOrganisations";
						else
							throw new InvalidOperationException ("Неизвестный раздел.");
					} else
						ReportName = "Cash.CashFlowOrganisations";

					break;
					default:
						if (checkDetail.Active) {
							if (comboPart.SelectedItem.Equals (Gamma.Widgets.SpecialComboState.All))
								ReportName = "Cash.CashFlowDetail";
							else if (comboPart.SelectedItem.Equals (ReportParts.IncomeAll))
								ReportName = "Cash.CashFlowDetailIncomeAll";
							else if (comboPart.SelectedItem.Equals (ReportParts.Income))
								ReportName = "Cash.CashFlowDetailIncome";
							else if (comboPart.SelectedItem.Equals (ReportParts.IncomeReturn))
								ReportName = "Cash.CashFlowDetailIncomeReturn";
							else if (comboPart.SelectedItem.Equals (ReportParts.ExpenseAll))
								ReportName = "Cash.CashFlowDetailExpenseAll";
							else if (comboPart.SelectedItem.Equals (ReportParts.Expense))
								ReportName = "Cash.CashFlowDetailExpense";
							else if (comboPart.SelectedItem.Equals (ReportParts.Advance))
								ReportName = "Cash.CashFlowDetailAdvance";
							else if (comboPart.SelectedItem.Equals (ReportParts.AdvanceReport))
								ReportName = "Cash.CashFlowDetailAdvanceReport";
							else if (comboPart.SelectedItem.Equals (ReportParts.UnclosedAdvance))
								ReportName = "Cash.CashFlowDetailUnclosedAdvance";
							else
								throw new InvalidOperationException ("Неизвестный раздел.");
						} else
							ReportName = "Cash.CashFlow";

						break;
			}

			var inCat = 
				comboIncomeCategory.SelectedItem == null
				|| comboIncomeCategory.SelectedItem.Equals (Gamma.Widgets.SpecialComboState.All)
				? -1
				: (comboIncomeCategory.SelectedItem as IncomeCategory).Id;

			TreeIter iter;
			comboExpenseCategory.GetActiveIter(out iter);
			var exCategory = (ExpenseCategory)comboExpenseCategory.Model.GetValue(iter, 0);
			bool exCategorySelected = exCategory != allItem;
			var ids = new List<int>();
			
			if (exCategorySelected)
				FineIds(ids, exCategory);
			else
				ids.Add(0); //Add fake value

			var casherId = evmeCashier.Subject == null ? -1 : evmeCashier.SubjectId;
			var casherName = evmeCashier.Subject == null ? "" : ((Employee)evmeCashier.Subject).ShortName;
			var employeeId = evmeEmployee.Subject == null ? -1 : evmeEmployee.SubjectId;
			var employeeName = evmeEmployee.Subject == null ? "" : ((Employee)evmeEmployee.Subject).ShortName;

			IEnumerable<int> cashSubdivisions;
			IEnumerable<int> organisations;
			
			if (specialListCmbCashSubdivisions.SelectedItem == null) {
				cashSubdivisions = UserSubdivisions.Any() ? UserSubdivisions.Select(x => x.Id) : new[] {-1};
			}
			else {
				cashSubdivisions = new[] {(specialListCmbCashSubdivisions.SelectedItem as Subdivision).Id};
			}
			
			if (specialListCmbOrganisations.SelectedItem == null) {
				organisations = Organisations.Any() ? Organisations.Select(x => x.Id) : new[] {-1};
			}
			else {
				organisations = new[] {(specialListCmbOrganisations.SelectedItem as Organization).Id};
			}
			
			var cashSubdivisionsName = specialListCmbCashSubdivisions.SelectedItem == null ?
				string.Join(", ", UserSubdivisions.Select(x => x.Name)) 
				: UserSubdivisions.Where(x => x.Id == (specialListCmbCashSubdivisions.SelectedItem as Subdivision).Id)
				                  .Select(x => x.Name)
				                  .SingleOrDefault();

			var reportInfo =  new ReportInfo {
				Identifier = ReportName,
				Parameters = new Dictionary<string, object> {
					{ "StartDate", dateStart.DateOrNull.Value },
					{ "EndDate", dateEnd.DateOrNull.Value },
					{ "IncomeCategory", inCat },
					{ "ExpenseCategory", ids },
					{ "ExpenseCategoryUsed", exCategorySelected ? 1 : 0 },
					{ "Casher", casherId },
					{ "Employee", employeeId },
					{ "CasherName", casherName },
					{ "EmployeeName", employeeName }
				}
			};

			if (checkOrganisations.Active)
			{
				reportInfo.Parameters.Add("organisations", organisations);
				reportInfo.Parameters.Add("organisation_name",
					(specialListCmbOrganisations.SelectedItem as Organization) != null
						? (specialListCmbOrganisations.SelectedItem as Organization).Name
						: "Все организации");
			}
			else
			{
				reportInfo.Parameters.Add("cash_subdivisions", cashSubdivisions);
				reportInfo.Parameters.Add("cash_subdivisions_name", cashSubdivisionsName);
			}

			var cashCategoryParametersProvider = new OrganizationCashTransferDocumentParametersProvider(new ParametersProvider());
			reportInfo.Parameters.Add("cash_income_category_transfer_id", cashCategoryParametersProvider.CashIncomeCategoryTransferId);
			reportInfo.Parameters.Add("cash_expense_category_transfer_id", cashCategoryParametersProvider.CashExpenseCategoryTransferId);

			return reportInfo;
		}

		private void FineIds(IList<int> result, ExpenseCategory cat)
		{
			result.Add(cat.Id);
			if (cat.Childs == null)
				return;

			foreach(var childCat in cat.Childs)
			{
				FineIds(result, childCat);
			}
		}

		protected void OnDateChanged(object sender, EventArgs e)
		{
			buttonRun.Sensitive = dateStart.DateOrNull != null && dateEnd.DateOrNull != null;
		}

		protected void OnCheckDetailToggled (object sender, EventArgs e)
		{
			comboPart.Sensitive = comboExpenseCategory.Sensitive =
				comboIncomeCategory.Sensitive = checkDetail.Active;
		}

		protected void OnComboPartEnumItemSelected (object sender, Gamma.Widgets.ItemSelectedEventArgs e)
		{
			if (comboPart.SelectedItem.Equals (Gamma.Widgets.SpecialComboState.All)
				|| comboPart.SelectedItem.Equals (ReportParts.IncomeAll))
				comboExpenseCategory.Sensitive = comboIncomeCategory.Sensitive = true;
			else if (comboPart.SelectedItem.Equals (ReportParts.Income)) {
				comboExpenseCategory.Sensitive = false;
				comboIncomeCategory.Sensitive = true;
			} else if (comboPart.SelectedItem.Equals (ReportParts.ExpenseAll)
			           || comboPart.SelectedItem.Equals (ReportParts.Expense)
			           || comboPart.SelectedItem.Equals (ReportParts.Advance)
			           || comboPart.SelectedItem.Equals (ReportParts.AdvanceReport)
					|| comboPart.SelectedItem.Equals (ReportParts.UnclosedAdvance)
				|| comboPart.SelectedItem.Equals (ReportParts.IncomeReturn)) {
				comboExpenseCategory.Sensitive = true;
				comboIncomeCategory.Sensitive = false;
			} else
				throw new InvalidOperationException ("Неизвестный раздел.");
		}

		enum ReportParts
		{
			[Display (Name = "Поступления суммарно")]
			IncomeAll,
			[Display (Name = "Приход")]
			Income,
			[Display (Name = "Сдача")]
			IncomeReturn,
			[Display (Name = "Расходы суммарно")]
			ExpenseAll,
			[Display (Name = "Расход")]
			Expense,
			[Display (Name = "Авансы")]
			Advance,
			[Display (Name = "Авансовые отчеты")]
			AdvanceReport,
			[Display (Name = "Незакрытые авансы")]
			UnclosedAdvance
		}
	}
}
