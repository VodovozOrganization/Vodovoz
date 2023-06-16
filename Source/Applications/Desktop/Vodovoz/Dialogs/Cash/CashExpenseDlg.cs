using QS.Dialog.GtkUI;
using QS.DomainModel.Entity.EntityPermissions.EntityExtendedPermission;
using QS.DomainModel.UoW;
using QS.Project.Services;
using QS.Validation;
using QSOrmProject;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Cash;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Organizations;
using Vodovoz.EntityRepositories.Cash;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.EntityRepositories.Operations;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.Parameters;
using Vodovoz.PermissionExtensions;
using Vodovoz.TempAdapters;
using QSProjectsLib;
using System.Globalization;

namespace Vodovoz
{
	public partial class CashExpenseDlg : QS.Dialog.Gtk.EntityDialogBase<Expense>
	{
		private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();
		private static readonly IParametersProvider _parametersProvider = new ParametersProvider();

		private readonly bool _canEdit = true;
		private readonly bool _canCreate;
		private readonly bool _canEditRectroactively;
		private readonly bool _canEditDate =
			ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("can_edit_cash_income_expense_date");
		
		private readonly IEmployeeRepository _employeeRepository = new EmployeeRepository();
		private readonly ICategoryRepository _categoryRepository = new CategoryRepository(_parametersProvider);
		private readonly IWagesMovementRepository _wagesMovementRepository = new WagesMovementRepository();
		private readonly IExpenseParametersProvider _expenseParametersProvider = new ExpenseParametersProvider(_parametersProvider, new OrganizationParametersProvider(_parametersProvider));

		private readonly RouteListCashOrganisationDistributor _routeListCashOrganisationDistributor =
			new RouteListCashOrganisationDistributor(
				new CashDistributionCommonOrganisationProvider(
					new OrganizationParametersProvider(_parametersProvider)),
				new RouteListItemCashDistributionDocumentRepository(),
				new OrderRepository());

		private readonly ExpenseCashOrganisationDistributor _expenseCashOrganisationDistributor =
			new ExpenseCashOrganisationDistributor();

		private readonly FuelCashOrganisationDistributor _fuelCashOrganisationDistributor =
			new FuelCashOrganisationDistributor(
				new CashDistributionCommonOrganisationProvider(
					new OrganizationParametersProvider(_parametersProvider)));

		public CashExpenseDlg()
		{
			Build();
			UoWGeneric = UnitOfWorkFactory.CreateWithNewRoot<Expense>();
			Entity.Casher = _employeeRepository.GetEmployeeForCurrentUser(UoW);
			if(Entity.Casher == null)
			{
				MessageDialogHelper.RunErrorDialog(
					"Ваш пользователь не привязан к действующему сотруднику, вы не можете создавать кассовые документы, так как некого указывать в качестве кассира.");
				FailInitialize = true;
				return;
			}
			
			_canCreate = permissionResult.CanCreate;
			if(!_canCreate)
			{
				MessageDialogHelper.RunErrorDialog("Отсутствуют права на создание расходного ордера");
				FailInitialize = true;
				return;
			}

			if(!accessfilteredsubdivisionselectorwidget.Configure(UoW, false, typeof(Expense)))
			{
				MessageDialogHelper.RunErrorDialog(accessfilteredsubdivisionselectorwidget.ValidationErrorMessage);
				FailInitialize = true;
				return;
			}

			Entity.Organisation = UoW.GetById<Organization>(_expenseParametersProvider.DefaultExpenseOrganizationId);
			Entity.Date = DateTime.Now;
			ConfigureDlg();
		}

		public CashExpenseDlg(int id)
		{
			Build();
			UoWGeneric = UnitOfWorkFactory.CreateForRoot<Expense>(id);

			if(!accessfilteredsubdivisionselectorwidget.Configure(UoW, false, typeof(Expense)))
			{
				MessageDialogHelper.RunErrorDialog(accessfilteredsubdivisionselectorwidget.ValidationErrorMessage);
				FailInitialize = true;
				return;
			}

			_canEdit = permissionResult.CanUpdate;

			var permmissionValidator =
				new EntityExtendedPermissionValidator(PermissionExtensionSingletonStore.GetInstance(), _employeeRepository);
			_canEditRectroactively =
				permmissionValidator.Validate(
					typeof(Expense), ServicesConfig.UserService.CurrentUserId, nameof(RetroactivelyClosePermission));

			ConfigureDlg();
		}

