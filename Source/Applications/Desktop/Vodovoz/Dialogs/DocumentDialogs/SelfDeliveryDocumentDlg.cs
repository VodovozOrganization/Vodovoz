using Autofac;
using Gamma.ColumnConfig;
using Gamma.Utilities;
using Gtk;
using NLog;
using QS.Dialog;
using QS.Dialog.Gtk;
using QS.Dialog.GtkUI;
using QS.DomainModel.Entity.EntityPermissions.EntityExtendedPermission;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.Project.Services;
using QS.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Core.Domain.Warehouses;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Sale;
using Vodovoz.EntityRepositories.Cash;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.EntityRepositories.Goods;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.EntityRepositories.Operations;
using Vodovoz.EntityRepositories.Stock;
using Vodovoz.EntityRepositories.Store;
using Vodovoz.Extensions;
using Vodovoz.Factories;
using Vodovoz.PermissionExtensions;
using Vodovoz.Settings.Employee;
using Vodovoz.Settings.Nomenclature;
using Vodovoz.TempAdapters;
using Vodovoz.Tools.CallTasks;
using Vodovoz.Tools.Store;
using Vodovoz.ViewModels.Journals.FilterViewModels.Goods;
using Vodovoz.ViewModels.Journals.JournalNodes.Goods;
using Vodovoz.ViewModels.Journals.JournalViewModels.Goods;
using Vodovoz.ViewModels.TrueMark;
using Vodovoz.ViewModels.ViewModels.Documents.SelfDeliveryCodesScan;
using VodovozBusiness.Controllers;
using VodovozBusiness.Domain.Client.Specifications;

namespace Vodovoz
{
	public partial class SelfDeliveryDocumentDlg : EntityDialogBase<SelfDeliveryDocument>
	{
		private ILifetimeScope _lifetimeScope = Startup.AppDIContainer.BeginLifetimeScope();
		private static Logger logger = LogManager.GetCurrentClassLogger();

		private IEmployeeRepository _employeeRepository;
		private IStockRepository _stockRepository;
		private IBottlesRepository _bottlesRepository;
		private IStoreDocumentHelper _storeDocumentHelper;
		private INomenclatureRepository _nomenclatureRepository;
		private readonly IValidationContextFactory _validationContextFactory =  ScopeProvider.Scope.Resolve<IValidationContextFactory>();
		private IGenericRepository<FormalEdoRequest> _orderEdoRequestRepository;
		private readonly IInteractiveService _interactiveService = ServicesConfig.InteractiveService;
		private CodesScanViewModel _codesScanViewModel;
		private ValidationContext _validationContext;
		private ICounterpartyEdoAccountController _edoAccountController;
		public IList<StagingTrueMarkCode> _allScannedStagingCodes = new List<StagingTrueMarkCode>();

		private GenericObservableList<GoodsReceptionVMNode> GoodsReceptionList = new GenericObservableList<GoodsReceptionVMNode>();

		private GeoGroup _warehouseGeoGroup;

