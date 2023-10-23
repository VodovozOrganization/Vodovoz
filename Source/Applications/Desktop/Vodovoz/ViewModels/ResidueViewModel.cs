using Autofac;
using QS.Commands;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Services;
using QS.Utilities;
using QS.ViewModels;
using System;
using System.ComponentModel;
using System.Linq;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Complaints;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Operations;
using Vodovoz.EntityRepositories.Operations;
using Vodovoz.EntityRepositories.Subdivisions;
using Vodovoz.Infrastructure.Services;
using Vodovoz.Parameters;
using Vodovoz.Services;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Complaints;
using Vodovoz.ViewModels.Journals.FilterViewModels.Goods;

namespace Vodovoz.ViewModels
{
	public class ResidueViewModel : EntityTabViewModelBase<Residue>
	{
		private readonly IEmployeeService _employeeService;
		private readonly IRepresentationEntityPicker _entityPicker;
		private readonly IBottlesRepository _bottlesRepository;
		private readonly IDepositRepository _depositRepository;
		private readonly IMoneyRepository _moneyRepository;
		private readonly ICounterpartyJournalFactory _counterpartyJournalFactory;
		private readonly ILifetimeScope _lifetimeScope;

		public ResidueViewModel(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory uowFactory,
			IEmployeeService employeeService,
			IRepresentationEntityPicker entityPicker,
			IBottlesRepository bottlesRepository,
			IDepositRepository depositRepository,
			IMoneyRepository moneyRepository,
			ICommonServices commonServices,
			IEmployeeJournalFactory employeeJournalFactory,
			ISubdivisionParametersProvider subdivisionParametersProvider,
			ICounterpartyJournalFactory counterpartyJournalFactory,
			ILifetimeScope lifetimeScope,
			INavigationManager navigationManager)
			: base(uowBuilder, uowFactory, commonServices, navigationManager)
		{
			if(navigationManager is null)
			{
				throw new ArgumentNullException(nameof(navigationManager));
			}

			_employeeService = employeeService ?? throw new ArgumentNullException(nameof(employeeService));
			_entityPicker = entityPicker ?? throw new ArgumentNullException(nameof(entityPicker));
			_bottlesRepository = bottlesRepository ?? throw new ArgumentNullException(nameof(bottlesRepository));
			_depositRepository = depositRepository ?? throw new ArgumentNullException(nameof(depositRepository));
			_moneyRepository = moneyRepository ?? throw new ArgumentNullException(nameof(moneyRepository));
			_counterpartyJournalFactory = counterpartyJournalFactory ?? throw new ArgumentNullException(nameof(counterpartyJournalFactory));
			_lifetimeScope = lifetimeScope ?? throw new ArgumentNullException(nameof(lifetimeScope));

			TabName = "Ввод остатков";
			if(CurrentEmployee == null)
			{
				AbortOpening("Ваш пользователь не привязан к действующему сотруднику, вы не можете создавать складские документы, " +
					"так как некого указывать в качестве кладовщика.", "Невозможно открыть ввод остатков");
			}

			if(UoW.IsNew)
			{
				Entity.Author = CurrentEmployee;
				Entity.Date = new DateTime(2017, 4, 23);
			}

			CreateCommands();
			ConfigureEntityPropertyChanges();
			UpdateResidue();

			GuiltyItemsVM = new GuiltyItemsViewModel(
				new Complaint(),
				UoW,
				this,
				_lifetimeScope,
				commonServices,
				new SubdivisionRepository(new ParametersProvider()),
				employeeJournalFactory,
				subdivisionParametersProvider);

			Entity.ObservableEquipmentDepositItems.PropertyOfElementChanged += OnObservableEquipmentItemsPropertyOfElementChanged;
		}

		public void OnObservableEquipmentItemsPropertyOfElementChanged(object sender, PropertyChangedEventArgs e)
		{
			if(!(sender is ResidueEquipmentDepositItem item))
			{
				return;
			}

			if(nameof(ResidueEquipmentDepositItem.EquipmentCount) == e.PropertyName) {
				item.DepositCount = item.EquipmentCount;
			}
		}

		public GuiltyItemsViewModel GuiltyItemsVM { get; set; }

		private void ConfigureEntityPropertyChanges()
		{
			OnEntityPropertyChanged(
				UpdateResidue,
				e => e.DeliveryPoint,
				e => e.Customer
			);
		}

		private Employee currentEmployee;
		public Employee CurrentEmployee {
			get {
				if(currentEmployee == null) {
					currentEmployee = _employeeService.GetEmployeeForUser(UoW, UserService.CurrentUserId);
				}
				return currentEmployee;
			}
		}

		public bool CanEdit => true;

