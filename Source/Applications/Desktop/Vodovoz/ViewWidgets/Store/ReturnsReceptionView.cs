using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using NHibernate.Criterion;
using NHibernate.Transform;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Project.Services;
using Vodovoz.Core.DataService;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Operations;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Service;
using Vodovoz.Domain.Store;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.EntityRepositories.Stock;
using Vodovoz.EntityRepositories.Store;
using Vodovoz.EntityRepositories.Subdivisions;
using Vodovoz.Parameters;
using Vodovoz.Repository.Store;
using Vodovoz.Services;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Journals.FilterViewModels.Goods;

namespace Vodovoz
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class ReturnsReceptionView : QS.Dialog.Gtk.WidgetOnDialogBase
	{
		GenericObservableList<ReceptionItemNode> ReceptionReturnsList = new GenericObservableList<ReceptionItemNode>();
		private readonly ITerminalNomenclatureProvider _terminalNomenclatureProvider;
		private readonly ISubdivisionRepository _subdivisionRepository;
		private readonly ICarLoadDocumentRepository _carLoadDocumentRepository;
		private readonly ICarUnloadRepository _carUnloadRepository;
		private readonly INomenclatureParametersProvider _nomenclatureParametersProvider;

		private bool? _userHasOnlyAccessToWarehouseAndComplaints;

		public IList<ReceptionItemNode> Items => ReceptionReturnsList;

		public void AddItem(ReceptionItemNode item) => ReceptionReturnsList.Add(item);

		public ReturnsReceptionView()
		{
			var baseParameters = new BaseParametersProvider(new ParametersProvider());
			_terminalNomenclatureProvider = baseParameters;
			var routeListRepository = new RouteListRepository(new StockRepository(), baseParameters);
			_carLoadDocumentRepository = new CarLoadDocumentRepository(routeListRepository);
			_carUnloadRepository = new CarUnloadRepository();
			_subdivisionRepository = new SubdivisionRepository(new ParametersProvider());
			_nomenclatureParametersProvider = new NomenclatureParametersProvider(new ParametersProvider());

			Build();

			ytreeReturns.ColumnsConfig = Gamma.GtkWidgets.ColumnsConfigFactory.Create<ReceptionItemNode>()
				.AddColumn("Номенклатура").AddTextRenderer(node => node.Name)
				.AddColumn("№ Кулера").AddTextRenderer(node => node.Redhead)
					.AddSetter((cell, node) => cell.Editable = node.NomenclatureCategory == NomenclatureCategory.additional)
				.AddColumn("Кол-во")
					.AddNumericRenderer(node => node.Amount, false)
					.Adjustment(new Gtk.Adjustment(0, 0, 9999, 1, 100, 0))
					.AddSetter((cell, node) => cell.Editable = node.EquipmentId == 0)
					.AddSetter((cell, node) => CalculateAmount(node))
				.AddColumn("Ожидаемое кол-во")
					.AddNumericRenderer(node => node.ExpectedAmount, false)
					.Digits(0)
				.AddColumn("")
				.Finish();

			ytreeReturns.ItemsDataSource = ReceptionReturnsList;
		}

		private void CalculateAmount(ReceptionItemNode node)
		{
			if (node.Name == "Терминал для оплаты" && node.Amount > node.ExpectedAmount && UoW.IsNew) 
				node.Amount = node.ExpectedAmount;
		}

		private IUnitOfWork uow;

		public IUnitOfWork UoW {
			get => uow;
			set {
				if(uow == value)
					return;
				uow = value;
			}
		}

		Warehouse warehouse;
		public Warehouse Warehouse {
			get => warehouse;
			set {
				warehouse = value;
				FillListReturnsFromRoute(_terminalNomenclatureProvider.GetNomenclatureIdForTerminal);
			}
		}

		RouteList routeList;
		public RouteList RouteList {
			get => routeList;
			set {
				if(routeList == value)
					return;
				routeList = value;
				if(routeList != null) {
					FillListReturnsFromRoute(_terminalNomenclatureProvider.GetNomenclatureIdForTerminal);
				} else {
					ReceptionReturnsList.Clear();
				}
			}
		}

		public bool Sensitive {
			set => ytreeReturns.Sensitive = buttonAddNomenclature.Sensitive = value;
		}

		public IList<Equipment> AlreadyUnloadedEquipment = new List<Equipment>();

		void FillListReturnsFromRoute(int terminalId)
		{
			if(Warehouse == null || RouteList == null)
				return;
			
			ReceptionReturnsList.Clear();

			ReceptionItemNode resultAlias = null;
			Domain.Orders.Order orderAlias = null;
			Nomenclature nomenclatureAlias = null;
			OrderEquipment orderEquipmentAlias = null;
			RouteListItem routeListItemAlias = null;
			RouteList routeListAlias = null;
			AdditionalLoadingDocument additionalLoadingDocumentAlias = null;
			AdditionalLoadingDocumentItem additionalLoadingDocumentItemAlias = null;
			DeliveryFreeBalanceOperation freeBalanceOperationAlias = null;
			CarUnloadDocumentItem carUnloadDocumentItemAlias = null;
			OrderItem orderItemAlias = null;

			IList<ReceptionItemNode> returnable = new List<ReceptionItemNode>();
			IList<ReceptionItemNode> orderItemsNomenclatures = new List<ReceptionItemNode>();
			IList<ReceptionItemNode> orderEquipmentsNomenclatures = new List<ReceptionItemNode>();
			IList<ReceptionItemNode> additionalLoadingNomenclatures = new List<ReceptionItemNode>();

			ReceptionItemNode returnableTerminal = null;
			int loadedTerminalAmount = default(int);

			var cashSubdivision = _subdivisionRepository.GetCashSubdivisions(uow);
			if(cashSubdivision.Contains(Warehouse.OwningSubdivision)) {
				
				loadedTerminalAmount = (int)_carLoadDocumentRepository.LoadedTerminalAmount(UoW, RouteList.Id, terminalId);

				var unloadedTerminalAmount = (int)_carUnloadRepository.UnloadedTerminalAmount(UoW, RouteList.Id, terminalId);

				if (loadedTerminalAmount > 0)
                {
					var terminal = UoW.GetById<Nomenclature>(terminalId);

					returnableTerminal = new ReceptionItemNode
					{
						NomenclatureId = terminal.Id,
						Name = terminal.Name,
						ExpectedAmount = loadedTerminalAmount - unloadedTerminalAmount
					};
                }
			}
			else
			{
				var pickUpSubquery = QueryOver.Of(() => routeListItemAlias)
					.JoinAlias(() => routeListItemAlias.Order, () => orderAlias)
					.JoinAlias(() => orderAlias.OrderEquipments, () => orderEquipmentAlias)
					.Where(() => routeListItemAlias.RouteList.Id == RouteList.Id)
					.And(() => orderEquipmentAlias.Direction == Domain.Orders.Direction.PickUp)
					.And(() => orderEquipmentAlias.Nomenclature.Id == freeBalanceOperationAlias.Nomenclature.Id)
					.Select(Projections.Property(() => orderEquipmentAlias.Nomenclature.Id));

				var carUnloadSubquery = QueryOver.Of(() => carUnloadDocumentItemAlias)
					.Where(x => x.DeliveryFreeBalanceOperation.Id == freeBalanceOperationAlias.Id)
					.Select(Projections.Property(() => carUnloadDocumentItemAlias.Id));

				returnable = UoW.Session.QueryOver(() => freeBalanceOperationAlias)
					.JoinAlias(() => freeBalanceOperationAlias.Nomenclature, () => nomenclatureAlias)
					.Where(f => f.RouteList.Id == RouteList.Id)
					.WithSubquery.WhereNotExists(pickUpSubquery)
					.WithSubquery.WhereNotExists(carUnloadSubquery)
					.SelectList(list => list
						.SelectGroup(() => nomenclatureAlias.Id).WithAlias(() => resultAlias.NomenclatureId)
						.Select(() => nomenclatureAlias.Name).WithAlias(() => resultAlias.Name)
						.Select(() => nomenclatureAlias.Category).WithAlias(() => resultAlias.NomenclatureCategory)
						.SelectSum(() => freeBalanceOperationAlias.Amount).WithAlias(() => resultAlias.ExpectedAmount)
					).TransformUsing(Transformers.AliasToBean<ReceptionItemNode>())
					.List<ReceptionItemNode>();

				orderItemsNomenclatures = UoW.Session.QueryOver(() => routeListItemAlias)
					.JoinAlias(() => routeListItemAlias.Order, () => orderAlias)
					.JoinAlias(() => orderAlias.OrderItems, () => orderItemAlias)
					.JoinAlias(() => orderItemAlias.Nomenclature, () => nomenclatureAlias)
					.Where(() => routeListItemAlias.RouteList.Id == RouteList.Id)
					.Where(() => nomenclatureAlias.Category != NomenclatureCategory.deposit)
					.Where(() => nomenclatureAlias.Category != NomenclatureCategory.service)
					.SelectList(list => list
						.SelectGroup(() => nomenclatureAlias.Id).WithAlias(() => resultAlias.NomenclatureId)
						.Select(() => nomenclatureAlias.Name).WithAlias(() => resultAlias.Name)
						.Select(() => nomenclatureAlias.Category).WithAlias(() => resultAlias.NomenclatureCategory)
					).TransformUsing(Transformers.AliasToBean<ReceptionItemNode>())
					.List<ReceptionItemNode>();

				orderEquipmentsNomenclatures = UoW.Session.QueryOver(() => routeListItemAlias)
					.JoinAlias(() => routeListItemAlias.Order, () => orderAlias)
					.JoinAlias(() => orderAlias.OrderEquipments, () => orderEquipmentAlias)
					.JoinAlias(() => orderEquipmentAlias.Nomenclature, () => nomenclatureAlias)
					.Where(() => routeListItemAlias.RouteList.Id == RouteList.Id)
					.Where(() => orderEquipmentAlias.Direction == Vodovoz.Domain.Orders.Direction.Deliver)
					.Where(Restrictions.Or(
							Restrictions.In(
								Projections.Property(() => orderAlias.OrderStatus),
								new[] { OrderStatus.DeliveryCanceled, OrderStatus.NotDelivered, OrderStatus.Canceled }),
							Restrictions.NotEqProperty(Projections.Property(() => orderEquipmentAlias.ActualCount), Projections.Property(() => orderEquipmentAlias.Count))
						)
					)
					.Where(() => nomenclatureAlias.Category != NomenclatureCategory.deposit)
					.SelectList(list => list
						.SelectGroup(() => nomenclatureAlias.Id).WithAlias(() => resultAlias.NomenclatureId)
						.Select(() => nomenclatureAlias.Name).WithAlias(() => resultAlias.Name)
						.Select(() => nomenclatureAlias.Category).WithAlias(() => resultAlias.NomenclatureCategory)
					).TransformUsing(Transformers.AliasToBean<ReceptionItemNode>())
					.List<ReceptionItemNode>();

				additionalLoadingNomenclatures = UoW.Session.QueryOver<RouteList>(() => routeListAlias)
					.JoinAlias(() => routeListAlias.AdditionalLoadingDocument, () => additionalLoadingDocumentAlias)
					.JoinAlias(() => additionalLoadingDocumentAlias.Items, () => additionalLoadingDocumentItemAlias)
					.JoinAlias(() => additionalLoadingDocumentItemAlias.Nomenclature, () => nomenclatureAlias)
					.Where(() => routeListAlias.Id == RouteList.Id)
					.SelectList(list => list
						.SelectGroup(() => nomenclatureAlias.Id).WithAlias(() => resultAlias.NomenclatureId)
						.Select(() => nomenclatureAlias.Name).WithAlias(() => resultAlias.Name)
						.Select(() => nomenclatureAlias.Category).WithAlias(() => resultAlias.NomenclatureCategory)
					)
					.TransformUsing(Transformers.AliasToBean<ReceptionItemNode>())
					.List<ReceptionItemNode>();
			}

			foreach(var item in returnable)
			{
				if(ReceptionReturnsList.All(i => i.NomenclatureId != item.NomenclatureId))
				{
					ReceptionReturnsList.Add(item);
				}
			}

			foreach(var item in orderItemsNomenclatures)
			{
				if(ReceptionReturnsList.All(i => i.NomenclatureId != item.NomenclatureId))
				{
					ReceptionReturnsList.Add(item);
				}
			}

			foreach(var item in orderEquipmentsNomenclatures)
			{
				if(ReceptionReturnsList.All(i => i.NomenclatureId != item.NomenclatureId))
				{
					ReceptionReturnsList.Add(item);
				}
			}

			foreach(var item in additionalLoadingNomenclatures)
			{
				if(ReceptionReturnsList.All(i => i.NomenclatureId != item.NomenclatureId))
				{
					ReceptionReturnsList.Add(item);
				}
			}

			if(returnableTerminal != null && loadedTerminalAmount > 0)
			{
				if(ReceptionReturnsList.All(i => i.NomenclatureId != returnableTerminal.NomenclatureId))
				{
					ReceptionReturnsList.Add(returnableTerminal);
				}
			}

			var defaultBottleNomenclature = _nomenclatureParametersProvider.GetDefaultBottleNomenclature(uow);

			if(ReceptionReturnsList.All(i => i.NomenclatureId != defaultBottleNomenclature.Id))
			{
				ReceptionReturnsList.Add(new ReceptionItemNode
				{
					NomenclatureId = defaultBottleNomenclature.Id,
					Name = defaultBottleNomenclature.Name,
					NomenclatureCategory = defaultBottleNomenclature.Category
				});
			}
		}
		
		protected void OnButtonAddNomenclatureClicked(object sender, EventArgs e)
		{
			var filter = new NomenclatureFilterViewModel();
			filter.AvailableCategories =
				Nomenclature.GetCategoriesForGoods()
					.Where(c => c != NomenclatureCategory.bottle && c != NomenclatureCategory.equipment)
					.ToArray();

			var nomenclatureJournalFactory = new NomenclatureJournalFactory();
			var journal = nomenclatureJournalFactory.CreateNomenclaturesJournalViewModel(filter, true);
			journal.OnEntitySelectedResult += Journal_OnEntitySelectedResult;

			if(_userHasOnlyAccessToWarehouseAndComplaints == null)
			{
				_userHasOnlyAccessToWarehouseAndComplaints =
					ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission(
						"user_have_access_only_to_warehouse_and_complaints")
					&& !ServicesConfig.CommonServices.UserService.GetCurrentUser().IsAdmin;
			}

			if(_userHasOnlyAccessToWarehouseAndComplaints.Value)
			{
				journal.HideButtons();
			}

			MyTab.TabParent.AddSlaveTab(MyTab, journal);
		}

		private void Journal_OnEntitySelectedResult(object sender, QS.Project.Journal.JournalSelectedNodesEventArgs e)
		{
			if(!e.SelectedNodes.Any())
			{
				return;
			}

			var nomenclatures = UoW.GetById<Nomenclature>(e.SelectedNodes.Select(x => x.Id));
			foreach(var nomenclature in nomenclatures)
			{
				if(Items.Any(x => x.NomenclatureId == nomenclature.Id))
					continue;
				ReceptionReturnsList.Add(new ReceptionItemNode(nomenclature, 0));
			}
		}
	}

	public class ReceptionItemNode : PropertyChangedBase
	{
		private decimal _amount;
		private decimal _expectedAmount;

		public NomenclatureCategory NomenclatureCategory { get; set; }
		public int NomenclatureId { get; set; }
		public string Name { get; set; }

		public virtual decimal Amount {
			get => _amount;
			set => SetField(ref _amount, value, () => Amount);
		}

		public virtual decimal ExpectedAmount {
			get => _expectedAmount;
			set => SetField(ref _expectedAmount, value, () => ExpectedAmount);
		}

		int equipmentId;
		[PropertyChangedAlso("Serial")]
		public int EquipmentId {
			get => equipmentId;
			set => SetField(ref equipmentId, value, () => EquipmentId);
		}

		[Display(Name = "№ кулера")]
		public string Redhead {
			get => CarUnloadDocumentItem.Redhead;
			set {
				if(value != CarUnloadDocumentItem.Redhead)
					CarUnloadDocumentItem.Redhead = value;
			}
		}

		ServiceClaim serviceClaim;

		public virtual ServiceClaim ServiceClaim {
			get => serviceClaim;
			set => SetField(ref serviceClaim, value, () => ServiceClaim);
		}

		public Equipment NewEquipment { get; set; }
		public bool Returned {
			get => Amount > 0;
			set => Amount = value ? 1 : 0;
		}

		GoodsAccountingOperation movementOperation = new GoodsAccountingOperation();

		public virtual GoodsAccountingOperation MovementOperation {
			get => movementOperation;
			set => SetField(ref movementOperation, value, () => MovementOperation);
		}

		public ReceptionItemNode(Nomenclature nomenclature, int amount)
		{
			Name = nomenclature.Name;
			NomenclatureId = nomenclature.Id;
			NomenclatureCategory = nomenclature.Category;
			_amount = amount;
		}

		public ReceptionItemNode(GoodsAccountingOperation movementOperation) : this(movementOperation.Nomenclature, (int)movementOperation.Amount)
		{
			this.movementOperation = movementOperation;
		}

		CarUnloadDocumentItem carUnloadDocumentItem = new CarUnloadDocumentItem();

		public virtual CarUnloadDocumentItem CarUnloadDocumentItem {
			get => carUnloadDocumentItem;
			set => SetField(ref carUnloadDocumentItem, value, () => CarUnloadDocumentItem);
		}

		public ReceptionItemNode(CarUnloadDocumentItem carUnloadDocumentItem) : this(carUnloadDocumentItem.GoodsAccountingOperation)
		{
			this.carUnloadDocumentItem = carUnloadDocumentItem;
		}

		public ReceptionItemNode() { }
	}
}