		private bool CanEdit => UoW.IsNew
								&& _canCreate
								|| _canEdit
								&& Entity.Date.Date == DateTime.Now.Date
								|| _canEditRectroactively;

		private void ConfigureDlg()
		{
			if(!UoW.IsNew)
			{
				enumcomboOperation.Sensitive = false;
				specialListCmbOrganisation.Sensitive = false;
			}

			accessfilteredsubdivisionselectorwidget.OnSelected += Accessfilteredsubdivisionselectorwidget_OnSelected;
			if(Entity.RelatedToSubdivision != null)
			{
				accessfilteredsubdivisionselectorwidget.SelectIfPossible(Entity.RelatedToSubdivision);
			}

			enumcomboOperation.ItemsEnum = typeof(ExpenseType);
			enumcomboOperation.Binding.AddBinding(Entity, s => s.TypeOperation, w => w.SelectedItem).InitializeFromSource();

			var employeeFactory = new EmployeeJournalFactory();
			evmeCashier.Binding.AddBinding(Entity, s => s.Casher, w => w.Subject).InitializeFromSource();
			evmeCashier.SetEntityAutocompleteSelectorFactory(employeeFactory.CreateEmployeeAutocompleteSelectorFactory());

			evmeEmployee.SetEntityAutocompleteSelectorFactory(employeeFactory.CreateWorkingEmployeeAutocompleteSelectorFactory());
			evmeEmployee.Binding.AddBinding(Entity, s => s.Employee, w => w.Subject).InitializeFromSource();
			evmeEmployee.ChangedByUser += (sender, e) => UpdateEmployeeBalaceInfo();

			ydateDocument.Binding.AddBinding(Entity, s => s.Date, w => w.Date).InitializeFromSource();
			ydateDocument.Sensitive = _canEditDate;

			var expenseFactory = new ExpenseCategorySelectorFactory(UnitOfWorkFactory.GetDefaultFactory, MainClass.AppDIContainer.BeginLifetimeScope());
			entityVMEntryExpenseCategory
				.SetEntityAutocompleteSelectorFactory(expenseFactory.CreateDefaultExpenseCategoryAutocompleteSelectorFactory());
			entityVMEntryExpenseCategory.Binding.AddBinding(Entity, e => e.ExpenseCategory, w => w.Subject).InitializeFromSource();

			specialListCmbOrganisation.ShowSpecialStateNot = true;
			specialListCmbOrganisation.ItemsList = UoW.GetAll<Organization>();
			specialListCmbOrganisation.Binding.AddBinding(Entity, e => e.Organisation, w => w.SelectedItem).InitializeFromSource();

			yspinMoney.Binding.AddBinding(Entity, s => s.Money, w => w.ValueAsDecimal).InitializeFromSource();
			yspinMoney.Output += YspinMoneyOnOutput;
			yspinMoney.Input += YspinMoneyOnInput;

			ytextviewDescription.Binding.AddBinding(Entity, s => s.Description, w => w.Buffer.Text).InitializeFromSource();

			UpdateEmployeeBalanceVisibility();
			UpdateEmployeeBalaceInfo();
			UpdateSubdivision();

			if(!CanEdit)
			{
				table1.Sensitive = false;
				accessfilteredsubdivisionselectorwidget.Sensitive = false;
				buttonSave.Sensitive = false;
				ytextviewDescription.Editable = false;
			}
		}

		public void ConfigureForSalaryGiveout(int employeeId, decimal balance, bool canChangeEmployee, ExpenseType expenseType)
		{
			evmeEmployee.Subject = UoW.GetById<Employee>(employeeId);
			evmeEmployee.Sensitive = canChangeEmployee;
			Entity.TypeOperation = expenseType;
			yspinMoney.ValueAsDecimal = balance;
			UpdateEmployeeBalanceVisibility();
		}

		public void ConfigureForRouteListChangeGiveout(int employeeId, decimal balance, string description)
		{
			evmeEmployee.Subject = UoW.GetById<Employee>(employeeId);
			evmeEmployee.Sensitive = false;
			ydateDocument.Sensitive = false;
			Entity.TypeOperation = ExpenseType.Advance;
			Entity.Description = description;
			Entity.ExpenseCategory = UoW.GetById<ExpenseCategory>(_expenseParametersProvider.ChangeCategoryId);
			Entity.Organisation = UoW.GetById<Organization>(_expenseParametersProvider.DefaultChangeOrganizationId);
			yspinMoney.ValueAsDecimal = balance;
			UpdateEmployeeBalanceVisibility();
		}

