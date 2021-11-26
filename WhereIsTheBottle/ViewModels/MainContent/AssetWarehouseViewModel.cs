using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using NHibernate.Transform;
using NLog;
using QS.DomainModel.UoW;
using QS.Models;
using QS.Project.DB;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Operations;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Goods;
using Vodovoz.EntityRepositories.Goods.BottleAnalytics;
using Vodovoz.EntityRepositories.Store;
using WhereIsTheBottle.Commands;
using WhereIsTheBottle.Models.MainContent;
using WhereIsTheBottle.Models.MainContent.CommonRestrictions;
using WhereIsTheBottle.Models.MainContent.Nodes;
using Order = Vodovoz.Domain.Orders.Order;

namespace WhereIsTheBottle.ViewModels.MainContent
{
	public class AssetWarehouseViewModel : BottleAnalyticsReportViewModelBase<AssetWarehouseModel>
	{
		private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
		private RelayCommand _loadDataCommand;

		public AssetWarehouseViewModel(
			AssetWarehouseModel model)
			: base(model)
		{
			Items = new ObservableCollection<AssetMovementNode>() {new AssetMovementNode
			{
				Delta = 123
			}};
		}

		public RelayCommand LoadDataCommand => _loadDataCommand ??= new RelayCommand(
			async () =>
			{
				if(StartDate == null || EndDate == null)
				{
					return;
				}

				try
				{
					IsDataLoading = true;
					IEnumerable<AssetMovementNode> nodes = null;

					// var task = Task.Run(() => nodes = GetData(UnitOfWorkFactory.GetDefaultFactory, StartDate.Value.DateString, EndDate.Value.DateString,
					// 	SelectedAssetNode.Id.Value));
					// await task;

					Items = new ObservableCollection<AssetMovementNode>(nodes);
					DateFormed = DateTime.Now;
					IsDataLoaded = true;
					OnPropertyChanged(nameof(HeaderString));
				}
				catch(Exception ex)
				{
					_logger.Error(ex);
					throw;
				}
				finally
				{
					IsDataLoading = false;
				}
			}, () => !IsDataLoading
		);

