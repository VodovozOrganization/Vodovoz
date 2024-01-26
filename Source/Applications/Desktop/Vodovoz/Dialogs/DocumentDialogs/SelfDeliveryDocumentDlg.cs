using Autofac;
using Gamma.ColumnConfig;
using Gamma.Utilities;
using QS.Dialog.GtkUI;
using QS.DomainModel.Entity.EntityPermissions.EntityExtendedPermission;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.Project.Services;
using QS.Services;
using QS.Validation;
using System;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Vodovoz.Core.DataService;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Permissions.Warehouses;
using Vodovoz.Domain.Sale;
using Vodovoz.EntityRepositories.CallTasks;
using Vodovoz.EntityRepositories.Cash;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.EntityRepositories.Goods;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.EntityRepositories.Operations;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.EntityRepositories.Stock;
using Vodovoz.EntityRepositories.Store;
using Vodovoz.Parameters;
using Vodovoz.PermissionExtensions;
using Vodovoz.Services;
using Vodovoz.TempAdapters;
using Vodovoz.Tools;
using Vodovoz.Tools.CallTasks;
using Vodovoz.Tools.Store;
using Vodovoz.ViewModels.Journals.FilterViewModels.Goods;
using Vodovoz.ViewModels.Journals.JournalNodes.Goods;
using Vodovoz.ViewModels.Journals.JournalViewModels.Goods;

namespace Vodovoz
{
	public partial class SelfDeliveryDocumentDlg : QS.Dialog.Gtk.EntityDialogBase<SelfDeliveryDocument>
	{
		private ILifetimeScope _lifetimeScope = Startup.AppDIContainer.BeginLifetimeScope();
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
		private readonly IEmployeeRepository _employeeRepository = new EmployeeRepository();
		private readonly IStockRepository _stockRepository = new StockRepository();
		private readonly BottlesRepository _bottlesRepository = new BottlesRepository();
		private readonly StoreDocumentHelper _storeDocumentHelper = new StoreDocumentHelper(new UserSettingsGetter());

		private readonly INomenclatureRepository _nomenclatureRepository =
			new NomenclatureRepository(new NomenclatureParametersProvider(new ParametersProvider()));
		private GenericObservableList<GoodsReceptionVMNode> GoodsReceptionList = new GenericObservableList<GoodsReceptionVMNode>();

		private GeoGroup _warehouseGeoGroup;

		public SelfDeliveryDocumentDlg()
		{
			Build();

			UoWGeneric = UnitOfWorkFactory.CreateWithNewRoot<SelfDeliveryDocument>();

			Entity.Author = _employeeRepository.GetEmployeeForCurrentUser(UoW);
			if(Entity.Author == null) {
				MessageDialogHelper.RunErrorDialog("Ваш пользователь не привязан к действующему сотруднику, вы не можете создавать складские документы, так как некого указывать в качестве кладовщика.");
				FailInitialize = true;
				return;
			}

			Entity.Warehouse = _storeDocumentHelper.GetDefaultWarehouse(UoW, WarehousePermissionsType.SelfDeliveryEdit);
			var validationResult = CheckPermission();
			if(!validationResult.CanRead) {
				MessageDialogHelper.RunErrorDialog("Нет прав для доступа к документу отпуска самовывоза");
				FailInitialize = true;
				return;
			}

			if(!validationResult.CanCreate) {
				MessageDialogHelper.RunErrorDialog("Нет прав для создания документа отпуска самовывоза");
				FailInitialize = true;
				return;
			}

			canEditDocument = true;
			ConfigureDlg();
		}

		public SelfDeliveryDocumentDlg(int id)
		{
			this.Build();
			UoWGeneric = UnitOfWorkFactory.CreateForRoot<SelfDeliveryDocument>(id);
			var validationResult = CheckPermission();
			if(!validationResult.CanRead) {
				MessageDialogHelper.RunErrorDialog("Нет прав для доступа к документу отпуска самовывоза");
				FailInitialize = true;
				return;
			}
			canEditDocument = validationResult.CanUpdate;

			ConfigureDlg();
		}

		public SelfDeliveryDocumentDlg(SelfDeliveryDocument sub) : this(sub.Id)
		{
		}

		public INavigationManager NavigationManager { get; } = Startup.MainWin.NavigationManager;

		private IPermissionResult CheckPermission()
		{
			IPermissionService permissionService = ServicesConfig.CommonServices.PermissionService;
			return permissionService.ValidateUserPermission(typeof(SelfDeliveryDocument), ServicesConfig.UserService.CurrentUserId);
		}