		public void CopyExpenseFrom(Expense doc)
		{
			Entity.TypeOperation = doc.TypeOperation;
			Entity.ExpenseCategory = doc.ExpenseCategory;
			Entity.Description = doc.Description;
			Entity.RelatedToSubdivision = doc.RelatedToSubdivision;
			accessfilteredsubdivisionselectorwidget.SelectIfPossible(Entity.RelatedToSubdivision);
			UpdateSubdivision();
		}

		private void Accessfilteredsubdivisionselectorwidget_OnSelected(object sender, EventArgs e)
		{
			UpdateSubdivision();
		}

		private void UpdateSubdivision()
		{
			if(accessfilteredsubdivisionselectorwidget.SelectedSubdivision != null
				&& accessfilteredsubdivisionselectorwidget.NeedChooseSubdivision)
			{
				Entity.RelatedToSubdivision = accessfilteredsubdivisionselectorwidget.SelectedSubdivision;
			}
		}

		public override bool Save()
		{
			var valid = new QSValidator<Expense>(UoWGeneric.Root);
			if(valid.RunDlgIfNotValid((Gtk.Window)Toplevel))
			{
				return false;
			}

			Entity.UpdateWagesOperations(UoW);

			if(UoW.IsNew)
			{
				DistributeCash();
			}
			else
			{
				UpdateCashDistributionsDocuments();
			}

			_logger.Info("Сохраняем расходный ордер...");
			UoWGeneric.Save();
			_logger.Info("Ok");
			return true;
		}

		private void DistributeCash()
		{
			if(Entity.TypeOperation == ExpenseType.Expense
				&& Entity.ExpenseCategory.Id == _categoryRepository.RouteListClosingExpenseCategory(UoW)?.Id)
			{
				_routeListCashOrganisationDistributor.DistributeExpenseCash(UoW, Entity.RouteListClosing, Entity, Entity.Money);
			}
			else if(Entity.TypeOperation == ExpenseType.EmployeeAdvance
					|| Entity.TypeOperation == ExpenseType.Salary)
			{
				_expenseCashOrganisationDistributor.DistributeCashForExpense(UoW, Entity, true);
			}
			else if(Entity.TypeOperation == ExpenseType.EmployeeAdvance
					|| Entity.TypeOperation == ExpenseType.Salary)
			{
				_expenseCashOrganisationDistributor.DistributeCashForExpense(UoW, Entity, true);
			}
			else
			{
				_expenseCashOrganisationDistributor.DistributeCashForExpense(UoW, Entity);
			}
		}

		private void UpdateCashDistributionsDocuments()
		{
			var editor = _employeeRepository.GetEmployeeForCurrentUser(UoW);
			var document = UoW.Session.QueryOver<CashOrganisationDistributionDocument>()
				.Where(x => x.Expense.Id == Entity.Id).List().FirstOrDefault();

			if(document != null)
			{
				switch(document.Type)
				{
					case CashOrganisationDistributionDocType.ExpenseCashDistributionDoc:
						_expenseCashOrganisationDistributor.UpdateRecords(UoW, (ExpenseCashDistributionDocument)document, Entity, editor);
						break;
					case CashOrganisationDistributionDocType.FuelExpenseCashOrgDistributionDoc:
						_fuelCashOrganisationDistributor.UpdateRecords(UoW, (FuelExpenseCashDistributionDocument)document, Entity, editor);
						break;
				}
			}
		}

		protected void OnEnumcomboOperationEnumItemSelected(object sender, Gamma.Widgets.ItemSelectedEventArgs e)
		{
			if(Entity.TypeOperation == ExpenseType.Salary || Entity.TypeOperation == ExpenseType.EmployeeAdvance)
			{
				if(Entity.Organisation != null)
				{
					Entity.Organisation = null;
				}

				ylabel1.Visible = specialListCmbOrganisation.Visible = false;
			}
			else
			{
				ylabel1.Visible = specialListCmbOrganisation.Visible = true;
			}

			UpdateEmployeeBalaceInfo();
		}

