using ClosedXML.Report;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Linq;
using NHibernate.Transform;
using OneOf;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Services;
using QS.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Core.Domain.Users.Settings;
using Vodovoz.Core.Domain.Warehouses;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Operations;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories;
using Vodovoz.Infrastructure.Report.SelectableParametersFilter;
using Vodovoz.NHibernateProjections.Orders;
using Vodovoz.ViewModels.Reports;
using Order = Vodovoz.Domain.Orders.Order;

namespace Vodovoz.ViewModels.ViewModels.Reports.Sales
{
	public partial class SalesBySubdivisionsAnalitycsReportViewModel : DialogTabViewModelBase
	{
		private const string _templatePath = @".\Reports\Sales\SalesBySubdivisionsAnalitycsReport.xlsx";
		private const string _templateWithDynamicsPath = @".\Reports\Sales\SalesBySubdivisionsAnalitycsWithDynamicsReport.xlsx";

		private readonly UserSettings _userSettings;
		private readonly SelectableParametersReportFilter _filter;
		private SelectableParameterReportFilterViewModel _filterViewModel;

		private readonly IUnitOfWork _userSettingsUnitOfWork;
		private readonly IInteractiveService _interactiveService;
		private readonly IUserRepository _userRepository;
		private bool _isSaving;
		private bool _canSave = false;
		private bool _isGenerating;
		private bool _canCancelGenerate;

		private IEnumerable<string> _lastGenerationErrors = Enumerable.Empty<string>();
		private OneOf<SalesBySubdivisionsAnalitycsReport, SalesBySubdivisionsAnalitycsWithDynamicsReport>? _report;
		private bool _splitByNomenclatures;
		private bool _splitBySubdivisions;
		private bool _splitByWarehouses;
		private DateTime? _firstPeriodStartDate;
		private DateTime? _firstPeriodEndDate;
		private DateTime? _secondPeriodStartDate;
		private DateTime? _secondPeriodEndDate;

