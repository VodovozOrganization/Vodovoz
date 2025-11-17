using ClosedXML.Report;
using Gamma.Utilities;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Transform;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal.DataLoader;
using QS.Project.Journal.Search;
using QS.Project.Search;
using QS.Project.Services.FileDialog;
using QS.Services;
using QS.ViewModels;
using QS.ViewModels.Control.EEVM;
using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Vodovoz.Core.Domain.BasicHandbooks;
using Vodovoz.Core.Domain.Employees;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Domain;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.WageCalculation;
using Vodovoz.EntityRepositories.Goods;
using Vodovoz.Settings.Nomenclature;
using Vodovoz.ViewModels.Goods.ProductGroups;
using Vodovoz.ViewModels.Journals.FilterViewModels.Orders;
using Vodovoz.ViewModels.Journals.JournalViewModels.Orders;
using Vodovoz.ViewModels.ViewModels.Goods;
using Vodovoz.ViewModels.ViewModels.Reports.NomenclaturePlanReport;
using Order = Vodovoz.Domain.Orders.Order;

namespace Vodovoz.ViewModels.ViewModels.Reports
{
	public class NomenclaturePlanReportViewModel : DialogTabViewModelBase
	{
		private readonly ICommonServices _commonServices;
		private readonly ViewModelEEVMBuilder<ProductGroup> _productGroupEEVMBuilder;
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly INomenclaturePlanSettings _nomenclaturePlanSettings;
		private readonly INomenclatureRepository _nomenclatureRepository;
		private readonly IInteractiveService _interactiveService;
		private readonly IFileDialogService _fileDialogService;

		private List<SelectedNomenclature> _savedNomenclatures;
		private List<SelectedEquipmentKind> _savedEquipmentKinds;
		private List<SelectedEquipmentType> _savedEquipmentTypes;

		private bool IsOneDay => StartDate == EndDate;
		private IList<NomenclaturePlanReportColumn> _selectedReportColumns;
		private int? _callCenterEmployeesCount;
		private IList<Nomenclature> _nomenclaturePlans;
		private DelegateCommand _nomenclaturePlanCommand;
		private DelegateCommand _showInfoWindowCommand;
		private DelegateCommand _saveProceedsCommand;
		private DelegateCommand _saveReportCommand;
		private DelegateCommand<ScrollPositionNode> _loadNextCommand;
		private const string _templatePath = @".\Reports\Orders\NomenclaturePlanReport.xlsx";

		private ICriterion GetNomenclatureSearchCriterion(params Expression<Func<object>>[] aliasPropertiesExpr)
		{
			var searchCriterion = new SearchCriterion(NomenclatureSearchVM);
			var result = searchCriterion.By(aliasPropertiesExpr).Finish();
			return result;
		}

		private ICriterion GetEmployeeSearchCriterion(params Expression<Func<object>>[] aliasPropertiesExpr)
		{
			var searchCriterion = new SearchCriterion(EmployeeSearchVM);
			var result = searchCriterion.By(aliasPropertiesExpr).Finish();
			return result;
		}

		private ICriterion GetEquipmentKindSearchCriterion(params Expression<Func<object>>[] aliasPropertiesExpr)
		{
			var searchCriterion = new SearchCriterion(EquipmentKindSearchVM);
			var result = searchCriterion.By(aliasPropertiesExpr).Finish();
			return result;
		}

