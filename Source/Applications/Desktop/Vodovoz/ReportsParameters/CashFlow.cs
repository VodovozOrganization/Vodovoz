using Autofac;
using DateTimeHelpers;
using Org.BouncyCastle.Security;
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
using System.IO;
using System.Linq;
using System.Reflection;
using Vodovoz.Core.Domain.Employees;
using Vodovoz.Domain.Cash.FinancialCategoriesGroups;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Organizations;
using Vodovoz.EntityRepositories.Subdivisions;
using Vodovoz.Reports.Editing;
using Vodovoz.Reports.Editing.Modifiers.CashFlowDetailReports;
using Vodovoz.Settings.Cash;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Cash.FinancialCategoriesGroups;
using Vodovoz.ViewModels.Journals.FilterViewModels.Employees;

namespace Vodovoz.Reports
{
	public partial class CashFlow : SingleUowTabBase, IParametersWidget, INotifyPropertyChanged
	{
		private readonly IReportInfoFactory _reportInfoFactory;
		private readonly ISubdivisionRepository _subdivisionRepository;
		private readonly ICommonServices _commonServices;
		private readonly ILifetimeScope _lifetimeScope;
		private readonly IFileDialogService _fileDialogService;

		private const string _defaultReportFileName = "CashFlow.rdl";
		private const string _organisationsReportFileName = "CashFlowOrganisations.rdl";
		private const string _defaultDetailReportFileName = "CashFlowDetail.rdl";
		private const string _organisationsDetailReportFileName = "CashFlowDetailOrganisations.rdl";

		private bool _canGenerateCashReportsForOrganisations;

		private FinancialExpenseCategory _financialExpenseCategory;
		private FinancialIncomeCategory _financialIncomeCategory;
		private ITdiTab _parentTab;

		private DateTime _startDate;
		private DateTime _endDate;

		public CashFlow(
			IUnitOfWorkFactory unitOfWorkFactory,
			IReportInfoFactory reportInfoFactory,
			IEmployeeJournalFactory employeeJournalFactory,
			ISubdivisionRepository subdivisionRepository,
			ICommonServices commonServices,
			INavigationManager navigationManager,
			ILifetimeScope lifetimeScope,
			IFileDialogService fileDialogService
			)
		{
			if(employeeJournalFactory == null)
			{
				throw new ArgumentNullException(nameof(employeeJournalFactory));
			}

			_reportInfoFactory = reportInfoFactory ?? throw new ArgumentNullException(nameof(reportInfoFactory));
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

			evmeCashier.SetEntityAutocompleteSelectorFactory(
				employeeJournalFactory.CreateWorkingOfficeEmployeeAutocompleteSelectorFactory(true));
			evmeCashier.CanOpenWithoutTabParent = true;

			evmeEmployee.SetEntityAutocompleteSelectorFactory(employeeJournalFactory.CreateWorkingEmployeeAutocompleteSelectorFactory());
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
				commonServices.PermissionService.ValidateUserPresetPermission(Vodovoz.Core.Domain.Permissions.CashPermissions.CanGenerateCashReportsForOrganizations, currentUserId);

			checkOrganisations.Visible = _canGenerateCashReportsForOrganisations;
			checkOrganisations.Toggled += CheckOrganisationsToggled;

			entryExpenseFinancialCategory.Sensitive = false;
			entryIncomeFinancialCategory.Sensitive = false;
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
			_subdivisionRepository.GetCashSubdivisionsAvailableForUser(UoW, _commonServices.UserService.GetCurrentUser());

		#region IParametersWidget implementation

		public string Title => "Доходы и расходы";

		public INavigationManager NavigationManager { get; }

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
			var source = GetReportSource();

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

			var parameters = new Dictionary<string, object> {
					{ "StartDate", dateStart.DateOrNull.Value },
					{ "EndDate", dateEnd.DateOrNull.Value },
					{ "IncomeCategory", inCat },
					{ "ExpenseCategory", ids },
					{ "ExpenseCategoryUsed", exCategorySelected ? 1 : 0 },
					{ "Casher", casherId },
					{ "Employee", employeeId },
					{ "CasherName", casherName },
					{ "EmployeeName", employeeName }
				};

			if(checkOrganisations.Active)
			{
				parameters.Add("organisations", organisations);
				parameters.Add("organisation_name",
					(specialListCmbOrganisations.SelectedItem as Organization) != null
						? (specialListCmbOrganisations.SelectedItem as Organization).Name
						: "Все организации");
			}
			else
			{
				parameters.Add("cash_subdivisions", cashSubdivisions);
				parameters.Add("cash_subdivisions_name", cashSubdivisionsName);
			}

			var cashCategorySettings = _lifetimeScope.Resolve<IOrganizationCashTransferDocumentSettings>();

			parameters.Add("cash_income_category_transfer_id", cashCategorySettings.CashIncomeCategoryTransferId);
			parameters.Add("cash_expense_category_transfer_id", cashCategorySettings.CashExpenseCategoryTransferId);

			var reportInfo = _reportInfoFactory.Create();
			reportInfo.Parameters = parameters;
			reportInfo.Source = source;
			reportInfo.Title = Title;

			return reportInfo;
		}

		private string GetReportSource()
		{
			string source;

			if(checkDetail.Active)
			{
				var reportFileName = checkOrganisations.Active
					? _organisationsDetailReportFileName
					: _defaultDetailReportFileName;

				var modifier = new CashFlowDetailReportModifier();

				if(comboPart.SelectedItem.Equals(Gamma.Widgets.SpecialComboState.All))
				{
					source = ModifyReport(reportFileName, null);
				}
				else
				{
					if(comboPart.SelectedItem is ReportParts reportPart)
					{
						modifier.Setup(reportPart);
					}
					else
					{
						throw new InvalidOperationException("Неизвестный раздел.");
					}

					source = ModifyReport(reportFileName, modifier);
				}
			}
			else
			{
				var reportFileName = checkOrganisations.Active
					? _organisationsReportFileName
					: _defaultReportFileName;

				source = ModifyReport(reportFileName, null);
			}

			return source;
		}

		private string ModifyReport(string reportFileName, ReportModifierBase modifier)
		{
			if(string.IsNullOrWhiteSpace(reportFileName))
			{
				throw new InvalidParameterException("Для загрузки шаблона отчета необходимо указанть имя файла");
			}

			var root = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			var path = System.IO.Path.Combine(root, "Reports", "Cash", reportFileName);

			using(ReportController reportController = new ReportController(path))
			using(var reportStream = new MemoryStream())
			{
				reportController.AddModifier(modifier);
				reportController.Modify();
				reportController.Save(reportStream);

				using(var reader = new StreamReader(reportStream))
				{
					reportStream.Position = 0;
					var outputSource = reader.ReadToEnd();
					return outputSource;
				}
			}
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
