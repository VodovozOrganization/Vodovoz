using System;
using Vodovoz.Infrastructure.ViewModels;
using Vodovoz.Domain.Fuel;
using Vodovoz.Infrastructure.Services;
using Vodovoz.ViewModelBased;
using Vodovoz.Domain.Employees;
using QS.RepresentationModel.GtkUI;
using Vodovoz.ViewModel;
using Vodovoz.Representations;
using System.Collections.Generic;
using Vodovoz.Domain.Goods;
using NHibernate.Criterion;
using System.Linq;
using Vodovoz.Domain.Logistic;
using QS.DomainModel.NotifyChange;
using Vodovoz.EntityRepositories.Subdivisions;
using QS.DomainModel.UoW;
using Vodovoz.EntityRepositories.Fuel;
using QS.DomainModel.Entity;

namespace Vodovoz.Dialogs.Fuel
{
	public class FuelIncomeInvoiceViewModel : EntityTabViewModelBase<FuelIncomeInvoice>
	{
		private readonly IEmployeeService employeeService;
		private readonly IRepresentationEntityPicker entityPicker;
		private readonly ISubdivisionRepository subdivisionRepository;
		private readonly IFuelRepository fuelRepository;

		public FuelIncomeInvoiceViewModel
		(
			IEntityConstructorParam entityCtorParam, 
			IEmployeeService employeeService,
			IRepresentationEntityPicker entityPicker,
			ISubdivisionRepository subdivisionRepository,
			IFuelRepository fuelRepository,
			ICommonServices commonServices
		) : base(entityCtorParam, commonServices)
		{
			this.employeeService = employeeService;
			this.entityPicker = entityPicker ?? throw new ArgumentNullException(nameof(entityPicker));
			this.subdivisionRepository = subdivisionRepository ?? throw new ArgumentNullException(nameof(subdivisionRepository));
			this.fuelRepository = fuelRepository ?? throw new ArgumentNullException(nameof(fuelRepository));
			TabName = "Входящая накладная по топливу";

			if(CurrentEmployee == null) {
				AbortOpening("К вашему пользователю не привязан сотрудник, невозможно открыть документ");
			}

			if(entityCtorParam.IsNewEntity) {
				Entity.СreationTime = DateTime.Now;
				Entity.Author = CurrentEmployee;
			}

			FuelBalanceViewModel = new FuelBalanceViewModel(subdivisionRepository, fuelRepository, commonServices);

			CreateCommands();
			ConfigEntityUpdateSubscribes();
			ConfigureEntityPropertyChanges();
			UpdateCashSubdivisions();
			UpdateBalanceCache();
			ValidationContext.ServiceContainer.AddService(typeof(IFuelRepository), fuelRepository);
		}

		private void ConfigEntityUpdateSubscribes()
		{
			NotifyConfiguration.Instance.BatchSubscribeOnEntity<FuelTransferDocument, FuelIncomeInvoice>(OnExternalUpdate);
		}

		private void ConfigureEntityPropertyChanges()
		{
			OnEntityPropertyChanged(UpdateBalanceCache,
				e => e.Subdivision
			);
		}

		void OnExternalUpdate(EntityChangeEvent[] changeEvents)
		{
			UpdateBalanceCache();
		}

		protected override void BeforeSave()
		{
			Entity.UpdateOperations(fuelRepository);
			base.BeforeSave();
		}

		#region Properties

		public FuelBalanceViewModel FuelBalanceViewModel { get; }

		private Employee currentEmployee;
		public Employee CurrentEmployee {
			get {
				if(currentEmployee == null) {
					currentEmployee = employeeService.GetEmployeeForUser(UoW, UserService.CurrentUserId);
				}
				return currentEmployee;
			}
		}

		private FuelIncomeInvoiceItem selectedItem;
		[PropertyChangedAlso(nameof(CanDeleteItems))]
		public virtual FuelIncomeInvoiceItem SelectedItem {
			get => selectedItem;
			set => SetField(ref selectedItem, value, () => SelectedItem);
		}

		public bool CanEdit => PermissionResult.CanUpdate;
		public bool CanAddItems => CanEdit;
		public bool CanDeleteItems => CanEdit && SelectedItem != null;

		#endregion Properties

