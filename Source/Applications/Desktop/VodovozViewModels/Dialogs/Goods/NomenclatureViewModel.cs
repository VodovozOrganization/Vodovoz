using System;
using System.Linq;
using NLog;
using QS.Commands;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Project.Journal.EntitySelector;
using QS.Services;
using QS.ViewModels;
using QS.ViewModels.Extension;
using Vodovoz.Domain.Goods;
using Vodovoz.EntityRepositories;
using Vodovoz.EntityRepositories.Goods;
using Vodovoz.Services;
using Vodovoz.Models;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.ViewModels.Goods;

namespace Vodovoz.ViewModels.Dialogs.Goods
{
	public class NomenclatureViewModel : EntityTabViewModelBase<Nomenclature>, IAskSaveOnCloseViewModel
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();

		private readonly IEmployeeService _employeeService;
		private readonly INomenclatureRepository _nomenclatureRepository;
		private readonly IUserRepository _userRepository;

		public Action PricesViewSaveChanges;

		public NomenclatureViewModel(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory uowFactory,
			ICommonServices commonServices,
			IEmployeeService employeeService,
			INomenclatureJournalFactory nomenclatureSelectorFactory,
			ICounterpartyJournalFactory counterpartySelectorFactory,
			INomenclatureRepository nomenclatureRepository,
			IUserRepository userRepository) : base(uowBuilder, uowFactory, commonServices)
		{
			if(nomenclatureSelectorFactory is null)
			{
				throw new ArgumentNullException(nameof(nomenclatureSelectorFactory));
			}

			_employeeService = employeeService ?? throw new ArgumentNullException(nameof(employeeService));
			_nomenclatureRepository = nomenclatureRepository ?? throw new ArgumentNullException(nameof(nomenclatureRepository));
			_userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
			NomenclatureSelectorFactory = nomenclatureSelectorFactory.GetDefaultNomenclatureSelectorFactory();
			CounterpartySelectorFactory =
				(counterpartySelectorFactory ?? throw new ArgumentNullException(nameof(counterpartySelectorFactory)))
				.CreateCounterpartyAutocompleteSelectorFactory();

			NomenclatureCostPricesViewModel =
				new NomenclatureCostPricesViewModel(Entity, new NomenclatureCostPriceModel(commonServices.CurrentPermissionService));
			NomenclaturePurchasePricesViewModel =
				new NomenclaturePurchasePricesViewModel(Entity, new NomenclaturePurchasePriceModel(commonServices.CurrentPermissionService));
			NomenclatureInnerDeliveryPricesViewModel =
				new NomenclatureInnerDeliveryPricesViewModel(Entity, new NomenclatureInnerDeliveryPriceModel(commonServices.CurrentPermissionService));
			
			ConfigureEntityPropertyChanges();
			ConfigureValidationContext();
			SetPermissions();
		}

		public IEntityAutocompleteSelectorFactory NomenclatureSelectorFactory { get; }
		public IEntityAutocompleteSelectorFactory CounterpartySelectorFactory { get; }

		public bool ImageLoaded { get; set; }
		public NomenclatureImage PopupMenuOn { get; set; }
		public bool IsWaterCategory => Entity.Category == NomenclatureCategory.water;
		public bool IsWaterOrBottleCategory => IsWaterCategory || IsBottleCategory;
		public bool IsWaterInNotDisposableTare => IsWaterCategory && !Entity.IsDisposableTare;
		public bool IsSaleCategory => Nomenclature.GetCategoriesWithSaleCategory().Contains(Entity.Category);
		public bool IsMasterCategory => Entity.Category == NomenclatureCategory.master;
		public bool IsDepositCategory => Entity.Category == NomenclatureCategory.deposit;
		public bool IsFuelCategory => Entity.Category == NomenclatureCategory.fuel;
		public bool IsBottleCategory => Entity.Category == NomenclatureCategory.bottle;
		public bool Is19lTareVolume => Entity.TareVolume == TareVolume.Vol19L;
		public bool IsEquipmentCategory => Entity.Category == NomenclatureCategory.equipment;
		public bool IsNotServiceAndDepositCategory => !(Entity.Category == NomenclatureCategory.service || IsDepositCategory);
		public bool IsEshopNomenclature => Entity?.ProductGroup?.ExportToOnlineStore ?? false;
		public bool IsOnlineStoreNomenclature => Entity?.OnlineStore != null;
		public bool WithoutDependsOnNomenclature => Entity.DependsOnNomenclature == null;
		public bool CanEdit => PermissionResult.CanUpdate || (PermissionResult.CanCreate && IsNewEntity);
		public bool CanCreateAndArcNomenclatures { get; private set; }
		public bool CanEditAlternativeNomenclaturePrices { get; private set; }
		public bool AskSaveOnClose => CanEdit;
		public bool UserCanCreateNomenclaturesWithInventoryAccounting =>
			IsNewEntity && CanCreateNomenclaturesWithInventoryAccountingPermission;
		public NomenclatureCostPricesViewModel NomenclatureCostPricesViewModel { get; }
		public NomenclaturePurchasePricesViewModel NomenclaturePurchasePricesViewModel { get; }
		public NomenclatureInnerDeliveryPricesViewModel NomenclatureInnerDeliveryPricesViewModel { get; }
		private bool IsNewEntity => Entity.Id == 0;
		private bool CanCreateNomenclaturesWithInventoryAccountingPermission { get; set; }