		private static IEnumerable<AssetMovementNode> GetData(IUnitOfWorkFactory unitOfWorkFactory, DateTime startDate, DateTime endDate,
			int warehouseId)
		{
			var uow = unitOfWorkFactory.CreateWithoutRoot();
			startDate = startDate.Date;
			endDate = endDate.Date.AddDays(1).AddTicks(-1);

			AmountOnDateNode amountOnDateAlias = null;
			Nomenclature nomenclatureAlias = null;
			Nomenclature nomenclatureAlias2 = null;
			WarehouseMovementOperation warehouseOperationAlias = null;
			WarehouseMovementOperation warehouseOperationAlias2 = null;
			RouteList routeListAlias = null;
			RouteListItem routeListItemAlias = null;
			MovementDocument movementDocumentAlias = null;
			MovementDocumentItem movementDocumentItemAlias = null;
			RegradingOfGoodsDocumentItem regradingOfGoodsDocumentItemAlias = null;
			WriteoffDocument writeoffDocumentAlias = null;
			WriteoffDocumentItem writeoffDocumentItemAlias = null;
			InventoryDocument inventoryDocumentAlias = null;
			InventoryDocumentItem inventoryDocumentItemAlias = null;
			IncomingInvoice incomingInvoiceAlias = null;
			IncomingInvoiceItem incomingInvoiceItemAlias = null;
			SelfDeliveryDocument selfDeliveryDocumentAlias = null;
			SelfDeliveryDocumentItem selfDeliveryDocumentItemAlias = null;
			SelfDeliveryDocumentReturned selfDeliveryDocumentReturnedAlias = null;
			Order orderAlias = null;
			OrderItem orderItemAlias = null;
			OrderEquipment orderEquipmentAlias = null;
			EmployeeNomenclatureMovementOperation employeeOperationAlias = null;

			#region Актив складов

			var warehouseIncomeAssetQuery = uow.Session.QueryOver(() => warehouseOperationAlias)
				.Left.JoinAlias(() => warehouseOperationAlias.Nomenclature, () => nomenclatureAlias)
				.Where(MainContentCommonRestrictions.NomenclatureRestriction)
				.And(() => warehouseOperationAlias.IncomingWarehouse != null)
				.And(() => warehouseOperationAlias.OperationTime < endDate)
				.And(() => warehouseOperationAlias.IncomingWarehouse.Id == warehouseId)
				.SelectList(list => list
					.Select(Projections.GroupProperty(
						CustomProjections.Date(Projections.Property(() => warehouseOperationAlias.OperationTime))))
					.WithAlias(() => amountOnDateAlias.DateTime)
					.Select(Projections.Cast(NHibernateUtil.Int32,
						Projections.Sum(Projections.Property(() => warehouseOperationAlias.Amount))))
					.WithAlias(() => amountOnDateAlias.Amount))
				.TransformUsing(Transformers.AliasToBean<AmountOnDateNode>())
				.Future<AmountOnDateNode>();

			var warehouseWriteoffAssetQuery = uow.Session.QueryOver(() => warehouseOperationAlias)
				.Left.JoinAlias(() => warehouseOperationAlias.Nomenclature, () => nomenclatureAlias)
				.Where(MainContentCommonRestrictions.NomenclatureRestriction)
				.And(() => warehouseOperationAlias.WriteoffWarehouse != null)
				.And(() => warehouseOperationAlias.OperationTime < endDate)
				.And(() => warehouseOperationAlias.WriteoffWarehouse.Id == warehouseId)
				.SelectList(list => list
					.Select(Projections.GroupProperty(
						CustomProjections.Date(Projections.Property(() => warehouseOperationAlias.OperationTime))))
					.WithAlias(() => amountOnDateAlias.DateTime)
					.Select(Projections.Cast(NHibernateUtil.Int32,
						Projections.Sum(Projections.Property(() => warehouseOperationAlias.Amount))))
					.WithAlias(() => amountOnDateAlias.Amount))
				.TransformUsing(Transformers.AliasToBean<AmountOnDateNode>())
				.Future<AmountOnDateNode>();

			#endregion

			#region Документ пересорта (-)

			var regradingOfGoodsLossQuery = uow.Session.QueryOver(() => regradingOfGoodsDocumentItemAlias)
				.Left.JoinAlias(() => regradingOfGoodsDocumentItemAlias.WarehouseWriteOffOperation, () => warehouseOperationAlias)
				.Left.JoinAlias(() => warehouseOperationAlias.Nomenclature, () => nomenclatureAlias)
				.Left.JoinAlias(() => regradingOfGoodsDocumentItemAlias.WarehouseIncomeOperation, () => warehouseOperationAlias2)
				.Left.JoinAlias(() => warehouseOperationAlias2.Nomenclature, () => nomenclatureAlias2)
				.Where(MainContentCommonRestrictions.NomenclatureRestriction)
				.And(!MainContentCommonRestrictions.NomenclatureRestriction2)
				.And(() => warehouseOperationAlias.OperationTime >= startDate)
				.And(() => warehouseOperationAlias.OperationTime <= endDate)
				.And(() => warehouseOperationAlias.WriteoffWarehouse.Id == warehouseId)
				.SelectList(list => list
					.Select(Projections.GroupProperty(
						CustomProjections.Date(Projections.Property(() => warehouseOperationAlias.OperationTime))))
					.WithAlias(() => amountOnDateAlias.DateTime)
					.Select(Projections.Cast(NHibernateUtil.Int32,
						Projections.Sum(Projections.Property(() => warehouseOperationAlias.Amount))))
					.WithAlias(() => amountOnDateAlias.Amount))
				.TransformUsing(Transformers.AliasToBean<AmountOnDateNode>())
				.Future<AmountOnDateNode>();

			#endregion

			#region Документ пересорта (+)

			var regradingOfGoodsIncomeQuery = uow.Session.QueryOver(() => regradingOfGoodsDocumentItemAlias)
				.Left.JoinAlias(() => regradingOfGoodsDocumentItemAlias.WarehouseWriteOffOperation, () => warehouseOperationAlias)
				.Left.JoinAlias(() => warehouseOperationAlias.Nomenclature, () => nomenclatureAlias)
				.Left.JoinAlias(() => regradingOfGoodsDocumentItemAlias.WarehouseIncomeOperation, () => warehouseOperationAlias2)
				.Left.JoinAlias(() => warehouseOperationAlias2.Nomenclature, () => nomenclatureAlias2)
				.Where(!MainContentCommonRestrictions.NomenclatureRestriction)
				.And(MainContentCommonRestrictions.NomenclatureRestriction2)
				.And(() => warehouseOperationAlias2.OperationTime >= startDate)
				.And(() => warehouseOperationAlias2.OperationTime <= endDate)
				.And(() => warehouseOperationAlias2.IncomingWarehouse.Id == warehouseId)
				.SelectList(list => list
					.Select(Projections.GroupProperty(
						CustomProjections.Date(Projections.Property(() => warehouseOperationAlias2.OperationTime))))
					.WithAlias(() => amountOnDateAlias.DateTime)
					.Select(Projections.Cast(NHibernateUtil.Int32,
						Projections.Sum(Projections.Property(() => warehouseOperationAlias2.Amount))))
					.WithAlias(() => amountOnDateAlias.Amount))
				.TransformUsing(Transformers.AliasToBean<AmountOnDateNode>())
				.Future<AmountOnDateNode>();

			#endregion

			#region Акт списания (-)

			var writeoffDocumentQuery = uow.Session.QueryOver(() => writeoffDocumentItemAlias)
				.Left.JoinAlias(() => writeoffDocumentItemAlias.Document, () => writeoffDocumentAlias)
				.Left.JoinAlias(() => writeoffDocumentItemAlias.WarehouseWriteoffOperation, () => warehouseOperationAlias)
				.Left.JoinAlias(() => warehouseOperationAlias.Nomenclature, () => nomenclatureAlias)
				.Where(MainContentCommonRestrictions.NomenclatureRestriction)
				.And(() => warehouseOperationAlias.OperationTime >= startDate)
				.And(() => warehouseOperationAlias.OperationTime <= endDate)
				.And(() => warehouseOperationAlias.WriteoffWarehouse.Id == warehouseId)
				.SelectList(list => list
					.Select(Projections.GroupProperty(
						CustomProjections.Date(Projections.Property(() => warehouseOperationAlias.OperationTime))))
					.WithAlias(() => amountOnDateAlias.DateTime)
					.Select(Projections.Cast(NHibernateUtil.Int32,
						Projections.Sum(Projections.Property(() => warehouseOperationAlias.Amount))))
					.WithAlias(() => amountOnDateAlias.Amount))
				.TransformUsing(Transformers.AliasToBean<AmountOnDateNode>())
				.Future<AmountOnDateNode>();

			#endregion

			#region Инвентаризация (-)

			var inventoryNegativeDocumentQuery = uow.Session.QueryOver(() => inventoryDocumentItemAlias)
				.Left.JoinAlias(() => inventoryDocumentItemAlias.Document, () => inventoryDocumentAlias)
				.Left.JoinAlias(() => inventoryDocumentItemAlias.WarehouseChangeOperation, () => warehouseOperationAlias)
				.Left.JoinAlias(() => warehouseOperationAlias.Nomenclature, () => nomenclatureAlias)
				.Where(MainContentCommonRestrictions.NomenclatureRestriction)
				.And(() => warehouseOperationAlias.WriteoffWarehouse != null)
				.And(() => warehouseOperationAlias.OperationTime >= startDate)
				.And(() => warehouseOperationAlias.OperationTime <= endDate)
				.And(() => warehouseOperationAlias.WriteoffWarehouse.Id == warehouseId)
				.SelectList(list => list
					.Select(Projections.GroupProperty(
						CustomProjections.Date(Projections.Property(() => warehouseOperationAlias.OperationTime))))
					.WithAlias(() => amountOnDateAlias.DateTime)
					.Select(Projections.Cast(NHibernateUtil.Int32,
						Projections.Sum(Projections.Property(() => warehouseOperationAlias.Amount))))
					.WithAlias(() => amountOnDateAlias.Amount))
				.TransformUsing(Transformers.AliasToBean<AmountOnDateNode>())
				.Future<AmountOnDateNode>();

			#endregion

			#region Инвентаризация (+)

			var inventoryPositiveDocumentQuery = uow.Session.QueryOver(() => inventoryDocumentItemAlias)
				.Left.JoinAlias(() => inventoryDocumentItemAlias.Document, () => inventoryDocumentAlias)
				.Left.JoinAlias(() => inventoryDocumentItemAlias.WarehouseChangeOperation, () => warehouseOperationAlias)
				.Left.JoinAlias(() => warehouseOperationAlias.Nomenclature, () => nomenclatureAlias)
				.Where(MainContentCommonRestrictions.NomenclatureRestriction)
				.And(() => warehouseOperationAlias.IncomingWarehouse != null)
				.And(() => warehouseOperationAlias.OperationTime >= startDate)
				.And(() => warehouseOperationAlias.OperationTime <= endDate)
				.And(() => warehouseOperationAlias.IncomingWarehouse.Id == warehouseId)
				.SelectList(list => list
					.Select(Projections.GroupProperty(
						CustomProjections.Date(Projections.Property(() => warehouseOperationAlias.OperationTime))))
					.WithAlias(() => amountOnDateAlias.DateTime)
					.Select(Projections.Cast(NHibernateUtil.Int32,
						Projections.Sum(Projections.Property(() => warehouseOperationAlias.Amount))))
					.WithAlias(() => amountOnDateAlias.Amount))
				.TransformUsing(Transformers.AliasToBean<AmountOnDateNode>())
				.Future<AmountOnDateNode>();

			#endregion

			#region Документ самовывоза

			var deliveredSubquery = QueryOver.Of(() => selfDeliveryDocumentItemAlias)
				.Inner.JoinAlias(() => selfDeliveryDocumentItemAlias.WarehouseMovementOperation, () => warehouseOperationAlias)
				.Inner.JoinAlias(() => warehouseOperationAlias.Nomenclature, () => nomenclatureAlias)
				.Where(() => selfDeliveryDocumentItemAlias.Document.Id == selfDeliveryDocumentAlias.Id)
				.And(MainContentCommonRestrictions.NomenclatureRestriction)
				.Select(Projections.Sum(() => warehouseOperationAlias.Amount));

			var returnedSubquery = QueryOver.Of(() => selfDeliveryDocumentReturnedAlias)
				.Left.JoinAlias(() => selfDeliveryDocumentReturnedAlias.WarehouseMovementOperation, () => warehouseOperationAlias)
				.Left.JoinAlias(() => warehouseOperationAlias.Nomenclature, () => nomenclatureAlias)
				.Where(() => selfDeliveryDocumentReturnedAlias.Document.Id == selfDeliveryDocumentAlias.Id)
				.And(MainContentCommonRestrictions.NomenclatureRestriction)
				.Select(Projections.Sum(() => warehouseOperationAlias.Amount));

			var selfDeliveryQuery = uow.Session.QueryOver(() => selfDeliveryDocumentAlias)
				.Where(Restrictions.Disjunction()
					.Add(Subqueries.Exists(deliveredSubquery.DetachedCriteria))
					.Add(Subqueries.Exists(returnedSubquery.DetachedCriteria)))
				.And(() => selfDeliveryDocumentAlias.TimeStamp >= startDate)
				.And(() => selfDeliveryDocumentAlias.TimeStamp <= endDate)
				.And(() => selfDeliveryDocumentAlias.Warehouse.Id == warehouseId)
				.SelectList(list => list
					.Select(CustomProjections.Date(() => selfDeliveryDocumentAlias.TimeStamp))
					.WithAlias(() => amountOnDateAlias.DateTime)
					.Select(Projections.SqlFunction(
						new SQLFunctionTemplate(NHibernateUtil.Int32, "CAST(IFNULL(?1, 0) - IFNULL(?2,0) as SIGNED)"),
						NHibernateUtil.Int32,
						Projections.SubQuery(deliveredSubquery),
						Projections.SubQuery(returnedSubquery))
					)
					.WithAlias(() => amountOnDateAlias.Amount))
				.Where(Restrictions.NotEqProperty(
					Projections.SqlFunction(new SQLFunctionTemplate(NHibernateUtil.Int32, "CAST(IFNULL(?1, 0) - IFNULL(?2,0) as SIGNED)"),
						NHibernateUtil.Int32,
						Projections.SubQuery(deliveredSubquery),
						Projections.SubQuery(returnedSubquery)
					),
					Projections.Constant(0))
				)
				.TransformUsing(Transformers.AliasToBean<AmountOnDateNode>())
				.Future<AmountOnDateNode>();

			#endregion

			#region Входящая накладная (+)

			var incomingInvoiceQuery = uow.Session.QueryOver(() => incomingInvoiceItemAlias)
				.Left.JoinAlias(() => incomingInvoiceItemAlias.Document, () => incomingInvoiceAlias)
				.Left.JoinAlias(() => incomingInvoiceItemAlias.IncomeGoodsOperation, () => warehouseOperationAlias)
				.Left.JoinAlias(() => warehouseOperationAlias.Nomenclature, () => nomenclatureAlias)
				.Where(MainContentCommonRestrictions.NomenclatureRestriction)
				.And(() => warehouseOperationAlias.IncomingWarehouse != null)
				.And(() => warehouseOperationAlias.OperationTime >= startDate)
				.And(() => warehouseOperationAlias.OperationTime <= endDate)
				.And(() => warehouseOperationAlias.IncomingWarehouse.Id == warehouseId)
				.SelectList(list => list
					.Select(Projections.GroupProperty(
						CustomProjections.Date(Projections.Property(() => warehouseOperationAlias.OperationTime))))
					.WithAlias(() => amountOnDateAlias.DateTime)
					.Select(Projections.Cast(NHibernateUtil.Int32,
						Projections.Sum(Projections.Property(() => warehouseOperationAlias.Amount))))
					.WithAlias(() => amountOnDateAlias.Amount))
				.TransformUsing(Transformers.AliasToBean<AmountOnDateNode>())
				.Future<AmountOnDateNode>();

			#endregion

			#region Расчёт самовывоза

			var selfDeliveryLossQuery = selfDeliveryQuery
				.Where(x => x.Amount > 0)
				.GroupBy(x => x.DateTime)
				.Select(g => new AmountOnDateNode
				{
					Amount = g.Sum(x => x.Amount),
					DateTime = g.Key
				})
				.ToList();

			var selfDeliveryIncomeQuery = selfDeliveryQuery
				.Where(x => x.Amount < 0)
				.GroupBy(x => x.DateTime)
				.Select(g => new AmountOnDateNode
				{
					Amount = g.Sum(x => -x.Amount),
					DateTime = g.Key
				})
				.ToList();

			#endregion

			IList<AssetMovementNode> results = new List<AssetMovementNode>();
			for(var date = startDate; date < endDate; date = date.AddDays(1))
			{
				var node = new AssetMovementNode
				{
					// Date = date.ToString("dd.MM"),
					CounterpartySelfDeliveryLoss =
						-(selfDeliveryLossQuery.FirstOrDefault(x => x.DateTime == date)?.Amount ?? 0),
					RegradingOfGoodsLoss =
						-(regradingOfGoodsLossQuery.FirstOrDefault(x => x.DateTime == date)?.Amount ?? 0),
					RegradingOfGoodsIncome =
						+(regradingOfGoodsIncomeQuery.FirstOrDefault(x => x.DateTime == date)?.Amount ?? 0),
					// VariousLoss =
					// 	-(writeoffDocumentQuery.FirstOrDefault(x => x.DateTime == date)?.Amount ?? 0)
					// 	- (inventoryNegativeDocumentQuery.FirstOrDefault(x => x.DateTime == date)?.Amount ?? 0),
					// PurchaseIncome =
					// 	+incomingInvoiceQuery.FirstOrDefault(x => x.DateTime == date)?.Amount ?? 0,
					// CounterpartyReturnIncome =
					// 	+(selfDeliveryIncomeQuery.FirstOrDefault(x => x.DateTime == date)?.Amount ?? 0),
					// InventarizationIncome =
					// 	+(inventoryPositiveDocumentQuery.FirstOrDefault(x => x.DateTime == date)?.Amount ?? 0),
					// AssetByMorning =
					// 	+warehouseIncomeAssetQuery.Where(x => x.DateTime.Date < date).Sum(x => x.Amount)
					// 	- warehouseWriteoffAssetQuery.Where(x => x.DateTime.Date < date).Sum(x => x.Amount)
				};

				results.Add(node);
			}
			foreach(var node in results)
			{
				// node.Calculate();
			}
			uow.Dispose();
			return results.OrderByDescending(x => x.Date);
		}

