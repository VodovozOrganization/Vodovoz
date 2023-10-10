using System;
using System.Linq;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using Vodovoz.EntityRepositories.RentPackages;
using QS.Project.Journal.EntitySelector;
using Vodovoz.Domain.Goods.NomenclaturesOnlineParameters;
using Vodovoz.Domain.Goods.Rent;
using Vodovoz.TempAdapters;

namespace Vodovoz.ViewModels.ViewModels.Rent
{
    public class FreeRentPackageViewModel : EntityTabViewModelBase<FreeRentPackage>
    {
		private readonly INomenclatureJournalFactory _nomenclatureJournalFactory;
		private readonly IRentPackageRepository _rentPackageRepository;
		private IEntityAutocompleteSelectorFactory _depositServiceSelectorFactory;
		private FreeRentPackageOnlineParameters _kulerSaleWebSiteFreeRentPackageOnlineParameters;
		private FreeRentPackageOnlineParameters _vodovozWebSiteFreeRentPackageOnlineParameters;
		private FreeRentPackageOnlineParameters _mobileAppFreeRentPackageOnlineParameters;
		private int _currentPage;
		private bool _informationTabActive;
		private bool _sitesAndAppsTabActive;

		public FreeRentPackageViewModel(
            IEntityUoWBuilder uowBuilder,
            IUnitOfWorkFactory unitOfWorkFactory,
            ICommonServices commonServices,
			INomenclatureJournalFactory nomenclatureJournalFactory,
            IRentPackageRepository rentPackageRepository) : base(uowBuilder, unitOfWorkFactory, commonServices)
	    {
			_nomenclatureJournalFactory = nomenclatureJournalFactory ?? throw new ArgumentNullException(nameof(nomenclatureJournalFactory));
			_rentPackageRepository = rentPackageRepository ?? throw new ArgumentNullException(nameof(rentPackageRepository));
		    
		    ConfigureValidateContext();
			ConfigureOnlineParameters();
			ConfigurePropertyChangeRelations();
		}

		public IEntityAutocompleteSelectorFactory DepositServiceSelectorFactory
		{
			get
			{
				if(_depositServiceSelectorFactory == null)
				{
					_depositServiceSelectorFactory = _nomenclatureJournalFactory.GetDepositSelectorFactory();
				}
				return _depositServiceSelectorFactory;
			}
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

		private void ConfigureValidateContext()
        {
	        ValidationContext.ServiceContainer.AddService(typeof(IRentPackageRepository), _rentPackageRepository);
        }
		
		private void ConfigureOnlineParameters()
		{
			MobileAppFreeRentPackageOnlineParameters = GetPackageOnlineParameters(NomenclatureOnlineParameterType.ForMobileApp);
			VodovozWebSiteFreeRentPackageOnlineParameters = GetPackageOnlineParameters(NomenclatureOnlineParameterType.ForVodovozWebSite);
			KulerSaleWebSiteFreeRentPackageOnlineParameters = GetPackageOnlineParameters(NomenclatureOnlineParameterType.ForKulerSaleWebSite);
		}

		private FreeRentPackageOnlineParameters GetPackageOnlineParameters(NomenclatureOnlineParameterType parameterType)
		{
			var parameters = Entity.OnlineParameters.SingleOrDefault(x => x.Type == parameterType);
			return parameters ?? CreatePackageOnlineParameters(parameterType);
		}

		private FreeRentPackageOnlineParameters CreatePackageOnlineParameters(NomenclatureOnlineParameterType parameterType)
		{
			FreeRentPackageOnlineParameters parameters = null;
			
			switch(parameterType)
			{
				case NomenclatureOnlineParameterType.ForMobileApp:
					parameters = new MobileAppFreeRentPackageOnlineParameters();
					break;
				case NomenclatureOnlineParameterType.ForVodovozWebSite:
					parameters = new VodovozWebSiteFreeRentPackageOnlineParameters();
					break;
				case NomenclatureOnlineParameterType.ForKulerSaleWebSite:
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
	}
}
