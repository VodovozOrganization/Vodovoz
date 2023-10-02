using Autofac;
using NHibernate.Transform;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.Navigation;
using QS.Project.Filter;
using QS.Services;
using QS.ViewModels.Control.EEVM;
using QS.ViewModels.Dialog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Documents.MovementDocuments;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Store;
using Vodovoz.EntityRepositories;
using Vodovoz.Infrastructure.Report.SelectableParametersFilter;
using Vodovoz.Presentation.ViewModels.Common;
using Vodovoz.ViewModels.Dialogs.Goods;
using Vodovoz.ViewModels.Journals.FilterViewModels.Employees;
using Vodovoz.ViewModels.Journals.FilterViewModels.Goods;
using Vodovoz.ViewModels.Journals.JournalViewModels.Employees;
using Vodovoz.ViewModels.Journals.JournalViewModels.Goods;
using Vodovoz.ViewModels.Reports;
using Vodovoz.ViewModels.ViewModels.Employees;

namespace Vodovoz.ViewModels.Journals.FilterViewModels.Store
{
	public class WarehouseDocumentsItemsJournalFilterViewModel : FilterViewModelBase<WarehouseDocumentsItemsJournalFilterViewModel>
	{
		private const string _haveAccessOnlyToWarehouseAndComplaintsPermissionName = "user_have_access_only_to_warehouse_and_complaints";

		private readonly ICurrentPermissionService _currentPermissionService;
		private readonly INavigationManager _navigationManager;
		private readonly ILifetimeScope _lifetimeScope;
		private readonly IUserService _userService;
		private readonly IUserRepository _userRepository;
		private readonly IInteractiveService _interactiveService;
		private readonly SelectableParametersReportFilter _filter;
		private DateTime? _startDate;
		private DateTime? _endDate;
		private MovementDocumentStatus? _movementDocumentStatus;
		private Employee _driver;
		private DocumentType? _documentType;
		private TargetSource _targetSource;
		private SelectableFilterType _filterType;
		private SelectableParameterReportFilterViewModel _filterViewModel;
		private List<int> _counterpartyIds = new List<int>();
		private List<int> _warhouseIds = new List<int>();
		private DialogViewModelBase _journalViewModel;
		private Employee _author;
		private Employee _lastEditor;
		private Nomenclature _nomenclature;
		private bool _showNotAffectedBalance = false;
		private int? _documentId;
		private IncludeExludeFiltersViewModel _includeExcludeFilterViewModel;

		public WarehouseDocumentsItemsJournalFilterViewModel(
			ICurrentPermissionService currentPermissionService,
			INavigationManager navigationManager,
			ILifetimeScope lifetimeScope,
			IUserService userService,
			IUserRepository userRepository,
			IInteractiveService interactiveService)
		{
			_currentPermissionService = currentPermissionService ?? throw new ArgumentNullException(nameof(currentPermissionService));
			_navigationManager = navigationManager ?? throw new ArgumentNullException(nameof(navigationManager));
			_lifetimeScope = lifetimeScope ?? throw new ArgumentNullException(nameof(lifetimeScope));
			_userService = userService ?? throw new ArgumentNullException(nameof(userService));
			_userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
			_interactiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));
			StartDate = DateTime.Today.AddDays(-7);
			EndDate = DateTime.Today.AddDays(1);
			TargetSource = TargetSource.Both;