		public SalesBySubdivisionsAnalitycsReportViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			IInteractiveService interactiveService,
			INavigationManager navigation,
			IUserRepository userRepository,
			IUserService userService,
			ICurrentPermissionService currentPermissionService)
			: base(unitOfWorkFactory, interactiveService, navigation)
		{
			if(userService is null)
			{
				throw new ArgumentNullException(nameof(userService));
			}

			if(currentPermissionService is null)
			{
				throw new ArgumentNullException(nameof(currentPermissionService));
			}

			if(!currentPermissionService.ValidatePresetPermission(Vodovoz.Core.Domain.Permissions.ReportPermissions.Sales.CanAccessSalesReports))
			{
				throw new AbortCreatingPageException("У вас нет разрешения на доступ в этот отчет", "Доступ запрещен");
			}

			_interactiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));
			_userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
			_userSettingsUnitOfWork = unitOfWorkFactory.CreateWithoutRoot();

			_userSettings = _userRepository.GetUserSettings(_userSettingsUnitOfWork, userService.CurrentUserId);

			TabName = "Аналитика продаж КБ";

			_filter = new SelectableParametersReportFilter(UoW);
			ConfigureFilter();
		}

		[PropertyChangedAlso(
			nameof(CanSplitByWarehouse),
			nameof(SplitByWarehouses),
			nameof(GenerateSensitive))]
		public DateTime? FirstPeriodStartDate
		{
			get => _firstPeriodStartDate;
			set => SetField(ref _firstPeriodStartDate, value);
		}

		[PropertyChangedAlso(
			nameof(CanSplitByWarehouse),
			nameof(SplitByWarehouses),
			nameof(GenerateSensitive))]
		public DateTime? FirstPeriodEndDate
		{
			get => _firstPeriodEndDate;
			set => SetField(ref _firstPeriodEndDate, value);
		}

		[PropertyChangedAlso(
			nameof(CanSplitByWarehouse),
			nameof(SplitByWarehouses))]

		public DateTime? SecondPeriodStartDate
		{
			get => _secondPeriodStartDate;
			set => SetField(ref _secondPeriodStartDate, value);
		}

		[PropertyChangedAlso(
			nameof(CanSplitByWarehouse),
			nameof(SplitByWarehouses))]
		public DateTime? SecondPeriodEndDate
		{
			get => _secondPeriodEndDate;
			set => SetField(ref _secondPeriodEndDate, value);
		}

		public bool SplitByNomenclatures
		{
			get => _splitByNomenclatures;
			set => SetField(ref _splitByNomenclatures, value);
		}

		public bool SplitBySubdivisions
		{
			get => _splitBySubdivisions;
			set => SetField(ref _splitBySubdivisions, value);
		}

		public bool SplitByWarehouses
		{
			get => CanSplitByWarehouse ? _splitByWarehouses : (_splitByWarehouses = false);
			set => SetField(ref _splitByWarehouses, value);
		}

		public bool CanSplitByWarehouse =>
			((FirstPeriodEndDate - FirstPeriodStartDate)?.TotalDays < 1)
			&& SecondPeriodStartDate is null && SecondPeriodEndDate is null;

		#region Reporting properties

		public CancellationTokenSource ReportGenerationCancelationTokenSource { get; set; }

		public virtual SelectableParameterReportFilterViewModel FilterViewModel
		{
			get => _filterViewModel;
			set => SetField(ref _filterViewModel, value);
		}

		public OneOf<SalesBySubdivisionsAnalitycsReport, SalesBySubdivisionsAnalitycsWithDynamicsReport>? Report
		{
			get => _report;
			set => SetField(ref _report, value);
		}

		public bool CanSave
		{
			get => _canSave;
			set => SetField(ref _canSave, value);
		}

		public bool IsSaving
		{
			get => _isSaving;
			set
			{
				SetField(ref _isSaving, value);
				CanSave = !IsSaving;
			}
		}

		public bool CanGenerate => !IsGenerating;

		public bool GenerateSensitive => FirstPeriodStartDate != null && FirstPeriodEndDate != null;

		public bool CanCancelGenerate
		{
			get => _canCancelGenerate;
			set => SetField(ref _canCancelGenerate, value);
		}

		public bool IsGenerating
		{
			get => _isGenerating;
			set
			{
				SetField(ref _isGenerating, value);
				OnPropertyChanged(nameof(CanGenerate));
				CanSave = !value && Report != null;
				CanCancelGenerate = value;
			}
		}

		public IEnumerable<string> LastGenerationErrors
		{
			get => _lastGenerationErrors;
			set => SetField(ref _lastGenerationErrors, value);
		}

		#endregion

		private void ConfigureFilter()
		{
			var nomenclatureTypeParam = _filter.CreateParameterSet(
				"Склады",
				nameof(Warehouse),
				new ParametersFactory(UoW, (filters) =>
				{
					Warehouse warehouseAlias = null;
					SelectableEntityParameter<Warehouse> resultAlias = null;
					var query = UoW.Session.QueryOver(() => warehouseAlias);
					if(filters != null && filters.Any())
					{
						foreach(var f in filters)
						{
							query.Where(f());
						}
					}

					query.SelectList(list => list
						.Select(() => warehouseAlias.Id).WithAlias(() => resultAlias.EntityId)
						.Select(() => warehouseAlias.Name).WithAlias(() => resultAlias.EntityTitle)
						.Select(Projections.Conditional(
							Restrictions.In(Projections.Property(() => warehouseAlias.Id),
								_userSettings.SalesBySubdivisionsAnalitycsReportSubdivisions.ToArray()),
							Projections.Constant(true),
							Projections.Constant(false))).WithAlias(() => resultAlias.Selected)
					);
					query.TransformUsing(Transformers.AliasToBean<SelectableEntityParameter<Warehouse>>());
					return query.List<SelectableParameter>();
				}));

			_filter.CreateParameterSet(
				"Подразделения",
				nameof(Subdivision),
				new ParametersFactory(UoW, (filters) =>
				{
					Subdivision subdivisionAlias = null;
					SelectableEntityParameter<Subdivision> resultAlias = null;
					var query = UoW.Session.QueryOver(() => subdivisionAlias);
					if(filters != null && filters.Any())
					{
						foreach(var f in filters)
						{
							query.Where(f());
						}
					}

					query.SelectList(list => list
						.Select(() => subdivisionAlias.Id).WithAlias(() => resultAlias.EntityId)
						.Select(() => subdivisionAlias.Name).WithAlias(() => resultAlias.EntityTitle)
						.Select(Projections.Conditional(
							Restrictions.In(Projections.Property(() => subdivisionAlias.Id),
								_userSettings.SalesBySubdivisionsAnalitycsReportWarehouses.ToArray()),
							Projections.Constant(true),
							Projections.Constant(false))).WithAlias(() => resultAlias.Selected)
					);
					query.TransformUsing(Transformers.AliasToBean<SelectableEntityParameter<Subdivision>>());
					return query.List<SelectableParameter>();
				}));

			FilterViewModel = new SelectableParameterReportFilterViewModel(_filter);
		}

		public void ShowWarning(string message)
		{
			_interactiveService.ShowMessage(ImportanceLevel.Warning, message);
		}

		public async Task<OneOf<SalesBySubdivisionsAnalitycsReport, SalesBySubdivisionsAnalitycsWithDynamicsReport>> GenerateReport(CancellationToken cancellationToken)
		{
			try
			{
				var report = await Generate(cancellationToken);

				return report;
			}
			finally
			{
				UoW.Session.Clear();
			}
		}

		public async Task<OneOf<SalesBySubdivisionsAnalitycsReport, SalesBySubdivisionsAnalitycsWithDynamicsReport>> Generate(CancellationToken cancellationToken)
		{
			if(FirstPeriodStartDate is null || FirstPeriodEndDate is null)
			{
				throw new InvalidOperationException("Не задан период");
			}

			ValidateParameters(
				FirstPeriodStartDate.Value,
				FirstPeriodEndDate.Value,
				SecondPeriodStartDate,
				SecondPeriodEndDate,
				SplitByWarehouses);

			var selectedSubdivisionsIds = _filter.ParameterSets
				.Single(ps => ps.ParameterName == nameof(Subdivision))
				.Parameters
					.Where(p => p.Selected)
					.Select(p => (int)p.Value)
					.OrderBy(x => x);

			var selectedWarehousesIds = _filter.ParameterSets
					.Single(ps => ps.ParameterName == nameof(Warehouse))
					.Parameters
						.Where(p => p.Selected)
						.Select(p => (int)p.Value)
						.OrderBy(x => x);

			_userSettings.SalesBySubdivisionsAnalitycsReportWarehouses = selectedWarehousesIds;
			_userSettings.SalesBySubdivisionsAnalitycsReportSubdivisions = selectedSubdivisionsIds;

			if(_userSettingsUnitOfWork.HasChanges)
			{
				_userSettingsUnitOfWork.Save(_userSettings);
				_userSettingsUnitOfWork.Commit();
			}

			if(SecondPeriodStartDate is null && SecondPeriodEndDate is null)
			{
				return await SalesBySubdivisionsAnalitycsReport.Create(
					FirstPeriodStartDate.Value,
					FirstPeriodEndDate.Value,
					SplitByNomenclatures,
					SplitBySubdivisions,
					SplitByWarehouses,
					selectedSubdivisionsIds.ToArray(),
					selectedWarehousesIds.ToArray(),
					GetData,
					GetWarehousesBalances,
					GetNomenclaturesAsync,
					GetProductGroupsAsync,
					GetSubdivisionsAsync,
					GetWarehousesAsync);
			}

			return await SalesBySubdivisionsAnalitycsWithDynamicsReport.Create(
				FirstPeriodStartDate.Value,
				FirstPeriodEndDate.Value,
				SecondPeriodStartDate,
				SecondPeriodEndDate,
				SplitByNomenclatures,
				SplitBySubdivisions,
				selectedSubdivisionsIds.ToArray(),
				GetData,
				GetNomenclaturesAsync,
				GetProductGroupsAsync,
				GetSubdivisionsAsync);
		}

		private AbstractCriterion GetOrderCriterion(OrderStatus[] filterOrderStatusInclude, DateTime startDate, DateTime endDate)
		{
			Order orderAlias = null;

			return Restrictions.And(
						Restrictions.And(
							Restrictions.Or(
								Restrictions.In(Projections.Property(() => orderAlias.OrderStatus), filterOrderStatusInclude),
								Restrictions.And(
									Restrictions.Eq(Projections.Property(() => orderAlias.OrderStatus), OrderStatus.WaitForPayment),
									Restrictions.And(
										Restrictions.Eq(Projections.Property(() => orderAlias.SelfDelivery), true),
										Restrictions.Eq(Projections.Property(() => orderAlias.PayAfterShipment), true)))),
							Restrictions.NotEqProperty(Projections.Property(() => orderAlias.IsContractCloser), Projections.Constant(true))),
						Restrictions.Between(Projections.Property(() => orderAlias.DeliveryDate), startDate, endDate));
		}

		private IEnumerable<SalesDataNode> GetData(
			DateTime startDate,
			DateTime endDate,
			int[] subdivisionsIds)
		{
			var filterOrderStatusInclude = new OrderStatus[]
			{
				OrderStatus.Accepted,
				OrderStatus.InTravelList,
				OrderStatus.OnLoading,
				OrderStatus.OnTheWay,
				OrderStatus.Shipped,
				OrderStatus.UnloadingOnStock,
				OrderStatus.Closed
			};

			SalesDataNode resultItemAlias = null;

			OrderItem orderItemAlias = null;
			Nomenclature nomenclatureAlias = null;
			ProductGroup productGroupAlias = null;
			Order orderAlias = null;
			Employee authorAlias = null;
			Subdivision subdivisionAlias = null;

			var query = UoW.Session.QueryOver(() => orderItemAlias);

			query.Where(GetOrderCriterion(filterOrderStatusInclude, startDate, endDate))
				.And(Restrictions.In(Projections.Property(() => subdivisionAlias.Id), subdivisionsIds));

			return query
				.Left.JoinAlias(() => orderItemAlias.Nomenclature, () => nomenclatureAlias)
				.Left.JoinAlias(() => nomenclatureAlias.ProductGroup, () => productGroupAlias)
				.Left.JoinAlias(() => orderItemAlias.Order, () => orderAlias)
				.Left.JoinAlias(() => orderAlias.Author, () => authorAlias)
				.Left.JoinAlias(() => authorAlias.Subdivision, () => subdivisionAlias)
				.SelectList(list =>
					list.Select(() => nomenclatureAlias.Id).WithAlias(() => resultItemAlias.NomenclatureId)
						.Select(() => productGroupAlias.Id).WithAlias(() => resultItemAlias.ProductGroupId)
						.Select(() => subdivisionAlias.Id).WithAlias(() => resultItemAlias.SubdivisionId)
						.Select(OrderProjections.GetOrderItemCurrentCountProjection()).WithAlias(() => resultItemAlias.Amount)
						.Select(OrderProjections.GetOrderItemSumProjection()).WithAlias(() => resultItemAlias.Price))
				.TransformUsing(Transformers.AliasToBean<SalesDataNode>())
				.SetTimeout(0)
				.ReadOnly()
				.List<SalesDataNode>();
		}
		
		private IEnumerable<ResidueDataNode> GetWarehousesBalances(
			DateTime dateTime,
			int[] warehousesIds)
		{
			var balancesQuery =
				from wmo in UoW.Session.Query<WarehouseBulkGoodsAccountingOperation>()
				join n in UoW.Session.Query<Nomenclature>()
					on wmo.Nomenclature.Id equals n.Id
				join productGroup in UoW.Session.Query<ProductGroup>()
					on n.ProductGroup.Id equals productGroup.Id
				join w in UoW.Session.Query<Warehouse>()
					on wmo.Warehouse.Id equals w.Id
				where !n.IsArchive
					&& wmo.OperationTime <= dateTime
					&& warehousesIds.Contains(w.Id)
				select new
				{
				   NomenclatureId = n.Id,
				   ProductGroupId = productGroup.Id,
				   WarehouseId = w.Id,
				   wmo.Amount
				};

			var balances = balancesQuery.ToList();

			var result = 
				balances.GroupBy(x => (x.WarehouseId, x.NomenclatureId))
				.Select(group => new ResidueDataNode
				{
					NomenclatureId = group.Key.NomenclatureId,
					ProductGroupId = group.First().ProductGroupId,
					WarehouseId = group.Key.WarehouseId,
					Residue = group.Sum(x => x.Amount),
				})
				.Where(row => row.Residue != 0);

			return result;
		}

		private async Task<IDictionary<int, string>> GetWarehousesAsync(IEnumerable<int> warehouseIds)
		{
			if(warehouseIds is null || warehouseIds.Count() == 0)
			{
				return new Dictionary<int, string>();
			}

			var query = from warehouse in UoW.Session.Query<Warehouse>()
						where warehouseIds.Contains(warehouse.Id)
						select new { warehouse.Id, warehouse.Name };

			var list = await query.ToListAsync();

			return list.ToDictionary(x => x.Id, x => x.Name);
		}

		private async Task<IDictionary<int, string>> GetNomenclaturesAsync(IEnumerable<int> nomenclatureIds)
		{
			if(nomenclatureIds is null || nomenclatureIds.Count() == 0)
			{
				return new Dictionary<int, string>();
			}

			var query = from nomenclature in UoW.Session.Query<Nomenclature>()
						where nomenclatureIds.Contains(nomenclature.Id)
						select new { nomenclature.Id, nomenclature.OfficialName };

			var list = await query.ToListAsync();

			return list.ToDictionary(x => x.Id, x => x.OfficialName);
		}

		private async Task<IDictionary<int, string>> GetProductGroupsAsync(IEnumerable<int> productGroupIds)
		{
			if(productGroupIds is null || productGroupIds.Count() == 0)
			{
				return new Dictionary<int, string>();
			}

			var query = from productGroup in UoW.Session.Query<ProductGroup>()
						where productGroupIds.Contains(productGroup.Id)
						select new { productGroup.Id, productGroup.Name };

			var list = await query.ToListAsync();

			return list.ToDictionary(x => x.Id, x => x.Name);
		}

		private async Task<IDictionary<int, string>> GetSubdivisionsAsync(IEnumerable<int> subdivisionIds)
		{
			if(subdivisionIds is null || subdivisionIds.Count() == 0)
			{
				return new Dictionary<int, string>();
			}

			var query = from subdivision in UoW.Session.Query<Subdivision>()
						where subdivisionIds.Contains(subdivision.Id)
						select new { subdivision.Id, subdivision.Name };

			var list = await query.ToListAsync();

			return list.ToDictionary(x => x.Id, x => x.Name);
		}

		public void ExportReport(string path)
		{
			var template = new XLTemplate(Report.Value.Match(
				_ => _templatePath,
				_ => _templateWithDynamicsPath));

			Report.Value.Switch(
				report => template.AddVariable(report),
				reportWithDynamics => template.AddVariable(reportWithDynamics));

			template.Generate();

			template.SaveAs(path);
		}

		public void ShowInfo()
		{
			var info = "Первый раз нужно выбрать необходимые склады и подразделения\n" +
				"Далее отчет формируется по выбранным Складам и Подразделениям\n" +
				"Склады и Подразделения запоминаются при формировании отчета\n" +
				"Остатки по складам формируются только при выборе 1 периода с интервалом в 1 день";

			_interactiveService.ShowMessage(ImportanceLevel.Info, info, "Информация");
		}

		private static void ValidateParameters(
			DateTime firstPeriodStartDate,
			DateTime firstPeriodEndDate,
			DateTime? secondPeriodStartDate,
			DateTime? secondPeriodEndDate,
			bool splitByWarehouses)
		{
			if(splitByWarehouses && (secondPeriodStartDate != null || secondPeriodEndDate != null))
			{
				throw new ArgumentException("Нельзя выбрать разбивку по складам для отчета с двумя периодами",
					nameof(splitByWarehouses));
			}

			if(splitByWarehouses
				&& (firstPeriodEndDate - firstPeriodStartDate).TotalDays > 1)
			{
				throw new ArgumentException("Нельзя выбрать разбивку по складам для отчета с интервалом более одного дня",
					nameof(splitByWarehouses));
			}
		}

		public override void Dispose()
		{
			ReportGenerationCancelationTokenSource?.Dispose();
			base.Dispose();
		}
	}
}