		#region Commands
		public DelegateCommand AddItemCommand { get; private set; }
		public DelegateCommand DeleteItemCommand { get; private set; }

		private void CreateCommands()
		{
			AddItemCommand = new DelegateCommand(
				() => {
					IRepresentationModel model = new EntityCommonRepresentationModelConstructor<Nomenclature>()
						.AddColumn("Название", x => x.OfficialName).AddSearch(x => x.OfficialName)
						.AddColumn("Тип топлива", x => x.FuelType.Name)
						.SetFixedRestriction(Restrictions.Where<Nomenclature>(x => x.Category == NomenclatureCategory.fuel && !x.IsArchive))
						.Finish();

					entityPicker.OpenMultipleSelectionJournal<Nomenclature>(
						model,
						(nomenclatures) => {
							if(!nomenclatures.Any()) {
								return;
							}
							foreach(var nomenclature in nomenclatures) {
								Entity.AddItem(nomenclature);
							}
						},
						(journalTab) => { TabParent.AddTab(journalTab, this); }
					);
				},
				() => { return CanAddItems; }
			);

			DeleteItemCommand = new DelegateCommand(
				() => { Entity.DeleteItem(SelectedItem); },
				() => { return CanDeleteItems; }
			);
		}

		#endregion Commands

		#region Representation models

		public IRepresentationModel counterpartyVM;
		public IRepresentationModel CounterpartyVM {
			get {
				if(counterpartyVM == null) {
					counterpartyVM = new CounterpartyVM();
				}
				return counterpartyVM;
			}
		}

		public IRepresentationModel subdivisionVM;
		public IRepresentationModel SubdivisionVM {
			get {
				if(subdivisionVM == null) {
					subdivisionVM = new SubdivisionsVM();
				}
				return subdivisionVM;
			}
		}

		#endregion Representation models

		#region Настройка списков доступных подразделений кассы

		public IEnumerable<Subdivision> AvailableSubdivisions { get; private set; }

		private void UpdateCashSubdivisions()
		{
			AvailableSubdivisions = subdivisionRepository.GetCashSubdivisionsAvailableForUser(UoW, CurrentUser);
		}

		#endregion Настройка списков доступных подразделений кассы

		#region FuelBalance

		private Dictionary<FuelType, decimal> fuelBalanceCache;

		private void UpdateBalanceCache()
		{
			if(fuelBalanceCache == null) {
				fuelBalanceCache = new Dictionary<FuelType, decimal>();
			}
			fuelBalanceCache.Clear();
			if(Entity.Subdivision == null) {
				return;
			}
			using(var localUoW = UnitOfWorkFactory.CreateWithoutRoot()) {
				fuelBalanceCache = fuelRepository.GetAllFuelsBalanceForSubdivision(UoW, Entity.Subdivision);
			}
		}

		private decimal GetAvailableLiters(FuelType fuelType)
		{
			if(fuelBalanceCache == null) {
				UpdateBalanceCache();
			}
			if(!fuelBalanceCache.ContainsKey(fuelType)) {
				return 0m;
			}
			return fuelBalanceCache[fuelType];
		}

		private Dictionary<int, decimal> baseItemsLitersCache = new Dictionary<int, decimal>();

		public decimal GetMinimumAvailableLiters(FuelIncomeInvoiceItem item)
		{
			if(item?.Nomenclature?.FuelType == null) {
				return 0m;
			}
			decimal currentLiters = item.Liters;
			if(!UoW.IsNew && item.Id > 0 ) {
				if(baseItemsLitersCache.ContainsKey(item.Id)) {
					currentLiters = baseItemsLitersCache[item.Id];
				} else {
					using(var localUoW = UnitOfWorkFactory.CreateWithoutRoot()) {
						FuelIncomeInvoiceItem invoiceItem = localUoW.GetById<FuelIncomeInvoiceItem>(item.Id);
						if(invoiceItem != null) {
							currentLiters = invoiceItem.Liters;
							baseItemsLitersCache.Add(invoiceItem.Id, invoiceItem.Liters);
						}
					}
				}
			}

			decimal result = currentLiters - GetAvailableLiters(item.Nomenclature.FuelType);
			return result < 0 ? 0 : result;
		}

		#endregion FuelBalance

	}
}
