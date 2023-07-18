using Autofac;
using DateTimeHelpers;
using Gtk;
using QS.Commands;
using QS.Dialog;
using QS.Dialog.Gtk;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Services;
using QS.Project.Services.FileDialog;
using QS.Report;
using QS.Services;
using QS.Tdi;
using QS.ViewModels.Control.EEVM;
using QSReport;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using Vodovoz.Domain.Cash.FinancialCategoriesGroups;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Organizations;
using Vodovoz.EntityRepositories.Subdivisions;
using Vodovoz.Parameters;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Cash.FinancialCategoriesGroups;
using Vodovoz.ViewModels.Journals.FilterViewModels.Employees;

namespace Vodovoz.Reports
{
	public partial class CashFlow : SingleUowTabBase, IParametersWidget, INotifyPropertyChanged
	{
		private readonly ISubdivisionRepository _subdivisionRepository;
		private readonly ICommonServices _commonServices;
		private readonly ILifetimeScope _lifetimeScope;
		private readonly IFileDialogService _fileDialogService;

		private readonly CashFlowDdsReportRenderer _cashFlowDdsReportRenderer = new CashFlowDdsReportRenderer();

		private bool _canGenerateCashReportsForOrganisations;
		private bool _canGenerateCashFlowDdsReport;

		private FinancialExpenseCategory _financialExpenseCategory;
		private FinancialIncomeCategory _financialIncomeCategory;
		private ITdiTab _parentTab;
		private bool _canGenerateDdsReport;

		private DateTime _startDate;
		private DateTime _endDate;

		public CashFlow(
			IUnitOfWorkFactory unitOfWorkFactory,
			ISubdivisionRepository subdivisionRepository,
			ICommonServices commonServices,
			INavigationManager navigationManager,
			ILifetimeScope lifetimeScope,
			IFileDialogService fileDialogService)
		{
			_subdivisionRepository = subdivisionRepository ?? throw new ArgumentNullException(nameof(subdivisionRepository));
			_commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
			NavigationManager = navigationManager ?? throw new ArgumentNullException(nameof(navigationManager));
			_lifetimeScope = lifetimeScope ?? throw new ArgumentNullException(nameof(lifetimeScope));
			_fileDialogService = fileDialogService ?? throw new ArgumentNullException(nameof(fileDialogService));

			Build();

			UoW = unitOfWorkFactory.CreateWithoutRoot();

			comboPart.ItemsEnum = typeof(ReportParts);

			var now = DateTime.Now;

			StartDate = now.Date;
			EndDate = now.LatestDayTime();

			dateStart.Binding.AddBinding(this, dlg => dlg.StartDate, w => w.Date).InitializeFromSource();
			dateEnd.Binding.AddBinding(this, dlg => dlg.EndDate, w => w.Date).InitializeFromSource();

			var officeFilter = new EmployeeFilterViewModel();

			officeFilter.SetAndRefilterAtOnce(
				x => x.Status = EmployeeStatus.IsWorking,
				x => x.RestrictCategory = EmployeeCategory.office);

			var employeeFactory = new EmployeeJournalFactory(officeFilter);

			evmeCashier.SetEntityAutocompleteSelectorFactory(employeeFactory.CreateWorkingOfficeEmployeeAutocompleteSelectorFactory());
			evmeCashier.CanOpenWithoutTabParent = true;

			evmeEmployee.SetEntityAutocompleteSelectorFactory(employeeFactory.CreateWorkingEmployeeAutocompleteSelectorFactory());
			evmeEmployee.CanOpenWithoutTabParent = true;

			UserSubdivisions = GetSubdivisionsForUser();

			specialListCmbCashSubdivisions.SetRenderTextFunc<Subdivision>(s => s.Name);
			specialListCmbCashSubdivisions.ItemsList = UserSubdivisions;

			ylblOrganisations.Visible = specialListCmbOrganisations.Visible = false;
			Organisations = UoW.GetAll<Organization>();
			specialListCmbOrganisations.SetRenderTextFunc<Organization>(s => s.Name);
			specialListCmbOrganisations.ItemsList = Organisations;

			int currentUserId = commonServices.UserService.CurrentUserId;

			_canGenerateCashReportsForOrganisations =
				commonServices.PermissionService.ValidateUserPresetPermission(Permissions.Cash.CanGenerateCashReportsForOrganizations, currentUserId);

			_canGenerateCashFlowDdsReport = commonServices.PermissionService.ValidateUserPresetPermission(Permissions.Cash.CanGenerateCashFlowDdsReport, currentUserId);

			checkOrganisations.Visible = _canGenerateCashReportsForOrganisations;
			checkOrganisations.Toggled += CheckOrganisationsToggled;

			entryExpenseFinancialCategory.Sensitive = false;
			entryIncomeFinancialCategory.Sensitive = false;

			CanGenerateDdsReport = _canGenerateCashFlowDdsReport;

			GenerateDdsReportCommand = new DelegateCommand(OnButtonGenerateDDSClicked, () => CanGenerateDdsReport);

			buttonGenerateDDS.Binding
				.AddBinding(this, dlg => dlg.CanGenerateDdsReport, w => w.Sensitive)
				.InitializeFromSource();

			buttonGenerateDDS.Clicked += (s, e) => GenerateDdsReportCommand.Execute();
		}

