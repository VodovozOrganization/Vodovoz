using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
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
using Vodovoz.Domain.Goods.NomenclaturesOnlineParameters;
using Vodovoz.EntityRepositories;
using Vodovoz.EntityRepositories.Goods;
using Vodovoz.Extensions;
using Vodovoz.Services;
using Vodovoz.Models;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Dialogs.Nodes;
using Vodovoz.ViewModels.ViewModels.Goods;
using VodovozInfrastructure.StringHandlers;

namespace Vodovoz.ViewModels.Dialogs.Goods
{
	public class NomenclatureViewModel : EntityTabViewModelBase<Nomenclature>, IAskSaveOnCloseViewModel
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();

		private readonly IEmployeeService _employeeService;
		private readonly INomenclatureRepository _nomenclatureRepository;
		private readonly IUserRepository _userRepository;
		private readonly INomenclatureOnlineParametersProvider _nomenclatureOnlineParametersProvider;
		private NomenclatureOnlineParameters _mobileAppNomenclatureOnlineParameters;
		private NomenclatureOnlineParameters _vodovozWebSiteNomenclatureOnlineParameters;
		private NomenclatureOnlineParameters _kulerSaleWebSiteNomenclatureOnlineParameters;
		private bool _needCheckOnlinePrices;
		private IList<NomenclatureOnlineCategory> _onlineCategories;

		public Action PricesViewSaveChanges;

		public NomenclatureViewModel(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory uowFactory,
			ICommonServices commonServices,
			IEmployeeService employeeService,
			INomenclatureJournalFactory nomenclatureSelectorFactory,
			ICounterpartyJournalFactory counterpartySelectorFactory,
			INomenclatureRepository nomenclatureRepository,
			IUserRepository userRepository,
			IStringHandler stringHandler,
			INomenclatureOnlineParametersProvider nomenclatureOnlineParametersProvider) : base(uowBuilder, uowFactory, commonServices)
		{
			if(nomenclatureSelectorFactory is null)
			{
				throw new ArgumentNullException(nameof(nomenclatureSelectorFactory));
			}

			StringHandler = stringHandler ?? throw new ArgumentNullException(nameof(stringHandler));
			_employeeService = employeeService ?? throw new ArgumentNullException(nameof(employeeService));
			_nomenclatureRepository = nomenclatureRepository ?? throw new ArgumentNullException(nameof(nomenclatureRepository));
			_userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
			_nomenclatureOnlineParametersProvider =
				nomenclatureOnlineParametersProvider ?? throw new ArgumentNullException(nameof(nomenclatureOnlineParametersProvider));
			NomenclatureSelectorFactory = nomenclatureSelectorFactory.GetDefaultNomenclatureSelectorFactory();
			CounterpartySelectorFactory =
				(counterpartySelectorFactory ?? throw new ArgumentNullException(nameof(counterpartySelectorFactory)))
				.CreateCounterpartyAutocompleteSelectorFactory();

			ConfigureEntryViewModels();
			ConfigureOnlineParameters();
			ConfigureEntityPropertyChanges();
			ConfigureValidationContext();
			SetPermissions();
		}

		public IStringHandler StringHandler { get; }
		public IEntityAutocompleteSelectorFactory NomenclatureSelectorFactory { get; }
		public IEntityAutocompleteSelectorFactory CounterpartySelectorFactory { get; }

		public GenericObservableList<NomenclatureOnlinePricesNode> NomenclatureOnlinePrices { get; private set; }
			= new GenericObservableList<NomenclatureOnlinePricesNode>();

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
		public bool AskSaveOnClose => CanEdit;
		public bool UserCanCreateNomenclaturesWithInventoryAccounting =>
			IsNewEntity && CanCreateNomenclaturesWithInventoryAccountingPermission;

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

			if(Entity.NomenclatureOnlineGroup.Id == _nomenclatureOnlineParametersProvider.WaterNomenclatureOnlineGroupId)
			{
				Entity.ResetNotWaterOnlineParameters();
			}

			if(Entity.NomenclatureOnlineCategory is null)
			{
				return;
			}
			
			if(Entity.NomenclatureOnlineCategory.Id == _nomenclatureOnlineParametersProvider.KulerNomenclatureOnlineCategoryId)
			{
				Entity.ResetNotKulerOnlineParameters();
			}
			
			if(Entity.NomenclatureOnlineCategory.Id == _nomenclatureOnlineParametersProvider.PurifierNomenclatureOnlineCategoryId)
			{
				Entity.ResetNotPurifierOnlineParameters();
			}
			