		public SelfDeliveryDocumentDlg()
		{
			Build();
			UoWGeneric = ServicesConfig.UnitOfWorkFactory.CreateWithNewRoot<SelfDeliveryDocument>();
			ResolveDependencies();

			Entity.AuthorId = _employeeRepository.GetEmployeeForCurrentUser(UoW)?.Id;
			if(Entity.AuthorId == null) {
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
			Build();
			UoWGeneric = ServicesConfig.UnitOfWorkFactory.CreateForRoot<SelfDeliveryDocument>(id);
			ResolveDependencies();

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

		private void ResolveDependencies()
		{
			_employeeRepository = _lifetimeScope.Resolve<IEmployeeRepository>();
			_stockRepository = _lifetimeScope.Resolve<IStockRepository>();
			_bottlesRepository = _lifetimeScope.Resolve<IBottlesRepository>();
			_storeDocumentHelper = _lifetimeScope.Resolve<IStoreDocumentHelper>();
			_nomenclatureRepository = _lifetimeScope.Resolve<INomenclatureRepository>();
			_orderEdoRequestRepository = _lifetimeScope.Resolve<IGenericRepository<FormalEdoRequest>>();
			_edoAccountController = _lifetimeScope.Resolve<ICounterpartyEdoAccountController>();
		}
		
		private void ConfigureValidationContext(IValidationContextFactory validationContextFactory)
		{
			_validationContext = validationContextFactory.CreateNewValidationContext(Entity);
			_validationContext.ServiceContainer.AddService(typeof(IUnitOfWork), UoW);
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
				.Adjustment(new Adjustment(0, 0, 9999, 1, 100, 0))
				.Editing(true)
				.AddColumn("Ожидаемое кол-во").AddNumericRenderer(node => node.ExpectedAmount)
				.AddColumn("Категория").AddTextRenderer(node => node.Category.GetEnumTitle())
				.AddColumn("Направление").AddTextRenderer(node => node.Direction != null ? node.Direction.GetEnumTitle() : "")
				.AddColumn("Принадлежность").AddEnumRenderer(node => node.OwnType, true, new Enum[] { OwnTypes.None })
				.AddSetter((c, n) => {
					c.Editable = false;
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
						case DirectionReason.TradeIn:
							c.Text = "По акции \"Трейд-Ин\"";
							break;
						case DirectionReason.ClientGift:
							c.Text = "Подарок от клиента";
							break;
						default:
							break;
					}
					c.Editable = false;
				})
				.AddColumn("")
				.Finish();
			yTreeOtherGoods.ColumnsConfig = goodsColumnsConfig;
			yTreeOtherGoods.ItemsDataSource = GoodsReceptionList;

			btnAddOtherGoods.Sensitive = false;

			var permmissionValidator =
				new EntityExtendedPermissionValidator(ServicesConfig.UnitOfWorkFactory, PermissionExtensionSingletonStore.GetInstance(), _employeeRepository);
			
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

				buttonSave.Sensitive = false;
			} else {
				Entity.CanEdit = true;
			}

			FillTrees();

			spnTareToReturn.ValueChanged += (sender, e) => HasChanges = true;
			GoodsReceptionList.ListContentChanged += (sender, e) => HasChanges = true;
			
			ybuttonScanCodes.Clicked +=	OnYbuttonScanCodesOnClicked;

			ConfigureValidationContext(_validationContextFactory);

			var buttonOpenOrderCodes = new Gamma.GtkWidgets.yButton();
			buttonOpenOrderCodes.CanFocus = true;
			buttonOpenOrderCodes.Name = "ybuttonOpenOrderCodes";
			buttonOpenOrderCodes.UseUnderline = true;
			buttonOpenOrderCodes.Label = Mono.Unix.Catalog.GetString("Просмотреть коды заказа");
			hbox5.Add(buttonOpenOrderCodes);
			var w8 = ((Box.BoxChild)(hbox5[buttonOpenOrderCodes]));
			w8.PackType = PackType.End;
			w8.Position = 4;
			w8.Expand = false;
			w8.Fill = false;
			buttonOpenOrderCodes.Show();
			buttonOpenOrderCodes.Clicked += OpenOrderCodesDialog;
		}

		private void OpenOrderCodesDialog(object sender, EventArgs e)
		{
			NavigationManager.OpenViewModel<OrderCodesViewModel, int>(null, Entity.Order.Id, OpenPageOptions.IgnoreHash);
		}

