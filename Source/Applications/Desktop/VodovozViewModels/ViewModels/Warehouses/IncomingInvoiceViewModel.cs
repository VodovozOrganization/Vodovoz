using Autofac;
using QS.Commands;
using QS.DomainModel.Entity.EntityPermissions.EntityExtendedPermission;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Project.Journal.EntitySelector;
using QS.Report;
using QS.Services;
using QS.ViewModels;
using QS.ViewModels.Control.EEVM;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Core.Domain.Warehouses;
using Vodovoz.Domain.Documents.IncomingInvoices;
using Vodovoz.Domain.Documents.MovementDocuments;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Permissions.Warehouses;
using Vodovoz.EntityRepositories.Stock;
using Vodovoz.EntityRepositories.Store;
using Vodovoz.Infrastructure.Print;
using Vodovoz.Infrastructure.Services;
using Vodovoz.Journals.JournalNodes;
using Vodovoz.Models;
using Vodovoz.PermissionExtensions;
using Vodovoz.PrintableDocuments;
using Vodovoz.Services;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Journals.FilterViewModels.Goods;
using Vodovoz.ViewModels.Journals.JournalNodes.Goods;
using Vodovoz.ViewModels.Journals.JournalViewModels.Goods;
using Vodovoz.ViewModels.Journals.JournalViewModels.Nomenclatures;
using Vodovoz.ViewModels.Journals.JournalViewModels.Store;
using Vodovoz.ViewModels.ViewModels.Goods;
using Order = Vodovoz.Domain.Orders.Order;

namespace Vodovoz.ViewModels.Warehouses
{
	public class IncomingInvoiceViewModel : EntityTabViewModelBase<IncomingInvoice>
	{
		private readonly bool _canEditRetroactively;
		private readonly IEmployeeService _employeeService;
		private readonly IOrderSelectorFactory _orderSelectorFactory;
		private readonly IRDLPreviewOpener _rdlPreviewOpener;
		private readonly INomenclaturePurchasePriceModel _nomenclaturePurchasePriceModel;
		private readonly IWarehousePermissionValidator _warehousePermissionValidator;
		private readonly IStockRepository _stockRepository;
		private bool _canDuplicateInstance;
		private Employee _currentEmployee;
		private IEnumerable<Warehouse> _allowedWarehousesFrom;
		private IncomingInvoiceItem _selectedItem;

		private DelegateCommand _deleteItemCommand;
		private DelegateCommand _addItemCommand;
		private DelegateCommand _fillFromOrdersCommand;
		private DelegateCommand _printCommand;
		private DelegateCommand _addInventoryInstanceCommand;
		private DelegateCommand _copyInventoryInstanceCommand;

		#region Конструктор

