using Autofac;
using Microsoft.Extensions.Logging;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Project.Journal.EntitySelector;
using QS.Services;
using QS.ViewModels;
using QS.ViewModels.Control.EEVM;
using QS.ViewModels.Extension;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Bindings.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Vodovoz.Application.FileStorage;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Goods.NomenclaturesOnlineParameters;
using Vodovoz.EntityRepositories;
using Vodovoz.EntityRepositories.Goods;
using Vodovoz.Extensions;
using Vodovoz.Models;
using Vodovoz.Presentation.ViewModels.AttachedFiles;
using Vodovoz.Services;
using Vodovoz.Settings.Nomenclature;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Dialogs.Nodes;
using Vodovoz.ViewModels.Journals.JournalViewModels.Goods;
using Vodovoz.ViewModels.Journals.JournalViewModels.Logistic;
using Vodovoz.ViewModels.ViewModels.Goods;
using Vodovoz.ViewModels.ViewModels.Logistic;
using Vodovoz.ViewModels.Widgets.Goods;
using VodovozBusiness.Services;
using VodovozInfrastructure.StringHandlers;
using Vodovoz.ViewModels.Goods.ProductGroups;
using Vodovoz.Core.Domain.Goods;

namespace Vodovoz.ViewModels.Dialogs.Goods
{
	public class NomenclatureViewModel : EntityTabViewModelBase<Nomenclature>, IAskSaveOnCloseViewModel
	{
		private static ILogger<NomenclatureViewModel> _logger;

		private readonly IEmployeeService _employeeService;
		private readonly INomenclatureRepository _nomenclatureRepository;
		private readonly IUserRepository _userRepository;
		private readonly int[] _equipmentKindsHavingGlassHolder;
		private readonly INomenclatureOnlineSettings _nomenclatureOnlineSettings;
		private readonly INomenclatureService _nomenclatureService;
		private readonly INomenclatureFileStorageService _nomenclatureFileStorageService;
		private readonly ViewModelEEVMBuilder<ProductGroup> _productGroupEEVMBuilder;
		private ILifetimeScope _lifetimeScope;
		private readonly IInteractiveService _interactiveService;
		private NomenclatureOnlineParameters _mobileAppNomenclatureOnlineParameters;
		private NomenclatureOnlineParameters _vodovozWebSiteNomenclatureOnlineParameters;
		private NomenclatureOnlineParameters _kulerSaleWebSiteNomenclatureOnlineParameters;
		private bool _needCheckOnlinePrices;
		private bool _isMagnetGlassHolderSelected;
		private bool _isScrewGlassHolderSelected;
		private bool _activeSitesAndAppsTab;
		private IList<NomenclatureOnlineCategory> _onlineCategories;

		private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

		public NomenclatureViewModel(
			ILogger<NomenclatureViewModel> logger,
			ILifetimeScope lifetimeScope,
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory uowFactory,
			ICommonServices commonServices,
			INavigationManager navigationManager,
			IInteractiveService interactiveService,
			IEmployeeService employeeService,
			ICounterpartyJournalFactory counterpartySelectorFactory,
			INomenclatureRepository nomenclatureRepository,
			IUserRepository userRepository,
			IStringHandler stringHandler,
			INomenclatureOnlineSettings nomenclatureOnlineSettings,
			Settings.Nomenclature.INomenclatureSettings nomenclatureSettings,
			INomenclatureService nomenclatureService,
			NomenclatureMinimumBalanceByWarehouseViewModel nomenclatureMinimumBalanceByWarehouseViewModel,
			INomenclatureFileStorageService nomenclatureFileStorageService,
			IAttachedFileInformationsViewModelFactory attachedFileInformationsViewModelFactory,
			ViewModelEEVMBuilder<ProductGroup> productGroupEEVMBuilder)
			: base(uowBuilder, uowFactory, commonServices, navigationManager)
		{
			if(nomenclatureSettings is null)
			{
				throw new ArgumentNullException(nameof(nomenclatureSettings));
			}

			if(attachedFileInformationsViewModelFactory is null)
			{
				throw new ArgumentNullException(nameof(attachedFileInformationsViewModelFactory));
			}

			StringHandler = stringHandler ?? throw new ArgumentNullException(nameof(stringHandler));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_lifetimeScope = lifetimeScope ?? throw new ArgumentNullException(nameof(lifetimeScope));
			_interactiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));
			_employeeService = employeeService ?? throw new ArgumentNullException(nameof(employeeService));
			_nomenclatureRepository = nomenclatureRepository ?? throw new ArgumentNullException(nameof(nomenclatureRepository));
			_userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
			_nomenclatureOnlineSettings =
				nomenclatureOnlineSettings ?? throw new ArgumentNullException(nameof(nomenclatureOnlineSettings));
			CounterpartySelectorFactory =
				(counterpartySelectorFactory ?? throw new ArgumentNullException(nameof(counterpartySelectorFactory)))
				.CreateCounterpartyAutocompleteSelectorFactory(_lifetimeScope);
			_nomenclatureService = nomenclatureService ?? throw new ArgumentNullException(nameof(nomenclatureService));
			_nomenclatureFileStorageService = nomenclatureFileStorageService ?? throw new ArgumentNullException(nameof(nomenclatureFileStorageService));
			_productGroupEEVMBuilder = productGroupEEVMBuilder ?? throw new ArgumentNullException(nameof(productGroupEEVMBuilder));