		public ITdiTab ParentTab
		{
			get => _parentTab;
			set
			{
				_parentTab = value;
				FinancialExpenseCategoryViewModel = BuildFinancialExpenseCategoryViewModel();
				FinancialIncomeCategoryViewModel = BuildFinancialIncomeCategoryViewModel();
				entryExpenseFinancialCategory.ViewModel = FinancialExpenseCategoryViewModel;
				entryIncomeFinancialCategory.ViewModel = FinancialIncomeCategoryViewModel;
			}
		}

		private IEnumerable<Subdivision> UserSubdivisions { get; }
		private IEnumerable<Organization> Organisations { get; }

		public FinancialExpenseCategory FinancialExpenseCategory
		{
			get => _financialExpenseCategory;
			set => _financialExpenseCategory = value;
		}

		public FinancialIncomeCategory FinancialIncomeCategory
		{
			get => _financialIncomeCategory;
			set => _financialIncomeCategory = value;
		}

		public IEntityEntryViewModel FinancialExpenseCategoryViewModel { get; private set; }

		private IEntityEntryViewModel BuildFinancialExpenseCategoryViewModel()
		{
			var financialExpenseCategoryEntryViewModelBuilder = new LegacyEEVMBuilderFactory<CashFlow>(ParentTab, this, UoW, NavigationManager, _lifetimeScope);

			var viewModel = financialExpenseCategoryEntryViewModelBuilder
				.ForProperty(x => x.FinancialExpenseCategory)
				.UseViewModelJournalAndAutocompleter<FinancialCategoriesGroupsJournalViewModel, FinancialCategoriesJournalFilterViewModel>(
					filter =>
					{
						filter.RestrictFinancialSubtype = FinancialSubType.Expense;
						filter.RestrictNodeSelectTypes.Add(typeof(FinancialExpenseCategory));
					})
				.Finish();

			return viewModel;
		}

		public IEntityEntryViewModel FinancialIncomeCategoryViewModel { get; private set; }

		private IEntityEntryViewModel BuildFinancialIncomeCategoryViewModel()
		{
			var financialIncomeCategoryEntryViewModelBuilder = new LegacyEEVMBuilderFactory<CashFlow>(ParentTab, this, UoW, NavigationManager, _lifetimeScope);

			var viewModel = financialIncomeCategoryEntryViewModelBuilder
				.ForProperty(x => x.FinancialIncomeCategory)
				.UseViewModelJournalAndAutocompleter<FinancialCategoriesGroupsJournalViewModel, FinancialCategoriesJournalFilterViewModel>(
					filter =>
					{
						filter.RestrictFinancialSubtype = FinancialSubType.Income;
						filter.RestrictNodeSelectTypes.Add(typeof(FinancialIncomeCategory));
					})
				.Finish();

			return viewModel;
		}