		public IncomingInvoiceViewModel(
			ILifetimeScope lifetimeScope,
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			IWarehousePermissionService warehousePermissionService,
			IEmployeeService employeeService,
			IEntityExtendedPermissionValidator entityExtendedPermissionValidator,
			IOrderSelectorFactory orderSelectorFactory,
			IWarehouseRepository warehouseRepository,
			IRDLPreviewOpener rdlPreviewOpener,
			ICommonServices commonServices,
			INomenclaturePurchasePriceModel nomenclaturePurchasePriceModel,
			IStockRepository stockRepository,
			INavigationManager navigationManager,
			ICounterpartyJournalFactory counterpartyJournalFactory,
			ViewModelEEVMBuilder<Warehouse> warehouseViewModelEEVMBuilder)
			: base(uowBuilder, unitOfWorkFactory, commonServices, navigationManager)
		{
			if(warehouseViewModelEEVMBuilder is null)
			{
				throw new ArgumentNullException(nameof(warehouseViewModelEEVMBuilder));
			}

			_employeeService = employeeService ?? throw new ArgumentNullException(nameof(employeeService));
			_orderSelectorFactory = orderSelectorFactory ?? throw new ArgumentNullException(nameof(orderSelectorFactory));

			if(warehouseRepository == null)
			{
				throw new ArgumentNullException(nameof(warehouseRepository));
			}

			_rdlPreviewOpener = rdlPreviewOpener ?? throw new ArgumentNullException(nameof(rdlPreviewOpener));
			_nomenclaturePurchasePriceModel =
				nomenclaturePurchasePriceModel ?? throw new ArgumentNullException(nameof(nomenclaturePurchasePriceModel));
			_stockRepository = stockRepository ?? throw new ArgumentNullException(nameof(stockRepository));
			CounterpartyAutocompleteSelectorFactory =
				(counterpartyJournalFactory ?? throw new ArgumentNullException(nameof(counterpartyJournalFactory)))
				.CreateCounterpartyAutocompleteSelectorFactory(lifetimeScope);

			if(navigationManager is null)
			{
				throw new ArgumentNullException(nameof(navigationManager));
			}

			_warehousePermissionValidator = warehousePermissionService.GetValidator();

			_canEditRetroactively =
				(entityExtendedPermissionValidator ?? throw new ArgumentNullException(nameof(entityExtendedPermissionValidator)))
				.Validate(typeof(MovementDocument), CommonServices.UserService.CurrentUserId, nameof(RetroactivelyClosePermission));
			ConfigureEntityChangingRelations();

			ValidationContext.ServiceContainer.AddService(typeof(IWarehouseRepository), warehouseRepository);
			UserHasOnlyAccessToWarehouseAndComplaints =
				CommonServices.CurrentPermissionService.ValidatePresetPermission(Vodovoz.Core.Domain.Permissions.UserPermissions.UserHaveAccessOnlyToWarehouseAndComplaints)
				&& !CurrentUser.IsAdmin;

			var instancePermissionResult =
				CommonServices.CurrentPermissionService.ValidateEntityPermission(typeof(InventoryNomenclatureInstance));
			_canDuplicateInstance = instancePermissionResult.CanUpdate || instancePermissionResult.CanCreate;

			Entity.ObservableItems.ListContentChanged += OnObservableItemsContentChanged;

			WarehouseViewModel = warehouseViewModelEEVMBuilder
				.SetUnitOfWork(UoW)
				.SetViewModel(this)
				.ForProperty(Entity, e => e.Warehouse)
				.UseViewModelJournalAndAutocompleter<WarehouseJournalViewModel>()
				.UseViewModelDialog<WarehouseViewModel>()
				.Finish();
		}

		#endregion

		#region Properties

		public bool UserHasOnlyAccessToWarehouseAndComplaints { get; }

		public bool CanEdit =>
			(UoW.IsNew && PermissionResult.CanCreate)
			|| (PermissionResult.CanUpdate)
			|| _canEditRetroactively;

		public bool CanAddItem => CanEdit && Entity.CanAddItem && Entity.Warehouse != null;
		public bool CanDeleteItems => CanEdit && Entity.CanDeleteItems;
		public bool CanFillFromOrders => CanEdit && Entity.CanAddItem && Entity.Warehouse != null;

		public string TotalSum => $"Итого: {Entity.TotalSum:N2} ₽";

		public IEnumerable<Warehouse> AllowedWarehousesFrom
		{
			get
			{
				if(_allowedWarehousesFrom == null)
				{
					ReloadAllowedWarehousesFrom();
				}
				return _allowedWarehousesFrom;
			}
		}

		public IEnumerable<Warehouse> Warehouses
		{
			get
			{
				var result = new List<Warehouse>(AllowedWarehousesFrom);
				if(Entity.Warehouse != null && !AllowedWarehousesFrom.Contains(Entity.Warehouse))
				{
					result.Add(Entity.Warehouse);
				}
				return result;
			}
		}

		public Employee CurrentEmployee => _currentEmployee ??
			(_currentEmployee = _employeeService.GetEmployeeForUser(UoW, CommonServices.UserService.CurrentUserId));

		public bool CanCreate => PermissionResult.CanCreate;
		public bool CanUpdate => PermissionResult.CanUpdate;
		public bool CanCreateOrUpdate => Entity.Id == 0 ? CanCreate : CanUpdate;
		public bool HasSelectedItem => SelectedItem != null;
		public bool CanDuplicateInstance => CanAddItem && SelectedItem is InventoryInstanceIncomingInvoiceItem;

		public IncomingInvoiceItem SelectedItem
		{
			get => _selectedItem;
			set
			{
				if(SetField(ref _selectedItem, value))
				{
					OnPropertyChanged(nameof(HasSelectedItem));
					OnPropertyChanged(nameof(CanDuplicateInstance));
				}
			}
		}