			RouteColumnViewModel = BuildRouteColumnEntryViewModel();
			ProductGroupEntityEntryViewModel = CreateProductGroupEEVM();

			ConfigureEntryViewModels();
			ConfigureOnlineParameters();
			ConfigureEntityPropertyChanges();
			ConfigureValidationContext();
			SetPermissions();
			SetProperties();

			_equipmentKindsHavingGlassHolder = nomenclatureSettings.EquipmentKindsHavingGlassHolder;
			SetGlassHolderCheckboxesSelection();

			Entity.PropertyChanged += OnEntityPropertyChanged;

			SaveCommand = CreateSaveCommand();
			CopyPricesWithoutDiscountFromMobileAppToVodovozWebSiteCommand =
				CreateCopyPricesWithoutDiscountFromMobileAppToVodovozWebSiteCommand();

			ArchiveCommand = new DelegateCommand(Archive);
			UnArchiveCommand = new DelegateCommand(UnArchive);

			AttachedFileInformationsViewModel = attachedFileInformationsViewModelFactory
				.CreateAndInitialize<Nomenclature, NomenclatureFileInformation>(
					UoW,
					Entity,
					_nomenclatureFileStorageService,
					_cancellationTokenSource.Token,
					Entity.AddFileInformation,
					Entity.RemoveFileInformation);

			AttachedFileInformationsViewModel.ReadOnly = !CanEdit;