		protected override bool BeforeSave()
		{
			Entity.LastEditAuthor = CurrentEmployee;
			Entity.LastEditTime = DateTime.Now;
			if(Entity.DeliveryPoint != null)
				Entity.DeliveryPoint.HaveResidue = true;
			Entity.UpdateOperations(UoW, _bottlesRepository, _moneyRepository, _depositRepository, CommonServices.ValidationService);

			return base.BeforeSave();
		}

		private string _currentBottlesDebt;
		public virtual string CurrentBottlesDebt {
			get => _currentBottlesDebt;
			set => SetField(ref _currentBottlesDebt, value);
		}

		private string _currentBottlesDeposit;
		public virtual string CurrentBottlesDeposit {
			get => _currentBottlesDeposit;
			set => SetField(ref _currentBottlesDeposit, value);
		}

		private string _currentEquipmentDeposit;
		public virtual string CurrentEquipmentDeposit {
			get => _currentEquipmentDeposit;
			set => SetField(ref _currentEquipmentDeposit, value, () => CurrentEquipmentDeposit);
		}

		private string currentMoneyDebt;
		public virtual string CurrentMoneyDebt {
			get => currentMoneyDebt;
			set => SetField(ref currentMoneyDebt, value, () => CurrentMoneyDebt);
		}

		private void UpdateResidue()
		{
			if(Entity.Customer == null)
				return;

			int bottleDebt;
			bottleDebt = Entity.DeliveryPoint == null
				? _bottlesRepository.GetBottlesDebtAtCounterparty(UoW, Entity.Customer, Entity.Date)
				: _bottlesRepository.GetBottlesDebtAtDeliveryPoint(UoW, Entity.DeliveryPoint, Entity.Date);

			CurrentBottlesDebt = NumberToTextRus.FormatCase(bottleDebt, "{0} бутыль", "{0} бутыли", "{0} бутылей");

			decimal bottleDeposit;
			if(Entity.DeliveryPoint == null)
				bottleDeposit = _depositRepository.GetDepositsAtCounterparty(UoW, Entity.Customer, DepositType.Bottles, Entity.Date);
			else
				bottleDeposit = _depositRepository.GetDepositsAtDeliveryPoint(UoW, Entity.DeliveryPoint, DepositType.Bottles, Entity.Date);
			CurrentBottlesDeposit = CurrencyWorks.GetShortCurrencyString(bottleDeposit);

			decimal equipDeposit;
			if(Entity.DeliveryPoint == null)
				equipDeposit = _depositRepository.GetDepositsAtCounterparty(UoW, Entity.Customer, DepositType.Equipment, Entity.Date);
			else
				equipDeposit = _depositRepository.GetDepositsAtDeliveryPoint(UoW, Entity.DeliveryPoint, DepositType.Equipment, Entity.Date);
			CurrentEquipmentDeposit = CurrencyWorks.GetShortCurrencyString(equipDeposit);

			decimal debt = _moneyRepository.GetCounterpartyDebt(UoW, Entity.Customer, Entity.Date);
			CurrentMoneyDebt = CurrencyWorks.GetShortCurrencyString(debt);
		}

		#region Commands

		public DelegateCommand AddDepositEquipmentItemCommand { get; private set; }
		public DelegateCommand<ResidueEquipmentDepositItem> RemoveDepositEquipmentItemCommand { get; private set; }

		public ICounterpartyJournalFactory CounterpartyJournalFactory => _counterpartyJournalFactory;

		private void CreateCommands()
		{
			AddDepositEquipmentItemCommand = new DelegateCommand(
				() => {
					var filter = new NomenclatureFilterViewModel();
					filter.RestrictCategory = NomenclatureCategory.equipment;

					var nomenclatureJournalFactory = new NomenclatureJournalFactory();
					var journal = nomenclatureJournalFactory.CreateNomenclaturesJournalViewModel();
					journal.FilterViewModel = filter;
					journal.OnEntitySelectedResult += Journal_OnEntitySelectedResult; ;
					TabParent.AddSlaveTab(this, journal);
				},
				() => CanEdit
			);

			RemoveDepositEquipmentItemCommand = new DelegateCommand<ResidueEquipmentDepositItem>(
				Entity.RemoveEquipmentDepositItem,
				(selected) => CanEdit && selected != null && Entity.ObservableEquipmentDepositItems.Contains(selected)
			);
		}

		private void Journal_OnEntitySelectedResult(object sender, QS.Project.Journal.JournalSelectedNodesEventArgs e)
		{
			var selectedNode = e.SelectedNodes.FirstOrDefault();
			if(selectedNode == null)
			{
				return;
			}
			var nomenclature = UoWGeneric.Session.Get<Nomenclature>(selectedNode.Id);
			Entity.AddEquipmentDepositItem(nomenclature);
		}

		#endregion Commands
	}
}
