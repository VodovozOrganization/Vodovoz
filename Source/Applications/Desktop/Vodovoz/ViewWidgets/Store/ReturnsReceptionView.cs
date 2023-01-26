using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using NHibernate.SqlCommand;
using NHibernate.Transform;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Project.Dialogs;
using QS.Project.Services;
using QSOrmProject;
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
					.Digits(2)
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

		public IList<Equipment> AlreadyUnloadedEquipment;

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
			DeliveryFreeBalanceOperation freeBalanceOperation = null;
			Nomenclature equipmetnNomenclatureAlias = null;

			IList<ReceptionItemNode> returnableItems = new List<ReceptionItemNode>();
			IList<ReceptionItemNode> returnableEquipment = new List<ReceptionItemNode>();

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
				returnableItems = UoW.Session.QueryOver(() => freeBalanceOperation)
					.JoinAlias(() => freeBalanceOperation.Nomenclature, () => nomenclatureAlias)
					.WhereRestrictionOn(() => nomenclatureAlias.Category).Not.IsIn(new[] { NomenclatureCategory.deposit, NomenclatureCategory.service })
					.And(() => freeBalanceOperation.RouteList.Id == RouteList.Id)
					.SelectList(list => list
						.SelectGroup(() => nomenclatureAlias.Id).WithAlias(() => resultAlias.NomenclatureId)
						.Select(() => nomenclatureAlias.Name).WithAlias(() => resultAlias.Name)
						.Select(() => nomenclatureAlias.Category).WithAlias(() => resultAlias.NomenclatureCategory)
						.SelectSum(() => freeBalanceOperation.Amount).WithAlias(() => resultAlias.ExpectedAmount
						)
					).TransformUsing(Transformers.AliasToBean<ReceptionItemNode>())
					.List<ReceptionItemNode>();

				var equipmentSubquery = QueryOver.Of(() => routeListAlias)
					.JoinAlias(() => routeListAlias.Addresses, () => routeListItemAlias)
					.JoinAlias(() => routeListItemAlias.Order, () => orderAlias)
					.JoinAlias(() => orderAlias.OrderEquipments, () => orderEquipmentAlias)
					.JoinAlias(() => orderEquipmentAlias.Nomenclature, () => equipmetnNomenclatureAlias)
					.Where(Restrictions.Or(
							Restrictions.In(
								Projections.Property(() => orderAlias.OrderStatus),
								new[] { OrderStatus.DeliveryCanceled, OrderStatus.NotDelivered, OrderStatus.Canceled }),
							Restrictions.NotEqProperty(Projections.Property(() => orderEquipmentAlias.ActualCount),
								Projections.Property(() => orderEquipmentAlias.Count))
							)
					)
					.And(() => equipmetnNomenclatureAlias.Category != NomenclatureCategory.deposit)
					.And(() => orderEquipmentAlias.Direction == Vodovoz.Domain.Orders.Direction.Deliver)
					.And(() => routeListAlias.Id == freeBalanceOperation.RouteList.Id)
					.And(() => equipmetnNomenclatureAlias.Id == nomenclatureAlias.Id)
					.Select(Projections.Property(() => equipmetnNomenclatureAlias.Id));

				returnableEquipment = UoW.Session.QueryOver(() => freeBalanceOperation)
					.JoinAlias(() => freeBalanceOperation.Nomenclature, () => nomenclatureAlias)
					.Where(()=> freeBalanceOperation.RouteList.Id == RouteList.Id)
					.WithSubquery.WhereExists(equipmentSubquery)
					.SelectList(list => list
						.SelectGroup(() => nomenclatureAlias.Id).WithAlias(() => resultAlias.NomenclatureId)
						.Select(() => nomenclatureAlias.Name).WithAlias(() => resultAlias.Name)
						.Select(() => nomenclatureAlias.Category).WithAlias(() => resultAlias.NomenclatureCategory)
						.SelectSum(() => freeBalanceOperation.Amount).WithAlias(() => resultAlias.ExpectedAmount
						)
					).TransformUsing(Transformers.AliasToBean<ReceptionItemNode>())
					.List<ReceptionItemNode>();
			}

			foreach(var item in returnableItems) {
				if(ReceptionReturnsList.All(i => i.NomenclatureId != item.NomenclatureId))
					ReceptionReturnsList.Add(item);
			}

			foreach(var item in returnableEquipment) {
				if(ReceptionReturnsList.All(i => i.NomenclatureId != item.NomenclatureId))
					ReceptionReturnsList.Add(item);
			}

			if (returnableTerminal != null && loadedTerminalAmount > 0) {
				if (ReceptionReturnsList.All(i => i.NomenclatureId != returnableTerminal.NomenclatureId))
					ReceptionReturnsList.Add(returnableTerminal);
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
					&& !ServicesConfig.CommonServices.UserService.GetCurrentUser(UoW).IsAdmin;
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
		public NomenclatureCategory NomenclatureCategory { get; set; }
		public int NomenclatureId { get; set; }
		public string Name { get; set; }

		decimal amount;
		public virtual decimal Amount {
			get => amount;
			set => SetField(ref amount, value, () => Amount);
		}

		decimal expectedAmount;
		public virtual decimal ExpectedAmount {
			get => expectedAmount;
			set => SetField(ref expectedAmount, value, () => ExpectedAmount);
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

		WarehouseMovementOperation movementOperation = new WarehouseMovementOperation();

		public virtual WarehouseMovementOperation MovementOperation {
			get => movementOperation;
			set => SetField(ref movementOperation, value, () => MovementOperation);
		}

		public ReceptionItemNode(Nomenclature nomenclature, int amount)
		{
			Name = nomenclature.Name;
			NomenclatureId = nomenclature.Id;
			NomenclatureCategory = nomenclature.Category;
			this.amount = amount;
		}

		public ReceptionItemNode(WarehouseMovementOperation movementOperation) : this(movementOperation.Nomenclature, (int)movementOperation.Amount)
		{
			this.movementOperation = movementOperation;
		}

		CarUnloadDocumentItem carUnloadDocumentItem = new CarUnloadDocumentItem();

		public virtual CarUnloadDocumentItem CarUnloadDocumentItem {
			get => carUnloadDocumentItem;
			set => SetField(ref carUnloadDocumentItem, value, () => CarUnloadDocumentItem);
		}

		public ReceptionItemNode(CarUnloadDocumentItem carUnloadDocumentItem) : this(carUnloadDocumentItem.WarehouseMovementOperation)
		{
			this.carUnloadDocumentItem = carUnloadDocumentItem;
		}

		public ReceptionItemNode() { }

		[Display(Name = "Цена")]
		public virtual decimal PrimeCost {
			get => MovementOperation.PrimeCost;
			set {
				if(value != MovementOperation.PrimeCost)
					MovementOperation.PrimeCost = value;
			}
		}

		public virtual decimal Sum => PrimeCost * Amount;
	}
}