		private bool canEditDocument;

		private void ConfigureDlg()
		{
			if(_storeDocumentHelper.CheckAllPermissions(UoW.IsNew, WarehousePermissionsType.SelfDeliveryEdit, Entity.Warehouse)) {
				FailInitialize = true;
				return;
			}

			Entity.InitializeDefaultValues(UoW, _nomenclatureRepository);
			vbxMain.Sensitive = canEditDocument;
			buttonCancel.Sensitive = true;

			var editing = _storeDocumentHelper.CanEditDocument(WarehousePermissionsType.SelfDeliveryEdit, Entity.Warehouse);
			evmeOrder.IsEditable = lstWarehouse.Sensitive = ytextviewCommnet.Editable = editing && canEditDocument;
			selfdeliverydocumentitemsview1.Sensitive = hbxTareToReturn.Sensitive = editing && canEditDocument;

			ylabelDate.Binding.AddFuncBinding(Entity, e => e.TimeStamp.ToString("g"), w => w.LabelProp).InitializeFromSource();
			lstWarehouse.ItemsList = _storeDocumentHelper.GetRestrictedWarehousesList(UoW, WarehousePermissionsType.SelfDeliveryEdit);
			lstWarehouse.Binding.AddBinding(Entity, e => e.Warehouse, w => w.SelectedItem).InitializeFromSource();
			lstWarehouse.ItemSelected += OnWarehouseSelected;
			ytextviewCommnet.Binding.AddBinding(Entity, e => e.Comment, w => w.Buffer.Text).InitializeFromSource();
			var orderFactory = _lifetimeScope.Resolve<IOrderSelectorFactory>();
			evmeOrder.SetEntityAutocompleteSelectorFactory(orderFactory.CreateSelfDeliveryDocumentOrderAutocompleteSelector(() => _warehouseGeoGroup));
			evmeOrder.Binding.AddBinding(Entity, e => e.Order, w => w.Subject).InitializeFromSource();
			evmeOrder.CanEditReference = ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("can_delete");
			evmeOrder.ChangedByUser += (sender, e) => 
			{
				UpdateOrderInfo();
				Entity.FillByOrder();
				Entity.UpdateStockAmount(UoW, _stockRepository);
				Entity.UpdateAlreadyUnloaded(UoW, _nomenclatureRepository, _bottlesRepository);
				UpdateAmounts();
				FillTrees(); 
			};

			UpdateOrderInfo();
			Entity.UpdateStockAmount(UoW, _stockRepository);
			Entity.UpdateAlreadyUnloaded(UoW, _nomenclatureRepository, _bottlesRepository);
			selfdeliverydocumentitemsview1.DocumentUoW = UoWGeneric;
			//bottlereceptionview1.UoW = UoW;
			UpdateWidgets();
			lblTareReturnedBefore.Binding.AddFuncBinding(Entity, e => e.ReturnedTareBeforeText, w => w.Text).InitializeFromSource();
			spnTareToReturn.Binding.AddBinding(Entity, e => e.TareToReturn, w => w.ValueAsInt).InitializeFromSource();

			IColumnsConfig goodsColumnsConfig = FluentColumnsConfig<GoodsReceptionVMNode>.Create()
				.AddColumn("Номенклатура").AddTextRenderer(node => node.Name)
				.AddColumn("Кол-во").AddNumericRenderer(node => node.Amount)
				.Adjustment(new Gtk.Adjustment(0, 0, 9999, 1, 100, 0))
				.Editing(true)
				.AddColumn("Ожидаемое кол-во").AddNumericRenderer(node => node.ExpectedAmount)
				.AddColumn("Категория").AddTextRenderer(node => node.Category.GetEnumTitle())
				.AddColumn("Направление").AddTextRenderer(node => node.Direction != null ? node.Direction.GetEnumTitle() : "")
				.AddColumn("Принадлежность").AddEnumRenderer(node => node.OwnType, true, new Enum[] { OwnTypes.None })
				.AddSetter((c, n) => {
					c.Editable = false;
					c.Editable = n.Category == NomenclatureCategory.equipment;
				})
				.AddColumn("Причина").AddEnumRenderer(
					node => node.DirectionReason
					,true
				).AddSetter((c, n) =>
				{
					switch (n.DirectionReason)
					{
						case DirectionReason.Rent:
							c.Text = "Закрытие аренды";
							break;
						case DirectionReason.Repair:
							c.Text = "В ремонт";
							break;
						case DirectionReason.Cleaning:
							c.Text = "На санобработку";
							break;
						case DirectionReason.RepairAndCleaning:
							c.Text = "В ремонт и санобработку";
							break;
						default:
							break;
					}
					c.Editable = false;
					c.Editable = n.Category == NomenclatureCategory.equipment;
				})


				.AddColumn("")
				.Finish();
			yTreeOtherGoods.ColumnsConfig = goodsColumnsConfig;
			yTreeOtherGoods.ItemsDataSource = GoodsReceptionList;

			var permmissionValidator =
				new EntityExtendedPermissionValidator(PermissionExtensionSingletonStore.GetInstance(), _employeeRepository);
			
			Entity.CanEdit =
				permmissionValidator.Validate(
					typeof(SelfDeliveryDocument), ServicesConfig.UserService.CurrentUserId, nameof(RetroactivelyClosePermission));
			
			if(!Entity.CanEdit && Entity.TimeStamp.Date != DateTime.Now.Date) {
				yTreeOtherGoods.Binding.AddBinding(Entity, e => e.CanEdit, w => w.Sensitive).InitializeFromSource();
				evmeOrder.Binding.AddBinding(Entity, e => e.CanEdit, w => w.Sensitive).InitializeFromSource();
				ytextviewCommnet.Binding.AddBinding(Entity, e => e.CanEdit, w => w.Sensitive).InitializeFromSource();
				ytextviewOrderInfo.Binding.AddBinding(Entity, e => e.CanEdit, w => w.Sensitive).InitializeFromSource();
				lstWarehouse.Sensitive = false;
				selfdeliverydocumentitemsview1.Sensitive = false;
				spnTareToReturn.Sensitive = false;
				btnAddOtherGoods.Sensitive = false;

				buttonSave.Sensitive = false;
			} else {
				Entity.CanEdit = true;
			}

			FillTrees();

			spnTareToReturn.ValueChanged += (sender, e) => HasChanges = true;
			GoodsReceptionList.ListContentChanged += (sender, e) => HasChanges = true;
		}