			NomenclatureMinimumBalanceByWarehouseViewModel = nomenclatureMinimumBalanceByWarehouseViewModel
				?? throw new ArgumentNullException(nameof(nomenclatureMinimumBalanceByWarehouseViewModel));
			NomenclatureMinimumBalanceByWarehouseViewModel.Initialize(this, Entity, UoW);
		}

		public IStringHandler StringHandler { get; }
		public IEntityAutocompleteSelectorFactory CounterpartySelectorFactory { get; }
		public IEntityEntryViewModel RouteColumnViewModel { get; }
		public IEntityEntryViewModel ProductGroupEntityEntryViewModel { get; }

		public GenericObservableList<NomenclatureOnlinePricesNode> NomenclatureOnlinePrices { get; private set; }
			= new GenericObservableList<NomenclatureOnlinePricesNode>();

		public bool PriceChanged { get; set; }
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
		public bool HasAccessToSitesAndAppsTab { get; private set; }
		public bool OldHasConditionAccounting { get; private set; }
		public bool AskSaveOnClose => CanEdit;
		public bool UserCanCreateNomenclaturesWithInventoryAccounting =>
			IsNewEntity && CanCreateNomenclaturesWithInventoryAccountingPermission;
		public bool UserCanEditConditionAccounting =>
			!OldHasConditionAccounting && CanCreateNomenclaturesWithInventoryAccountingPermission;
		public bool IsShowGlassHolderSelectionControls => 
			_equipmentKindsHavingGlassHolder.Any(i => i == Entity.Kind?.Id);

		public bool IsMagnetGlassHolderSelected
		{
			get => _isMagnetGlassHolderSelected;
			set
			{
				if(SetField(ref _isMagnetGlassHolderSelected, value))
				{
					SetEntityGlassHolderType();
				}
			}
		}

		public bool IsScrewGlassHolderSelected
		{
			get => _isScrewGlassHolderSelected;
			set
			{
				if(SetField(ref _isScrewGlassHolderSelected, value))
				{
					SetEntityGlassHolderType();
				}
			}
		}

		public NomenclatureOnlineParameters MobileAppNomenclatureOnlineParameters
		{
			get => _mobileAppNomenclatureOnlineParameters;
			set => SetField(ref _mobileAppNomenclatureOnlineParameters, value);
		}
		public NomenclatureOnlineParameters VodovozWebSiteNomenclatureOnlineParameters
		{
			get => _vodovozWebSiteNomenclatureOnlineParameters;
			set => SetField(ref _vodovozWebSiteNomenclatureOnlineParameters, value);
		}
		public NomenclatureOnlineParameters KulerSaleWebSiteNomenclatureOnlineParameters
		{
			get => _kulerSaleWebSiteNomenclatureOnlineParameters;
			set => SetField(ref _kulerSaleWebSiteNomenclatureOnlineParameters, value);
		}

		public IList<MobileAppNomenclatureOnlineCatalog> MobileAppNomenclatureOnlineCatalogs { get; private set; }
		public IList<VodovozWebSiteNomenclatureOnlineCatalog> VodovozWebSiteNomenclatureOnlineCatalogs { get; private set; }
		public IList<KulerSaleWebSiteNomenclatureOnlineCatalog> KulerSaleWebSiteNomenclatureOnlineCatalogs { get; private set; }
		public IList<NomenclatureOnlineGroup> NomenclatureOnlineGroups { get; private set; }
		public NomenclatureOnlineGroup SelectedOnlineGroup
		{
			get => Entity.NomenclatureOnlineGroup;
			set
			{
				if(Entity.NomenclatureOnlineGroup != value)
				{
					Entity.NomenclatureOnlineGroup = value;
					UpdateOnlineCategories();
					UpdateOnlineParameters();
				}
			}
		}

		private void UpdateOnlineCategories()
		{
			OnlineCategories =
				Entity.NomenclatureOnlineGroup != null
					? Entity.NomenclatureOnlineGroup.NomenclatureOnlineCategories
					: new List<NomenclatureOnlineCategory>();
		}

		private void UpdateOnlineParameters()
		{
			if(Entity.NomenclatureOnlineGroup is null)
			{
				return;
			}

			if(Entity.NomenclatureOnlineGroup.Id == _nomenclatureOnlineSettings.WaterNomenclatureOnlineGroupId)
			{
				Entity.ResetNotWaterOnlineParameters();
			}

			if(Entity.NomenclatureOnlineCategory is null)
			{
				return;
			}
			
			if(Entity.NomenclatureOnlineCategory.Id == _nomenclatureOnlineSettings.KulerNomenclatureOnlineCategoryId)
			{
				Entity.ResetNotKulerOnlineParameters();
			}
			
			if(Entity.NomenclatureOnlineCategory.Id == _nomenclatureOnlineSettings.PurifierNomenclatureOnlineCategoryId)
			{
				Entity.ResetNotPurifierOnlineParameters();
			}
			
			if(Entity.NomenclatureOnlineCategory.Id == _nomenclatureOnlineSettings.WaterPumpNomenclatureOnlineCategoryId)
			{
				Entity.ResetNotWaterPumpOnlineParameters();
			}
			
			if(Entity.NomenclatureOnlineCategory.Id == _nomenclatureOnlineSettings.CupHolderNomenclatureOnlineCategoryId)
			{
				Entity.ResetNotCupHolderOnlineParameters();
			}
		}

		public IList<NomenclatureOnlineCategory> OnlineCategories
		{
			get => _onlineCategories;
			set => SetField(ref _onlineCategories, value);
		}

		public NomenclatureOnlineCategory SelectedOnlineCategory
		{
			get => Entity.NomenclatureOnlineCategory;
			set
			{
				if(Entity.NomenclatureOnlineCategory != value)
				{
					Entity.NomenclatureOnlineCategory = value;
					UpdateOnlineParameters();
				}
			}
		}

		public bool? HasCooling
		{
			get => Entity.HasCooling;
			set
			{
				if(Entity.HasCooling != value)
				{
					Entity.HasCooling = value;

					if(!value.HasValue || !value.Value)
					{
						Entity.ResetCoolingParameters();
					}
				}
			}
		}

		public bool? HasHeating
		{
			get => Entity.HasHeating;
			set
			{
				if(Entity.HasHeating != value)
				{
					Entity.HasHeating = value;

					if(!value.HasValue || !value.Value)
					{
						Entity.ResetHeatingParameters();
					}
				}
			}
		}

		public IEntityEntryViewModel DependsOnNomenclatureEntryViewModel { get; private set; }

		public bool IsWaterParameters =>
			SelectedOnlineGroup != null
			&& SelectedOnlineGroup.Id == _nomenclatureOnlineSettings.WaterNomenclatureOnlineGroupId;
		public bool IsWaterCoolerParameters =>
			SelectedOnlineCategory != null
			&& SelectedOnlineCategory.Id == _nomenclatureOnlineSettings.KulerNomenclatureOnlineCategoryId;
		public bool IsWaterPumpParameters =>
			SelectedOnlineCategory != null
			&& SelectedOnlineCategory.Id == _nomenclatureOnlineSettings.WaterPumpNomenclatureOnlineCategoryId;
		public bool IsPurifierParameters =>
			SelectedOnlineCategory != null
			&& SelectedOnlineCategory.Id == _nomenclatureOnlineSettings.PurifierNomenclatureOnlineCategoryId;
		public bool IsCupHolderParameters =>
			SelectedOnlineCategory != null
			&& SelectedOnlineCategory.Id == _nomenclatureOnlineSettings.CupHolderNomenclatureOnlineCategoryId;
		public NomenclatureCostPricesViewModel NomenclatureCostPricesViewModel { get; private set; }
		public NomenclaturePurchasePricesViewModel NomenclaturePurchasePricesViewModel { get; private set; }
		public NomenclatureInnerDeliveryPricesViewModel NomenclatureInnerDeliveryPricesViewModel { get; private set; }
		public bool CanCreateNomenclaturesWithInventoryAccountingPermission { get; private set; }
		public bool CanShowConditionAccounting => Entity.HasInventoryAccounting;
		private bool IsNewEntity => Entity.Id == 0;

		#region Commands

		public DelegateCommand SaveCommand { get; }


		private DelegateCommand CreateSaveCommand()
		{
			return new DelegateCommand(
				() =>
				{

					if(_needCheckOnlinePrices)
					{
						if(HasAccessToSitesAndAppsTab && AskQuestion(
							"Было произведено изменение цен номенклатуры, необходимо проверить корректность цен," +
							" установленных на вкладке Сайты и приложения.\n" +
							"Вы хотите переключиться на вкладку Сайты и приложения перед сохранением номенклатуры?"))
						{
							ActiveSitesAndAppsTab = true;
							return;
						}
					}

					Save(true);
				},
				() => true);
		}

		public DelegateCommand CopyPricesWithoutDiscountFromMobileAppToVodovozWebSiteCommand { get; }


		private DelegateCommand CreateCopyPricesWithoutDiscountFromMobileAppToVodovozWebSiteCommand()
		{
			return new DelegateCommand(
				() =>
				{
					CopyPricesWithoutDiscountFromMobileAppToOtherParameters(VodovozWebSiteNomenclatureOnlineParameters);
					UpdateNomenclatureOnlinePricesNodes();
				},
				() => true);
		}

		public DelegateCommand ArchiveCommand { get; }
		public DelegateCommand UnArchiveCommand { get; }
		public AttachedFileInformationsViewModel AttachedFileInformationsViewModel { get; }

		#endregion Commands

		public bool ActiveSitesAndAppsTab
		{
			get => _activeSitesAndAppsTab;
			set
			{
				if(SetField(ref _activeSitesAndAppsTab, value))
				{
					if(value)
					{
						_needCheckOnlinePrices = false;
					}
				}
			}
		}

		public NomenclatureMinimumBalanceByWarehouseViewModel NomenclatureMinimumBalanceByWarehouseViewModel { get; private set; }

		private void SetGlassHolderCheckboxesSelection()
		{
			if(Entity.Category != NomenclatureCategory.equipment
				|| !IsShowGlassHolderSelectionControls
				|| Entity.GlassHolderType == null
				|| Entity.GlassHolderType == GlassHolderType.None)
			{
				IsMagnetGlassHolderSelected = false;
				IsScrewGlassHolderSelected = false;

				return;
			}

			if(Entity.GlassHolderType == GlassHolderType.Magnet)
			{
				IsMagnetGlassHolderSelected = true;
				IsScrewGlassHolderSelected = false;

				return;
			}

			if(Entity.GlassHolderType == GlassHolderType.Screw)
			{
				IsMagnetGlassHolderSelected = false;
				IsScrewGlassHolderSelected = true;

				return;
			}

			if(Entity.GlassHolderType == GlassHolderType.Universal)
			{
				IsMagnetGlassHolderSelected = true;
				IsScrewGlassHolderSelected = true;

				return;
			}

			throw new ArgumentException("");
		}

		private void OnEntityPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof(Entity.Category)
				|| e.PropertyName == nameof(Entity.Kind))
			{
				SetEntityGlassHolderType();

				OnPropertyChanged(nameof(IsShowGlassHolderSelectionControls));
			}
		}

		private void SetEntityGlassHolderType()
		{
			if(Entity.Category == NomenclatureCategory.equipment
					&& IsShowGlassHolderSelectionControls)
			{
				if(!IsMagnetGlassHolderSelected && !IsScrewGlassHolderSelected)
				{
					Entity.GlassHolderType = GlassHolderType.None;
				}
				else if(IsMagnetGlassHolderSelected && !IsScrewGlassHolderSelected)
				{
					Entity.GlassHolderType = GlassHolderType.Magnet;
				}
				else if(!IsMagnetGlassHolderSelected && IsScrewGlassHolderSelected)
				{
					Entity.GlassHolderType = GlassHolderType.Screw;
				}
				else
				{
					Entity.GlassHolderType = GlassHolderType.Universal;
				}

				return;
			}

			Entity.GlassHolderType = null;
			IsMagnetGlassHolderSelected = false;
			IsScrewGlassHolderSelected = false;
		}

		public void AddNotKulerSaleOnlinePrice(NomenclaturePrice price)
		{
			MobileAppNomenclatureOnlineParameters.AddNewNomenclatureOnlinePrice(
				CreateNomenclatureOnlinePrice(price, GoodsOnlineParameterType.ForMobileApp));

			VodovozWebSiteNomenclatureOnlineParameters.AddNewNomenclatureOnlinePrice(
				CreateNomenclatureOnlinePrice(price, GoodsOnlineParameterType.ForVodovozWebSite));
			
			_needCheckOnlinePrices = true;
		}

		public void AddKulerSaleOnlinePrice(AlternativeNomenclaturePrice price)
		{
			KulerSaleWebSiteNomenclatureOnlineParameters.AddNewNomenclatureOnlinePrice(
				CreateNomenclatureOnlinePrice(price, GoodsOnlineParameterType.ForKulerSaleWebSite));
				
			_needCheckOnlinePrices = true;
		}

		public void RemoveNotKulerSalePrices(NomenclaturePrice price)
		{
			var mobileAppPrice = price.Id == 0
				? MobileAppNomenclatureOnlineParameters.NomenclatureOnlinePrices
					.SingleOrDefault(x => x.NomenclaturePrice.Equals(price))
				: MobileAppNomenclatureOnlineParameters.NomenclatureOnlinePrices
					.SingleOrDefault(x => x.NomenclaturePrice.Id == price.Id);

			var vodovozWebSitePrice = price.Id == 0
				? VodovozWebSiteNomenclatureOnlineParameters.NomenclatureOnlinePrices
					.SingleOrDefault(x => x.NomenclaturePrice.Equals(price))
				: VodovozWebSiteNomenclatureOnlineParameters.NomenclatureOnlinePrices
					.SingleOrDefault(x => x.NomenclaturePrice.Id == price.Id);
			
			MobileAppNomenclatureOnlineParameters.RemoveNomenclatureOnlinePrice(mobileAppPrice);
			VodovozWebSiteNomenclatureOnlineParameters.RemoveNomenclatureOnlinePrice(vodovozWebSitePrice);
			
			UpdateNomenclatureOnlinePricesNodes();
		}

		public void RemoveKulerSalePrices(AlternativeNomenclaturePrice alternativePrice)
		{
			var kulerSaleWebSitePrice = alternativePrice.Id == 0
				? KulerSaleWebSiteNomenclatureOnlineParameters.NomenclatureOnlinePrices
					.SingleOrDefault(x => x.NomenclaturePrice.Equals(alternativePrice))
				: KulerSaleWebSiteNomenclatureOnlineParameters.NomenclatureOnlinePrices
					.SingleOrDefault(x => x.NomenclaturePrice.Id == alternativePrice.Id);

			KulerSaleWebSiteNomenclatureOnlineParameters.RemoveNomenclatureOnlinePrice(kulerSaleWebSitePrice);
			
			UpdateNomenclatureOnlinePricesNodes();
		}

		public void SetNeedCheckOnlinePrices(bool value = true)
		{
			_needCheckOnlinePrices = value;
		}

		private void ConfigureValidationContext()
		{
			ValidationContext.ServiceContainer.AddService(typeof(INomenclatureRepository), _nomenclatureRepository);
		}

		private void ConfigureEntryViewModels()
		{
			DependsOnNomenclatureEntryViewModel = new CommonEEVMBuilderFactory<Nomenclature>(this, Entity, UoW, NavigationManager, _lifetimeScope)
				.ForProperty(x => x.DependsOnNomenclature)
				.UseViewModelJournalAndAutocompleter<NomenclaturesJournalViewModel>()
				.UseViewModelDialog<NomenclatureViewModel>()
				.Finish();

			NomenclatureCostPricesViewModel =
				new NomenclatureCostPricesViewModel(Entity, new NomenclatureCostPriceModel(CommonServices.CurrentPermissionService));
			NomenclaturePurchasePricesViewModel =
				new NomenclaturePurchasePricesViewModel(Entity, new NomenclaturePurchasePriceModel(CommonServices.CurrentPermissionService));
			NomenclatureInnerDeliveryPricesViewModel =
				new NomenclatureInnerDeliveryPricesViewModel(Entity, new NomenclatureInnerDeliveryPriceModel(CommonServices.CurrentPermissionService));
		}

		public IEntityEntryViewModel BuildRouteColumnEntryViewModel()
		{
			var routeColumnEntryViewModelBuilder = new CommonEEVMBuilderFactory<Nomenclature>(this, Entity, UoW, NavigationManager, _lifetimeScope);

			return routeColumnEntryViewModelBuilder
				.ForProperty(x => x.RouteListColumn)
				.UseViewModelJournalAndAutocompleter<RouteColumnJournalViewModel>()
				.UseViewModelDialog<RouteColumnViewModel>()
				.Finish();
		}

		private IEntityEntryViewModel CreateProductGroupEEVM()
		{
			var viewModel =
				_productGroupEEVMBuilder
				.SetViewModel(this)
				.SetUnitOfWork(UoW)
				.ForProperty(Entity, x => x.ProductGroup)
				.UseViewModelJournalAndAutocompleter<ProductGroupsJournalViewModel, ProductGroupsJournalFilterViewModel>(
					filter =>
					{
						filter.IsGroupSelectionMode = true;
					})
				.UseViewModelDialog<ProductGroupViewModel>()
				.Finish();


			viewModel.IsEditable = CanEdit;

			viewModel.CanViewEntity =
				CommonServices.CurrentPermissionService.ValidateEntityPermission(typeof(ProductGroup)).CanUpdate;

			return viewModel;
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

			SetPropertyChangeRelation(
				e => e.NomenclatureOnlineGroup,
				() => IsWaterParameters
			);
			
			SetPropertyChangeRelation(
				e => e.NomenclatureOnlineCategory,
				() => IsWaterCoolerParameters,
				() => IsWaterPumpParameters,
				() => IsPurifierParameters,
				() => IsCupHolderParameters
			);
			
			SetPropertyChangeRelation(
				e => e.HasInventoryAccounting,
				() => CanShowConditionAccounting);
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

		public void OnEnumCategoryChanged(object sender, EventArgs e) {
			if(Entity.Category != NomenclatureCategory.deposit) {
				Entity.TypeOfDepositCategory = null;
			}

			if(Entity.Category != NomenclatureCategory.equipment)
			{
				Entity.GlassHolderType = null;
			}
		}

		public void OnEnumCategoryChangedByUser(object sender, EventArgs e) {
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
				CommonServices.CurrentPermissionService.ValidatePresetPermission(
					Vodovoz.Permissions.Nomenclature.CanCreateAndArcNomenclatures);
			CanCreateNomenclaturesWithInventoryAccountingPermission =
				CommonServices.CurrentPermissionService.ValidatePresetPermission(
					Vodovoz.Permissions.Nomenclature.CanCreateNomenclaturesWithInventoryAccounting);
			CanEditAlternativeNomenclaturePrices =
				CommonServices.CurrentPermissionService.ValidatePresetPermission(
					Vodovoz.Permissions.Nomenclature.CanEditAlternativeNomenclaturePrices);
			HasAccessToSitesAndAppsTab =
				CommonServices.CurrentPermissionService.ValidatePresetPermission(
					Vodovoz.Permissions.Nomenclature.HasAccessToSitesAndAppsTab);
		}

		private void SetProperties()
		{
			OldHasConditionAccounting = Entity.HasConditionAccounting;
		}

		private void ConfigureOnlineParameters()
		{
			MobileAppNomenclatureOnlineParameters = GetNomenclatureOnlineParameters(GoodsOnlineParameterType.ForMobileApp);
			VodovozWebSiteNomenclatureOnlineParameters = GetNomenclatureOnlineParameters(GoodsOnlineParameterType.ForVodovozWebSite);
			KulerSaleWebSiteNomenclatureOnlineParameters = GetNomenclatureOnlineParameters(GoodsOnlineParameterType.ForKulerSaleWebSite);
			
			MobileAppNomenclatureOnlineCatalogs = UoW.GetAll<MobileAppNomenclatureOnlineCatalog>().ToList();
			VodovozWebSiteNomenclatureOnlineCatalogs = UoW.GetAll<VodovozWebSiteNomenclatureOnlineCatalog>().ToList();
			KulerSaleWebSiteNomenclatureOnlineCatalogs = UoW.GetAll<KulerSaleWebSiteNomenclatureOnlineCatalog>().ToList();
			NomenclatureOnlineGroups = UoW.GetAll<NomenclatureOnlineGroup>().ToList();
			
			UpdateOnlineCategories();
			UpdateNomenclatureOnlinePricesNodes();
		}

		private NomenclatureOnlineParameters GetNomenclatureOnlineParameters(GoodsOnlineParameterType type)
		{
			var parameters = Entity.NomenclatureOnlineParameters.SingleOrDefault(x => x.Type == type);
			return parameters ?? CreateNomenclatureOnlineParameters(type);
		}

		private NomenclatureOnlineParameters CreateNomenclatureOnlineParameters(GoodsOnlineParameterType type)
		{
			NomenclatureOnlineParameters parameters = null;
			switch(type)
			{
				case GoodsOnlineParameterType.ForMobileApp:
					parameters = new MobileAppNomenclatureOnlineParameters();

					foreach(var nomenclaturePrice in Entity.NomenclaturePrice)
					{
						parameters.AddNewNomenclatureOnlinePrice(
							CreateNomenclatureOnlinePrice(
								nomenclaturePrice,
								GoodsOnlineParameterType.ForMobileApp));
					}
					break;
				case GoodsOnlineParameterType.ForVodovozWebSite:
					parameters = new VodovozWebSiteNomenclatureOnlineParameters();
					
					foreach(var nomenclaturePrice in Entity.NomenclaturePrice)
					{
						parameters.AddNewNomenclatureOnlinePrice(
							CreateNomenclatureOnlinePrice(
								nomenclaturePrice,
								GoodsOnlineParameterType.ForVodovozWebSite));
					}
					break;
				case GoodsOnlineParameterType.ForKulerSaleWebSite:
					parameters = new KulerSaleWebSiteNomenclatureOnlineParameters();

					foreach(var alternativeNomenclaturePrice in Entity.AlternativeNomenclaturePrices)
					{
						parameters.AddNewNomenclatureOnlinePrice(
							CreateNomenclatureOnlinePrice(
								alternativeNomenclaturePrice,
								GoodsOnlineParameterType.ForKulerSaleWebSite));
					}
					break;
			}

			parameters.Nomenclature = Entity;
			Entity.NomenclatureOnlineParameters.Add(parameters);
			return parameters;
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
			_logger.LogInformation("Сохраняем номенклатуру...");
			Entity.SetNomenclatureCreationInfo(_userRepository);
			
			if(PriceChanged && Entity.Id > 0)
			{
				_logger.LogInformation("Проверяем связанные с ней промонаборы...");
				CheckPromoSetsWithNomenclature();
			}
			
			return base.BeforeSave();
		}

		public void UpdateNomenclatureOnlinePricesNodes()
		{
			NomenclatureOnlinePrices.Clear();
			var notSortedNodes = new List<NomenclatureOnlinePricesNode>();
			var onlinePrices = new Dictionary<decimal, NomenclatureOnlinePricesNode>();

			foreach(var nomenclatureOnlineParameter in Entity.NomenclatureOnlineParameters)
			{
				foreach(var onlinePrice in nomenclatureOnlineParameter.NomenclatureOnlinePrices)
				{
					onlinePrices.TryGetValue(onlinePrice.NomenclaturePrice.MinCount, out var nomenclatureOnlinePricesNode);

					if(nomenclatureOnlinePricesNode is null)
					{
						switch(onlinePrice.Type)
						{
							case GoodsOnlineParameterType.ForMobileApp:
								nomenclatureOnlinePricesNode = new NomenclatureOnlinePricesNode
								{
									MobileAppNomenclatureOnlinePrice = onlinePrice
								};
								break;
							case GoodsOnlineParameterType.ForVodovozWebSite:
								nomenclatureOnlinePricesNode = new NomenclatureOnlinePricesNode
								{
									VodovozWebSiteNomenclatureOnlinePrice = onlinePrice
								};
								break;
							case GoodsOnlineParameterType.ForKulerSaleWebSite:
								nomenclatureOnlinePricesNode = new NomenclatureOnlinePricesNode
								{
									KulerSaleWebSiteNomenclatureOnlinePrice = onlinePrice
								};
								break;
						}

						var minCount = onlinePrice.NomenclaturePrice.MinCount;
						onlinePrices.Add(minCount, nomenclatureOnlinePricesNode);
						notSortedNodes.Add(nomenclatureOnlinePricesNode);
					}
					else
					{
						switch(onlinePrice.Type)
						{
							case GoodsOnlineParameterType.ForMobileApp:
								nomenclatureOnlinePricesNode.MobileAppNomenclatureOnlinePrice = onlinePrice;
								break;
							case GoodsOnlineParameterType.ForVodovozWebSite:
								nomenclatureOnlinePricesNode.VodovozWebSiteNomenclatureOnlinePrice = onlinePrice;
								break;
							case GoodsOnlineParameterType.ForKulerSaleWebSite:
								nomenclatureOnlinePricesNode.KulerSaleWebSiteNomenclatureOnlinePrice = onlinePrice;
								break;
						}
					}
				}
			}

			foreach(var node in notSortedNodes.OrderBy(x => x.MinCount))
			{
				NomenclatureOnlinePrices.Add(node);
			}
		}

		private NomenclatureOnlinePrice CreateNomenclatureOnlinePrice(
			NomenclaturePriceBase nomenclaturePrice,
			GoodsOnlineParameterType type)
		{
			switch(type)
			{
				case GoodsOnlineParameterType.ForMobileApp:
					return new MobileAppNomenclatureOnlinePrice
					{
						NomenclaturePrice = nomenclaturePrice
					};
				case GoodsOnlineParameterType.ForVodovozWebSite:
					return new VodovozWebSiteNomenclatureOnlinePrice
					{
						NomenclaturePrice = nomenclaturePrice
					};
				case GoodsOnlineParameterType.ForKulerSaleWebSite:
					return new KulerSaleWebSiteNomenclatureOnlinePrice
					{
						NomenclaturePrice = nomenclaturePrice
					};
				default:
					throw new ArgumentOutOfRangeException(nameof(type), type, null);
			}
		}
		
		private void CheckPromoSetsWithNomenclature()
		{
			var promoSets = _nomenclatureRepository.GetPromoSetsWithNomenclature(UoW, Entity.Id);
			
			if(promoSets.Any())
			{
				var stringBuilder = new StringBuilder();
				stringBuilder.Append("Изменены цены на товар, входящий в состав промонаборов:\n");
				
				foreach(var item in promoSets)
				{
					stringBuilder.Append($"Код: {item.Id} название: {item.Name}\n");
				}
				
				ShowInfoMessage(stringBuilder.ToString());
			}
		}
		
		private void CopyPricesWithoutDiscountFromMobileAppToOtherParameters(NomenclatureOnlineParameters nomenclatureOnlineParameters)
		{
			for(var i = 0; i < MobileAppNomenclatureOnlineParameters.NomenclatureOnlinePrices.Count; i++)
			{
				var mobileAppPrice = MobileAppNomenclatureOnlineParameters.NomenclatureOnlinePrices[i];
				nomenclatureOnlineParameters.NomenclatureOnlinePrices[i].PriceWithoutDiscount = mobileAppPrice.PriceWithoutDiscount;
			}
		}

		private void Archive()
		{
			if(!UoWGeneric.IsNew)
			{
				var result = _nomenclatureService.Archive(UoW, Entity);

				if(result.IsFailure)
				{
					_interactiveService.ShowMessage(ImportanceLevel.Error, string.Join("\n", result.Errors.Select(e => e.Message)), "Не удалось архивирвоать номенклатуру");
				}
			}
			else
			{
				Entity.IsArchive = true;
			}
		}

		private void UnArchive()
		{
			Entity.IsArchive = false;
		}

		public override void Dispose()
		{
			_lifetimeScope = null;
			base.Dispose();
		}

		public override bool Save(bool close)
		{
			if(!base.Save(false))
			{
				return false;
			}

			AddAttachedFilesIfNeeded();
			UpdateAttachedFilesIfNeeded();
			DeleteAttachedFilesIfNeeded();
			AttachedFileInformationsViewModel.ClearPersistentInformationCommand.Execute();

			return base.Save(close);
		}

		private void AddAttachedFilesIfNeeded()
		{
			var errors = new Dictionary<string, string>();
			var repeat = false;

			if(!AttachedFileInformationsViewModel.FilesToAddOnSave.Any())
			{
				return;
			}

			do
			{
				foreach(var fileName in AttachedFileInformationsViewModel.FilesToAddOnSave)
				{
					var result = _nomenclatureFileStorageService.CreateFileAsync(Entity, fileName,
					new MemoryStream(AttachedFileInformationsViewModel.AttachedFiles[fileName]), _cancellationTokenSource.Token)
						.GetAwaiter()
						.GetResult();

					if(result.IsFailure && !result.Errors.All(x => x.Code == Application.Errors.S3.FileAlreadyExists.ToString()))
					{
						errors.Add(fileName, string.Join(", ", result.Errors.Select(e => e.Message)));
					}
				}

				if(errors.Any())
				{
					repeat = _interactiveService.Question(
						"Не удалось загрузить файлы:\n" +
						string.Join("\n- ", errors.Select(fekv => $"{fekv.Key} - {fekv.Value}")) + "\n" +
						"\n" +
						"Повторить попытку?",
						"Ошибка загрузки файлов");

					errors.Clear();
				}
				else
				{
					repeat = false;
				}
			}
			while(repeat);
		}

		private void UpdateAttachedFilesIfNeeded()
		{
			if(!AttachedFileInformationsViewModel.FilesToUpdateOnSave.Any())
			{
				return;
			}

			foreach(var fileName in AttachedFileInformationsViewModel.FilesToUpdateOnSave)
			{
				_nomenclatureFileStorageService.UpdateFileAsync(Entity, fileName, new MemoryStream(AttachedFileInformationsViewModel.AttachedFiles[fileName]), _cancellationTokenSource.Token)
					.GetAwaiter()
					.GetResult();
			}
		}

		private void DeleteAttachedFilesIfNeeded()
		{
			if(!AttachedFileInformationsViewModel.FilesToDeleteOnSave.Any())
			{
				return;
			}

			foreach(var fileName in AttachedFileInformationsViewModel.FilesToDeleteOnSave)
			{
				_nomenclatureFileStorageService.DeleteFileAsync(Entity, fileName, _cancellationTokenSource.Token)
					.GetAwaiter()
					.GetResult();
			}
		}
	}
}