		private void OnYbuttonScanCodesOnClicked(object sender, EventArgs e)
		{
			if(Entity?.Order?.Client is null)
			{
				_interactiveService.ShowMessage(ImportanceLevel.Error, "Не выбран контрагент.");

				return;
			}

			var allowedReasonsForLeaving = new[] { ReasonForLeaving.ForOwnNeeds, ReasonForLeaving.Resale };

			if(!allowedReasonsForLeaving.Contains(Entity.Order.Client.ReasonForLeaving))
			{
				_interactiveService.ShowMessage(ImportanceLevel.Error,
					$"У контрагента выбрана неподходящая причина выбытия. Допустимы только:" +
					$"{string.Join(", ", allowedReasonsForLeaving.Select(x => x.GetEnumDisplayName()))}");

				return;
			}

			var isOrderEdoRequestExists =
					_orderEdoRequestRepository
					.Get(UoW, OrderEdoRequestSpecification.CreateForOrderId(Entity.Order.Id))
					.Any();

			if(isOrderEdoRequestExists)
			{
				_interactiveService.ShowMessage(ImportanceLevel.Error,
					"Заявка на отправку документов заказа по ЭДО уже создана. Изменение кодов запрещено");
				return;
			}

			_codesScanViewModel = NavigationManager.OpenViewModel<CodesScanViewModel>(null).ViewModel;
			_codesScanViewModel.Initialize(UoW, Entity, _allScannedStagingCodes);
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
						|| ( item.Direction == Core.Domain.Orders.Direction.PickUp)))
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

			var validator = ServicesConfig.ValidationService;
			if(!validator.Validate(Entity, _validationContext))
			{
				return false;
			}

			var reasonsForLeavingForScan = new[] { ReasonForLeaving.ForOwnNeeds, ReasonForLeaving.Resale };

			if(reasonsForLeavingForScan.Contains(Entity.Order.Client.ReasonForLeaving)
			   && (!(_codesScanViewModel?.IsAllCodesScanned ?? false))
			   && !_interactiveService.Question("Не все коды отсканированы. Уверены, что хотите сохранить отпуск самовывоза?"))
			{
				return false;
			}

			var addingProductCodesResult = AddProductCodesAndCheckIsAllCodesAddedIfNeed();

			if(addingProductCodesResult.IsFailure)
			{
				_interactiveService.ShowMessage(ImportanceLevel.Error, addingProductCodesResult.GetErrorsString());
				return false;
			}

			Entity.LastEditorId = _employeeRepository.GetEmployeeForCurrentUser(UoW)?.Id;
			Entity.LastEditedTime = DateTime.Now;
			if(Entity.LastEditorId == null) {
				MessageDialogHelper.RunErrorDialog("Ваш пользователь не привязан к действующему сотруднику, вы не можете изменять складские документы, так как некого указывать в качестве кладовщика.");
				return false;
			}

			Entity.UpdateOperations(UoW);
			Entity.UpdateReceptions(UoW, GoodsReceptionList, _nomenclatureRepository, _bottlesRepository);

			var employeeSettings = _lifetimeScope.Resolve<IEmployeeSettings>();
			var nomenclatureSettings = _lifetimeScope.Resolve<INomenclatureSettings>();
			var callTaskWorker = _lifetimeScope.Resolve<ICallTaskWorker>();
			var cashRepository = _lifetimeScope.Resolve<ICashRepository>();
			var routeListItemRepository = _lifetimeScope.Resolve<IRouteListItemRepository>();
			var selfDeliveryRepository = _lifetimeScope.Resolve<ISelfDeliveryRepository>();

			if(Entity.FullyShiped(UoW, nomenclatureSettings, routeListItemRepository, selfDeliveryRepository, cashRepository, callTaskWorker))
			{
				MessageDialogHelper.RunInfoDialog("Заказ отгружен полностью.");
			}

			var edoRequest = _codesScanViewModel?.CreateEdoRequest(UoW, Entity.Order);
			
			logger.Info("Сохраняем документ самовывоза...");
			UoWGeneric.Save();

			try
			{
				if(edoRequest != null)
				{
					_codesScanViewModel.SendEdoRequestCreatedEvent(edoRequest).GetAwaiter().GetResult();
				}
			}
			catch(Exception e)
			{
				logger.Error("Произошла ошибка при попытке отправки события создания заявки ЭДО {EdoSendError}", e);
			}
			