		#region Properties

		public override string HeaderString => "GetHeaderString();";

		private WarehouseNode _selectedWarehouseNode;
		public WarehouseNode SelectedWarehouseNode
		{
			get => _selectedWarehouseNode;
			set
			{
				if(SetField(ref _selectedWarehouseNode, value))
				{
					// OnFilterChanged();
				}
			}
		}

		private ObservableCollection<WarehouseNode> _selectableWarehouseNodes;
		public ObservableCollection<WarehouseNode> SelectableWarehouseNodes
		{
			get => _selectableWarehouseNodes;
			set => SetField(ref _selectableWarehouseNodes, value);
		}

		private ObservableCollection<AssetMovementNode> _items;
		public ObservableCollection<AssetMovementNode> Items
		{
			get => _items;
			set => SetField(ref _items, value);
		}

		#endregion
	}

	public class AssetMovementNode
	{
		private string _assetByMorningString;
		private string _assetByEveningString;
		private string _dateString;

		public DateTime Date { get; set; }

		public string DateString
		{
			get => _dateString ?? Date.ToString("dd.MM");
			set => _dateString = value;
		}

		public int AssetByMorning => WarehousesAsset + RouteListAsset + MovementDocumentsAsset;

		public string AssetByMorningString
		{
			get => _assetByMorningString ?? AssetByMorning.ToString();
			set => _assetByMorningString = value;
		}