		public DelegateCommand GenerateDdsReportCommand { get; }

		private void OnButtonGenerateDDSClicked()
		{
			GenerateCashFlowDdsReport();
		}

		private void GenerateCashFlowDdsReport()
		{
			CanGenerateDdsReport = false;

			var path = RunSaveAsDialog();

			var cashFlowDdsReport = CashFlowDdsReport.GenerateReport(UoW, StartDate, EndDate);

			ExportCashFlowDdsReport(cashFlowDdsReport, path);

			CanGenerateDdsReport = true;

			RunOpenDialog(path);
		}

		private void ExportCashFlowDdsReport(CashFlowDdsReport cashFlowDdsReport, string path)
		{
			RenderCashFlowDdsReport(cashFlowDdsReport, path);
		}

		private void RunOpenDialog(string path)
		{
			Application.Invoke((s, e) =>
			{
				if(_commonServices.InteractiveService.Question(
				"Открыть отчет?",
				"Отчет сохранен"))
				{
					Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
				}
			});
		}

		private string RunSaveAsDialog()
		{
			var dialogSettings = new DialogSettings
			{
				Title = "Сохранить",
				DefaultFileExtention = ".xlsx",
				FileName = $"Отчет ДДС от {DateTime.Now:yyyy-MM-dd-HH-mm}.xlsx",
				InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
			};

			var result = _fileDialogService.RunSaveFileDialog(dialogSettings);

			if(result.Successful)
			{
				return result.Path;
			}

			return null;
		}

		private void RenderCashFlowDdsReport(CashFlowDdsReport report, string path)
		{
			var rendered = _cashFlowDdsReportRenderer.Render(report);
			rendered.SaveAs(path);
		}

		private void CheckOrganisationsToggled(object sender, EventArgs e)
		{
			if(checkOrganisations.Active)
			{
				ylblCashSubdivisions.Visible = specialListCmbCashSubdivisions.Visible = false;
				ylblOrganisations.Visible = specialListCmbOrganisations.Visible = true;
			}
			else
			{
				ylblCashSubdivisions.Visible = specialListCmbCashSubdivisions.Visible = true;
				ylblOrganisations.Visible = specialListCmbOrganisations.Visible = false;
			}
		}

		private IEnumerable<Subdivision> GetSubdivisionsForUser() =>
			_subdivisionRepository.GetCashSubdivisionsAvailableForUser(UoW, _commonServices.UserService.GetCurrentUser(UoW));

		#region IParametersWidget implementation

		public string Title => "Доходы и расходы";

		public INavigationManager NavigationManager { get; }