		#endregion

		#region Commands

		public DelegateCommand DeleteItemCommand
		{
			get
			{
				if(_deleteItemCommand == null)
				{
					_deleteItemCommand = new DelegateCommand(
						() =>
						{
							Entity.DeleteItem(SelectedItem);
							OnPropertyChanged(nameof(TotalSum));
						},
						() => CanDeleteItems && HasSelectedItem
					);
					_deleteItemCommand.CanExecuteChangedWith(this, x => x.CanDeleteItems);
				}
				return _deleteItemCommand;
			}
		}

		public DelegateCommand AddItemCommand
		{
			get
			{
				if(_addItemCommand == null)
				{
					_addItemCommand = new DelegateCommand(
						() =>
						{
							var alreadyAddedNomenclatures = Entity.Items
								.Where(x => x.Nomenclature != null && x.AccountingType == AccountingType.Bulk)
								.Select(x => x.EntityId).ToArray();

							var nomenclatureSelector =
								NavigationManager.OpenViewModel<NomenclaturesJournalViewModel, Action<NomenclatureFilterViewModel>>(
									this,
									filter =>
									{
										filter.RestrictArchive = true;
										filter.AvailableCategories = Nomenclature.GetCategoriesForGoods();
										filter.RestrictedExcludedIds = alreadyAddedNomenclatures;
									},
									OpenPageOptions.AsSlave,
									vm =>
									{
										vm.SelectionMode = JournalSelectionMode.Single;

										vm.OnSelectResult += (sender, e) =>
										{
											var selectedNodes = e.SelectedObjects.Cast<NomenclatureJournalNode>();
											if(!selectedNodes.Any())
											{
												return;
											}

											var selectedNomenclatures = UoW.GetById<Nomenclature>(
												selectedNodes.Select(x => x.Id)
											);

											foreach(var nomenclature in selectedNomenclatures)
											{
												Entity.AddItem(new NomenclatureIncomingInvoiceItem { Nomenclature = nomenclature, Amount = 1 });
												OnPropertyChanged(nameof(TotalSum));
											}
										};
									}
								).ViewModel;
						},
						() => CanAddItem
					);
					_addItemCommand.CanExecuteChangedWith(this, x => x.CanAddItem);
				}
				return _addItemCommand;
			}
		}

		public DelegateCommand FillFromOrdersCommand
		{
			get
			{
				if(_fillFromOrdersCommand == null)
				{
					_fillFromOrdersCommand = new DelegateCommand(
						() =>
						{
							bool IsOnlineStoreOrders = true;
							IEnumerable<OrderStatus> orderStatuses = new OrderStatus[] { OrderStatus.Accepted, OrderStatus.InTravelList, OrderStatus.OnLoading };
							var orderSelector = _orderSelectorFactory.CreateOrderSelectorForDocument(IsOnlineStoreOrders, orderStatuses);
							orderSelector.OnEntitySelectedResult += (sender, e) =>
							{
								IEnumerable<OrderForMovDocJournalNode> selectedNodes = e.SelectedNodes.Cast<OrderForMovDocJournalNode>();
								if(!selectedNodes.Any())
								{
									return;
								}
								var orders = UoW.GetById<Order>(selectedNodes.Select(x => x.Id));
								var orderItems = orders.SelectMany(x => x.OrderItems);
								var nomIds = orderItems.Where(x => x.Nomenclature != null).Select(x => x.Nomenclature.Id).ToList();

								var nomsAmount = new Dictionary<int, decimal>();
								if(nomIds != null && nomIds.Any())
								{
									nomIds = nomIds.Distinct().ToList();
									nomsAmount = _stockRepository.NomenclatureInStock(
										UoW,
										nomIds.ToArray(),
										new []{ Entity.Warehouse.Id });
								}
								//Если такие уже добавлены, то только увеличить их количество
								foreach(var item in orderItems)
								{
									var moveItem = Entity.Items.FirstOrDefault(
										x => x.AccountingType == AccountingType.Bulk
										&& x.EntityId == item.Nomenclature.Id);

									if(moveItem == null)
									{
										var count = item.Count > nomsAmount[item.Nomenclature.Id]
											? nomsAmount[item.Nomenclature.Id]
											: item.Count;
										if((count == 0) && item.Nomenclature.OnlineStore == null)
										{
											continue;
										}

										if(item.Nomenclature.Category == NomenclatureCategory.service
											|| item.Nomenclature.Category == NomenclatureCategory.master)
										{
											continue;
										}

										Entity.AddItem(new NomenclatureIncomingInvoiceItem
										{
											Nomenclature = item.Nomenclature,
											Amount = item.Count,
											PrimeCost = item.Price
										});
									}
									else
									{
										var count = (moveItem.Amount + item.Count) > nomsAmount[item.Nomenclature.Id] ?
											nomsAmount[item.Nomenclature.Id] :
											(moveItem.Amount + item.Count);
										if(count == 0)
										{
											continue;
										}

										moveItem.Amount = count;
									}
								}

								OnPropertyChanged(nameof(TotalSum));
							};
							TabParent.AddSlaveTab(this, orderSelector);
						},
						() => CanFillFromOrders
					);
					_fillFromOrdersCommand.CanExecuteChangedWith(this, x => x.CanFillFromOrders);
				}
				return _fillFromOrdersCommand;
			}
		}