		public NomenclaturePlanReportViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			IInteractiveService interactiveService,
			INavigationManager navigation,
			ICommonServices commonServices,
			ViewModelEEVMBuilder<ProductGroup> productGroupEEVMBuilder,
			INomenclaturePlanSettings nomenclaturePlanSettings,
			INomenclatureRepository nomenclatureRepository,
			IFileDialogService fileDialogService) : base(unitOfWorkFactory, interactiveService,
			navigation)
		{
			Title = "Отчёт по мотивации КЦ";
			_commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
			_productGroupEEVMBuilder = productGroupEEVMBuilder ?? throw new ArgumentNullException(nameof(productGroupEEVMBuilder));
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_nomenclaturePlanSettings = nomenclaturePlanSettings ??
												  throw new ArgumentNullException(nameof(nomenclaturePlanSettings));
			_nomenclatureRepository = nomenclatureRepository ?? throw new ArgumentNullException(nameof(nomenclatureRepository));
			_interactiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));
			_fileDialogService = fileDialogService ?? throw new ArgumentNullException(nameof(fileDialogService));

			ProductGroupEntityEntryViewModel = CreateProductGroupEEVM();
			ProductGroupEntityEntryViewModel.ChangedByUser += OnProductGroupChangedByUser;

			CallCenterSubdivisionId = _nomenclaturePlanSettings.CallCenterSubdivisionId;

			Configure();
		}

		#region Configure

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
			NomenclatureSearchVM.OnSearch += (sender, args) => NomenclatureSearchCommand.Execute();

			//Предзагрузка. Для избежания ленивой загрузки
			UoW.Session.QueryOver<ProductGroup>().Fetch(SelectMode.ChildFetch, x => x.Childs).List();

			_savedNomenclatures = UoW.Session.QueryOver<SelectedNomenclature>()
				.List<SelectedNomenclature>()
				.OrderBy(x => x.Nomenclature.Name)
				.ToList();

			SelectedNomenclatures = new GenericObservableList<NomenclatureReportColumn>(_savedNomenclatures
				.Select(x => new NomenclatureReportColumn()
				{
					Id = x.Nomenclature.Id,
					Name = x.Nomenclature.Name,
					PlanDay = x.Nomenclature.PlanDay,
					PlanMonth = x.Nomenclature.PlanMonth
				})
				.ToList());

			NomenclatureDataLoader = new ThreadDataLoader<NomenclatureReportColumn>(_unitOfWorkFactory) { PageSize = PageSize };
			NomenclatureDataLoader.AddQuery(NomenclatureItemsSourceQueryFunction);
		}

		private void EmployeesConfigure()
		{
			EmployeeSearchVM = new SearchViewModel();
			EmployeeSearchVM.OnSearch += (sender, args) => EmployeeSearchCommand.Execute();

			SubdivisionReportColumn subdivisionResultAlias = null;
			Subdivisions = UoW.Session.QueryOver<Subdivision>()
				.SelectList(list => list
					.Select(x => x.Id).WithAlias(() => subdivisionResultAlias.Id)
					.Select(x => x.Name).WithAlias(() => subdivisionResultAlias.Name)
				)
				.TransformUsing(Transformers.AliasToBean<SubdivisionReportColumn>())
				.List<SubdivisionReportColumn>()
				.OrderBy(x => x.Name);

			Subdivision = Subdivisions.FirstOrDefault(s => s.Id == _nomenclaturePlanSettings.CallCenterSubdivisionId);

			SelectedEmployees = new GenericObservableList<EmployeeReportColumn>();

			EmployeeDataLoader = new ThreadDataLoader<EmployeeReportColumn>(_unitOfWorkFactory) { PageSize = PageSize };
			EmployeeDataLoader.AddQuery(EmployeeItemsSourceQueryFunction);
		}

		private void EquipmentKindsConfigure()
		{
			EquipmentKindSearchVM = new SearchViewModel();
			EquipmentKindSearchVM.OnSearch += (sender, args) => EquipmentKindSearchCommand.Execute();

			_savedEquipmentKinds = UoW.Session.QueryOver<SelectedEquipmentKind>()
				.List()
				.OrderBy(x => x.EquipmentKind.Name)
				.ToList();

			SelectedEquipmentKinds = new GenericObservableList<EquipmentKindReportColumn>(_savedEquipmentKinds
				.Select(x => new EquipmentKindReportColumn()
				{
					Id = x.EquipmentKind.Id,
					Name = x.EquipmentKind.Name,
				})
				.ToList());

			EquipmentKindDataLoader = new ThreadDataLoader<EquipmentKindReportColumn>(_unitOfWorkFactory) { PageSize = PageSize };
			EquipmentKindDataLoader.AddQuery(EquipmentKindItemsSourceQueryFunction);
		}

		private void EquipmentTypesConfigure()
		{
			EquipmentTypeSearchVM = new SearchViewModel();
			EquipmentTypeSearchVM.OnSearch += (sender, args) => EquipmentTypeSearchCommand.Execute();

			_savedEquipmentTypes = UoW.Session.QueryOver<SelectedEquipmentType>()
				.List<SelectedEquipmentType>()
				.ToList();

			EquipmentTypes = new GenericObservableList<EquipmentTypeReportColumn>();
			foreach(EquipmentType equipmentType in Enum.GetValues(typeof(EquipmentType)))
			{
				if(!_savedEquipmentTypes.Any(x => x.EquipmentType == equipmentType))
				{
					EquipmentTypes.Add(new EquipmentTypeReportColumn() { EquipmentType = equipmentType });
				}
			}

			SelectedEquipmentTypes = new GenericObservableList<EquipmentTypeReportColumn>(_savedEquipmentTypes
				.Select(x => new EquipmentTypeReportColumn()
				{
					EquipmentType = x.EquipmentType
				})
				.ToList());
		}

		private IEntityEntryViewModel CreateProductGroupEEVM()
		{
			var viewModel =
				_productGroupEEVMBuilder
				.SetViewModel(this)
				.SetUnitOfWork(UoW)
				.ForProperty(this, x => x.ProductGroup)
				.UseViewModelJournalAndAutocompleter<ProductGroupsJournalViewModel, ProductGroupsJournalFilterViewModel>(
					filter =>
					{
						filter.IsGroupSelectionMode = true;
					})
				.UseViewModelDialog<ProductGroupViewModel>()
				.Finish();

			viewModel.CanViewEntity =
				_commonServices.CurrentPermissionService.ValidateEntityPermission(typeof(ProductGroup)).CanUpdate;

			return viewModel;
		}

		private void OnProductGroupChangedByUser(object sender, EventArgs e)
		{
			NomenclatureSearchCommand.Execute();
		}

		#endregion

		#region ItemsSourceQuery

		private Func<IUnitOfWork, IQueryOver<Nomenclature>> NomenclatureItemsSourceQueryFunction => (uow) =>
		{
			Nomenclature nomenclatureAlias = null;
			NomenclatureReportColumn nomenclatureResultAlias = null;

			var itemsQuery = UoW.Session.QueryOver(() => nomenclatureAlias)
				.Where(n => !n.IsArchive);

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
				.TransformUsing(Transformers.AliasToBean<NomenclatureReportColumn>());

			return itemsQuery;
		};

		private Func<IUnitOfWork, IQueryOver<Employee>> EmployeeItemsSourceQueryFunction => (uow) =>
		{
			Employee employeeAlias = null;
			EmployeeReportColumn employeeResultAlias = null;

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
				.TransformUsing(Transformers.AliasToBean<EmployeeReportColumn>());

			return itemsQuery;
		};

		private Func<IUnitOfWork, IQueryOver<EquipmentKind>> EquipmentKindItemsSourceQueryFunction => (uow) =>
		{
			EquipmentKind equipmentKindAlias = null;
			EquipmentKindReportColumn equipmentKindResultAlias = null;

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
				.TransformUsing(Transformers.AliasToBean<EquipmentKindReportColumn>());

			return itemsQuery;
		};

		#endregion

		private class ProceedsReportColumn : NomenclaturePlanReportColumn
		{
			public override string Name => "Выручка";
			public override NomenclaturePlanReportColumnType ColumnType => NomenclaturePlanReportColumnType.Proceeds;
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

		#region GenerateReport

		public void GenerateReport()
		{
			if(!StartDate.HasValue)
			{
				_interactiveService.ShowMessage(ImportanceLevel.Info, "Не выбрана дата");
			}

			Report = new NomenclaturePlanReportMain()
			{
				FilterStartDate = StartDate,
				FilterEndDate = EndDate,
				CreationDate = DateTime.Now
			};

			var rows = new List<NomenclaturePlanReportRow>();

			var titles = new List<string>();

			_selectedReportColumns = SelectedNomenclatures.Cast<NomenclaturePlanReportColumn>().Concat(SelectedEquipmentKinds).Concat(SelectedEquipmentTypes).ToList();

			if(SelectedProceeds.InludeProceeds)
			{
				_selectedReportColumns.Add(new ProceedsReportColumn());
			}

			foreach(NomenclaturePlanReportColumn node in _selectedReportColumns)
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

			_storageList = UoW.Session.QueryOver<Order>(() => orderAlias)
				.Left.JoinAlias(o => o.OrderItems, () => orderItemAlias)
				.Left.JoinAlias(o => o.Author, () => employeeAlias)
				.Left.JoinAlias(() => orderItemAlias.Nomenclature, () => nomenclatureAlias)
				.Left.JoinAlias(() => nomenclatureAlias.Kind, () => equipmentKindAlias)
				.Where(o => (o.Author.Id.IsIn(employeesIds) || employeeAlias.Subdivision.Id == _nomenclaturePlanSettings.CallCenterSubdivisionId) &&
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

			EmployeeSalesPlanNode employeeWageParameterNode = null;
			EmployeeWageParameter employeeWageParameterAlias = null;
			SalesPlanWageParameterItem salesPlanWageParameterItemAlias = null;
			EmployeeWageParameter subQueryEmployeeWageParameter = null;
			SalesPlanWageParameterItem subQuerySalesPlanWageParameterItem = null;

			var salesPlanSubQuery = QueryOver.Of<EmployeeWageParameter>(() => subQueryEmployeeWageParameter)
				.Inner.JoinAlias(() => subQueryEmployeeWageParameter.WageParameterItem, () => subQuerySalesPlanWageParameterItem)
				.Where(() => subQuerySalesPlanWageParameterItem.WageParameterItemType == WageParameterItemTypes.SalesPlan)
				.Where(() => subQueryEmployeeWageParameter.Employee.Id == employeeWageParameterAlias.Employee.Id)
				.Where(() => subQueryEmployeeWageParameter.StartDate <= EndDate &&
							 (subQueryEmployeeWageParameter.EndDate == null || subQueryEmployeeWageParameter.EndDate >= StartDate))
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
				).TransformUsing(Transformers.AliasToBean<EmployeeSalesPlanNode>())
				.List<EmployeeSalesPlanNode>();

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
				NomenclaturePlanReportRow row = new NomenclaturePlanReportRow { Employee = employee, Columns = new List<decimal>() };

				var employeeSalesPlan = employeeWageParameterNodeList.SingleOrDefault(x => x.EmployeeId == employee.Id);
				var salesPlan = salesPlans.SingleOrDefault(x => employeeSalesPlan != null && x.Id == employeeSalesPlan.SalesPlanId);

				foreach(var column in _selectedReportColumns)
				{
					decimal plan = GetSalesPlan(column, salesPlan);
					decimal fact = GetSalesFact(column, employee);
					decimal percent = plan > 0 ? fact * 100 / decimal.Round(plan, 2) : 100;
					row.Columns.AddRange(new List<decimal> { (int)fact, decimal.Round(plan, 2), decimal.Round(percent, 2) });
				}

				rows.Add(row);
			}
			return rows;
		}

		private decimal GetSalesFact(NomenclaturePlanReportColumn column, Employee employee)
		{
			switch(column.ColumnType)
			{
				case NomenclaturePlanReportColumnType.Nomenclature:
					{
						return GetNomenclatureSalesFact(column, employee);
					}
				case NomenclaturePlanReportColumnType.EquipmentKind:
					{
						return GetEquipmentKindSalesFact(column, employee);
					}
				case NomenclaturePlanReportColumnType.EquipmentType:
					{
						return GetEquipmentTypeSalesFact(column, employee);
					}
				case NomenclaturePlanReportColumnType.Proceeds:
					{
						return GetProceedsSalesFact(employee);
					}
			}

			return 0;
		}

		private decimal GetProceedsSalesFact(Employee employee)
		{
			return _storageList
				.Where(sl => sl.AuthorId == employee.Id)
				.Sum(i => i.Price * i.Count);
		}

		private decimal GetEquipmentTypeSalesFact(NomenclaturePlanReportColumn column, Employee employee)
		{
			var equipmentType = ((EquipmentTypeReportColumn)column).EquipmentType;

			return _storageList
				.Where(sl => sl.EquipmentType == equipmentType &&
							sl.AuthorId == employee.Id)
				.Sum(i => i.Count);
		}

		private decimal GetEquipmentKindSalesFact(NomenclaturePlanReportColumn column, Employee employee)
		{
			return _storageList
				.Where(sl => sl.EquipmentKindId == column.Id &&
							sl.AuthorId == employee.Id)
				.Sum(i => i.Count);
		}

		private decimal GetNomenclatureSalesFact(NomenclaturePlanReportColumn column, Employee employee)
		{
			return _storageList
				.Where(sl => sl.NomenclatureId == column.Id &&
							sl.AuthorId == employee.Id)
				.Sum(i => i.Count);
		}

		private decimal GetSalesPlan(NomenclaturePlanReportColumn column, SalesPlan salesPlan)
		{
			switch(column.ColumnType)
			{
				case NomenclaturePlanReportColumnType.Nomenclature:
					{
						var salesPlanItem = salesPlan?.NomenclatureItemSalesPlans
							.SingleOrDefault(x => x.Nomenclature.Id == column.Id);

						return GetNomenclatureSalesPlan(column, salesPlanItem);
					}
				case NomenclaturePlanReportColumnType.EquipmentKind:
					{
						var salesPlanItem = salesPlan?.EquipmentKindItemSalesPlans
							.SingleOrDefault(x => x.EquipmentKind.Id == column.Id);

						return IsOneDay ? salesPlanItem?.PlanDay ?? 0 : salesPlanItem?.PlanMonth ?? 0;
					}
				case NomenclaturePlanReportColumnType.EquipmentType:
					{
						var salesPlanItem = salesPlan?.EquipmentTypeItemSalesPlans
								.SingleOrDefault(x => x.EquipmentType == ((EquipmentTypeReportColumn)column).EquipmentType);

						return IsOneDay ? salesPlanItem?.PlanDay ?? 0 : salesPlanItem?.PlanMonth ?? 0;
					}
				case NomenclaturePlanReportColumnType.Proceeds:
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

		private decimal GetNomenclatureSalesPlan(NomenclaturePlanReportColumn column, SalesPlanItem salesPlanItem)
		{
			var nomenclature = NomenclaturePlans.SingleOrDefault(x => x.Id == column.Id);

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
					var soldBySubdivision = _storageList
						.Where(o => o.NomenclatureId == nomenclature.Id &&
									o.SubdivisionId == CallCenterSubdivisionId)
						.Sum(i => i.Count);

					return (decimal)(soldBySubdivision / CallCenterEmployeesCount);
				}
			}
		}

		private void ExportReport(string path)
		{
			var template = new XLTemplate(_templatePath);

			template.AddVariable(Report);
			template.Generate();

			template.SaveAs(path);
		}

		#endregion

		private class StorageNode
		{
			public decimal Price { get; set; }
			public decimal Count { get; set; }
			public int NomenclatureId { get; set; }
			public int SubdivisionId { get; set; }
			public int AuthorId { get; set; }
			public int EquipmentKindId { get; set; }
			public EquipmentType? EquipmentType { get; set; }
		}

		private int? CallCenterEmployeesCount
		{
			get => _callCenterEmployeesCount ?? (_callCenterEmployeesCount = UoW.Session.QueryOver<Employee>()
				.Where(e => e.Subdivision.Id == _nomenclaturePlanSettings.CallCenterSubdivisionId)
				.And(e => e.Status == Vodovoz.Core.Domain.Employees.EmployeeStatus.IsWorking)
				.Select(Projections.Count<Employee>(e => e.Id))
				.SingleOrDefault<int>());
			set {; }
		}

		private IList<Nomenclature> NomenclaturePlans
		{
			get => _nomenclaturePlans ?? (_nomenclaturePlans = UoW.Session.QueryOver<Nomenclature>()
				.Where(x => x.Id.IsIn(SelectedNomenclatures.Select(n => n.Id).ToArray()))
				.List());
			set => _nomenclaturePlans = value;
		}

		#region Commands

		public DelegateCommand NomenclaturePlanCommand =>
			_nomenclaturePlanCommand ?? (_nomenclaturePlanCommand = new DelegateCommand(() =>
			{
				TabParent.OpenTab(() => new NomenclaturesPlanJournalViewModel(
					new NomenclaturePlanFilterViewModel() { HidenByDefault = true },
					_unitOfWorkFactory,
					_commonServices,
					_nomenclatureRepository)
				);
			},
				() => true
			));

		public DelegateCommand ShowInfoWindowCommand =>
			_showInfoWindowCommand ?? (_showInfoWindowCommand = new DelegateCommand(() =>
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
			},
				() => true
			));

		public DelegateCommand SaveReportCommand =>
			_saveReportCommand ?? (_saveReportCommand = new DelegateCommand(() =>
			{
				var dialogSettings = new DialogSettings();
				dialogSettings.Title = "Сохранить";
				dialogSettings.DefaultFileExtention = ".xlsx";
				dialogSettings.FileName = $"{TabName} {Report.CreationDate:yyyy-MM-dd-HH-mm}.xlsx";

				var result = _fileDialogService.RunSaveFileDialog(dialogSettings);
				if(Report != null && result.Successful)
				{
					ExportReport(result.Path);
				}
			},
				() => true
			));

		public DelegateCommand<ScrollPositionNode> LoadNextCommand =>
			_loadNextCommand ?? (_loadNextCommand = new DelegateCommand<ScrollPositionNode>((loadNode) =>
			{
				switch(loadNode.ReportNodeType)
				{
					case NomenclaturePlanReportColumnType.Nomenclature:
						{
							if(NomenclatureDataLoader.HasUnloadedItems)
							{
								NomenclatureLastScrollPosition = loadNode.ScrollPosition;
								NomenclatureDataLoader.LoadData(true);
							}

							break;
						}
					case NomenclaturePlanReportColumnType.Employee:
						{
							if(EmployeeDataLoader.HasUnloadedItems)
							{
								EmployeeLastScrollPosition = loadNode.ScrollPosition;
								EmployeeDataLoader.LoadData(true);
							}

							break;
						}
					case NomenclaturePlanReportColumnType.EquipmentKind:
						{
							if(EquipmentKindDataLoader.HasUnloadedItems)
							{
								EquipmentKindLastScrollPosition = loadNode.ScrollPosition;
								EquipmentKindDataLoader.LoadData(true);
							}

							break;
						}
				}
			},
				(loadNode) => true
			));

		public DelegateCommand<NomenclaturePlanFilterRowSelectNode> SelectNodeCommand =>
			_selectNodeCommand ?? (_selectNodeCommand = new DelegateCommand<NomenclaturePlanFilterRowSelectNode>((nodes) =>
			{
				foreach(var node in nodes.NomenclaturePlanReportColumns)
				{
					switch(node.ColumnType)
					{
						case NomenclaturePlanReportColumnType.Nomenclature:
							{
								if(nodes.FilterRowSelectType == FilterRowSelectType.Select)
								{
									SelectedNomenclatures.Add((NomenclatureReportColumn)node);
								}
								else
								{
									SelectedNomenclatures.Remove((NomenclatureReportColumn)node);
								}

								NomenclatureDataLoader.PageSize = NomenclatureDataLoader.Items.Count + nodes.NomenclaturePlanReportColumns.Length;
								NomenclatureLastScrollPosition = nodes.ScrollPosition;
								NomenclatureDataLoader.LoadData(IsNomenclatureNextPage = false);

								break;
							}
						case NomenclaturePlanReportColumnType.Employee:
							{
								if(nodes.FilterRowSelectType == FilterRowSelectType.Select)
								{
									SelectedEmployees.Add((EmployeeReportColumn)node);
								}
								else
								{
									SelectedEmployees.Remove((EmployeeReportColumn)node);
								}

								EmployeeDataLoader.PageSize = EmployeeDataLoader.Items.Count + nodes.NomenclaturePlanReportColumns.Length;
								EmployeeLastScrollPosition = nodes.ScrollPosition;
								EmployeeDataLoader.LoadData(IsEmployeeNextPage = false);

								break;
							}
						case NomenclaturePlanReportColumnType.EquipmentKind:
							{
								if(nodes.FilterRowSelectType == FilterRowSelectType.Select)
								{
									SelectedEquipmentKinds.Add((EquipmentKindReportColumn)node);
								}
								else
								{
									SelectedEquipmentKinds.Remove((EquipmentKindReportColumn)node);
								}

								EquipmentKindDataLoader.PageSize = EquipmentKindDataLoader.Items.Count + nodes.NomenclaturePlanReportColumns.Length;
								EquipmentKindLastScrollPosition = nodes.ScrollPosition;
								EquipmentKindDataLoader.LoadData(IsEquipmentKindNextPage = false);

								break;
							}
						case NomenclaturePlanReportColumnType.EquipmentType:
							{
								if(nodes.FilterRowSelectType == FilterRowSelectType.Select)
								{
									SelectedEquipmentTypes.Add((EquipmentTypeReportColumn)node);
									EquipmentTypes.Remove((EquipmentTypeReportColumn)node);
								}
								else
								{
									EquipmentTypes.Add((EquipmentTypeReportColumn)node);
									SelectedEquipmentTypes.Remove((EquipmentTypeReportColumn)node);
								}

								EquipmentTypeSearchVM.Update();

								break;
							}
					}
				}
			},
				(nodes) => true
			));

		#region Save defaults commands

		public DelegateCommand SaveProceedsCommand =>
			_saveProceedsCommand ?? (_saveProceedsCommand = new DelegateCommand(() =>
				{
					UoW.Save(SelectedProceeds);
					UoW.Commit();
				},
				() => true
			));

		public DelegateCommand NomenclaturesSaveCommand =>
			_nomenclaturesSaveCommand ?? (_nomenclaturesSaveCommand = new DelegateCommand(() =>
			{
				foreach(var savedNomenclature in _savedNomenclatures.ToList())
				{
					if(!SelectedNomenclatures.Any(x => x.Id == savedNomenclature.Nomenclature.Id))
					{
						_savedNomenclatures.Remove(savedNomenclature);

						UoW.Delete(savedNomenclature);
					}
				}

				foreach(NomenclatureReportColumn selectedNode in SelectedNomenclatures)
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
			},
				() => true
			));

		public DelegateCommand EquipmentKindsSaveCommand =>
			_equipmentKindsSaveCommand ?? (_equipmentKindsSaveCommand = new DelegateCommand(() =>
			{
				foreach(var savedEquipmentKind in _savedEquipmentKinds.ToList())
				{
					if(!SelectedEquipmentKinds.Any(x => x.Id == savedEquipmentKind.EquipmentKind.Id))
					{
						_savedEquipmentKinds.Remove(savedEquipmentKind);

						UoW.Delete(savedEquipmentKind);
					}
				}

				foreach(EquipmentKindReportColumn selectedNode in SelectedEquipmentKinds)
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
			},
				() => true
			));

		public DelegateCommand EquipmentTypesSaveCommand =>
			_equipmentTypesSaveCommand ?? (_equipmentTypesSaveCommand = new DelegateCommand(() =>
			{
				foreach(var savedEquipmentType in _savedEquipmentTypes.ToList())
				{
					if(!SelectedEquipmentTypes.Any(x => x.EquipmentType == savedEquipmentType.EquipmentType))
					{
						_savedEquipmentTypes.Remove(savedEquipmentType);

						UoW.Delete(savedEquipmentType);
					}
				}

				foreach(EquipmentTypeReportColumn selectedNode in SelectedEquipmentTypes)
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
			},
				() => true
			));

		#endregion

		#region Search commands

		public DelegateCommand NomenclatureSearchCommand =>
			_nomenclatureSearchCommand ?? (_nomenclatureSearchCommand = new DelegateCommand(() =>
				{
					if(NomenclatureSearchVM.SearchValues?.Length == 0)
					{
						NomenclatureDataLoader.PageSize = PageSize;
					}
					NomenclatureLastScrollPosition = 0;
					NomenclatureDataLoader.LoadData(IsNomenclatureNextPage = false);
				},
				() => true
			));

		public DelegateCommand EmployeeSearchCommand =>
			_employeeSearchCommand ?? (_employeeSearchCommand = new DelegateCommand(() =>
				{
					if(EmployeeSearchVM.SearchValues?.Length == 0)
					{
						EmployeeDataLoader.PageSize = PageSize;
					}
					EmployeeLastScrollPosition = 0;
					EmployeeDataLoader.LoadData(IsEmployeeNextPage = false);
				},
				() => true
			));

		public DelegateCommand EquipmentKindSearchCommand =>
			_equipmentKindSearchCommand ?? (_equipmentKindSearchCommand = new DelegateCommand(() =>
				{
					if(EquipmentKindSearchVM.SearchValues?.Length == 0)
					{
						EquipmentKindDataLoader.PageSize = PageSize;
					}
					EquipmentKindLastScrollPosition = 0;
					EquipmentKindDataLoader.LoadData(IsEquipmentKindNextPage = false);
				},
				() => true
			));

		public DelegateCommand EquipmentTypeSearchCommand =>
			_equipmentTypeSearchCommand ?? (_equipmentTypeSearchCommand = new DelegateCommand(() =>
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
							EquipmentTypes.Add(new EquipmentTypeReportColumn() { EquipmentType = equipmentType });
						}

						if(!string.IsNullOrWhiteSpace(searchStr) && !EquipmentTypes.Any(x => x.EquipmentType == equipmentType) &&
						   !SelectedEquipmentTypes.Any(x => x.EquipmentType == equipmentType) &&
						   equipmentType.GetEnumTitle().ToLower().Contains(searchStr.ToLower()))
						{
							EquipmentTypes.Add(new EquipmentTypeReportColumn() { EquipmentType = equipmentType });
						}
					}
				},
				() => true
			));
		#endregion

		#endregion

		public SearchViewModel NomenclatureSearchVM { get; private set; }
		public SearchViewModel EmployeeSearchVM { get; private set; }
		public SearchViewModel EquipmentKindSearchVM { get; private set; }
		public SearchViewModel EquipmentTypeSearchVM { get; private set; }
		public GenericObservableList<NomenclatureReportColumn> SelectedNomenclatures { get; private set; }
		public GenericObservableList<EmployeeReportColumn> SelectedEmployees { get; private set; }
		public GenericObservableList<EquipmentKindReportColumn> SelectedEquipmentKinds { get; private set; }
		public GenericObservableList<EquipmentTypeReportColumn> SelectedEquipmentTypes { get; private set; }
		public GenericObservableList<EquipmentTypeReportColumn> EquipmentTypes { get; private set; }
		private IList<StorageNode> _storageList;
		private DelegateCommand<NomenclaturePlanFilterRowSelectNode> _selectNodeCommand;
		private DelegateCommand _nomenclaturesSaveCommand;
		private DelegateCommand _equipmentKindsSaveCommand;
		private DelegateCommand _equipmentTypesSaveCommand;
		private DelegateCommand _nomenclatureSearchCommand;
		private DelegateCommand _employeeSearchCommand;
		private DelegateCommand _equipmentKindSearchCommand;
		private DelegateCommand _equipmentTypeSearchCommand;
		public SelectedProceeds SelectedProceeds { get; set; }
		public bool CanSaveCallCenterMotivationReportFilter { get; private set; }
		public bool IsNomenclatureNextPage { get; set; }
		public bool IsEmployeeNextPage { get; set; }
		public bool IsEquipmentKindNextPage { get; set; }
		public ThreadDataLoader<NomenclatureReportColumn> NomenclatureDataLoader { get; private set; }
		public ThreadDataLoader<EmployeeReportColumn> EmployeeDataLoader { get; private set; }
		public ThreadDataLoader<EquipmentKindReportColumn> EquipmentKindDataLoader { get; private set; }
		public double NomenclatureLastScrollPosition { get; set; }
		public double EmployeeLastScrollPosition { get; set; }
		public double EquipmentKindLastScrollPosition { get; set; }
		public IEntityEntryViewModel ProductGroupEntityEntryViewModel { get; }
		public ProductGroup ProductGroup { get; set; }
		public NomenclatureCategory? NomenclatureCategory { get; set; }
		public EmployeeStatus? EmployeeStatus { get; set; } = Vodovoz.Core.Domain.Employees.EmployeeStatus.IsWorking;
		public IEnumerable<SubdivisionReportColumn> Subdivisions { get; private set; }
		public SubdivisionReportColumn Subdivision { get; private set; }
		public int PageSize => 100;
		public NomenclaturePlanReportMain Report { get; private set; }
		public int CallCenterSubdivisionId { get; }
		public DateTime? EndDate { get; set; }
		public DateTime? StartDate { get; set; }

		public override void Dispose()
		{
			ProductGroupEntityEntryViewModel.ChangedByUser -= OnProductGroupChangedByUser;

			base.Dispose();
		}
	}
}
