using ClosedXML.Report;
using Gamma.Utilities;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Transform;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal.DataLoader;
using QS.Project.Journal.EntitySelector;
using QS.Project.Journal.Search;
using QS.Project.Search;
using QS.Services;
using QS.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Vodovoz.Domain;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.WageCalculation;
using Vodovoz.Services;
using Vodovoz.ViewModels.Journals.FilterViewModels.Orders;
using Vodovoz.ViewModels.Journals.JournalFactories;
using Vodovoz.ViewModels.Journals.JournalViewModels.Orders;
using Order = Vodovoz.Domain.Orders.Order;

namespace Vodovoz.ViewModels.ViewModels.Reports
{
	public class NomenclaturePlanReportViewModel : DialogTabViewModelBase
	{
		private readonly ICommonServices _commonServices;
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly INomenclaturePlanParametersProvider _nomenclaturePlanParametersProvider;
		private readonly IInteractiveService _interactiveService;

		private List<SelectedNomenclature> _savedNomenclatures;
		private List<SelectedEquipmentKind> _savedEquipmentKinds;
		private List<SelectedEquipmentType> _savedEquipmentTypes;
		private SearchHelper _nomenclatureSearchHelper, _employeeSearchHelper, _equipmentKindSearchHelper;
		private bool IsOneDay => StartDate == EndDate;
		private IList<ReportNode> _selectedReportColumns;
		private int? _callCenterEmployeesCount;
		private IList<Nomenclature> _nomenclaturePlans;
		private const string _templatePath = @".\Reports\Orders\NomenclaturePlanReport.xlsx";

		private ICriterion GetNomenclatureSearchCriterion(params Expression<Func<object>>[] aliasPropertiesExpr) =>
			_nomenclatureSearchHelper.GetSearchCriterion(aliasPropertiesExpr);

		private ICriterion GetEmployeeSearchCriterion(params Expression<Func<object>>[] aliasPropertiesExpr) =>
			_employeeSearchHelper.GetSearchCriterion(aliasPropertiesExpr);

		private ICriterion GetEquipmentKindSearchCriterion(params Expression<Func<object>>[] aliasPropertiesExpr) =>
			_equipmentKindSearchHelper.GetSearchCriterion(aliasPropertiesExpr);

		public NomenclaturePlanReportViewModel(IUnitOfWorkFactory unitOfWorkFactory, IInteractiveService interactiveService,
			INavigationManager navigation, ICommonServices commonServices, IProductGroupJournalFactory productGroupJournalFactory,
			INomenclaturePlanParametersProvider nomenclaturePlanParametersProvider) : base(unitOfWorkFactory, interactiveService,
			navigation)
		{
			Title = "Отчёт по мотивации КЦ";
			_commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_nomenclaturePlanParametersProvider = nomenclaturePlanParametersProvider ??
												  throw new ArgumentNullException(nameof(nomenclaturePlanParametersProvider));
			_interactiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));

			ProductGroupSelectorFactory =
				(productGroupJournalFactory ?? throw new ArgumentNullException(nameof(productGroupJournalFactory)))
				.CreateProductGroupAutocompleteSelectorFactory();

			CallCenterSubdivisionId = _nomenclaturePlanParametersProvider.CallCenterSubdivisionId;