		public DelegateCommand PrintCommand
		{
			get
			{
				if(_printCommand == null)
				{
					_printCommand = new DelegateCommand(
						() =>
						{
							int? currentEmployeeId = _employeeService.GetEmployeeForUser(UoW, UserService.CurrentUserId)?.Id;
							var doc = new IncomingInvoiceDocumentRDL(Entity, currentEmployeeId) { Title = Entity.Title };
							if(doc is IPrintableRDLDocument)
							{
								_rdlPreviewOpener.OpenRldDocument(typeof(IncomingInvoice), doc);
							}
						},
						() => true
					);

				}
				return _printCommand;
			}
		}

		public DelegateCommand AddInventoryInstanceCommand =>
			_addInventoryInstanceCommand ?? (_addInventoryInstanceCommand = new DelegateCommand(
				() =>
				{
					IPage<InventoryInstancesJournalViewModel> page = null;
					var excludedInventoryInstancesIds =
						Entity.ObservableItems.Where(x => x.AccountingType == AccountingType.Instance)
							.Select(x => x.EntityId).ToArray();
					if(excludedInventoryInstancesIds.Any())
					{
						page = NavigationManager
							.OpenViewModel<InventoryInstancesJournalViewModel, Action<InventoryInstancesJournalFilterViewModel>>(
								this,
								f =>
								{
									f.ExcludedInventoryInstancesIds = excludedInventoryInstancesIds;
									f.OnlyWithZeroBalance = true;
									f.RestrictShowArchive = false;
								},
								OpenPageOptions.AsSlave);
					}
					else
					{
						page = NavigationManager
							.OpenViewModel<InventoryInstancesJournalViewModel, Action<InventoryInstancesJournalFilterViewModel>>(
								this,
								f =>
								{
									f.OnlyWithZeroBalance = true;
									f.RestrictShowArchive = false;
								},
								OpenPageOptions.AsSlave);
					}

					page.ViewModel.SelectionMode = JournalSelectionMode.Multiple;
					page.ViewModel.OnSelectResult += OnInventoryInstanceSelectResult;
				}));

		public DelegateCommand CopyInventoryInstanceCommand =>
			_copyInventoryInstanceCommand ?? (_copyInventoryInstanceCommand = new DelegateCommand(
				() =>
				{
					if(!(SelectedItem is InventoryInstanceIncomingInvoiceItem inventoryInstanceItem))
					{
						return;
					}

					if(!_canDuplicateInstance)
					{
						ShowWarningMessage("У Вас нет прав на создание/редактирование экземпляров");
						return;
					}

					var page = NavigationManager.OpenViewModel<InventoryInstanceViewModel, IEntityUoWBuilder, Nomenclature>(
						this,
						EntityUoWBuilder.ForCreate(),
						inventoryInstanceItem.Nomenclature,
						OpenPageOptions.AsSlave);

					page.ViewModel.EntitySaved += (sender, args) =>
					{
						var savedEntity = UoW.GetById<InventoryNomenclatureInstance>(args.GetEntity<InventoryNomenclatureInstance>().Id);
						var newItem = new InventoryInstanceIncomingInvoiceItem
						{
							Amount = 1,
							Nomenclature = savedEntity.Nomenclature,
							InventoryNomenclatureInstance = savedEntity
						};

						Entity.AddItem(newItem);
					};
					OnPropertyChanged(nameof(TotalSum));
				}));

