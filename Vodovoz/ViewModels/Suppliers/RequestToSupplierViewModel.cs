using System;
using System.Linq;
using QS.Commands;
using QS.DomainModel.Entity;
using QS.DomainModel.NotifyChange;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Services;
using QS.Utilities;
using QS.ViewModels;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Suppliers;
using Vodovoz.EntityRepositories.Suppliers;
using Vodovoz.FilterViewModels.Goods;
using Vodovoz.Infrastructure.Services;
using Vodovoz.JournalViewModels;

namespace Vodovoz.ViewModels.Suppliers
{
	public class RequestToSupplierViewModel : EntityTabViewModelBase<RequestToSupplier>
	{
		readonly ISupplierPriceItemsRepository supplierPriceItemsRepository;
		readonly IEmployeeService employeeService;
		readonly ICommonServices commonServices;
		public event EventHandler ListContentChanged;

		public RequestToSupplierViewModel(
			IEntityConstructorParam ctorParam,
			ICommonServices commonServices,
			IEmployeeService employeeService,
			ISupplierPriceItemsRepository supplierPriceItemsRepository
		) : base(ctorParam, commonServices)
		{
			this.commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
			this.employeeService = employeeService ?? throw new ArgumentNullException(nameof(employeeService));
			this.supplierPriceItemsRepository = supplierPriceItemsRepository ?? throw new ArgumentNullException(nameof(supplierPriceItemsRepository));
			CreateCommands();
			RefreshSuppliers();
			Entity.ObservableRequestingNomenclatureItems.ElementAdded += (aList, aIdx) => RefreshSuppliers();
			Entity.ObservableRequestingNomenclatureItems.ListContentChanged += (aList, aIdx) => RefreshSuppliers();
			Entity.ObservableRequestingNomenclatureItems.ElementRemoved += (aList, aIdx, aObject) => RefreshSuppliers();
			NotifyConfiguration.Instance.BatchSubscribeOnEntity<SupplierPriceItem>(NotifyCriteria);
		}

		bool canEdit = true;
		public bool CanEdit {
			get => canEdit;
			set => SetField(ref canEdit, value);
		}

		bool canRemove;
		[PropertyChangedAlso(nameof(CanTransfer))]
		public bool CanRemove {
			get => canRemove;
			set => SetField(ref canRemove, value);
		}

		bool needRefresh;
		public bool NeedRefresh {
			get => needRefresh;
			set => SetField(ref needRefresh, value);
		}

		public bool CanTransfer => CanRemove;

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

		void NotifyCriteria(EntityChangeEvent[] e)
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

		public string GenerateDelayDaysString(ILevelingRequestNode n)
		{
			if(n is SupplierNode)
				return n.SupplierPriceItem.Supplier.DelayDays > 0 ? string.Format("{0} дн.", n.SupplierPriceItem.Supplier.DelayDays) : "Нет";
			return string.Empty;
		}

		void RefreshSuppliers()
		{
			Entity.RequestingNomenclaturesListRefresh(UoW, supplierPriceItemsRepository, Entity.SuppliersOrdering);
			MinimalTotalSumText = string.Format("Минимальное ИТОГО: {0}", Entity.MinimalTotalSum.ToShortCurrencyString());
			ListContentChanged?.Invoke(this, new EventArgs());
			NeedRefresh = false;
		}

		protected override void BeforeValidation()
		{
			if(UoW.IsNew)
				Entity.Creator = CurrentEmployee;
			base.BeforeValidation();
		}

		#region Commands

		void CreateCommands()
		{
			CreateRefreshCommand();
			CreateAddRequestingNomenclatureCommand();
			CreateRemoveRequestingNomenclatureCommand();
			CreateTransferRequestingNomenclatureCommand();
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
					var filter = new NomenclatureFilterViewModel(CommonServices.InteractiveService) {
						HidenByDefault = true
					};
					NomenclaturesJournalViewModel journalViewModel = new NomenclaturesJournalViewModel(
						filter,
						QS.DomainModel.UoW.UnitOfWorkFactory.GetDefaultFactory,
						CommonServices
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
				},
				() => CanEdit
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
				},
				array => CanEdit && CanRemove
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
						EntityConstructorParam.ForCreate(),
						commonServices,
						employeeService,
						supplierPriceItemsRepository
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
				},
				array => CanTransfer
			);
		}

		#endregion TransferRequestingNomenclatureCommand

		#endregion Commands

		public override void Dispose()
		{
			NotifyConfiguration.Instance.UnsubscribeAll(this);
			base.Dispose();
		}
	}
}