		private void FillTrees()
		{
			if(Entity.ReturnedItems.Any())
			{
				LoadReturned();
			}
		}

		private void LoadReturned()
		{
			GoodsReceptionList.Clear();
			foreach(var item in Entity.ReturnedItems) {
				if((item.Nomenclature.Category != NomenclatureCategory.bottle) 
					&& ((item.Nomenclature.Category != NomenclatureCategory.equipment) 
						|| ( item.Direction == Domain.Orders.Direction.PickUp)))
					GoodsReceptionList.Add(new GoodsReceptionVMNode {
						NomenclatureId = item.Nomenclature.Id,
						Name = item.Nomenclature.Name,
						Category = item.Nomenclature.Category,
						Amount = item.Amount,
						ExpectedAmount = (int)item.Amount,
						Direction = item.Direction,
						DirectionReason = item.DirectionReason,
						OwnType = item.OwnType
					});
			}
		}

		public override bool Save()
		{
			if(!Entity.CanEdit)
				return false;

			var validator = new ObjectValidator(new GtkValidationViewFactory());
			if(!validator.Validate(Entity))
			{
				return false;
			}

			Entity.LastEditor = _employeeRepository.GetEmployeeForCurrentUser(UoW);
			Entity.LastEditedTime = DateTime.Now;
			if(Entity.LastEditor == null) {
				MessageDialogHelper.RunErrorDialog("Ваш пользователь не привязан к действующему сотруднику, вы не можете изменять складские документы, так как некого указывать в качестве кладовщика.");
				return false;
			}

			Entity.UpdateOperations(UoW);
			Entity.UpdateReceptions(UoW, GoodsReceptionList, _nomenclatureRepository, _bottlesRepository);

			IStandartNomenclatures standartNomenclatures = new BaseParametersProvider(new ParametersProvider());
			var callTaskWorker = new CallTaskWorker(
						CallTaskSingletonFactory.GetInstance(),
						new CallTaskRepository(),
						new OrderRepository(),
						_employeeRepository,
						new BaseParametersProvider(new ParametersProvider()),
						ServicesConfig.CommonServices.UserService,
						ErrorReporter.Instance);
			if(Entity.FullyShiped(UoW, standartNomenclatures, new RouteListItemRepository(), new SelfDeliveryRepository(), new CashRepository(), callTaskWorker))
				MessageDialogHelper.RunInfoDialog("Заказ отгружен полностью.");

			logger.Info("Сохраняем документ самовывоза...");
			UoWGeneric.Save();
			//FIXME Необходимо проверить правильность этого кода, так как если заказ именялся то уведомление на его придет и без кода.
			//А если в каком то месте нужно получать уведомления об изменениях текущего объекта, то логично чтобы этот объект на него и подписался.
			//OrmMain.NotifyObjectUpdated(new object[] { Entity.Order });
			logger.Info("Ok.");
			return true;
		}