			if(Entity.NomenclatureOnlineCategory.Id == _nomenclatureOnlineParametersProvider.WaterPumpNomenclatureOnlineCategoryId)
			{
				Entity.ResetNotWaterPumpOnlineParameters();
			}
			
			if(Entity.NomenclatureOnlineCategory.Id == _nomenclatureOnlineParametersProvider.CupHolderNomenclatureOnlineCategoryId)
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

		public bool IsWaterParameters =>
			SelectedOnlineGroup != null
			&& SelectedOnlineGroup.Id == _nomenclatureOnlineParametersProvider.WaterNomenclatureOnlineGroupId;
		public bool IsWaterCoolerParameters =>
			SelectedOnlineCategory != null
			&& SelectedOnlineCategory.Id == _nomenclatureOnlineParametersProvider.KulerNomenclatureOnlineCategoryId;
		public bool IsWaterPumpParameters =>
			SelectedOnlineCategory != null
			&& SelectedOnlineCategory.Id == _nomenclatureOnlineParametersProvider.WaterPumpNomenclatureOnlineCategoryId;
		public bool IsPurifierParameters =>
			SelectedOnlineCategory != null
			&& SelectedOnlineCategory.Id == _nomenclatureOnlineParametersProvider.PurifierNomenclatureOnlineCategoryId;
		public bool IsCupHolderParameters =>
			SelectedOnlineCategory != null
			&& SelectedOnlineCategory.Id == _nomenclatureOnlineParametersProvider.CupHolderNomenclatureOnlineCategoryId;
		public NomenclatureCostPricesViewModel NomenclatureCostPricesViewModel { get; private set; }
		public NomenclaturePurchasePricesViewModel NomenclaturePurchasePricesViewModel { get; private set; }
		public NomenclatureInnerDeliveryPricesViewModel NomenclatureInnerDeliveryPricesViewModel { get; private set; }
		private bool IsNewEntity => Entity.Id == 0;
		private bool CanCreateNomenclaturesWithInventoryAccountingPermission { get; set; }
		
		public void AddNotKulerSaleOnlinePrice(NomenclaturePrice price)
		{
			MobileAppNomenclatureOnlineParameters.AddNewNomenclatureOnlinePrice(
				CreateNomenclatureOnlinePrice(price, NomenclatureOnlineParameterType.ForMobileApp));

			VodovozWebSiteNomenclatureOnlineParameters.AddNewNomenclatureOnlinePrice(
				CreateNomenclatureOnlinePrice(price, NomenclatureOnlineParameterType.ForVodovozWebSite));
			
			_needCheckOnlinePrices = true;
		}
		
		public void AddKulerSaleOnlinePrice(AlternativeNomenclaturePrice price)
		{
			KulerSaleWebSiteNomenclatureOnlineParameters.AddNewNomenclatureOnlinePrice(
				CreateNomenclatureOnlinePrice(price, NomenclatureOnlineParameterType.ForKulerSaleWebSite));
				
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
			NomenclatureCostPricesViewModel =
				new NomenclatureCostPricesViewModel(Entity, new NomenclatureCostPriceModel(CommonServices.CurrentPermissionService));
			NomenclaturePurchasePricesViewModel =
				new NomenclaturePurchasePricesViewModel(Entity, new NomenclaturePurchasePriceModel(CommonServices.CurrentPermissionService));
			NomenclatureInnerDeliveryPricesViewModel =
				new NomenclatureInnerDeliveryPricesViewModel(Entity, new NomenclatureInnerDeliveryPriceModel(CommonServices.CurrentPermissionService));
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
			HasAccessToSitesAndAppsTab =
				CommonServices.CurrentPermissionService.ValidatePresetPermission("Nomenclature.HasAccessToSitesAndAppsTab");
		}
		
		private void ConfigureOnlineParameters()
		{
			MobileAppNomenclatureOnlineParameters = GetNomenclatureOnlineParameters(NomenclatureOnlineParameterType.ForMobileApp);
			VodovozWebSiteNomenclatureOnlineParameters = GetNomenclatureOnlineParameters(NomenclatureOnlineParameterType.ForVodovozWebSite);
			KulerSaleWebSiteNomenclatureOnlineParameters = GetNomenclatureOnlineParameters(NomenclatureOnlineParameterType.ForKulerSaleWebSite);
			
			MobileAppNomenclatureOnlineCatalogs = UoW.GetAll<MobileAppNomenclatureOnlineCatalog>().ToList();
			VodovozWebSiteNomenclatureOnlineCatalogs = UoW.GetAll<VodovozWebSiteNomenclatureOnlineCatalog>().ToList();
			KulerSaleWebSiteNomenclatureOnlineCatalogs = UoW.GetAll<KulerSaleWebSiteNomenclatureOnlineCatalog>().ToList();
			NomenclatureOnlineGroups = UoW.GetAll<NomenclatureOnlineGroup>().ToList();
			
			UpdateOnlineCategories();
			UpdateNomenclatureOnlinePricesNodes();
		}