		private void UpdateEmployeeBalaceInfo()
		{
			UpdateEmployeeBalanceVisibility();
			
			var currentEmployeeWage = default(decimal);

			var labelTemplate = "<span font='large' weight='bold'>Текущий баланс сотрудника: {0}</span>";

			if(evmeEmployee.Subject is Employee employee)
			{
				if (employee.Category == EmployeeCategory.driver && employee.Status == EmployeeStatus.IsWorking)
				{
					var driverBalanceWithoutFutureFines = _wagesMovementRepository.GetDriverWageBalanceWithoutFutureFines(UoW, employee.Id);
					var driverFutureFinesBalance = (-1) * _wagesMovementRepository.GetDriverFutureFinesBalance(UoW, employee.Id);

					labelTemplate += "      <span font='large' weight='bold'>Будущие штрафы: {1}</span>"; 
					
					ylabelEmployeeWageBalance.LabelProp = string.Format(labelTemplate, CurrencyWorks.GetShortCurrencyString(driverBalanceWithoutFutureFines), CurrencyWorks.GetShortCurrencyString(driverFutureFinesBalance));

					return;
				}

				currentEmployeeWage = _wagesMovementRepository.GetCurrentEmployeeWageBalance(UoW, employee.Id);
			}

			ylabelEmployeeWageBalance.LabelProp = string.Format(labelTemplate, CurrencyWorks.GetShortCurrencyString(currentEmployeeWage));
		}

		private void UpdateEmployeeBalanceVisibility()
		{
			switch((ExpenseType)enumcomboOperation.SelectedItem)
			{
				case ExpenseType.Advance:
					labelEmployee.LabelProp = "Подотчетное лицо:";
					ylabelEmployeeWageBalance.Visible = new[] { EmployeeCategory.office }.All(x => x != Entity?.Employee?.Category);
					break;
				case ExpenseType.Expense:
					labelEmployee.LabelProp = "Сотрудник:";
					ylabelEmployeeWageBalance.Visible = false;
					break;
				case ExpenseType.EmployeeAdvance:
					labelEmployee.LabelProp = "Сотрудник:";
					ylabelEmployeeWageBalance.Visible = true;
					break;
				case ExpenseType.Salary:
					labelEmployee.LabelProp = "Сотрудник:";
					ylabelEmployeeWageBalance.Visible = true;
					break;
			}
		}

		protected void OnButtonPrintClicked(object sender, EventArgs e)
		{
			if(UoWGeneric.HasChanges && CommonDialogs.SaveBeforePrint(typeof(Expense), "квитанции"))
			{
				Save();
			}

			var reportInfo = new QS.Report.ReportInfo
			{
				Title = $"Квитанция №{Entity.Id} от {Entity.Date:d}",
				Identifier = "Cash.Expense",
				Parameters = new Dictionary<string, object>
				{
					{ "id", Entity.Id }
				}
			};

			var report = new QSReport.ReportViewDlg(reportInfo);
			TabParent.AddTab(report, this, false);
		}

		protected void OnYspinMoneyFocusInEvent(object o, Gtk.FocusInEventArgs args)
		{
			if(yspinMoney.ValueAsDecimal == decimal.Zero)
			{
				yspinMoney.Text = "";
			}
		}

		private void YspinMoneyOnOutput(object o, Gtk.OutputArgs args)
		{
			yspinMoney.Numeric = false;

			NumberFormatInfo valueFormatInfo = new NumberFormatInfo();
			valueFormatInfo.NumberDecimalSeparator = ".";
			valueFormatInfo.NumberGroupSeparator = " ";

			yspinMoney.Text = string.Format(valueFormatInfo, "{0:#,0.00}", yspinMoney.Value);
			yspinMoney.Numeric = true;

			args.RetVal = 1;
		}

		private void YspinMoneyOnInput(object o, Gtk.InputArgs args)
		{
			if(yspinMoney.Text.Length == 0)
			{
				yspinMoney.Text = "0";
			}

			double newValue;
			if(double.TryParse(yspinMoney.Text.Replace(" ", ""), NumberStyles.Any, CultureInfo.GetCultureInfo("en-US"), out newValue))
			{
				args.NewValue = newValue;
			}
			else
			{
				args.NewValue = yspinMoney.Value;
			}
			args.RetVal = 1;
		}
	}
}