		public bool CanGenerateDdsReport
		{
			get => _canGenerateDdsReport;
			private set
			{
				if(_canGenerateDdsReport != value)
				{
					_canGenerateDdsReport = value;
					PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CanGenerateDdsReport)));
				}
			}
		}

		public DateTime StartDate
		{
			get => _startDate;
			private set
			{
				if(_startDate != value)
				{
					_startDate = value;
					PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StartDate)));
				}
			}
		}

		public DateTime EndDate
		{
			get => _endDate;
			private set
			{
				if(_endDate != value)
				{
					_endDate = value;
					PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(EndDate)));
				}
			}
		}

		public event EventHandler<LoadReportEventArgs> LoadReport;
		public event PropertyChangedEventHandler PropertyChanged;

		#endregion

		private void OnUpdate(bool hide = false)
		{
			if(!UserSubdivisions.Any())
			{
				ServicesConfig.InteractiveService.ShowMessage(ImportanceLevel.Error, "Пользователь не имеет доступа к кассам. Отчет сформировать невозможно.");
				return;
			}

			LoadReport?.Invoke(this, new LoadReportEventArgs(GetReportInfo(), hide));
		}

		protected void OnButtonRunClicked(object sender, EventArgs e) => OnUpdate(true);

		private ReportInfo GetReportInfo()
		{
			string ReportName;
			switch(checkOrganisations.Active)
			{
				case true:
					if(checkDetail.Active)
					{
						if(comboPart.SelectedItem.Equals(Gamma.Widgets.SpecialComboState.All))
						{
							ReportName = "Cash.CashFlowDetailOrganisations";
						}
						else if(comboPart.SelectedItem.Equals(ReportParts.IncomeAll))
						{
							ReportName = "Cash.CashFlowDetailIncomeAllOrganisations";
						}
						else if(comboPart.SelectedItem.Equals(ReportParts.Income))
						{
							ReportName = "Cash.CashFlowDetailIncomeOrganisations";
						}
						else if(comboPart.SelectedItem.Equals(ReportParts.IncomeReturn))
						{
							ReportName = "Cash.CashFlowDetailIncomeReturnOrganisations";
						}
						else if(comboPart.SelectedItem.Equals(ReportParts.ExpenseAll))
						{
							ReportName = "Cash.CashFlowDetailExpenseAllOrganisations";
						}
						else if(comboPart.SelectedItem.Equals(ReportParts.Expense))
						{
							ReportName = "Cash.CashFlowDetailExpenseOrganisations";
						}
						else if(comboPart.SelectedItem.Equals(ReportParts.Advance))
						{
							ReportName = "Cash.CashFlowDetailAdvanceOrganisations";
						}
						else if(comboPart.SelectedItem.Equals(ReportParts.AdvanceReport))
						{
							ReportName = "Cash.CashFlowDetailAdvanceReportOrganisations";
						}
						else if(comboPart.SelectedItem.Equals(ReportParts.UnclosedAdvance))
						{
							ReportName = "Cash.CashFlowDetailUnclosedAdvanceOrganisations";
						}
						else
						{
							throw new InvalidOperationException("Неизвестный раздел.");
						}
					}
					else
					{
						ReportName = "Cash.CashFlowOrganisations";
					}

					break;
				default:
					if(checkDetail.Active)
					{
						if(comboPart.SelectedItem.Equals(Gamma.Widgets.SpecialComboState.All))
						{
							ReportName = "Cash.CashFlowDetail";
						}
						else if(comboPart.SelectedItem.Equals(ReportParts.IncomeAll))
						{
							ReportName = "Cash.CashFlowDetailIncomeAll";
						}
						else if(comboPart.SelectedItem.Equals(ReportParts.Income))
						{
							ReportName = "Cash.CashFlowDetailIncome";
						}
						else if(comboPart.SelectedItem.Equals(ReportParts.IncomeReturn))
						{
							ReportName = "Cash.CashFlowDetailIncomeReturn";
						}
						else if(comboPart.SelectedItem.Equals(ReportParts.ExpenseAll))
						{
							ReportName = "Cash.CashFlowDetailExpenseAll";
						}
						else if(comboPart.SelectedItem.Equals(ReportParts.Expense))
						{
							ReportName = "Cash.CashFlowDetailExpense";
						}
						else if(comboPart.SelectedItem.Equals(ReportParts.Advance))
						{
							ReportName = "Cash.CashFlowDetailAdvance";
						}
						else if(comboPart.SelectedItem.Equals(ReportParts.AdvanceReport))
						{
							ReportName = "Cash.CashFlowDetailAdvanceReport";
						}
						else if(comboPart.SelectedItem.Equals(ReportParts.UnclosedAdvance))
						{
							ReportName = "Cash.CashFlowDetailUnclosedAdvance";
						}
						else
						{
							throw new InvalidOperationException("Неизвестный раздел.");
						}
					}
					else
					{
						ReportName = "Cash.CashFlow";
					}

					break;
			}

			var inCat =
				FinancialIncomeCategory is null
				? -1
				: FinancialIncomeCategory.Id;

			bool exCategorySelected = FinancialExpenseCategory != null;
			var ids = new List<int>();

			if(exCategorySelected)
			{
				FineIds(ids, FinancialExpenseCategory.Id);
			}
			else
			{
				ids.Add(0); //Add fake value
			}

			var casherId = evmeCashier.Subject == null ? -1 : evmeCashier.SubjectId;
			var casherName = evmeCashier.Subject == null ? "" : ((Employee)evmeCashier.Subject).ShortName;
			var employeeId = evmeEmployee.Subject == null ? -1 : evmeEmployee.SubjectId;
			var employeeName = evmeEmployee.Subject == null ? "" : ((Employee)evmeEmployee.Subject).ShortName;

			IEnumerable<int> cashSubdivisions;
			IEnumerable<int> organisations;

			if(specialListCmbCashSubdivisions.SelectedItem == null)
			{
				cashSubdivisions = UserSubdivisions.Any() ? UserSubdivisions.Select(x => x.Id) : new[] { -1 };
			}
			else
			{
				cashSubdivisions = new[] { (specialListCmbCashSubdivisions.SelectedItem as Subdivision).Id };
			}

			if(specialListCmbOrganisations.SelectedItem == null)
			{
				organisations = Organisations.Any() ? Organisations.Select(x => x.Id) : new[] { -1 };
			}
			else
			{
				organisations = new[] { (specialListCmbOrganisations.SelectedItem as Organization).Id };
			}

			var cashSubdivisionsName = specialListCmbCashSubdivisions.SelectedItem == null ?
				string.Join(", ", UserSubdivisions.Select(x => x.Name))
				: UserSubdivisions.Where(x => x.Id == (specialListCmbCashSubdivisions.SelectedItem as Subdivision).Id)
								  .Select(x => x.Name)
								  .SingleOrDefault();

			var reportInfo = new ReportInfo
			{
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

			if(checkOrganisations.Active)
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

		private void FineIds(IList<int> result, int categoryId)
		{
			result.Add(categoryId);
		}

		protected void OnDateChanged(object sender, EventArgs e)
		{
			buttonRun.Sensitive = dateStart.DateOrNull != null && dateEnd.DateOrNull != null;
		}

		protected void OnCheckDetailToggled(object sender, EventArgs e)
		{
			var sensitive = checkDetail.Active;

			comboPart.Sensitive = sensitive;
			entryExpenseFinancialCategory.Sensitive = sensitive;
			entryIncomeFinancialCategory.Sensitive = sensitive;
		}

		protected void OnComboPartEnumItemSelected(object sender, Gamma.Widgets.ItemSelectedEventArgs e)
		{
			if(comboPart.SelectedItem.Equals(Gamma.Widgets.SpecialComboState.All)
				|| comboPart.SelectedItem.Equals(ReportParts.IncomeAll))
			{
				entryExpenseFinancialCategory.Sensitive = true;
				entryIncomeFinancialCategory.Sensitive = true;
			}
			else if(comboPart.SelectedItem.Equals(ReportParts.Income))
			{
				entryExpenseFinancialCategory.Sensitive = false;
				entryIncomeFinancialCategory.Sensitive = true;
			}
			else if(comboPart.SelectedItem.Equals(ReportParts.ExpenseAll)
					   || comboPart.SelectedItem.Equals(ReportParts.Expense)
					   || comboPart.SelectedItem.Equals(ReportParts.Advance)
					   || comboPart.SelectedItem.Equals(ReportParts.AdvanceReport)
					|| comboPart.SelectedItem.Equals(ReportParts.UnclosedAdvance)
				|| comboPart.SelectedItem.Equals(ReportParts.IncomeReturn))
			{
				entryExpenseFinancialCategory.Sensitive = true;
				entryIncomeFinancialCategory.Sensitive = false;
			}
			else
			{
				throw new InvalidOperationException("Неизвестный раздел.");
			}
		}
	}
}