		private NomenclatureOnlineParameters GetNomenclatureOnlineParameters(NomenclatureOnlineParameterType type)
		{
			var parameters = Entity.NomenclatureOnlineParameters.SingleOrDefault(x => x.Type == type);
			return parameters ?? CreateNomenclatureOnlineParameters(type);
		}

		private NomenclatureOnlineParameters CreateNomenclatureOnlineParameters(NomenclatureOnlineParameterType type)
		{
			NomenclatureOnlineParameters parameters = null;
			switch(type)
			{
				case NomenclatureOnlineParameterType.ForMobileApp:
					parameters = new MobileAppNomenclatureOnlineParameters();

					foreach(var nomenclaturePrice in Entity.NomenclaturePrice)
					{
						parameters.AddNewNomenclatureOnlinePrice(
							CreateNomenclatureOnlinePrice(
								nomenclaturePrice,
								NomenclatureOnlineParameterType.ForMobileApp));
					}
					break;
				case NomenclatureOnlineParameterType.ForVodovozWebSite:
					parameters = new VodovozWebSiteNomenclatureOnlineParameters();
					
					foreach(var nomenclaturePrice in Entity.NomenclaturePrice)
					{
						parameters.AddNewNomenclatureOnlinePrice(
							CreateNomenclatureOnlinePrice(
								nomenclaturePrice,
								NomenclatureOnlineParameterType.ForVodovozWebSite));
					}
					break;
				case NomenclatureOnlineParameterType.ForKulerSaleWebSite:
					parameters = new KulerSaleWebSiteNomenclatureOnlineParameters();

					foreach(var alternativeNomenclaturePrice in Entity.AlternativeNomenclaturePrices)
					{
						parameters.AddNewNomenclatureOnlinePrice(
							CreateNomenclatureOnlinePrice(
								alternativeNomenclaturePrice,
								NomenclatureOnlineParameterType.ForKulerSaleWebSite));
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
			logger.Info("Сохраняем номенклатуру...");
			Entity.SetNomenclatureCreationInfo(_userRepository);
			PricesViewSaveChanges?.Invoke();
			return base.BeforeSave();
		}

		#region Commands

		private DelegateCommand saveCommand = null;
		private bool _activeSitesAndAppsTab;

		public DelegateCommand SaveCommand =>
			saveCommand ?? (saveCommand = new DelegateCommand(
				() => {

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
				() => true
			)
		);

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

		#endregion

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
							case NomenclatureOnlineParameterType.ForMobileApp:
								nomenclatureOnlinePricesNode = new NomenclatureOnlinePricesNode
								{
									MobileAppNomenclatureOnlinePrice = onlinePrice
								};
								break;
							case NomenclatureOnlineParameterType.ForVodovozWebSite:
								nomenclatureOnlinePricesNode = new NomenclatureOnlinePricesNode
								{
									VodovozWebSiteNomenclatureOnlinePrice = onlinePrice
								};
								break;
							case NomenclatureOnlineParameterType.ForKulerSaleWebSite:
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
							case NomenclatureOnlineParameterType.ForMobileApp:
								nomenclatureOnlinePricesNode.MobileAppNomenclatureOnlinePrice = onlinePrice;
								break;
							case NomenclatureOnlineParameterType.ForVodovozWebSite:
								nomenclatureOnlinePricesNode.VodovozWebSiteNomenclatureOnlinePrice = onlinePrice;
								break;
							case NomenclatureOnlineParameterType.ForKulerSaleWebSite:
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
			NomenclatureOnlineParameterType type)
		{
			switch(type)
			{
				case NomenclatureOnlineParameterType.ForMobileApp:
					return new MobileAppNomenclatureOnlinePrice
					{
						NomenclaturePrice = nomenclaturePrice
					};
				case NomenclatureOnlineParameterType.ForVodovozWebSite:
					return new VodovozWebSiteNomenclatureOnlinePrice
					{
						NomenclaturePrice = nomenclaturePrice
					};
				case NomenclatureOnlineParameterType.ForKulerSaleWebSite:
					return new KulerSaleWebSiteNomenclatureOnlinePrice
					{
						NomenclaturePrice = nomenclaturePrice
					};
				default:
					throw new ArgumentOutOfRangeException(nameof(type), type, null);
			}
		}
	}
}
