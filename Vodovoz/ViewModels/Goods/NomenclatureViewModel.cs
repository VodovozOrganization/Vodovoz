using System;
using System.Linq;
using NLog;
using QS.Commands;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Project.Journal.EntitySelector;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Store;
using Vodovoz.EntityRepositories;
using Vodovoz.EntityRepositories.Goods;
using Vodovoz.Infrastructure.Services;

namespace Vodovoz.ViewModels.Goods
{
	public class NomenclatureViewModel : EntityTabViewModelBase<Nomenclature>
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();
		
		private readonly IEmployeeService employeeService;
		private readonly INomenclatureRepository nomenclatureRepository;
		private readonly IUserRepository userRepository;
		
		public IEntityAutocompleteSelectorFactory NomenclatureSelectorFactory { get; }
		public IEntityAutocompleteSelectorFactory CounterpartySelectorFactory { get; }

		private Warehouse selectedWarehouse;
		public Warehouse SelectedWarehouse {
			get => selectedWarehouse;
			set => SetField(ref selectedWarehouse, value);
		}
		
		public bool ImageLoaded { get; set; }
		public NomenclatureImage PopupMenuOn { get; set; }

		public Action PricesViewSaveChanges;
		
		public NomenclatureViewModel(IEntityUoWBuilder uowBuilder,
		                             IUnitOfWorkFactory uowFactory,
		                             ICommonServices commonServices,
									 IEmployeeService employeeService,
		                             IEntityAutocompleteSelectorFactory nomenclatureSelectorFactory,
		                             IEntityAutocompleteSelectorFactory counterpartySelectorFactory,
		                             INomenclatureRepository nomenclatureRepository,
		                             IUserRepository userRepository) : base(uowBuilder, uowFactory, commonServices) {

			this.employeeService = employeeService ?? throw new ArgumentNullException(nameof(employeeService));
			this.nomenclatureRepository = nomenclatureRepository ?? throw new ArgumentNullException(nameof(nomenclatureRepository));
			this.userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
			NomenclatureSelectorFactory = nomenclatureSelectorFactory ?? throw new ArgumentNullException(nameof(nomenclatureSelectorFactory));
			CounterpartySelectorFactory = counterpartySelectorFactory ?? throw new ArgumentNullException(nameof(counterpartySelectorFactory));

			ConfigureEntityPropertyChanges();
		}

		public bool VisibilityWaterInNotDisposableTareCategoryItems =>
			Entity.Category == NomenclatureCategory.water && !Entity.IsDisposableTare;
		
		public bool VisibilityWaterCategoryItems =>
			Entity.Category == NomenclatureCategory.water;
		
		public bool VisibilityWaterOrBottleCategoryItems =>
			Entity.Category == NomenclatureCategory.water 
			|| Entity.Category == NomenclatureCategory.bottle;
		
		public bool VisibilitySalesCategoriesItems =>
			Nomenclature.GetCategoriesWithSaleCategory().Contains(Entity.Category);
		
		public bool VisibilityMasterCategoryItems =>
			Entity.Category == NomenclatureCategory.master;
		
		public bool VisibilityDepositCategoryItems =>
			Entity.Category == NomenclatureCategory.deposit;

		public bool VisibilityFuelCategoryItems =>
			Entity.Category == NomenclatureCategory.fuel;

		public bool VisibilityBottleCategoryItems =>
			Entity.Category == NomenclatureCategory.bottle;
		
		public bool VisibilityBottleCapColorItems => Entity.TareVolume == TareVolume.Vol19L;

		public bool SensitivityCheckIsArchive => 
			CommonServices.CurrentPermissionService.ValidatePresetPermission("can_create_and_arc_nomenclatures");
		
		public bool SensitivityEquipmentCategoryItems =>
			Entity.Category == NomenclatureCategory.equipment;

		public bool SensitivityNotServiceOrDepositCategoryItems =>
			!(Entity.Category == NomenclatureCategory.service || Entity.Category == NomenclatureCategory.deposit);
		
		public bool IsEshopNomenclature => Entity?.ProductGroup?.ExportToOnlineStore ?? false;
		
		public bool IsOnlineStoreProductGroup => Entity?.ProductGroup?.IsOnlineStore ?? false;

		public bool SensitivityRadioPriceButton => Entity.DependsOnNomenclature == null;

		void ConfigureEntityPropertyChanges() {
			SetPropertyChangeRelation(
				e => e.Category,
				() => VisibilityWaterInNotDisposableTareCategoryItems,
				() => VisibilityWaterOrBottleCategoryItems,
				() => VisibilityWaterCategoryItems,
				() => VisibilitySalesCategoriesItems,
				() => VisibilityMasterCategoryItems,
				() => VisibilityDepositCategoryItems,
				() => VisibilityFuelCategoryItems,
				() => VisibilityBottleCategoryItems,
				() => SensitivityEquipmentCategoryItems,
				() => SensitivityNotServiceOrDepositCategoryItems
			);
			
			SetPropertyChangeRelation(
				e => e.IsDisposableTare,
				() => VisibilityWaterInNotDisposableTareCategoryItems
			);
			
			SetPropertyChangeRelation(
				e => e.ProductGroup,
				() => IsEshopNomenclature,
				() => IsOnlineStoreProductGroup
			);

			SetPropertyChangeRelation(
				e => e.DependsOnNomenclature,
				() => SensitivityRadioPriceButton
			);
			
			SetPropertyChangeRelation(
				e => e.TareVolume,
				() => VisibilityBottleCapColorItems
			);
		}

		public string GetUserEmployeeName() {
			if(Entity.CreatedBy == null) {
				return "";
			}

			var employee = employeeService.GetEmployeeForUser(UoW, CurrentUser.Id);

			if(employee == null) {
				return Entity.CreatedBy.Name;
			}

			return employee.ShortName;
		}

		public void DeleteImage() {
			Entity.Images.Remove(PopupMenuOn);
			PopupMenuOn = null;
		}
		
		public void OnEnumTypeChanged(object sender, EventArgs e) {
			if(Entity.Category != NomenclatureCategory.deposit) {
				Entity.TypeOfDepositCategory = null;
			}
		}
		
		public void OnEnumTypeChangedByUser(object sender, EventArgs e) {
			if(Entity.Id == 0 && Nomenclature.GetCategoriesWithSaleCategory().Contains(Entity.Category))
				Entity.SaleCategory = SaleCategory.notForSale;

			if (Entity.Category != NomenclatureCategory.water && Entity.Category != NomenclatureCategory.bottle)
				Entity.IsDisposableTare = false;
		}

		protected override void BeforeValidation() {
			if(string.IsNullOrWhiteSpace(Entity.Code1c)) {
				Entity.Code1c = nomenclatureRepository.GetNextCode1c(UoW);
			}
		}
		
		protected override void BeforeSave() {
			logger.Info("Сохраняем номенклатуру...");
			Entity.SetNomenclatureCreationInfo(userRepository);
			PricesViewSaveChanges?.Invoke();
		}
		
		#region Commands

		private DelegateCommand saveCommand = null;
		public DelegateCommand SaveCommand =>
			saveCommand ?? (saveCommand = new DelegateCommand(
				() => {
					Save(true);
				},
				() => true
			)
		);

		#endregion
	}
}
