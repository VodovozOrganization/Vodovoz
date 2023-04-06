using ClosedXML.Report;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Linq;
using NHibernate.Transform;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Operations;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Store;
using static Vodovoz.ViewModels.ViewModels.Reports.Sales.SalesBySubdivisionsAnalitycsReport;
using Order = Vodovoz.Domain.Orders.Order;

namespace Vodovoz.ViewModels.ViewModels.Reports.Sales
{
	public class SalesBySubdivisionsAnalitycsReportViewModel : DialogTabViewModelBase,
		IClosedXmlAsyncReportViewModel<SalesBySubdivisionsAnalitycsReport>
	{
		private const string _templatePath = @".\Reports\Sales\SalesBySubdivisionsAnalitycsReport.xlsx";

		private readonly IUnitOfWork _unitOfWork;
		private readonly IInteractiveService _interactiveService;
		private bool _isSaving;
		private bool _canSave;
		private bool _isGenerating;
		private bool _canCancelGenerate;

		private IEnumerable<string> _lastGenerationErrors = Enumerable.Empty<string>();
		private SalesBySubdivisionsAnalitycsReport _report;
		private bool _splitByNomenclatures;
		private bool _splitBySubdivisions;
		private bool _splitByWarehouses;
		private DateTime _firstPeriodStartDate;
		private DateTime _firstPeriodEndDate;
		private DateTime? _secondPeriodStartDate;
		private DateTime? _secondPeriodEndDate;

		public SalesBySubdivisionsAnalitycsReportViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			IInteractiveService interactiveService,
			INavigationManager navigation)
			: base(unitOfWorkFactory, interactiveService, navigation)
		{
			_interactiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));

			TabName = "Аналитика продаж КБ";

			_unitOfWork = UnitOfWorkFactory.CreateWithoutRoot();
			_unitOfWork.Session.DefaultReadOnly = true;
		}

		public DateTime FirstPeriodStartDate
		{
			get => _firstPeriodStartDate;
			set => SetField(ref _firstPeriodStartDate, value);
		}

		public DateTime FirstPeriodEndDate
		{
			get => _firstPeriodEndDate;
			set => SetField(ref _firstPeriodEndDate, value);
		}

		public DateTime? SecondPeriodStartDate
		{
			get => _secondPeriodStartDate;
			set => SetField(ref _secondPeriodStartDate, value);
		}

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
			get => _splitByWarehouses;
			set => SetField(ref _splitByWarehouses, value);
		}

		#region Reporting properties

		public CancellationTokenSource ReportGenerationCancelationTokenSource { get; set; }

		public SalesBySubdivisionsAnalitycsReport Report
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
				CanCancelGenerate = IsGenerating;
			}
		}

		public IEnumerable<string> LastGenerationErrors
		{
			get => _lastGenerationErrors;
			set => SetField(ref _lastGenerationErrors, value);
		}

		#endregion

		public void ShowWarning(string message)
		{
			_interactiveService.ShowMessage(ImportanceLevel.Warning, message);
		}

		public async Task<SalesBySubdivisionsAnalitycsReport> GenerateReport(CancellationToken cancellationToken)
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

		public async Task<SalesBySubdivisionsAnalitycsReport> Generate(CancellationToken cancellationToken)
		{
			return await SalesBySubdivisionsAnalitycsReport.Create(
				FirstPeriodStartDate,
				FirstPeriodEndDate,
				SecondPeriodStartDate,
				SecondPeriodEndDate,
				SplitByNomenclatures,
				SplitBySubdivisions,
				SplitByWarehouses,
				GetData,
				GetWarhousesBalances,
				GetNomenclaturesAsync,
				GetProductGroupsAsync,
				GetSubdivisionsAsync,
				GetWarehousesAsync);
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
			DateTime firstPeriodStartDate,
			DateTime firstPeriodEndDate,
			DateTime? secondPeriodStartDate,
			DateTime? secondPeriodEndDate,
			bool splitByNomenclatures,
			bool splitBySubdivisions,
			bool splitByWarehouses)
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

			var subdivisionsIds = new int[]
			{
				35,
			};

			SalesDataNode resultItemAlias = null;

			OrderItem orderItemAlias = null;
			Nomenclature nomenclatureAlias = null;
			ProductGroup productGroupAlias = null;
			Order orderAlias = null;
			Employee authorAlias = null;
			Subdivision subdivisionAlias = null;

			var query = _unitOfWork.Session.QueryOver(() => orderItemAlias);

			query.Where(GetOrderCriterion(filterOrderStatusInclude, firstPeriodStartDate, firstPeriodEndDate))
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
						.Select(() => orderItemAlias.ActualCount).WithAlias(() => resultItemAlias.Amount)
						.Select(() => orderItemAlias.Price).WithAlias(() => resultItemAlias.Price))
				.TransformUsing(Transformers.AliasToBean<SalesDataNode>())
				.SetTimeout(0)
				.ReadOnly()
				.List<SalesDataNode>();
		}

		private IEnumerable<ResidueDataNode> GetWarhousesBalances(DateTime dateTime)
		{
			var incomesQuery = from wmo in _unitOfWork.Session.Query<WarehouseMovementOperation>()
							   join n in _unitOfWork.Session.Query<Nomenclature>()
							   on wmo.Nomenclature.Id equals n.Id
							   join productGroup in _unitOfWork.Session.Query<ProductGroup>()
							   on n.ProductGroup.Id equals productGroup.Id
							   join w in _unitOfWork.Session.Query<Warehouse>()
							   on wmo.IncomingWarehouse.Id equals w.Id
							   where !n.IsArchive
							   select new
							   {
								   NomenclatureId = n.Id,
								   ProductGroupId = productGroup.Id,
								   WarehouseId = w.Id,
								   wmo.Amount
							   };

			var incomes = incomesQuery.ToList();

			var writeOffQuery = from wmo in _unitOfWork.Session.Query<WarehouseMovementOperation>()
								join n in _unitOfWork.Session.Query<Nomenclature>()
								on wmo.Nomenclature.Id equals n.Id
								join productGroup in _unitOfWork.Session.Query<ProductGroup>()
								on n.ProductGroup.Id equals productGroup.Id
								join w in _unitOfWork.Session.Query<Warehouse>()
								on wmo.WriteoffWarehouse.Id equals w.Id
								where !n.IsArchive
								select new
								{
									NomenclatureId = n.Id,
									ProductGroupId = productGroup.Id,
									WarehouseId = w.Id,
									Amount = -wmo.Amount
								};

			var writeOff = writeOffQuery.ToList();

			var result = incomes.Concat(writeOff)
				.GroupBy(x => (x.WarehouseId, x.NomenclatureId))
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
				return ImmutableDictionary<int, string>.Empty;
			}

			var query = from warehouse in _unitOfWork.Session.Query<Warehouse>()
						where warehouseIds.Contains(warehouse.Id)
						select new { warehouse.Id, warehouse.Name };

			var list = await query.ToListAsync();

			return list.ToDictionary(x => x.Id, x => x.Name);
		}

		private async Task<IDictionary<int, string>> GetNomenclaturesAsync(IEnumerable<int> nomenclatureIds)
		{
			if(nomenclatureIds is null || nomenclatureIds.Count() == 0)
			{
				return ImmutableDictionary<int, string>.Empty;
			}

			var query = from nomenclature in _unitOfWork.Session.Query<Nomenclature>()
						where nomenclatureIds.Contains(nomenclature.Id)
						select new { nomenclature.Id, nomenclature.OfficialName };

			var list = await query.ToListAsync();

			return list.ToDictionary(x => x.Id, x => x.OfficialName);
		}

		private async Task<IDictionary<int, string>> GetProductGroupsAsync(IEnumerable<int> productGroupIds)
		{
			if(productGroupIds is null || productGroupIds.Count() == 0)
			{
				return ImmutableDictionary<int, string>.Empty;
			}

			var query = from productGroup in _unitOfWork.Session.Query<ProductGroup>()
						where productGroupIds.Contains(productGroup.Id)
						select new { productGroup.Id, productGroup.Name };

			var list = await query.ToListAsync();

			return list.ToDictionary(x => x.Id, x => x.Name);
		}

		private async Task<IDictionary<int, string>> GetSubdivisionsAsync(IEnumerable<int> subdivisionIds)
		{
			if(subdivisionIds is null || subdivisionIds.Count() == 0)
			{
				return ImmutableDictionary<int, string>.Empty;
			}

			var query = from subdivision in _unitOfWork.Session.Query<Subdivision>()
						where subdivisionIds.Contains(subdivision.Id)
						select new { subdivision.Id, subdivision.Name };

			var list = await query.ToListAsync();

			return list.ToDictionary(x => x.Id, x => x.Name);
		}

		public void ExportReport(string path)
		{
			string templatePath = GetTemplatePath();

			var template = new XLTemplate(templatePath);

			template.AddVariable(Report);
			template.Generate();

			template.SaveAs(path);
		}

		private string GetTemplatePath()
		{
			return _templatePath;
		}

		public override void Dispose()
		{
			ReportGenerationCancelationTokenSource?.Dispose();
			base.Dispose();
		}
	}
}
