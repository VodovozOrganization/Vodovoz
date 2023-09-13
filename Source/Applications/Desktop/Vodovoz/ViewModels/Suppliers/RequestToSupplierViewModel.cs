using QS.Commands;
using QS.DomainModel.NotifyChange;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Services;
using QS.Utilities;
using QS.ViewModels;
using QS.ViewModels.Extension;
using System;
using System.Linq;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Suppliers;
using Vodovoz.EntityRepositories;
using Vodovoz.EntityRepositories.Goods;
using Vodovoz.EntityRepositories.Suppliers;
using Vodovoz.Services;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Dialogs.Goods;
using Vodovoz.ViewModels.Journals.FilterViewModels.Goods;
using Vodovoz.ViewModels.Journals.JournalViewModels.Goods;
using VodovozInfrastructure.StringHandlers;

namespace Vodovoz.ViewModels.Suppliers
{
	public class RequestToSupplierViewModel : EntityTabViewModelBase<RequestToSupplier>, IAskSaveOnCloseViewModel
	{
		private readonly ISupplierPriceItemsRepository supplierPriceItemsRepository;
		private readonly INomenclatureRepository nomenclatureRepository;
		private readonly IUserRepository userRepository;
		private readonly IEmployeeService employeeService;
		private readonly IUnitOfWorkFactory unitOfWorkFactory;
		private readonly ICommonServices commonServices;
		private readonly ICounterpartyJournalFactory counterpartySelectorFactory;
		private readonly INomenclatureJournalFactory nomenclatureSelectorFactory;
		public event EventHandler ListContentChanged;

		public RequestToSupplierViewModel(
			IEntityUoWBuilder uoWBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			IEmployeeService employeeService,
			ISupplierPriceItemsRepository supplierPriceItemsRepository,
			ICounterpartyJournalFactory counterpartySelectorFactory,
			INomenclatureJournalFactory nomenclatureSelectorFactory,
			INomenclatureRepository nomenclatureRepository,
			IUserRepository userRepository) : base(uoWBuilder, unitOfWorkFactory, commonServices)
		{
			this.unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			this.commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
			this.employeeService = employeeService ?? throw new ArgumentNullException(nameof(employeeService));
			this.supplierPriceItemsRepository = supplierPriceItemsRepository ?? throw new ArgumentNullException(nameof(supplierPriceItemsRepository));
			this.nomenclatureRepository = nomenclatureRepository ?? throw new ArgumentNullException(nameof(nomenclatureRepository));
			this.userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
			this.counterpartySelectorFactory = counterpartySelectorFactory ?? throw new ArgumentNullException(nameof(counterpartySelectorFactory));
			this.nomenclatureSelectorFactory = nomenclatureSelectorFactory ?? throw new ArgumentNullException(nameof(nomenclatureSelectorFactory));
			
			CreateCommands();
			RefreshSuppliers();
			ConfigureEntityPropertyChanges();
			
			Entity.ObservableRequestingNomenclatureItems.ElementAdded += (aList, aIdx) => RefreshSuppliers();
			Entity.ObservableRequestingNomenclatureItems.ListContentChanged += (aList, aIdx) => RefreshSuppliers();
			Entity.ObservableRequestingNomenclatureItems.ElementRemoved += (aList, aIdx, aObject) => RefreshSuppliers();
			NotifyConfiguration.Instance.BatchSubscribeOnEntity<SupplierPriceItem>(PriceFromSupplierNotifyCriteria);
			NotifyConfiguration.Instance.BatchSubscribeOnEntity<NomenclaturePrice>(NomenclaturePriceNotifyCriteria);
			NotifyConfiguration.Instance.BatchSubscribeOnEntity<Counterparty>(CounterpartyNotifyCriteria);
			
			SetPermissions();
		}

		private void SetPermissions()
		{
			CanEdit = PermissionResult.CanUpdate || (PermissionResult.CanCreate && Entity.Id == 0);
			CanReadCounterparty = CommonServices.CurrentPermissionService.ValidateEntityPermission(typeof(Counterparty)).CanRead;
			CanReadNomenclature = CommonServices.CurrentPermissionService.ValidateEntityPermission(typeof(Nomenclature)).CanRead;
		}

		private bool CanReadCounterparty { get; set; }
		private bool CanReadNomenclature { get; set; }
		
		public bool CanEdit { get; private set; }

		public bool AskSaveOnClose => CanEdit;

		bool areNomenclatureNodesSelected;
		public bool AreNomenclatureNodesSelected {
			get => areNomenclatureNodesSelected;
			set => SetField(ref areNomenclatureNodesSelected, value);
		}

		bool areSupplierNodesSelected;
		public bool AreSupplierNodesSelected {
			get => areSupplierNodesSelected;
			set => SetField(ref areSupplierNodesSelected, value);
		}

		bool needRefresh;
		public bool NeedRefresh {
			get => needRefresh;
			set => SetField(ref needRefresh, value);
		}