		private void ConfigureValidationContext()
		{
			ValidationContext.ServiceContainer.AddService(typeof(INomenclatureRepository), _nomenclatureRepository);
		}

		private void ConfigureEntityPropertyChanges() {
			SetPropertyChangeRelation(
				e => e.Category,
				() => IsWaterInNotDisposableTare,
				() => IsWaterOrBottleCategory,
				() => IsWaterCategory,
				() => IsSaleCategory,
				() => IsMasterCategory,
				() => IsDepositCategory,
				() => IsFuelCategory,
				() => IsBottleCategory,
				() => IsEquipmentCategory,
				() => IsNotServiceAndDepositCategory
			);

			SetPropertyChangeRelation(
				e => e.IsDisposableTare,
				() => IsWaterInNotDisposableTare
			);

			SetPropertyChangeRelation(
				e => e.ProductGroup,
				() => IsEshopNomenclature
			);

			SetPropertyChangeRelation(
				e => e.DependsOnNomenclature,
				() => WithoutDependsOnNomenclature
			);

			SetPropertyChangeRelation(
				e => e.TareVolume,
				() => Is19lTareVolume
			);
			
			SetPropertyChangeRelation(
				e => e.Id,
				() => UserCanCreateNomenclaturesWithInventoryAccounting
			);
		}

		public string GetUserEmployeeName() {
			if(Entity.CreatedBy == null) {
				return "";
			}

			var employee = _employeeService.GetEmployeeForUser(UoW, Entity.CreatedBy.Id);

			if(employee == null) {
				return Entity.CreatedBy.Name;
			}

			return employee.ShortName;
		}

		public void DeleteImage() {
			Entity.Images.Remove(PopupMenuOn);
			PopupMenuOn = null;
		}

		public void OnEnumKindChanged(object sender, EventArgs e) {
			if(Entity.Category != NomenclatureCategory.deposit) {
				Entity.TypeOfDepositCategory = null;
			}
		}

		public void OnEnumKindChangedByUser(object sender, EventArgs e) {
			if(Entity.Id == 0 && IsSaleCategory)
			{
				Entity.SaleCategory = SaleCategory.notForSale;
			}

			if(!IsWaterCategory && !IsBottleCategory)
			{
				Entity.IsDisposableTare = false;
			}
		}

		private void SetPermissions()
		{
			CanCreateAndArcNomenclatures =
				CommonServices.CurrentPermissionService.ValidatePresetPermission("can_create_and_arc_nomenclatures");
			CanCreateNomenclaturesWithInventoryAccountingPermission =
				CommonServices.CurrentPermissionService.ValidatePresetPermission("can_create_nomenclatures_with_inventory_accounting");
			CanEditAlternativeNomenclaturePrices =
				CommonServices.CurrentPermissionService.ValidatePresetPermission("сan_edit_alternative_nomenclature_prices");
		}

		protected override bool BeforeValidation()
		{
			if(string.IsNullOrWhiteSpace(Entity.Code1c))
			{
				Entity.Code1c = _nomenclatureRepository.GetNextCode1c(UoW);
			}
			return true;
		}

		protected override bool BeforeSave() {
			logger.Info("Сохраняем номенклатуру...");
			Entity.SetNomenclatureCreationInfo(_userRepository);
			PricesViewSaveChanges?.Invoke();
			return base.BeforeSave();
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