		public IEntityAutocompleteSelectorFactory CounterpartyAutocompleteSelectorFactory { get; }
		public IEntityEntryViewModel WarehouseViewModel { get; }

		#endregion Commands

		#region Functions

		private void ConfigureEntityChangingRelations()
		{
			SetPropertyChangeRelation(e => e.CanAddItem, () => CanAddItem, () => CanFillFromOrders);
			SetPropertyChangeRelation(e => e.CanDeleteItems, () => CanDeleteItems);

			SetPropertyChangeRelation(e => e.Warehouse, () => CanAddItem, () => CanFillFromOrders);
			SetPropertyChangeRelation(e => e.Warehouse, () => CanCreate);
		}

		private void ReloadAllowedWarehousesFrom()
		{
			var allowedWarehouses =
				_warehousePermissionValidator.GetAllowedWarehouses(
					UoW.IsNew
						? WarehousePermissionsType.IncomingInvoiceCreate
						: WarehousePermissionsType.IncomingInvoiceEdit, CurrentEmployee);
			_allowedWarehousesFrom = UoW.Session.QueryOver<Warehouse>()
				.Where(x => !x.IsArchive)
				.WhereRestrictionOn(x => x.Id).IsIn(allowedWarehouses.Select(x => x.Id).ToArray())
				.List();
			OnPropertyChanged(nameof(AllowedWarehousesFrom));
			OnPropertyChanged(nameof(Warehouses));
		}

		private void OnObservableItemsContentChanged(object sender, EventArgs e)
		{
			OnPropertyChanged(nameof(TotalSum));
		}

		public override bool Save(bool close)
		{
			if(!CanEdit)
			{
				return false;
			}

			if(UoW.IsNew)
			{
				Entity.AuthorId = CurrentEmployee?.Id;
				Entity.TimeStamp = DateTime.Now;
			}
			else
			{
				if(Entity.LastEditorId == null)
				{
					throw new InvalidOperationException("Ваш пользователь не привязан к действующему сотруднику, вы не можете изменять складские документы, так как некого указывать в качестве кладовщика.");
				}
			}

			CreatePurchasePrices();

			Entity.LastEditorId = CurrentEmployee?.Id;
			Entity.LastEditedTime = DateTime.Now;


			return base.Save(close);
		}

		private void CreatePurchasePrices()
		{
			foreach(var item in Entity.Items)
			{
				if(item.Nomenclature.UsingInGroupPriceSet)
				{
					continue;
				}

				if(item is InventoryInstanceIncomingInvoiceItem instanceItem)
				{
					instanceItem.InventoryNomenclatureInstance.PurchasePrice = instanceItem.PrimeCost;
					continue;
				}

				var canCreateNewPrice = _nomenclaturePurchasePriceModel.CanCreatePrice(item.Nomenclature, Entity.TimeStamp.Date, item.PrimeCost);
				if(!canCreateNewPrice)
				{
					continue;
				}
				var newPrice = _nomenclaturePurchasePriceModel.CreatePrice(item.Nomenclature, Entity.TimeStamp.Date, item.PrimeCost);
				UoW.Save(newPrice);
			}
		}

		private void OnInventoryInstanceSelectResult(object sender, JournalSelectedEventArgs e)
		{
			var selectedItems = e.GetSelectedObjects<InventoryInstancesJournalNode>();

			if(!selectedItems.Any())
			{
				return;
			}

			foreach(var instanceNode in selectedItems)
			{
				var newItem = new InventoryInstanceIncomingInvoiceItem
				{
					Amount = 1,
					Nomenclature = UoW.GetById<Nomenclature>(instanceNode.NomenclatureId),
					InventoryNomenclatureInstance = UoW.GetById<InventoryNomenclatureInstance>(instanceNode.Id)
				};
				Entity.AddItem(newItem);
			}

			OnPropertyChanged(nameof(TotalSum));
		}

		#endregion Functions
	}
}