		public int WarehousesAsset { get; set; }
		public int RouteListAsset { get; set; }
		public int MovementDocumentsAsset { get; set; }

		public int AssetDriversIncome { get; set; }

		public int AssetDriversLoss { get; set; }

		public int AssetMovementsIncome { get; set; }

		public int AssetMovementsLoss { get; set; }

		public int SummaryMovements { get; set; }

		public int InventarizationIncome { get; set; }
		public int InventarizationLoss { get; set; }

		public int RegradingOfGoodsIncome { get; set; }
		public int RegradingOfGoodsLoss { get; set; }

		public int CounterpartySelfDeliveryIncome { get; set; }
		public int CounterpartySelfDeliveryLoss { get; set; }

		public int DriversDiscrepancyIncome { get; set; }
		public int DriversDiscrepancyLoss { get; set; }

		public int CounterpartyReturnIncome { get; set; }
		public int CounterpartyReturnLoss { get; set; }

		public int IncomingInvoiceIncome { get; set; }
		public int WriteoffDocumentLoss { get; set; }

		public int TotalLoss { get; set; }
		public int TotalIncome { get; set; }
		public int Delta { get; set; }
		public int AssetByEvening { get; set; }

		public string AssetByEveningString
		{
			get => _assetByEveningString ?? AssetByEvening.ToString();
			set => _assetByEveningString = value;
		}

		public void Calculate()
		{
			TotalLoss =
				CounterpartyReturnLoss
				+ CounterpartySelfDeliveryLoss
				+ RegradingOfGoodsLoss
				+ InventarizationLoss
				+ WriteoffDocumentLoss
				+ DriversDiscrepancyLoss;
			TotalIncome =
				DriversDiscrepancyIncome
				+ IncomingInvoiceIncome
				+ CounterpartyReturnIncome
				+ InventarizationIncome
				+ RegradingOfGoodsIncome
				+ CounterpartySelfDeliveryIncome;
			Delta = TotalIncome + TotalLoss;
			AssetByEvening = AssetByMorning + Delta;
		}
	}
}