		Employee currentEmployee;
		public Employee CurrentEmployee {
			get {
				if(currentEmployee == null)
					currentEmployee = employeeService.GetEmployeeForUser(UoW, UserService.CurrentUserId);
				return currentEmployee;
			}
		}

		string minimalTotalSumText;
		public string MinimalTotalSumText {
			get => minimalTotalSumText;
			set => SetField(ref minimalTotalSumText, value);
		}

		void PriceFromSupplierNotifyCriteria(EntityChangeEvent[] e)
		{
			var updatedPriceItems = e.Select(ev => ev.GetEntity<SupplierPriceItem>());
			var displayingNomenclatures = Entity.ObservableRequestingNomenclatureItems
												.Where(r => !r.Transfered)
												.Select(r => r.Nomenclature);
			foreach(var n in displayingNomenclatures) {
				if(updatedPriceItems.Select(p => p.NomenclatureToBuy.Id).Contains(n.Id)) {
					NeedRefresh = true;
					var response = AskQuestion(
						"Цены на некоторые ТМЦ, выбранные в список цен поставщиков, изменились.\nЖелаете обновить список?",
						"Обновить список цен поставщиков?"
					);
					if(response)
						RefreshCommand.Execute();
					return;
				}
			}
		}

		void NomenclaturePriceNotifyCriteria(EntityChangeEvent[] e)
		{
			var updatedPrices = e.Select(ev => ev.GetEntity<NomenclaturePrice>());
			var displayingPrices = Entity.ObservableRequestingNomenclatureItems
										 .Where(r => !r.Transfered)
										 .SelectMany(r => r.Nomenclature.NomenclaturePrice)
										 ;
			foreach(var n in displayingPrices) {
				if(updatedPrices.Select(p => p.Id).Contains(n.Id)) {
					NeedRefresh = true;
					var response = AskQuestion(
						"Цены продажи некоторых ТМЦ, выбранных в список номенклатур заявки поставщику, изменились.\nЖелаете обновить список?",
						"Обновить список цен продажи номенклатур?"
					);
					if(response)
						RefreshCommand.Execute();
					return;
				}
			}
		}

		void CounterpartyNotifyCriteria(EntityChangeEvent[] e)
		{
			var updatedCounterparties = e.Select(ev => ev.GetEntity<Counterparty>());
			var displayingSuppliers = Entity.ObservableRequestingNomenclatureItems
											.Where(r => !r.Transfered)
											.SelectMany(r => r.Children.OfType<SupplierNode>())
											;
			foreach(var s in displayingSuppliers) {
				if(updatedCounterparties.FirstOrDefault(c => c.Id == s.SupplierPriceItem.Supplier.Id)?.DelayDaysForProviders != s.SupplierPriceItem.Supplier.DelayDaysForProviders) {
					NeedRefresh = true;
					var response = AskQuestion(
						"Отсрочка у некоторых поставщиков из заявки изменилась.\nЖелаете обновить список?",
						"Обновить заявку?"
					);
					if(response)
						RefreshCommand.Execute();
					return;
				}
			}
		}

		public string GenerateDelayDaysString(ILevelingRequestNode n)
		{
			if(n is SupplierNode)
				return n.SupplierPriceItem.Supplier.DelayDaysForProviders > 0
					? $"{n.SupplierPriceItem.Supplier.DelayDaysForProviders} дн."
					: "Нет";
			return string.Empty;
		}

		void RefreshSuppliers()
		{
			Entity.RequestingNomenclaturesListRefresh(UoW, supplierPriceItemsRepository, Entity.SuppliersOrdering);
			MinimalTotalSumText = $"Минимальное ИТОГО: {Entity.MinimalTotalSum.ToShortCurrencyString()}";
			ListContentChanged?.Invoke(this, new EventArgs());
			NeedRefresh = false;
		}

		protected override bool BeforeValidation()
		{
			if(UoW.IsNew)
			{
				Entity.Creator = CurrentEmployee;
			}

			return base.BeforeValidation();
		}

		void ConfigureEntityPropertyChanges()
		{
			OnEntityPropertyChanged(
				RefreshCommand.Execute,
				e => e.WithDelayOnly
			);
		}

		#region Commands

		void CreateCommands()
		{
			CreateRefreshCommand();
			CreateAddRequestingNomenclatureCommand();
			CreateRemoveRequestingNomenclatureCommand();
			CreateTransferRequestingNomenclatureCommand();
			CreateOpenItemCommand();
		}

		#region RefreshCommand

		public DelegateCommand RefreshCommand { get; private set; }

		void CreateRefreshCommand()
		{
			RefreshCommand = new DelegateCommand(
				RefreshSuppliers,
				() => true
			);
		}

		#endregion RefreshCommand

		#region AddRequestingNomenclatureCommand

		public DelegateCommand AddRequestingNomenclatureCommand { get; private set; }

