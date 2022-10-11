using System;
using System.ComponentModel;
using System.Linq;
using QS.Commands;
using QS.DomainModel.UoW;
using QS.Project.Dialogs.GtkUI;
using QS.Project.Domain;
using QS.Project.Journal.EntitySelector;
using QS.RepresentationModel.GtkUI;
using QS.Services;
using QS.Utilities;
using QS.ViewModels;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Complaints;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Operations;
using Vodovoz.EntityRepositories.Operations;
using Vodovoz.EntityRepositories.Subdivisions;
using Vodovoz.Infrastructure.Services;
using Vodovoz.JournalFilters;
using Vodovoz.Parameters;
using Vodovoz.Services;
using Vodovoz.ViewModel;
using Vodovoz.ViewModels.Complaints;

namespace Vodovoz.ViewModels
{
	public class ResidueViewModel : EntityTabViewModelBase<Residue>
	{
		private readonly IEmployeeService employeeService;
		private readonly IRepresentationEntityPicker entityPicker;
		private readonly IBottlesRepository bottlesRepository;
		private readonly IDepositRepository depositRepository;
		private readonly IMoneyRepository moneyRepository;

		public ResidueViewModel(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory uowFactory,
			IEmployeeService employeeService,
			IRepresentationEntityPicker entityPicker,
			IBottlesRepository bottlesRepository,
			IDepositRepository depositRepository,
			IMoneyRepository moneyRepository,
			ICommonServices commonServices,
			IEntityAutocompleteSelectorFactory employeeSelectorFactory,
			ISubdivisionParametersProvider subdivisionParametersProvider
		)
		: base(uowBuilder, uowFactory, commonServices)
		{
			this.employeeService = employeeService ?? throw new ArgumentNullException(nameof(employeeService));
			this.entityPicker = entityPicker ?? throw new ArgumentNullException(nameof(entityPicker));
			this.bottlesRepository = bottlesRepository ?? throw new ArgumentNullException(nameof(bottlesRepository));
			this.depositRepository = depositRepository ?? throw new ArgumentNullException(nameof(depositRepository));
			this.moneyRepository = moneyRepository ?? throw new ArgumentNullException(nameof(moneyRepository));
			TabName = "Ввод остатков";
			if(CurrentEmployee == null) {
				AbortOpening("Ваш пользователь не привязан к действующему сотруднику, вы не можете создавать складские документы, " +
					"так как некого указывать в качестве кладовщика.", "Невозможно открыть ввод остатков");
			}
			if(UoW.IsNew) {
				Entity.Author = CurrentEmployee;
				Entity.Date = new DateTime(2017, 4, 23);
			}
			CreateCommands();
			ConfigureEntityPropertyChanges();
			UpdateResidue();
			GuiltyItemsVM = new GuiltyItemsViewModel(
				new Complaint(), UoW, commonServices, new SubdivisionRepository(new ParametersProvider()), employeeSelectorFactory,
				subdivisionParametersProvider);

			Entity.ObservableEquipmentDepositItems.PropertyOfElementChanged += OnObservableEquipmentItemsPropertyOfElementChanged;
		}

		public void OnObservableEquipmentItemsPropertyOfElementChanged(object sender, PropertyChangedEventArgs e)
		{
			if(!(sender is ResidueEquipmentDepositItem item))
				return;
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
					currentEmployee = employeeService.GetEmployeeForUser(UoW, UserService.CurrentUserId);
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
			Entity.UpdateOperations(UoW, bottlesRepository, moneyRepository, depositRepository, CommonServices.ValidationService);

			return base.BeforeSave();
		}

		private string currentBottlesDebt;
		public virtual string CurrentBottlesDebt {
			get => currentBottlesDebt;
			set => SetField(ref currentBottlesDebt, value, () => CurrentBottlesDebt);
		}

		private string currentBottlesDeposit;
		public virtual string CurrentBottlesDeposit {
			get => currentBottlesDeposit;
			set => SetField(ref currentBottlesDeposit, value, () => CurrentBottlesDeposit);
		}

		private string currentEquipmentDeposit;
		public virtual string CurrentEquipmentDeposit {
			get => currentEquipmentDeposit;
			set => SetField(ref currentEquipmentDeposit, value, () => CurrentEquipmentDeposit);
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
			if(Entity.DeliveryPoint == null)
				bottleDebt = bottlesRepository.GetBottlesDebtAtCounterparty(UoW, Entity.Customer, Entity.Date);
			else
				bottleDebt = bottlesRepository.GetBottlesDebtAtDeliveryPoint(UoW, Entity.DeliveryPoint, Entity.Date);
			CurrentBottlesDebt = NumberToTextRus.FormatCase(bottleDebt, "{0} бутыль", "{0} бутыли", "{0} бутылей");

			decimal bottleDeposit;
			if(Entity.DeliveryPoint == null)
				bottleDeposit = depositRepository.GetDepositsAtCounterparty(UoW, Entity.Customer, DepositType.Bottles, Entity.Date);
			else
				bottleDeposit = depositRepository.GetDepositsAtDeliveryPoint(UoW, Entity.DeliveryPoint, DepositType.Bottles, Entity.Date);
			CurrentBottlesDeposit = CurrencyWorks.GetShortCurrencyString(bottleDeposit);

			decimal equipDeposit;
			if(Entity.DeliveryPoint == null)
				equipDeposit = depositRepository.GetDepositsAtCounterparty(UoW, Entity.Customer, DepositType.Equipment, Entity.Date);
			else
				equipDeposit = depositRepository.GetDepositsAtDeliveryPoint(UoW, Entity.DeliveryPoint, DepositType.Equipment, Entity.Date);
			CurrentEquipmentDeposit = CurrencyWorks.GetShortCurrencyString(equipDeposit);

			decimal debt = moneyRepository.GetCounterpartyDebt(UoW, Entity.Customer, Entity.Date);
			CurrentMoneyDebt = CurrencyWorks.GetShortCurrencyString(debt);
		}

		#region Commands

		public DelegateCommand AddDepositEquipmentItemCommand { get; private set; }
		public DelegateCommand<ResidueEquipmentDepositItem> RemoveDepositEquipmentItemCommand { get; private set; }

		private void CreateCommands()
		{
			AddDepositEquipmentItemCommand = new DelegateCommand(
				() => {
					NomenclatureRepFilter nomenclatureFilter = new NomenclatureRepFilter(UoW);
					nomenclatureFilter.SetAndRefilterAtOnce(
						x => x.AvailableCategories = new NomenclatureCategory[] { NomenclatureCategory.equipment }
					);
					IRepresentationModel nomenclatureVM = new NomenclatureForSaleVM(nomenclatureFilter);

					RepresentationJournalDialog selector = new RepresentationJournalDialog(nomenclatureVM);
					selector.Mode = QS.Project.Dialogs.JournalSelectMode.Single;
					selector.ObjectSelected += (sender, e) => {
						if(!(e.Selected.FirstOrDefault() is NomenclatureForSaleVMNode selectedNode)) {
							return;
						}
						Entity.AddEquipmentDepositItem(UoW.GetById<Nomenclature>(selectedNode.Id));
					};
					TabParent.AddSlaveTab(this, selector);
				},
				() => CanEdit
			);

			RemoveDepositEquipmentItemCommand = new DelegateCommand<ResidueEquipmentDepositItem>(
				Entity.RemoveEquipmentDepositItem,
				(selected) => CanEdit && selected != null && Entity.ObservableEquipmentDepositItems.Contains(selected)
			);
		}

		#endregion Commands
	}
}