			//FIXME Необходимо проверить правильность этого кода, так как если заказ именялся то уведомление на его придет и без кода.
			//А если в каком то месте нужно получать уведомления об изменениях текущего объекта, то логично чтобы этот объект на него и подписался.
			//OrmMain.NotifyObjectUpdated(new object[] { Entity.Order });
			logger.Info("Ok.");
			return true;
		}

		private Result AddProductCodesAndCheckIsAllCodesAddedIfNeed()
		{
			var isAllTrueMarkProductCodesMustBeAdded =
				Entity.Order.IsNeedIndividualSetOnLoad(_edoAccountController)
				|| Entity.Order.IsOrderForResale
				|| Entity.Order.IsOrderForTender;

			if(_codesScanViewModel is null)
			{
				if(isAllTrueMarkProductCodesMustBeAdded)
				{
					return Result.Failure(Errors.TrueMark.TrueMarkCodeErrors.NotAllCodesAdded);
				}

				return Result.Success();
			}

			var addingCodesResult = AddProductCodesToSelfDeliveryDocumentItemAndDeleteStagingCodes(isAllTrueMarkProductCodesMustBeAdded);
			if(addingCodesResult.IsFailure)
			{
				return addingCodesResult;
			}

			if(isAllTrueMarkProductCodesMustBeAdded)
			{
				var isAllCodesAddedResult = _codesScanViewModel.IsAllTrueMarkProductCodesAdded();
				if(isAllCodesAddedResult.IsFailure)
				{
					return isAllCodesAddedResult;
				}
			}
			
			return Result.Success();
		}

		private Result AddProductCodesToSelfDeliveryDocumentItemAndDeleteStagingCodes(bool isAllTrueMarkProductCodesMustBeAdded = false)
		{
			if(_codesScanViewModel is null)
			{
				throw new InvalidOperationException("Невозможно добавить коды, так как окно сканирования кодов не открывалось.");
			}

			var addingCodesResult =
				_codesScanViewModel.AddProductCodesToSelfDeliveryDocumentItemAndDeleteStagingCodes(isAllTrueMarkProductCodesMustBeAdded)
				.GetAwaiter().GetResult();

			if(addingCodesResult.IsFailure)
			{
				return addingCodesResult;
			}

			return Result.Success();
		}

		private void UpdateWarehouseGeoGroup()
		{
			if(Entity.Warehouse == null)
			{
				_warehouseGeoGroup = null;
				return;
			}

			Subdivision parentSubdivision = null;

			if(Entity.Warehouse?.OwningSubdivisionId != null)
			{
				parentSubdivision = UoW.GetById<Subdivision>(Entity.Warehouse.OwningSubdivisionId.Value);
			}

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
				item.Amount = Math.Min(Entity.GetNomenclaturesCountInOrder(item.Nomenclature.Id) - item.AmountUnloaded, item.AmountInStock);
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
			(NavigationManager as ITdiCompatibilityNavigation)
				.OpenViewModelOnTdi<NomenclaturesJournalViewModel, Action<NomenclatureFilterViewModel>>(this, filter =>
				{
					filter.RestrictArchive = true;
					filter.AvailableCategories = Nomenclature.GetCategoriesForGoodsWithoutEmptyBottles();
				},
				OpenPageOptions.AsSlave,
				viewModel =>
				{
					viewModel.SelectionMode = JournalSelectionMode.Single;
					viewModel.OnSelectResult += NomenclatureSelectorOnEntitySelectedResult;
				});
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
				node.Direction = Core.Domain.Orders.Direction.PickUp;
			}

			if(!GoodsReceptionList.Any(n => n.NomenclatureId == node.NomenclatureId))
			{
				GoodsReceptionList.Add(node);
			}
		}

		public override void Destroy()
		{
			_employeeRepository = null;
			_stockRepository = null;
			_bottlesRepository = null;
			_storeDocumentHelper = null;
			_nomenclatureRepository = null;
			_lifetimeScope?.Dispose();
			_lifetimeScope = null;
			_codesScanViewModel = null;
			ybuttonScanCodes.Clicked -=	OnYbuttonScanCodesOnClicked;
			base.Destroy();
		}
	}
}
