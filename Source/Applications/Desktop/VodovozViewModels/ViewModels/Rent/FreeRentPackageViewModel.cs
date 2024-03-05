using System;
using System.Linq;
using Autofac;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using Vodovoz.EntityRepositories.RentPackages;
using QS.ViewModels.Control.EEVM;
using Vodovoz.Domain.Goods.NomenclaturesOnlineParameters;
using Vodovoz.Domain.Goods.Rent;
using Vodovoz.ViewModels.Dialogs.Goods;
using Vodovoz.ViewModels.Journals.FilterViewModels.Goods;
using Vodovoz.ViewModels.Journals.JournalViewModels.Goods;
using Vodovoz.ViewModels.ViewModels.Goods;

namespace Vodovoz.ViewModels.ViewModels.Rent
{
	public class FreeRentPackageViewModel : EntityTabViewModelBase<FreeRentPackage>
	{
		private readonly IRentPackageRepository _rentPackageRepository;
		private FreeRentPackageOnlineParameters _kulerSaleWebSiteFreeRentPackageOnlineParameters;
		private FreeRentPackageOnlineParameters _vodovozWebSiteFreeRentPackageOnlineParameters;
		private FreeRentPackageOnlineParameters _mobileAppFreeRentPackageOnlineParameters;
		private ILifetimeScope _lifetimeScope;
		private int _currentPage;
		private bool _informationTabActive;
		private bool _sitesAndAppsTabActive;

		public FreeRentPackageViewModel(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			ILifetimeScope lifetimeScope,
			INavigationManager navigationManager,
			ICommonServices commonServices,
			IRentPackageRepository rentPackageRepository) : base(uowBuilder, unitOfWorkFactory, commonServices, navigationManager)
		{
			_lifetimeScope = lifetimeScope ?? throw new ArgumentNullException(nameof(lifetimeScope));
			_rentPackageRepository = rentPackageRepository ?? throw new ArgumentNullException(nameof(rentPackageRepository));

			ConfigureValidateContext();
			ConfigureEntryViewModels();
			ConfigureOnlineParameters();
			ConfigurePropertyChangeRelations();
		}

		public bool IsNewEntity => UoW.IsNew;
		public string IdString => Entity.Id.ToString();

		public int CurrentPage
		{
			get => _currentPage;
			set => SetField(ref _currentPage, value);
		}

		public FreeRentPackageOnlineParameters MobileAppFreeRentPackageOnlineParameters
		{
			get => _mobileAppFreeRentPackageOnlineParameters;
			set => SetField(ref _mobileAppFreeRentPackageOnlineParameters, value);
		}

		public FreeRentPackageOnlineParameters VodovozWebSiteFreeRentPackageOnlineParameters
		{
			get => _vodovozWebSiteFreeRentPackageOnlineParameters;
			set => SetField(ref _vodovozWebSiteFreeRentPackageOnlineParameters, value);
		}

		public FreeRentPackageOnlineParameters KulerSaleWebSiteFreeRentPackageOnlineParameters
		{
			get => _kulerSaleWebSiteFreeRentPackageOnlineParameters;
			set => SetField(ref _kulerSaleWebSiteFreeRentPackageOnlineParameters, value);
		}

		public bool InformationTabActive
		{
			get => _informationTabActive;
			set
			{
				if(SetField(ref _informationTabActive, value) && value)
				{
					CurrentPage = 0;
				}
			}
		}

		public bool SitesAndAppsTabActive
		{
			get => _sitesAndAppsTabActive;
			set
			{
				if(SetField(ref _sitesAndAppsTabActive, value) && value)
				{
					CurrentPage = 1;
				}
			}
		}

		public IEntityEntryViewModel DepositServiceNomenclatureViewModel { get; private set; }
		public IEntityEntryViewModel EquipmentKindViewModel { get; private set; }

		private void ConfigureValidateContext()
		{
			ValidationContext.ServiceContainer.AddService(typeof(IRentPackageRepository), _rentPackageRepository);
		}

		private void ConfigureEntryViewModels()
		{
			var builder = new CommonEEVMBuilderFactory<FreeRentPackage>(this, Entity, UoW, NavigationManager, _lifetimeScope);

			DepositServiceNomenclatureViewModel = builder
				.ForProperty(x => x.DepositService)
				.UseViewModelJournalAndAutocompleter<NomenclaturesJournalViewModel, NomenclatureFilterViewModel>(filter => { })
				.UseViewModelDialog<NomenclatureViewModel>()
				.Finish();

			EquipmentKindViewModel = builder
				.ForProperty(x => x.EquipmentKind)
				.UseViewModelJournalAndAutocompleter<EquipmentKindJournalViewModel>()
				.UseViewModelDialog<EquipmentKindViewModel>()
				.Finish();
		}

		private void ConfigureOnlineParameters()
		{
			MobileAppFreeRentPackageOnlineParameters = GetPackageOnlineParameters(GoodsOnlineParameterType.ForMobileApp);
			VodovozWebSiteFreeRentPackageOnlineParameters = GetPackageOnlineParameters(GoodsOnlineParameterType.ForVodovozWebSite);
			KulerSaleWebSiteFreeRentPackageOnlineParameters = GetPackageOnlineParameters(GoodsOnlineParameterType.ForKulerSaleWebSite);
		}

		private FreeRentPackageOnlineParameters GetPackageOnlineParameters(GoodsOnlineParameterType parameterType)
		{
			var parameters = Entity.OnlineParameters.SingleOrDefault(x => x.Type == parameterType);
			return parameters ?? CreatePackageOnlineParameters(parameterType);
		}

		private FreeRentPackageOnlineParameters CreatePackageOnlineParameters(GoodsOnlineParameterType parameterType)
		{
			FreeRentPackageOnlineParameters parameters = null;

			switch(parameterType)
			{
				case GoodsOnlineParameterType.ForMobileApp:
					parameters = new MobileAppFreeRentPackageOnlineParameters();
					break;
				case GoodsOnlineParameterType.ForVodovozWebSite:
					parameters = new VodovozWebSiteFreeRentPackageOnlineParameters();
					break;
				case GoodsOnlineParameterType.ForKulerSaleWebSite:
					parameters = new KulerSaleWebSiteFreeRentPackageOnlineParameters();
					break;
			}

			parameters.FreeRentPackage = Entity;
			Entity.OnlineParameters.Add(parameters);
			return parameters;
		}

		private void ConfigurePropertyChangeRelations()
		{
			SetPropertyChangeRelation(e => e.Id,
				() => IdString);
		}

		public override void Dispose()
		{
			_lifetimeScope = null;
			base.Dispose();
		}
	}
}