		void CreateAddRequestingNomenclatureCommand()
		{
			AddRequestingNomenclatureCommand = new DelegateCommand(
				() => {
					var existingNomenclatures = Entity.ObservableRequestingNomenclatureItems
													  .Where(i => !i.Transfered)
													  .Select(i => i.Nomenclature.Id)
													  .Distinct();
					var filter = new NomenclatureFilterViewModel() {
						HidenByDefault = true
					};
					NomenclaturesJournalViewModel journalViewModel = new NomenclaturesJournalViewModel(
						filter,
						QS.DomainModel.UoW.UnitOfWorkFactory.GetDefaultFactory,
						CommonServices,
						employeeService,
						nomenclatureSelectorFactory,
						counterpartySelectorFactory,
						nomenclatureRepository,
						userRepository
					) {
						SelectionMode = JournalSelectionMode.Single,
						ExcludingNomenclatureIds = existingNomenclatures.ToArray()
					};
					journalViewModel.OnEntitySelectedResult += (sender, e) => {
						var selectedNode = e.SelectedNodes.FirstOrDefault();
						if(selectedNode == null)
							return;
						Entity.ObservableRequestingNomenclatureItems.Add(
							new RequestToSupplierItem {
								Nomenclature = UoW.GetById<Nomenclature>(selectedNode.Id),
								RequestToSupplier = Entity
							}
						);
					};
					this.TabParent.AddSlaveTab(this, journalViewModel);
				}
			);
		}

		#endregion AddRequestingNomenclatureCommand

		#region RemoveRequestingNomenclatureCommand

		public DelegateCommand<ILevelingRequestNode[]> RemoveRequestingNomenclatureCommand { get; private set; }

		void CreateRemoveRequestingNomenclatureCommand()
		{
			RemoveRequestingNomenclatureCommand = new DelegateCommand<ILevelingRequestNode[]>(
				array => {
					foreach(var item in array)
						Entity.RemoveNomenclatureRequest(item.Nomenclature.Id);
				}
			);
		}

		#endregion RemoveRequestingNomenclatureCommand

		#region TransferRequestingNomenclatureCommand

		public DelegateCommand<ILevelingRequestNode[]> TransferRequestingNomenclatureCommand { get; private set; }

		void CreateTransferRequestingNomenclatureCommand()
		{
			TransferRequestingNomenclatureCommand = new DelegateCommand<ILevelingRequestNode[]>(
				array => {
					if(!SaveBeforeContinue())
						return;

					RequestToSupplierViewModel vm = new RequestToSupplierViewModel(
						EntityUoWBuilder.ForCreate(),
						unitOfWorkFactory,
						commonServices,
						employeeService,
						supplierPriceItemsRepository,
						counterpartySelectorFactory,
						nomenclatureSelectorFactory,
						nomenclatureRepository,
						userRepository
					);
					foreach(var item in array) {
						if(item is RequestToSupplierItem requestItem) {
							var newItem = new RequestToSupplierItem {
								Nomenclature = requestItem.Nomenclature,
								Quantity = requestItem.Quantity,
								RequestToSupplier = vm.Entity,
								TransferedFromItem = requestItem
							};

							vm.Entity.ObservableRequestingNomenclatureItems.Add(newItem);
							requestItem.Transfered = true;
						}
					}

					vm.EntitySaved += (sender, e) => RefreshSuppliers();
					this.TabParent.AddSlaveTab(this, vm);
				}
			);
		}

		#endregion TransferRequestingNomenclatureCommand

		#region OpenItemCommand

		public DelegateCommand<ILevelingRequestNode[]> OpenItemCommand { get; private set; }
		void CreateOpenItemCommand()
		{
			OpenItemCommand = new DelegateCommand<ILevelingRequestNode[]>(
				array => {
					var item = array.FirstOrDefault();
					if(item is RequestToSupplierItem requestItem) {
						var nom = requestItem.Nomenclature;
						this.TabParent.AddSlaveTab(this, new NomenclatureViewModel(EntityUoWBuilder.ForOpen(nom.Id),
							UnitOfWorkFactory, commonServices, employeeService, nomenclatureSelectorFactory,
							counterpartySelectorFactory, nomenclatureRepository, userRepository, new StringHandler()));
						return;
					}
					if(item is SupplierNode supplierItem) {
						var sup = supplierItem.SupplierPriceItem.Supplier;
						this.TabParent.AddSlaveTab(this, new CounterpartyDlg(sup));
						return;
					}
				},
				array => {
					if(array.Count() != 1)
						return false;

					if(AreNomenclatureNodesSelected && CanReadNomenclature)
						return true;

					if(AreSupplierNodesSelected && CanReadCounterparty)
						return true;

					return false;
				}
			);
		}

		#endregion OpenItemCommand

		#endregion Commands

		public override void Dispose()
		{
			NotifyConfiguration.Instance.UnsubscribeAll(this);
			base.Dispose();
		}
	}
}