		private void UpdateWarehouseGeoGroup()
		{
			if(Entity.Warehouse == null)
			{
				_warehouseGeoGroup = null;
				return;
			}

			var parentSubdivision = Entity.Warehouse?.OwningSubdivision;
			var geoGroup = parentSubdivision?.GeographicGroup;

			while(geoGroup == null && parentSubdivision != null )
			{
				parentSubdivision = parentSubdivision.ParentSubdivision;
				geoGroup = parentSubdivision?.GeographicGroup;
			}

			_warehouseGeoGroup = geoGroup;
		}

		private void UpdateOrderInfo()
		{
			if(Entity.Order == null) {
				ytextviewOrderInfo.Buffer.Text = String.Empty;
				return;
			}

			string text = String.Format("Клиент: {0}\nБутылей на возврат: {1}\nАвтор заказа:{2}",
							  Entity.Order.Client.Name,
							  Entity.Order.BottlesReturn,
							  Entity.Order.Author?.ShortName
						  );
			ytextviewOrderInfo.Buffer.Text = text;
		}

		protected void OnWarehouseSelected(object sender, EventArgs e)
		{
			Entity.FillByOrder();
			Entity.UpdateStockAmount(UoW, _stockRepository);
			Entity.UpdateAlreadyUnloaded(UoW, _nomenclatureRepository, _bottlesRepository);
			UpdateAmounts();
			UpdateWidgets();
			FillTrees();
			UpdateWarehouseGeoGroup();
		}

		private void UpdateAmounts()
		{
			foreach(var item in Entity.Items)
				item.Amount = Math.Min(Entity.GetNomenclaturesCountInOrder(item.Nomenclature) - item.AmountUnloaded, item.AmountInStock);
		}

		private void UpdateWidgets()
		{
			bool bottles = Entity.Warehouse != null && Entity.Warehouse.CanReceiveBottles;
			bool goods = Entity.Warehouse != null && Entity.Warehouse.CanReceiveEquipment;
			hbxTareToReturn.Visible = bottles;
			vBoxOtherGoods.Visible = goods;
		}

		protected void OnBtnAddOtherGoodsClicked(object sender, EventArgs e)
		{
			var page = (NavigationManager as ITdiCompatibilityNavigation)
				.OpenViewModelOnTdi<NomenclaturesJournalViewModel, Action<NomenclatureFilterViewModel>>(this, filter =>
				{
					filter.RestrictArchive = true;
					filter.AvailableCategories = Nomenclature.GetCategoriesForGoodsWithoutEmptyBottles();
				},
				OpenPageOptions.AsSlave,
				viewModel =>
				{
					viewModel.SelectionMode = JournalSelectionMode.Single;
				});

			page.ViewModel.OnSelectResult += NomenclatureSelectorOnEntitySelectedResult;
		}

		private void NomenclatureSelectorOnEntitySelectedResult(object sender, JournalSelectedEventArgs e)
		{
			var nomenclatureNode = e.SelectedObjects.Cast<NomenclatureJournalNode>().FirstOrDefault();
			
			if(nomenclatureNode == null)
			{
				return;
			}

			var nomenclature = UoW.GetById<Nomenclature>(nomenclatureNode.Id);
			
			var node = new GoodsReceptionVMNode
			{
				Category = nomenclature.Category,
				NomenclatureId = nomenclature.Id,
				Name = nomenclature.Name
			};

			if(node.Category == NomenclatureCategory.equipment)
			{
				node.Direction = Domain.Orders.Direction.PickUp;
			}

			if(!GoodsReceptionList.Any(n => n.NomenclatureId == node.NomenclatureId))
			{
				GoodsReceptionList.Add(node);
			}
		}

		public override void Destroy()
		{
			_lifetimeScope?.Dispose();
			_lifetimeScope = null;
			base.Destroy();
		}
	}
}
