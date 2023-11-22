using System;
using System.Linq;
using QS.Commands;
using QS.DomainModel.UoW;
using QS.Project.Journal;
using QS.Project.Journal.EntitySelector;
using QS.Project.Search;
using QS.Services;
using QS.Tdi;
using QS.ViewModels;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Goods;
using Vodovoz.EntityRepositories;
using Vodovoz.EntityRepositories.Goods;
using Vodovoz.FilterViewModels.Goods;
using Vodovoz.Infrastructure.Services;
using Vodovoz.JournalViewModels;
using Vodovoz.Services;
using Vodovoz.Settings.Nomenclature;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Journals.FilterViewModels.Goods;
using Vodovoz.ViewModels.Journals.JournalViewModels.Goods;

namespace Vodovoz.ViewModels.Client
{
	public class SupplierPricesWidgetViewModel : EntityWidgetViewModelBase<Counterparty>
	{
		private readonly ITdiTab dialogTab;
		private readonly IEmployeeService employeeService;
		private readonly INomenclatureRepository nomenclatureRepository;
		private readonly IUserRepository userRepository;
		private readonly INomenclatureSettings _nomenclatureSettings;
		private readonly ICounterpartyJournalFactory counterpartySelectorFactory;
		private readonly INomenclatureJournalFactory _nomenclatureSelectorFactory;

		public event EventHandler ListContentChanged;

		public IJournalSearch Search { get; private set; }

		public SupplierPricesWidgetViewModel(Counterparty entity,
			IUnitOfWork uow,
			ITdiTab dialogTab,
			ICommonServices commonServices,
			IEmployeeService employeeService,
			ICounterpartyJournalFactory counterpartySelectorFactory,
			INomenclatureJournalFactory nomenclatureSelectorFactory,
			INomenclatureRepository nomenclatureRepository,
			IUserRepository userRepository,
			INomenclatureSettings nomenclatureSettings)
			: base(entity, commonServices)
		{
			this.dialogTab = dialogTab ?? throw new ArgumentNullException(nameof(dialogTab));
			UoW = uow ?? throw new ArgumentNullException(nameof(uow));
			this.employeeService = employeeService ?? throw new ArgumentNullException(nameof(employeeService));
			this.nomenclatureRepository = nomenclatureRepository ?? throw new ArgumentNullException(nameof(nomenclatureRepository));
			this.userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
			_nomenclatureSettings = nomenclatureSettings ?? throw new ArgumentNullException(nameof(nomenclatureSettings));
			this.counterpartySelectorFactory = counterpartySelectorFactory ?? throw new ArgumentNullException(nameof(counterpartySelectorFactory));
			_nomenclatureSelectorFactory = nomenclatureSelectorFactory ?? throw new ArgumentNullException(nameof(nomenclatureSelectorFactory));

			CreateCommands();
			RefreshPrices();
			
			Search = new SearchViewModel();
			Search.OnSearch += (sender, e) => RefreshPrices();
			Entity.ObservableSuplierPriceItems.ElementAdded += (aList, aIdx) => RefreshPrices();
			Entity.ObservableSuplierPriceItems.ElementRemoved += (aList, aIdx, aObject) => RefreshPrices();
		}

		void CreateCommands()
		{
			CreateAddItemCommand();
			CreateRemoveItemCommand();
			CreateEditItemCommand();
		}

		void RefreshPrices()
		{
			Entity.SupplierPriceListRefresh(Search?.SearchValues);
			ListContentChanged?.Invoke(this, new EventArgs());
		}


		public bool CanAdd { get; set; } = true;
		public bool CanEdit { get; set; } = false;//задача редактирования пока не актуальна

		bool canRemove = false;
		public bool CanRemove {
			get => canRemove;
			set => SetField(ref canRemove, value);
		}
		#region Commands

		#region AddItemCommand

		public DelegateCommand AddItemCommand { get; private set; }

		private void CreateAddItemCommand()
		{
			AddItemCommand = new DelegateCommand(
				() => {
					var existingNomenclatures = Entity.ObservableSuplierPriceItems.Select(i => i.NomenclatureToBuy.Id).Distinct();
					var filter = new NomenclatureFilterViewModel() {
						HidenByDefault = true
					};
					NomenclaturesJournalViewModel journalViewModel = new NomenclaturesJournalViewModel(
						filter,
						UnitOfWorkFactory.GetDefaultFactory,
						CommonServices,
						employeeService,
						_nomenclatureSelectorFactory,
						counterpartySelectorFactory,
						nomenclatureRepository,
						userRepository,
						_nomenclatureSettings
					) {
						SelectionMode = JournalSelectionMode.Single,
						ExcludingNomenclatureIds = existingNomenclatures.ToArray()
					};
					journalViewModel.OnEntitySelectedResult += (sender, e) => {
						var selectedNode = e.SelectedNodes.FirstOrDefault();
						if(selectedNode == null)
							return;
						Entity.AddSupplierPriceItems(UoW.GetById<Nomenclature>(selectedNode.Id));
					};
					dialogTab.TabParent.AddSlaveTab(dialogTab, journalViewModel);
				},
				() => true
			);
		}

		#endregion AddItemCommand

		#region RemoveItemCommand

		public DelegateCommand<ISupplierPriceNode> RemoveItemCommand { get; private set; }

		private void CreateRemoveItemCommand()
		{
			RemoveItemCommand = new DelegateCommand<ISupplierPriceNode>(
				n => Entity.RemoveNomenclatureWithPrices(n.NomenclatureToBuy.Id),
				n => CanRemove
			);
		}

		#endregion RemoveItemCommand

		#region EditItemCommand

		public DelegateCommand EditItemCommand { get; private set; }

		private void CreateEditItemCommand()
		{
			EditItemCommand = new DelegateCommand(
				() => throw new NotImplementedException(nameof(EditItemCommand)),
				() => false
			);
		}

		#endregion EditItemCommand

		#endregion Commands

	}
}