			_filter = new SelectableParametersReportFilter(UoW);
			ShowInfoCommand = new DelegateCommand(ShowInfo);
			ConfigureFilter();
		}

		public DelegateCommand ShowInfoCommand { get; }

		public int? DocumentId
		{
			get => _documentId;
			set => UpdateFilterField(ref _documentId, value);
		}

		public DateTime? StartDate
		{
			get => _startDate;
			set => UpdateFilterField(ref _startDate, value);
		}

		public DateTime? EndDate
		{
			get => _endDate;
			set => UpdateFilterField(ref _endDate, value);
		}

		public Employee Driver
		{
			get => _driver;
			set => UpdateFilterField(ref _driver, value);
		}

		[PropertyChangedAlso(nameof(ShowMovementDocumentFilterDetails))]
		public DocumentType? DocumentType
		{
			get => _documentType;
			set => UpdateFilterField(ref _documentType, value);
		}

		public MovementDocumentStatus? MovementDocumentStatus
		{
			get => _movementDocumentStatus;
			set => UpdateFilterField(ref _movementDocumentStatus, value);
		}

		public TargetSource TargetSource
		{
			get => _targetSource;
			set => UpdateFilterField(ref _targetSource, value);
		}

		public Employee Author
		{
			get => _author;
			set => UpdateFilterField(ref _author, value);
		}

		public Employee LastEditor
		{
			get => _lastEditor;
			set => UpdateFilterField(ref _lastEditor, value);
		}

		public Nomenclature Nomenclature
		{
			get => _nomenclature;
			set => UpdateFilterField(ref _nomenclature, value);
		}

		public List<int> CounterpartyIds
		{
			get => _counterpartyIds;
			private set => UpdateFilterField(ref _counterpartyIds, value);
		}

		public string CounterpartiesNames => GetSelectedParametersTitles(_filter.GetSelectedParametersTitlesFromParameterSet(nameof(Counterparty)));

		public List<int> WarehouseIds
		{
			get => _warhouseIds;
			private set => UpdateFilterField(ref _warhouseIds, value);
		}

		public string WarehousesNames => GetSelectedParametersTitles(_filter.GetSelectedParametersTitlesFromParameterSet(nameof(Warehouse)));

		public SelectableParameterReportFilterViewModel FilterViewModel
		{
			get => _filterViewModel;
			set => UpdateFilterField(ref _filterViewModel, value);
		}

		public IncludeExludeFiltersViewModel IncludeExcludeFilterViewModel => _includeExcludeFilterViewModel;

		public SelectableFilterType FilterType
		{
			get => _filterType;
			set => UpdateFilterField(ref _filterType, value);
		}

		public bool ShowNotAffectedBalance
		{
			get => _showNotAffectedBalance;
			set => UpdateFilterField(ref _showNotAffectedBalance, value);
		}

		public DialogViewModelBase JournalViewModel
		{
			get => _journalViewModel;
			set
			{
				_journalViewModel = value;

				var driverEntryViewModel =
					new CommonEEVMBuilderFactory<WarehouseDocumentsItemsJournalFilterViewModel>(value, this, UoW, _navigationManager, _lifetimeScope)
					.ForProperty(x => x.Driver)
					.UseViewModelDialog<EmployeeViewModel>()
					.UseViewModelJournalAndAutocompleter<EmployeesJournalViewModel, EmployeeFilterViewModel>(
						filter =>
						{
							filter.RestrictCategory = EmployeeCategory.driver;
							filter.Status = EmployeeStatus.IsWorking;
						}
					)
					.Finish();

				driverEntryViewModel.CanViewEntity = false;

				DriverEntityEntryViewModel = driverEntryViewModel;

				var authorEntryViewModel =
					new CommonEEVMBuilderFactory<WarehouseDocumentsItemsJournalFilterViewModel>(value, this, UoW, _navigationManager, _lifetimeScope)
					.ForProperty(x => x.Author)
					.UseViewModelDialog<EmployeeViewModel>()
					.UseViewModelJournalAndAutocompleter<EmployeesJournalViewModel, EmployeeFilterViewModel>(
						filter =>
						{
							filter.Status = EmployeeStatus.IsWorking;
						}
					)
					.Finish();

				authorEntryViewModel.CanViewEntity = false;

				AuthorEntityEntryViewModel = authorEntryViewModel;

				var lastEditorEntryViewModel =
					new CommonEEVMBuilderFactory<WarehouseDocumentsItemsJournalFilterViewModel>(value, this, UoW, _navigationManager, _lifetimeScope)
					.ForProperty(x => x.LastEditor)
					.UseViewModelDialog<EmployeeViewModel>()
					.UseViewModelJournalAndAutocompleter<EmployeesJournalViewModel, EmployeeFilterViewModel>(
						filter =>
						{
							filter.Status = EmployeeStatus.IsWorking;
						}
					)
					.Finish();

				lastEditorEntryViewModel.CanViewEntity = false;

				LastEditorEntityEntryViewModel = lastEditorEntryViewModel;

				var nomenclatureEntryViewModel =
					new CommonEEVMBuilderFactory<WarehouseDocumentsItemsJournalFilterViewModel>(value, this, UoW, _navigationManager, _lifetimeScope)
					.ForProperty(x => x.Nomenclature)
					.UseViewModelDialog<NomenclatureViewModel>()
					.UseViewModelJournalAndAutocompleter<NomenclaturesJournalViewModel, NomenclatureFilterViewModel>(
						filter =>
						{
						}
					)
					.Finish();

				nomenclatureEntryViewModel.CanViewEntity = false;

				NomenclatureEntityEntryViewModel = nomenclatureEntryViewModel;
			}
		}

		public bool CanReadWarehouse => !_currentPermissionService.ValidatePresetPermission(_haveAccessOnlyToWarehouseAndComplaintsPermissionName) || _userService.GetCurrentUser().IsAdmin;

		public bool CanUpdateWarehouse => CanReadWarehouse;

		public bool ShowMovementDocumentFilterDetails => DocumentType.HasValue && (DocumentType.Value == Domain.Documents.DocumentType.MovementDocument);

		public EntityEntryViewModel<Employee> DriverEntityEntryViewModel { get; private set; }

		public EntityEntryViewModel<Employee> AuthorEntityEntryViewModel { get; private set; }

		public EntityEntryViewModel<Employee> LastEditorEntityEntryViewModel { get; private set; }

		public EntityEntryViewModel<Nomenclature> NomenclatureEntityEntryViewModel { get; private set; }

		public bool CanReadEmployee => _currentPermissionService.ValidateEntityPermission(typeof(Employee)).CanRead;

		public bool CanReadNomenclature => _currentPermissionService.ValidateEntityPermission(typeof(Nomenclature)).CanRead;

		private void ConfigureFilter()
		{
			_filter.CreateParameterSet(
				"Контрагент",
				nameof(Counterparty),
				new ParametersFactory(UoW, (filters) =>
				{
					SelectableEntityParameter<Counterparty> resultAlias = null;
					var query = UoW.Session.QueryOver<Counterparty>()
						.Where(x => !x.IsArchive);
					if(filters != null && filters.Any())
					{
						foreach(var f in filters)
						{
							query.Where(f());
						}
					}

					query.SelectList(list => list
						.Select(x => x.Id).WithAlias(() => resultAlias.EntityId)
						.Select(x => x.FullName).WithAlias(() => resultAlias.EntityTitle)
					);
					query.TransformUsing(Transformers.AliasToBean<SelectableEntityParameter<Counterparty>>());
					return query.List<SelectableParameter>();
				}));

			_filter.CreateParameterSet(
				"Склад",
				nameof(Warehouse),
				new ParametersFactory(UoW, (filters) =>
				{
					SelectableEntityParameter<Warehouse> resultAlias = null;
					var query = UoW.Session.QueryOver<Warehouse>()
						.Where(x => !x.IsArchive);
					if(filters != null && filters.Any())
					{
						foreach(var f in filters)
						{
							query.Where(f());
						}
					}

					query.SelectList(list => list
						.Select(x => x.Id).WithAlias(() => resultAlias.EntityId)
						.Select(x => x.Name).WithAlias(() => resultAlias.EntityTitle)
					);
					query.TransformUsing(Transformers.AliasToBean<SelectableEntityParameter<Warehouse>>());
					return query.List<SelectableParameter>();
				}));

			FilterViewModel = new SelectableParameterReportFilterViewModel(_filter);

			FilterViewModel.SelectionChanged += OnFilterViewModelSelectionChanged;
			FilterViewModel.FilterModeChanged += OnFilterViewModelFilterModeChanged;

			_includeExcludeFilterViewModel = new IncludeExludeFiltersViewModel(_interactiveService);
			//_includeExcludeFilterViewModel.AddFilter(UoW, _subdivisionRepository);
		}

		private void OnFilterViewModelFilterModeChanged(object sender, FilterTypeChangedArgs e)
		{
			FilterType = e.FilterType;
		}

		private void OnFilterViewModelSelectionChanged(object sender, SelectableParameterReportFilterSelectionChangedArgs e)
		{
			switch(e.Name)
			{
				case nameof(Counterparty):
					foreach(var parameter in e.ParametersChanged)
					{
						if(parameter.Value)
						{
							_counterpartyIds.Add((int)parameter.Id);
						}
						else
						{
							_counterpartyIds.Remove((int)parameter.Id);
						}
					}
					SetAndRefilterAtOnce();
					break;
				case nameof(Warehouse):
					foreach(var parameter in e.ParametersChanged)
					{
						if(parameter.Value)
						{
							_warhouseIds.Add((int)parameter.Id);
						}
						else
						{
							_warhouseIds.Remove((int)parameter.Id);
						}
					}
					SetAndRefilterAtOnce();
					break;
				default:
					throw new InvalidOperationException($"Сет параметров с именем {e.Name} не поддерживается");
			}
		}

		private string GetSelectedParametersTitles(IDictionary<string, string> selectedParametersTitles)
		{
			var sb = new StringBuilder();

			var notSetValues = new string[]{
				"Все",
				"Нет"
			};

			if(selectedParametersTitles.Any())
			{
				foreach(var item in selectedParametersTitles)
				{
					if(!notSetValues.Contains(item.Value))
					{
						sb.AppendLine($"{item.Key}{item.Value.Trim('\n', '\r')}");
					}
				}
			}

			return sb.ToString().TrimEnd('\n');
		}

		private void ShowInfo()
		{
			_interactiveService.ShowMessage(ImportanceLevel.Info,
				"При двойном клике по любой строке в новой вкладке открывается документ, содержащий данную строку.\n" +
				"Кнопка \"Выгрузить карточку складского учета\" становится доступной при выборе в фильтрах одного товара и одного склада.\n" +
				"Доступен поиск по источникам/получателям/источнико-получателям(оба) номенклатур\n" +
				"Галочка \"Отображать строки не влияющие на баланс\" -включает отображение документов/строк документов не влияющих на баланс остатков.",
				"Справка");
		}
	}
}
