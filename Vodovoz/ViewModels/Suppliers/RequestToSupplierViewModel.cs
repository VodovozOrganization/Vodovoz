using System;
using System.Linq;
using QS.Commands;
using QS.DomainModel.Config;
using QS.DomainModel.NotifyChange;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Suppliers;
using Vodovoz.EntityRepositories.Suppliers;
using Vodovoz.FilterViewModels.Goods;
using Vodovoz.Infrastructure.Services;
using Vodovoz.JournalViewModels;
using QS.Utilities;

namespace Vodovoz.ViewModels.Suppliers
{
	public class RequestToSupplierViewModel : EntityTabViewModelBase<RequestToSupplier>
	{
		readonly ISupplierPriceItemsRepository supplierPriceItemsRepository;
		readonly IEntityConfigurationProvider entityConfigurationProvider;
		readonly IEmployeeService employeeService;

		public RequestToSupplierViewModel(
			IEntityConstructorParam ctorParam,
			ICommonServices commonServices,
			IEntityConfigurationProvider entityConfigurationProvider,
			IEmployeeService employeeService,
			ISupplierPriceItemsRepository supplierPriceItemsRepository
		) : base(ctorParam, commonServices)
		{
			this.employeeService = employeeService ?? throw new ArgumentNullException(nameof(employeeService));
			this.entityConfigurationProvider = entityConfigurationProvider ?? throw new ArgumentNullException(nameof(entityConfigurationProvider));
			this.supplierPriceItemsRepository = supplierPriceItemsRepository ?? throw new ArgumentNullException(nameof(supplierPriceItemsRepository));
			CreateCommands();
			RefreshSuppliers();
			Entity.ObservableRequestingNomenclatureItems.ElementAdded += (aList, aIdx) => RefreshSuppliers();
			Entity.ObservableRequestingNomenclatureItems.ElementRemoved += (aList, aIdx, aObject) => RefreshSuppliers();
			NotifyConfiguration.Instance.BatchSubscribeOnEntity<SupplierPriceItem>(NotifyCriteria);
		}
		public event EventHandler ListContentChanged;
		public event EventHandler SupplierPricesUpdated;

		bool canEdit = true;
		public bool CanEdit {
			get => canEdit;
			set => SetField(ref canEdit, value);
		}

		bool canRemove;
		public bool CanRemove {
			get => canRemove;
			set => SetField(ref canRemove, value);
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

		void NotifyCriteria(EntityChangeEvent[] e)
		{
			var updatedPriceItems = e.Select(ev => ev.GetEntity<SupplierPriceItem>());
			var displayingNomenclatures = Entity.ObservableRequestingNomenclatureItems.Select(r => r.Nomenclature);
			foreach(var n in displayingNomenclatures) {
				if(updatedPriceItems.Select(p => p.NomenclatureToBuy.Id).Contains(n.Id)) {
					NeedRefresh = true;
					SupplierPricesUpdated?.Invoke(updatedPriceItems, new EventArgs());
					return;
				}
			}
		}

		#region Commands

		void CreateCommands()
		{
			CreateRefreshCommand();
			CreateAddRequestingNomenclatureCommand();
			CreateRemoveRequestingNomenclatureCommand();
			CreateTransferRequestingNomenclatureCommand();
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
					var existingNomenclatures = Entity.ObservableRequestingNomenclatureItems.Select(i => i.Nomenclature.Id).Distinct();
					var filter = new NomenclatureFilterViewModel(CommonServices.InteractiveService) {
						HidenByDefault = true
					};
					NomenclaturesJournalViewModel journalViewModel = new NomenclaturesJournalViewModel(
						filter,
						entityConfigurationProvider,
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

		public DelegateCommand<ILevelingRequestNode> RemoveRequestingNomenclatureCommand { get; private set; }

		void CreateRemoveRequestingNomenclatureCommand()
		{
			RemoveRequestingNomenclatureCommand = new DelegateCommand<ILevelingRequestNode>(
				n => Entity.RemoveNomenclatureRequest(n.Nomenclature.Id),
				n => CanEdit && CanRemove
			);
		}

		#endregion RemoveRequestingNomenclatureCommand

		#region TransferRequestingNomenclatureCommand

		public DelegateCommand TransferRequestingNomenclatureCommand { get; private set; }

		void CreateTransferRequestingNomenclatureCommand()
		{
			TransferRequestingNomenclatureCommand = new DelegateCommand(
				() => { },
				() => true
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