			Configure();
		}

		public int CallCenterSubdivisionId { get; }

		private void Configure()
		{
			NomenclaturesConfigure();
			EmployeesConfigure();
			EquipmentKindsConfigure();
			EquipmentTypesConfigure();

			StartDate = EndDate = DateTime.Today;

			CanSaveCallCenterMotivationReportFilter =
				_commonServices.CurrentPermissionService.ValidatePresetPermission("can_save_callcenter_motivation_report_filter");

			SelectedProceeds = UoW.Session.QueryOver<SelectedProceeds>().SingleOrDefault() ?? new SelectedProceeds();

			NomenclatureDataLoader.PostLoadProcessingFunc = (items, since) =>
			{
				EmployeeDataLoader.LoadData(false);
				NomenclatureDataLoader.PostLoadProcessingFunc = null;
			};

			EmployeeDataLoader.PostLoadProcessingFunc = (items, since) =>
			{
				EquipmentKindDataLoader.LoadData(false);
				EmployeeDataLoader.PostLoadProcessingFunc = null;
			};
		}

		private void NomenclaturesConfigure()
		{
			NomenclatureSearchVM = new SearchViewModel();
			NomenclatureSearchVM.OnSearch += NomenclatureSearchOnSearch;
			_nomenclatureSearchHelper = new SearchHelper(NomenclatureSearchVM);

			//Предзагрузка. Для избежания ленивой загрузки
			UoW.Session.QueryOver<ProductGroup>().Fetch(SelectMode.ChildFetch, x => x.Childs).List();

			_savedNomenclatures = UoW.Session.QueryOver<SelectedNomenclature>()
				.List<SelectedNomenclature>()
				.OrderBy(x => x.Nomenclature.Name)
				.ToList();

			SelectedNomenclatures = new GenericObservableList<NomenclatureReportNode>(_savedNomenclatures
				.Select(x => new NomenclatureReportNode()
				{
					Id = x.Nomenclature.Id,
					Name = x.Nomenclature.Name,
					PlanDay = x.Nomenclature.PlanDay,
					PlanMonth = x.Nomenclature.PlanMonth
				})
				.ToList());

			NomenclatureDataLoader = new ThreadDataLoader<NomenclatureReportNode>(_unitOfWorkFactory) { PageSize = PageSize };
			NomenclatureDataLoader.AddQuery(NomenclatureItemsSourceQueryFunction);
		}

		public void NomenclatureSearchOnSearch(object sender, EventArgs e)
		{
			NomenclatureLastScrollPosition = 0;
			NomenclatureDataLoader.LoadData(IsNomenclatureNextPage = false);
		}

		private Func<IUnitOfWork, IQueryOver<Nomenclature>> NomenclatureItemsSourceQueryFunction => (uow) =>
		{
			Nomenclature nomenclatureAlias = null;
			NomenclatureReportNode nomenclatureResultAlias = null;

			var itemsQuery = UoW.Session.QueryOver(() => nomenclatureAlias);

			itemsQuery.Where(GetNomenclatureSearchCriterion(
					() => nomenclatureAlias.Id,
					() => nomenclatureAlias.Name,
					() => nomenclatureAlias.OnlineStoreExternalId
				)
			);

			if(ProductGroup != null)
			{
				var productGroups = GetProductGroupsRecursive(ProductGroup);
				itemsQuery.Where(n => n.ProductGroup.Id.IsIn(productGroups.Select(p => p.Id).ToArray()));
			}

			if(NomenclatureCategory != null)
			{
				itemsQuery.Where(x => x.Category == NomenclatureCategory);
			}

			if(SelectedNomenclatures?.Count > 0)
			{
				itemsQuery.WhereNot(n => n.Id.IsIn(SelectedNomenclatures.Select(sn => sn.Id).ToArray()));
			}

			itemsQuery
				.SelectList(list => list
					.Select(x => x.Id).WithAlias(() => nomenclatureResultAlias.Id)
					.Select(x => x.Name).WithAlias(() => nomenclatureResultAlias.Name)
					.Select(x => x.PlanDay).WithAlias(() => nomenclatureResultAlias.PlanDay)
					.Select(x => x.PlanMonth).WithAlias(() => nomenclatureResultAlias.PlanMonth)
				)
				.OrderBy(x => x.Name).Asc
				.TransformUsing(Transformers.AliasToBean<NomenclatureReportNode>());

			return itemsQuery;
		};

		private void EmployeesConfigure()
		{
			EmployeeSearchVM = new SearchViewModel();
			EmployeeSearchVM.OnSearch += EmployeeSearchOnSearch;
			_employeeSearchHelper = new SearchHelper(EmployeeSearchVM);

			SubdivisionReportNode subdivisionResultAlias = null;
			Subdivisions = UoW.Session.QueryOver<Subdivision>()
				.SelectList(list => list
					.Select(x => x.Id).WithAlias(() => subdivisionResultAlias.Id)
					.Select(x => x.Name).WithAlias(() => subdivisionResultAlias.Name)
				)
				.TransformUsing(Transformers.AliasToBean<SubdivisionReportNode>())
				.List<SubdivisionReportNode>()
				.OrderBy(x => x.Name);

			Subdivision = Subdivisions.FirstOrDefault(s => s.Id == _nomenclaturePlanParametersProvider.CallCenterSubdivisionId);

			SelectedEmployees = new GenericObservableList<EmployeeReportNode>();

			EmployeeDataLoader = new ThreadDataLoader<EmployeeReportNode>(_unitOfWorkFactory) { PageSize = PageSize };
			EmployeeDataLoader.AddQuery(EmployeeItemsSourceQueryFunction);
		}


		public void EmployeeSearchOnSearch(object sender, EventArgs e)
		{
			EmployeeLastScrollPosition = 0;
			EmployeeDataLoader.LoadData(IsEmployeeNextPage = false);
		}

		private Func<IUnitOfWork, IQueryOver<Employee>> EmployeeItemsSourceQueryFunction => (uow) =>
		{
			Employee employeeAlias = null;
			EmployeeReportNode employeeResultAlias = null;

			var itemsQuery = UoW.Session.QueryOver(() => employeeAlias);

			itemsQuery.Where(GetEmployeeSearchCriterion(
					() => employeeAlias.Id,
					() => employeeAlias.LastName,
					() => employeeAlias.Name,
					() => employeeAlias.Patronymic
				)
			);

			if(Subdivision != null)
			{
				itemsQuery.Where(e => e.Subdivision.Id == Subdivision.Id);
			}

			if(SelectedEmployees?.Count > 0)
			{
				itemsQuery.WhereNot(e => e.Id.IsIn(SelectedEmployees.Select(se => se.Id).ToArray()));
			}

			if(EmployeeStatus != null)
			{
				itemsQuery.Where(x => x.Status == EmployeeStatus);
			}

			itemsQuery
				.SelectList(list => list
					.Select(x => x.Id).WithAlias(() => employeeResultAlias.Id)
					.Select(x => x.LastName).WithAlias(() => employeeResultAlias.LastName)
					.Select(x => x.Name).WithAlias(() => employeeResultAlias.Name)
					.Select(x => x.Patronymic).WithAlias(() => employeeResultAlias.Patronymic)
				)
				.OrderBy(x => x.LastName).Asc
				.ThenBy(x => x.Name).Asc
				.ThenBy(x => x.Patronymic).Asc
				.TransformUsing(Transformers.AliasToBean<EmployeeReportNode>());

			return itemsQuery;
		};

		private void EquipmentKindsConfigure()
		{
			EquipmentKindSearchVM = new SearchViewModel();
			EquipmentKindSearchVM.OnSearch += EquipmentKindSearchOnSearch;
			_equipmentKindSearchHelper = new SearchHelper(EquipmentKindSearchVM);

			_savedEquipmentKinds = UoW.Session.QueryOver<SelectedEquipmentKind>()
				.List()
				.OrderBy(x => x.EquipmentKind.Name)
				.ToList();

			SelectedEquipmentKinds = new GenericObservableList<EquipmentKindReportNode>(_savedEquipmentKinds
				.Select(x => new EquipmentKindReportNode()
				{
					Id = x.EquipmentKind.Id,
					Name = x.EquipmentKind.Name,
				})
				.ToList());

			EquipmentKindDataLoader = new ThreadDataLoader<EquipmentKindReportNode>(_unitOfWorkFactory) { PageSize = PageSize };
			EquipmentKindDataLoader.AddQuery(EquipmentKindItemsSourceQueryFunction);
		}

		public void EquipmentKindSearchOnSearch(object sender, EventArgs e)
		{
			EquipmentKindLastScrollPosition = 0;
			EquipmentKindDataLoader.LoadData(IsEquipmentKindNextPage = false);
		}

		private void EquipmentTypesConfigure()
		{
			EquipmentTypeSearchVM = new SearchViewModel();
			EquipmentTypeSearchVM.OnSearch += EquipmentTypeSearchOnSearch;

			_savedEquipmentTypes = UoW.Session.QueryOver<SelectedEquipmentType>()
				.List<SelectedEquipmentType>()
				.ToList();

			EquipmentTypes = new GenericObservableList<EquipmentTypeReportNode>();
			foreach(EquipmentType equipmentType in Enum.GetValues(typeof(EquipmentType)))
			{
				if(!_savedEquipmentTypes.Any(x => x.EquipmentType == equipmentType))
				{
					EquipmentTypes.Add(new EquipmentTypeReportNode() { EquipmentType = equipmentType });
				}
			}

			SelectedEquipmentTypes = new GenericObservableList<EquipmentTypeReportNode>(_savedEquipmentTypes
				.Select(x => new EquipmentTypeReportNode()
				{
					EquipmentType = x.EquipmentType
				})
				.ToList());
		}

		public void EquipmentTypeSearchOnSearch(object sender, EventArgs e)
		{
			var searchStr = EquipmentTypeSearchVM.SearchValues?.FirstOrDefault();

			foreach(EquipmentType equipmentType in Enum.GetValues(typeof(EquipmentType)))
			{
				if(!string.IsNullOrWhiteSpace(searchStr) && EquipmentTypes.Any(x => x.EquipmentType == equipmentType) &&
				   !equipmentType.GetEnumTitle().ToLower().Contains(searchStr.ToLower()))
				{
					EquipmentTypes.Remove(EquipmentTypes.FirstOrDefault(x => x.EquipmentType == equipmentType));
				}

				if(string.IsNullOrWhiteSpace(searchStr) && !EquipmentTypes.Any(x => x.EquipmentType == equipmentType) &&
				   !SelectedEquipmentTypes.Any(x => x.EquipmentType == equipmentType))
				{
					EquipmentTypes.Add(new EquipmentTypeReportNode() { EquipmentType = equipmentType });
				}

				if(!string.IsNullOrWhiteSpace(searchStr) && !EquipmentTypes.Any(x => x.EquipmentType == equipmentType) &&
				   !SelectedEquipmentTypes.Any(x => x.EquipmentType == equipmentType) &&
				   equipmentType.GetEnumTitle().ToLower().Contains(searchStr.ToLower()))
				{
					EquipmentTypes.Add(new EquipmentTypeReportNode() { EquipmentType = equipmentType });
				}
			}
		}

		private Func<IUnitOfWork, IQueryOver<EquipmentKind>> EquipmentKindItemsSourceQueryFunction => (uow) =>
		{
			EquipmentKind equipmentKindAlias = null;
			EquipmentKindReportNode equipmentKindResultAlias = null;

			var itemsQuery = UoW.Session.QueryOver(() => equipmentKindAlias);

			itemsQuery.Where(GetEquipmentKindSearchCriterion(
					() => equipmentKindAlias.Id,
					() => equipmentKindAlias.Name
				)
			);

			if(SelectedEquipmentKinds?.Count > 0)
			{
				itemsQuery.WhereNot(e => e.Id.IsIn(SelectedEquipmentKinds.Select(se => se.Id).ToArray()));
			}

			itemsQuery
				.SelectList(list => list
					.Select(x => x.Id).WithAlias(() => equipmentKindResultAlias.Id)
					.Select(x => x.Name).WithAlias(() => equipmentKindResultAlias.Name)
				)
				.TransformUsing(Transformers.AliasToBean<EquipmentKindReportNode>());

			return itemsQuery;
		};

		public void ShowInfoWindow()
		{
			var info =
				"Кнопками со стрелками влево/вправо, либо двойным щелчком мыши выберите необходимые фильтры для отчёта.\n" +
				"Строками отчёта являются выбранные сотрудники. Колонками отчёта являются выбранные ТМЦ, виды и типы оборудования,\n" +
				"а также выручка.\n" +
				"Подсчёт происходит по заказам, кроме заказов со статусами \"Доставка отменена\", \"Отменён\", \"Недовоз\" \n" +
				"и кроме заказов-закрывашек по контракту.\n\n" +
				"Фильтр периода дат применяется для даты создания заказа. Если указан 1 день, то сравнивается с планом на день.\n" +
				"Если указан период, то сравнивается с планом на месяц.\n" +
				"Все данные по плану продаж берутся из плана, указанного в сотруднике. А для ТМЦ, если у сотрудника нет такого плана с выбранной\n" +
				"ТМЦ, то план берётся из Журнала плана продаж для КЦ (для настройки данного плана продаж нажмите на соответствующую кнопку сверху).\n" +
				"А если и в Журнале плана продаж для КЦ также не заданы плановые показатели за день или месяц, то рассчитывается \n" +
				"среднее значение в подразделении КЦ проданных сотрудниками выбранных ТМЦ.";

			_interactiveService.ShowMessage(ImportanceLevel.Info, info, "Информация");
		}

		public void ButtonNomenclaturePlanClicked()
		{
			TabParent.OpenTab(() => new NomenclaturesPlanJournalViewModel(
				new NomenclaturePlanFilterViewModel() { HidenByDefault = true },
				_unitOfWorkFactory,
				_commonServices)
			);
		}

		public void ButtonNomenclaturesSaveClicked()
		{
			foreach(var savedNomenclature in _savedNomenclatures.ToList())
			{
				if(!SelectedNomenclatures.Any(x => x.Id == savedNomenclature.Nomenclature.Id))
				{
					_savedNomenclatures.Remove(savedNomenclature);

					UoW.Delete(savedNomenclature);
				}
			}

			foreach(NomenclatureReportNode selectedNode in SelectedNomenclatures)
			{
				if(!_savedNomenclatures.Any(x => x.Nomenclature.Id == selectedNode.Id))
				{
					SelectedNomenclature selectedNomenclature = new SelectedNomenclature()
					{
						Nomenclature = new Nomenclature()
						{
							Id = selectedNode.Id
						}
					};

					UoW.Save(selectedNomenclature);

					_savedNomenclatures.Add(selectedNomenclature);
				}
			}

			UoW.Commit();
		}

		public void ButtonEquipmentKindsSaveClicked()
		{
			foreach(var savedEquipmentKind in _savedEquipmentKinds.ToList())
			{
				if(!SelectedEquipmentKinds.Any(x => x.Id == savedEquipmentKind.EquipmentKind.Id))
				{
					_savedEquipmentKinds.Remove(savedEquipmentKind);

					UoW.Delete(savedEquipmentKind);
				}
			}

			foreach(EquipmentKindReportNode selectedNode in SelectedEquipmentKinds)
			{
				if(!_savedEquipmentKinds.Any(x => x.EquipmentKind.Id == selectedNode.Id))
				{
					SelectedEquipmentKind selectedEquipmentKindPlan = new SelectedEquipmentKind()
					{
						EquipmentKind = new EquipmentKind()
						{
							Id = selectedNode.Id
						}
					};

					UoW.Save(selectedEquipmentKindPlan);

					_savedEquipmentKinds.Add(selectedEquipmentKindPlan);
				}
			}

			UoW.Commit();
		}

		public void ButtonEquipmentTypesSaveClicked()
		{
			foreach(var savedEquipmentType in _savedEquipmentTypes.ToList())
			{
				if(!SelectedEquipmentTypes.Any(x => x.EquipmentType == savedEquipmentType.EquipmentType))
				{
					_savedEquipmentTypes.Remove(savedEquipmentType);

					UoW.Delete(savedEquipmentType);
				}
			}

			foreach(EquipmentTypeReportNode selectedNode in SelectedEquipmentTypes)
			{
				if(!_savedEquipmentTypes.Any(x => x.EquipmentType == selectedNode.EquipmentType))
				{
					SelectedEquipmentType selectedEquipmentTypePlan = new SelectedEquipmentType()
					{
						EquipmentType = selectedNode.EquipmentType
					};

					UoW.Save(selectedEquipmentTypePlan);

					_savedEquipmentTypes.Add(selectedEquipmentTypePlan);
				}
			}

			UoW.Commit();
		}

		public void ButtonSaveProceedsClicked()
		{
			UoW.Save(SelectedProceeds);
			UoW.Commit();
		}

		public void SelectNomenclature(NomenclatureReportNode[] nodes)
		{
			if(nodes.Length == 0)
			{
				return;
			}

			foreach(var node in nodes)
			{
				SelectedNomenclatures.Add(node);
			}
		}

		public void DeselectNomenclature(NomenclatureReportNode[] nodes)
		{
			if(nodes.Length == 0)
			{
				return;
			}

			foreach(var node in nodes)
			{
				SelectedNomenclatures.Remove(node);
			}
		}

		public void SelectEmployee(EmployeeReportNode[] nodes)
		{
			if(nodes.Length == 0)
			{
				return;
			}

			foreach(var node in nodes)
			{
				SelectedEmployees.Add(node);
			}
		}

		public void DeselectEmployee(EmployeeReportNode[] nodes)
		{
			if(nodes.Length == 0)
			{
				return;
			}

			foreach(var node in nodes)
			{
				SelectedEmployees.Remove(node);
			}
		}

		public void SelectEquipmentKind(EquipmentKindReportNode[] nodes)
		{
			if(nodes.Length == 0)
			{
				return;
			}

			foreach(var node in nodes)
			{
				SelectedEquipmentKinds.Add(node);
			}
		}

		public void DeselectEquipmentKind(EquipmentKindReportNode[] nodes)
		{
			if(nodes.Length == 0)
			{
				return;
			}

			foreach(var node in nodes)
			{
				SelectedEquipmentKinds.Remove(node);
			}
		}

		public void SelectEquipmentType(EquipmentTypeReportNode[] nodes)
		{
			if(nodes.Length == 0)
			{
				return;
			}

			foreach(var node in nodes)
			{
				SelectedEquipmentTypes.Add(node);
				EquipmentTypes.Remove(node);
			}

			EquipmentTypeSearchVM.Update();
		}

		public void DeselectEquipmentType(EquipmentTypeReportNode[] nodes)
		{
			if(nodes.Length == 0)
			{
				return;
			}

			foreach(var node in nodes)
			{
				SelectedEquipmentTypes.Remove(node);
				EquipmentTypes.Add(node);
			}

			EquipmentTypeSearchVM.Update();
		}


		public override void Dispose()
		{
			IsDestroyed = true;
			NomenclatureDataLoader.CancelLoading();
			EmployeeDataLoader.CancelLoading();
			EquipmentKindDataLoader.CancelLoading();
			base.Dispose();
		}

		public class NomenclatureReportNode : ReportNode
		{
			public int? PlanDay { get; set; }
			public int? PlanMonth { get; set; }
			public override ReportColumnType ColumnType => ReportColumnType.Nomenclature;
		}

		public class EmployeeReportNode : ReportNode
		{
			public string LastName { get; set; }
			public string Patronymic { get; set; }
			public string FullName => $"{LastName} {Name} {Patronymic}";
		}

		public class SubdivisionReportNode : ReportNode { }

		public class EquipmentKindReportNode : ReportNode
		{
			public override ReportColumnType ColumnType => ReportColumnType.EquipmentKind;
		}

		public class EquipmentTypeReportNode : ReportNode
		{
			public EquipmentType EquipmentType { get; set; }
			public override string Name => EquipmentType.GetEnumTitle();
			public override ReportColumnType ColumnType => ReportColumnType.EquipmentType;
		}

		public class ProceedsReportNode : ReportNode
		{
			public override string Name => "Выручка";
			public override ReportColumnType ColumnType => ReportColumnType.Proceeds;
		}

		private List<ProductGroup> GetProductGroupsRecursive(ProductGroup parentProductGroup)
		{
			var productGroups = new List<ProductGroup> { parentProductGroup };

			foreach(var productGroup in parentProductGroup.Childs)
			{
				productGroups.AddRange(GetProductGroupsRecursive(productGroup));
			}

			return productGroups;
		}

		public void LoadNextNomenclatures(double scrollPosition)
		{
			if(NomenclatureDataLoader.HasUnloadedItems)
			{
				NomenclatureLastScrollPosition = scrollPosition;
				NomenclatureDataLoader.LoadData(true);
			}
		}

		public void LoadNextEmployees(double scrollPosition)
		{
			if(EmployeeDataLoader.HasUnloadedItems)
			{
				EmployeeLastScrollPosition = scrollPosition;
				EmployeeDataLoader.LoadData(true);
			}
		}

		public void LoadNextEquipmentKinds(double scrollPosition)
		{
			if(EquipmentKindDataLoader.HasUnloadedItems)
			{
				EquipmentKindLastScrollPosition = scrollPosition;
				EquipmentKindDataLoader.LoadData(true);
			}
		}

		public void ExportReport(string path)
		{
			var template = new XLTemplate(_templatePath);

			template.AddVariable(Report);
			template.Generate();

			template.SaveAs(path);
		}

		public void GenerateReport()
		{
			if(!StartDate.HasValue)
			{
				_interactiveService.ShowMessage(ImportanceLevel.Info, "Не выбрана дата");
			}

			Report = new NomenclaturePlanReport()
			{
				FilterStartDate = StartDate,
				FilterEndDate = EndDate,
				CreationDate = DateTime.Now
			};

			var rows = new List<NomenclaturePlanReportRow>();

			var titles = new List<string>();

			_selectedReportColumns = SelectedNomenclatures.Cast<ReportNode>().Concat(SelectedEquipmentKinds).Concat(SelectedEquipmentTypes).ToList();

			if(SelectedProceeds.InludeProceeds)
			{
				_selectedReportColumns.Add(new ProceedsReportNode());
			}

			foreach(ReportNode node in _selectedReportColumns)
			{
				titles.Add($"{node.Name}\n(факт)");
				titles.Add($"{node.Name}\n(план)");
				titles.Add($"{node.Name}\n(процент)");
			}

			Report.Titles = titles;
			Report.Rows = GenerateRows();

			UoW.Session.Clear();
		}

		private IEnumerable<NomenclaturePlanReportRow> GenerateRows()
		{
			NomenclaturePlans = null;
			CallCenterEmployeesCount = null;

			var employeesIds = SelectedEmployees.Select(x => x.Id).ToArray();

			var statusList = new List<OrderStatus>
			{
				OrderStatus.Canceled, OrderStatus.DeliveryCanceled, OrderStatus.NotDelivered
			};

			Order orderAlias = null;
			OrderItem orderItemAlias = null;
			StorageNode storageNode = null;
			Employee employeeAlias = null;
			Nomenclature nomenclatureAlias = null;
			EquipmentKind equipmentKindAlias = null;

			StorageList = UoW.Session.QueryOver<Order>(() => orderAlias)
				.Left.JoinAlias(o => o.OrderItems, () => orderItemAlias)
				.Left.JoinAlias(o => o.Author, () => employeeAlias)
				.Left.JoinAlias(() => orderItemAlias.Nomenclature, () => nomenclatureAlias)
				.Left.JoinAlias(() => nomenclatureAlias.Kind, () => equipmentKindAlias)
				.Where(o => (o.Author.Id.IsIn(employeesIds) || employeeAlias.Subdivision.Id == _nomenclaturePlanParametersProvider.CallCenterSubdivisionId) &&
							!o.OrderStatus.IsIn(statusList) &&
							!o.IsContractCloser &&
							o.CreateDate.Value.Date >= StartDate && o.CreateDate.Value.Date <= EndDate)
				.SelectList(list => list
					.Select(() => orderItemAlias.Price).WithAlias(() => storageNode.Price)
					.Select(() => orderItemAlias.Count).WithAlias(() => storageNode.Count)
					.Select(() => employeeAlias.Subdivision.Id).WithAlias(() => storageNode.SubdivisionId)
					.Select(() => orderItemAlias.Nomenclature.Id).WithAlias(() => storageNode.NomenclatureId)
					.Select(() => orderAlias.Author.Id).WithAlias(() => storageNode.AuthorId)
					.Select(() => nomenclatureAlias.Kind.Id).WithAlias(() => storageNode.EquipmentKindId)
					.Select(() => equipmentKindAlias.EquipmentType).WithAlias(() => storageNode.EquipmentType)
				).TransformUsing(Transformers.AliasToBean<StorageNode>())
				.List<StorageNode>();

			EmployeeWageParameterNode employeeWageParameterNode = null;
			EmployeeWageParameter employeeWageParameterAlias = null;
			SalesPlanWageParameterItem salesPlanWageParameterItemAlias = null;
			EmployeeWageParameter subQueryEmployeeWageParameter = null;
			SalesPlanWageParameterItem subQuerySalesPlanWageParameterItem = null;

			var salesPlanSubQuery = QueryOver.Of<EmployeeWageParameter>(() => subQueryEmployeeWageParameter)
				.Inner.JoinAlias(() => subQueryEmployeeWageParameter.WageParameterItem, () => subQuerySalesPlanWageParameterItem)
				.Where(() => subQuerySalesPlanWageParameterItem.WageParameterItemType == WageParameterItemTypes.SalesPlan)
				.Where(() => subQueryEmployeeWageParameter.Employee.Id == employeeWageParameterAlias.Employee.Id)
				.SelectList(list => list
					.Select(() => subQuerySalesPlanWageParameterItem.SalesPlan.Id))
				.OrderBy(x => x.EndDate.Coalesce(DateTime.MaxValue)).Desc
				.ThenBy(x => x.StartDate).Desc
				.Take(1);

			var employeeWageParameterNodeList = UoW.Session.QueryOver<EmployeeWageParameter>(() => employeeWageParameterAlias)
				.Inner.JoinAlias(() => employeeWageParameterAlias.WageParameterItem, () => salesPlanWageParameterItemAlias)
				.Where(() => employeeWageParameterAlias.Employee.Id.IsIn(SelectedEmployees.Select(e => e.Id).ToArray()))
				.And(() => salesPlanWageParameterItemAlias.WageParameterItemType == WageParameterItemTypes.SalesPlan)
				.SelectList(list => list
					.SelectGroup(x => x.Employee.Id).WithAlias(() => employeeWageParameterNode.EmployeeId)
					.SelectSubQuery(salesPlanSubQuery).WithAlias(() => employeeWageParameterNode.SalesPlanId)
				).TransformUsing(Transformers.AliasToBean<EmployeeWageParameterNode>())
				.List<EmployeeWageParameterNode>();

			var employees = UoW.Session.Query<Employee>()
				.Where(e => employeesIds.Contains(e.Id))
				.ToList()
				.OrderBy(x => x.FullName);

			var salesPlans = UoW.Session.QueryOver<SalesPlan>()
				.Where(x => x.Id.IsIn(employeeWageParameterNodeList.Select(e => e.SalesPlanId).ToArray()))
				.List();

			var rows = new List<NomenclaturePlanReportRow>();

			foreach(var employee in employees)
			{
				NomenclaturePlanReportRow row = new NomenclaturePlanReportRow { Employee = employee, Items = new List<decimal>() };

				var employeeSalesPlan = employeeWageParameterNodeList.SingleOrDefault(x => x.EmployeeId == employee.Id);
				var salesPlan = salesPlans.SingleOrDefault(x => employeeSalesPlan != null && x.Id == employeeSalesPlan.SalesPlanId);

				foreach(var column in _selectedReportColumns)
				{
					decimal plan = GetSalesPlan(column, salesPlan);
					decimal fact = GetSalesFact(column, employee);
					decimal percent = plan > 0 ? fact * 100 / plan : 100;
					row.Items.AddRange(new List<decimal> { decimal.Round(fact, 2), decimal.Round(plan, 2), decimal.Round(percent, 2) });
				}

				rows.Add(row);
			}
			return rows;
		}

		private decimal GetSalesFact(ReportNode node, Employee employee)
		{
			switch(node.ColumnType)
			{
				case ReportColumnType.Nomenclature:
					{
						return GetNomenclatureSalesFact(node, employee);
					}
				case ReportColumnType.EquipmentKind:
					{
						return GetEquipmentKindSalesFact(node, employee);
					}
				case ReportColumnType.EquipmentType:
					{
						return GetEquipmentTypeSalesFact(node, employee);
					}
				case ReportColumnType.Proceeds:
					{
						return GetProceedsSalesFact(employee);
					}
			}

			return 0;
		}

		private decimal GetProceedsSalesFact(Employee employee)
		{
			return StorageList
				.Where(sl => sl.AuthorId == employee.Id)
				.Sum(i => i.Price * i.Count);
		}

		private decimal GetEquipmentTypeSalesFact(ReportNode node, Employee employee)
		{
			var equipmentType = ((EquipmentTypeReportNode)node).EquipmentType;

			return StorageList
				.Where(sl => sl.EquipmentType == equipmentType &&
							sl.AuthorId == employee.Id)
				.Sum(i => i.Count);
		}

		private decimal GetEquipmentKindSalesFact(ReportNode node, Employee employee)
		{
			return StorageList
				.Where(sl => sl.EquipmentKindId == node.Id &&
							sl.AuthorId == employee.Id)
				.Sum(i => i.Count);
		}

		private decimal GetNomenclatureSalesFact(ReportNode node, Employee employee)
		{
			return StorageList
				.Where(sl => sl.NomenclatureId == node.Id &&
							sl.AuthorId == employee.Id)
				.Sum(i => i.Count);
		}

		private decimal GetSalesPlan(ReportNode node, SalesPlan salesPlan)
		{
			switch(node.ColumnType)
			{
				case ReportColumnType.Nomenclature:
					{
						var salesPlanItem = salesPlan?.NomenclatureItemSalesPlans
							.SingleOrDefault(x => x.Nomenclature.Id == node.Id);

						return GetNomenclatureSalesPlan(node, salesPlanItem);
					}
				case ReportColumnType.EquipmentKind:
					{
						var salesPlanItem = salesPlan?.EquipmentKindItemSalesPlans
							.SingleOrDefault(x => x.EquipmentKind.Id == node.Id);

						return IsOneDay ? salesPlanItem?.PlanDay ?? 0 : salesPlanItem?.PlanMonth ?? 0;
					}
				case ReportColumnType.EquipmentType:
					{
						var salesPlanItem = salesPlan?.EquipmentTypeItemSalesPlans
								.SingleOrDefault(x => x.EquipmentType == ((EquipmentTypeReportNode)node).EquipmentType);

						return IsOneDay ? salesPlanItem?.PlanDay ?? 0 : salesPlanItem?.PlanMonth ?? 0;
					}
				case ReportColumnType.Proceeds:
					{
						if(salesPlan != null)
						{
							return IsOneDay ? salesPlan.ProceedsDay : salesPlan.ProceedsMonth;
						}

						break;
					}
			}

			return 0;
		}

		private decimal GetNomenclatureSalesPlan(ReportNode node, SalesPlanItem salesPlanItem)
		{
			var nomenclature = NomenclaturePlans.SingleOrDefault(x => x.Id == node.Id);

			if(salesPlanItem != null)
			{
				return IsOneDay ? salesPlanItem?.PlanDay ?? 0 : salesPlanItem?.PlanMonth ?? 0;
			}
			else
			{
				if((IsOneDay && nomenclature?.PlanDay > 0) || (!IsOneDay && nomenclature?.PlanMonth > 0))
				{
					return IsOneDay ? nomenclature.PlanDay ?? 0 : nomenclature.PlanMonth ?? 0;
				}
				else
				{
					var soldBySubdivision = StorageList
						.Where(o => o.NomenclatureId == nomenclature.Id &&
									o.SubdivisionId == CallCenterSubdivisionId)
						.Sum(i => i.Count);

					return (decimal)(soldBySubdivision / CallCenterEmployeesCount);
				}
			}
		}

		public class EmployeeWageParameterNode
		{
			public int EmployeeId { get; set; }
			public int SalesPlanId { get; set; }
		}

		public class ReportNode
		{
			public int Id { get; set; }
			public virtual string Name { get; set; }
			public virtual ReportColumnType ColumnType { get; set; }
		}

		public enum ReportColumnType
		{
			Nomenclature,
			EquipmentKind,
			EquipmentType,
			Proceeds
		}

		public class NomenclaturePlanReport
		{
			public IEnumerable<NomenclaturePlanReportRow> Rows { get; set; }
			public List<string> Titles { get; set; }
			public DateTime? FilterStartDate, FilterEndDate;
			public DateTime CreationDate { get; set; }
			public string ReportDates => FilterStartDate == FilterEndDate ? $"{FilterStartDate.Value.ToShortDateString()}"
				: $"период {FilterStartDate.Value.ToShortDateString()} - {FilterEndDate.Value.ToShortDateString()}";
		}

		public class NomenclaturePlanReportRow
		{
			public string Name => Employee.FullName;
			public Employee Employee { get; set; }
			public List<decimal> Items { get; set; }
		}

		public int? CallCenterEmployeesCount
		{
			get => _callCenterEmployeesCount ?? (_callCenterEmployeesCount = UoW.Session.QueryOver<Employee>()
				.Where(e => e.Subdivision.Id == _nomenclaturePlanParametersProvider.CallCenterSubdivisionId)
				.And(e => e.Status == Domain.Employees.EmployeeStatus.IsWorking)
				.Select(Projections.Count<Employee>(e => e.Id))
				.SingleOrDefault<int>());
			private set {; }
		}

		public IList<Nomenclature> NomenclaturePlans
		{
			get => _nomenclaturePlans ?? (_nomenclaturePlans = UoW.Session.QueryOver<Nomenclature>()
				.Where(x => x.Id.IsIn(SelectedNomenclatures.Select(n => n.Id).ToArray()))
				.List());
			private set {; }
		}

		public class StorageNode
		{
			public decimal Price { get; set; }
			public decimal Count { get; set; }
			public int NomenclatureId { get; set; }
			public int SubdivisionId { get; set; }
			public int AuthorId { get; set; }
			public int EquipmentKindId { get; set; }
			public EquipmentType? EquipmentType { get; set; }
		}

		public SearchViewModel NomenclatureSearchVM { get; private set; }
		public SearchViewModel EmployeeSearchVM { get; private set; }
		public SearchViewModel EquipmentKindSearchVM { get; private set; }
		public SearchViewModel EquipmentTypeSearchVM { get; private set; }
		public GenericObservableList<NomenclatureReportNode> SelectedNomenclatures { get; private set; }
		public GenericObservableList<EmployeeReportNode> SelectedEmployees { get; private set; }
		public GenericObservableList<EquipmentKindReportNode> SelectedEquipmentKinds { get; private set; }
		public GenericObservableList<EquipmentTypeReportNode> SelectedEquipmentTypes { get; private set; }
		public GenericObservableList<EquipmentTypeReportNode> EquipmentTypes { get; private set; }
		public IList<StorageNode> StorageList { get; set; }
		public SelectedProceeds SelectedProceeds { get; set; }
		public bool CanSaveCallCenterMotivationReportFilter { get; private set; }
		public bool IsNomenclatureNextPage { get; set; }
		public bool IsEmployeeNextPage { get; set; }
		public bool IsEquipmentKindNextPage { get; set; }
		public ThreadDataLoader<NomenclatureReportNode> NomenclatureDataLoader { get; private set; }
		public ThreadDataLoader<EmployeeReportNode> EmployeeDataLoader { get; private set; }
		public ThreadDataLoader<EquipmentKindReportNode> EquipmentKindDataLoader { get; private set; }
		public bool IsDestroyed { get; private set; }
		public double NomenclatureLastScrollPosition { get; set; }
		public double EmployeeLastScrollPosition { get; set; }
		public double EquipmentKindLastScrollPosition { get; set; }
		public IEntityAutocompleteSelectorFactory ProductGroupSelectorFactory { get; }
		public ProductGroup ProductGroup { get; set; }
		public NomenclatureCategory? NomenclatureCategory { get; set; }
		public EmployeeStatus? EmployeeStatus { get; set; } = Domain.Employees.EmployeeStatus.IsWorking;
		public IEnumerable<SubdivisionReportNode> Subdivisions { get; private set; }
		public SubdivisionReportNode Subdivision { get; set; }
		public int PageSize => 100;
		public NomenclaturePlanReport Report { get; private set; }
		public DateTime? EndDate { get; set; }
		public DateTime? StartDate { get; set; }
	}
}