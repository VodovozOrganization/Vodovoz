using System;
using System.Linq;
using QS.Commands;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Goods.NomenclaturesOnlineParameters;
using Vodovoz.Domain.Goods.PromotionalSetsOnlineParameters;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories;
using Vodovoz.EntityRepositories.Goods;
using Vodovoz.Services;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Journals.FilterViewModels.Goods;
using Vodovoz.ViewModels.Journals.JournalNodes.Goods;
using Vodovoz.ViewModels.Journals.JournalViewModels.Goods;
using VodovozInfrastructure.StringHandlers;

namespace Vodovoz.ViewModels.Orders
{
	public class PromotionalSetViewModel : EntityTabViewModelBase<PromotionalSet>, IPermissionResult
	{
		private readonly IEmployeeService _employeeService;
		private readonly INomenclatureRepository _nomenclatureRepository;
		private readonly IUserRepository _userRepository;
		private readonly ICounterpartyJournalFactory _counterpartySelectorFactory;
		private readonly INomenclatureJournalFactory _nomenclatureSelectorFactory;

		private PromotionalSetItem _selectedPromoItem;
		private PromotionalSetActionBase _selectedAction;
		private WidgetViewModelBase _selectedActionViewModel;
		private bool _informationTabActive;
		private bool _sitesAndAppsTabActive;
		private int _currentPage;
		
		public PromotionalSetViewModel(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			IEmployeeService employeeService,
			ICounterpartyJournalFactory counterpartySelectorFactory,
			INomenclatureJournalFactory nomenclatureSelectorFactory,
			INomenclatureRepository nomenclatureRepository,
			IUserRepository userRepository) : base(uowBuilder, unitOfWorkFactory, commonServices)
		{
			_employeeService = employeeService ?? throw new ArgumentNullException(nameof(employeeService));
			_nomenclatureRepository = nomenclatureRepository ?? throw new ArgumentNullException(nameof(nomenclatureRepository));
			_userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
			_counterpartySelectorFactory =
				counterpartySelectorFactory ?? throw new ArgumentNullException(nameof(counterpartySelectorFactory));
			_nomenclatureSelectorFactory =
				nomenclatureSelectorFactory ?? throw new ArgumentNullException(nameof(nomenclatureSelectorFactory));
			CanChangeType =
				commonServices.CurrentPermissionService.ValidatePresetPermission(
					Vodovoz.Permissions.Order.PromotionalSet.CanChangeTypePromoSet);

			if(!CanRead)
			{
				AbortOpening("У вас недостаточно прав для просмотра");
			}

			CreateCommands();
			ConfigureOnlineParameters();
		}

		public string CreationDate => Entity.Id != 0 ? Entity.CreateDate.ToString("dd-MM-yyyy") : string.Empty;

		public PromotionalSetItem SelectedPromoItem
		{
			get => _selectedPromoItem;
			set
			{
				SetField(ref _selectedPromoItem, value);
				OnPropertyChanged(nameof(CanRemoveNomenclature));
			}
		}

		public PromotionalSetActionBase SelectedAction
		{
			get => _selectedAction;
			set
			{
				SetField(ref _selectedAction, value);
				OnPropertyChanged(nameof(CanRemoveAction));
			}
		}

		public WidgetViewModelBase SelectedActionViewModel
		{
			get => _selectedActionViewModel;
			set => SetField(ref _selectedActionViewModel, value);
		}
		
		public int CurrentPage
		{
			get => _currentPage;
			set => SetField(ref _currentPage, value);
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
		
		public PromotionalSetOnlineParameters MobileAppPromotionalSetOnlineParameters { get; private set; }
		public PromotionalSetOnlineParameters VodovozWebSitePromotionalSetOnlineParameters { get; private set; }
		public PromotionalSetOnlineParameters KulerSaleWebSitePromotionalSetOnlineParameters { get; private set; }

		#region Permissions

		public bool CanCreate => PermissionResult.CanCreate;
		public bool CanRead => PermissionResult.CanRead;
		public bool CanUpdate => PermissionResult.CanUpdate;
		public bool CanDelete => PermissionResult.CanDelete;

		public bool CanCreateOrUpdate => Entity.Id == 0 ? CanCreate : CanUpdate;
		public bool CanRemoveNomenclature => SelectedPromoItem != null && CanUpdate;
		public bool CanRemoveAction => _selectedAction != null && CanDelete && Entity.Id == 0;

		public bool CanChangeType { get; }

		#endregion

		#region Commands

		private void CreateCommands()
		{
			CreateAddNomenclatureCommand();
			CreateRemoveNomenclatureCommand();
			CreateAddActionCommand();
			CreateRemoveActionCommand();
		}

		public DelegateCommand AddNomenclatureCommand;

		private void CreateAddNomenclatureCommand()
		{
			AddNomenclatureCommand = new DelegateCommand(
				() =>
				{
					var nomenFilter = new NomenclatureFilterViewModel();
					nomenFilter.SetAndRefilterAtOnce(
						x => x.AvailableCategories = Nomenclature.GetCategoriesForSaleToOrder(),
						x => x.SelectCategory = NomenclatureCategory.water,
						x => x.SelectSaleCategory = SaleCategory.forSale);

					var nomenJournalViewModel = new NomenclaturesJournalViewModel(nomenFilter, UnitOfWorkFactory,
						CommonServices, _employeeService, _nomenclatureSelectorFactory, _counterpartySelectorFactory,
						_nomenclatureRepository, _userRepository)
					{
						SelectionMode = JournalSelectionMode.Single
					};

					nomenJournalViewModel.OnEntitySelectedResult += (sender, e) =>
					{
						var selectedNode = e.SelectedNodes.Cast<NomenclatureJournalNode>().FirstOrDefault();
						if(selectedNode == null)
						{
							return;
						}

						var nomenclature = UoW.GetById<Nomenclature>(selectedNode.Id);
						if(Entity.ObservablePromotionalSetItems.Any(i => i.Nomenclature.Id == nomenclature.Id))
						{
							return;
						}

						Entity.ObservablePromotionalSetItems.Add(new PromotionalSetItem
						{
							Nomenclature = nomenclature,
							Count = 0,
							Discount = 0,
							PromoSet = Entity
						});
					};
					TabParent.AddSlaveTab(this, nomenJournalViewModel);
				},
				() => true
			);
		}

		public DelegateCommand RemoveNomenclatureCommand;

		private void CreateRemoveNomenclatureCommand()
		{
			RemoveNomenclatureCommand = new DelegateCommand(
				() => Entity.ObservablePromotionalSetItems.Remove(SelectedPromoItem),
				() => CanRemoveNomenclature
			);
			RemoveNomenclatureCommand.CanExecuteChangedWith(this, x => CanRemoveNomenclature);
		}

		public DelegateCommand<PromotionalSetActionType> AddActionCommand;

		private void CreateAddActionCommand()
		{
			AddActionCommand = new DelegateCommand<PromotionalSetActionType>(
				(actionType) =>
				{
					var resolver = new PromotionalSetActionWidgetResolver(UoW,
						_counterpartySelectorFactory, _nomenclatureRepository, _userRepository);
					SelectedActionViewModel = resolver.Resolve(Entity, actionType);

					if(SelectedActionViewModel is ICreationControl)
					{
						(SelectedActionViewModel as ICreationControl).CancelCreation += () => { SelectedActionViewModel = null; };
					}
				}
			);
		}

		public DelegateCommand RemoveActionCommand;

		private void CreateRemoveActionCommand()
		{
			RemoveActionCommand = new DelegateCommand(
				() => Entity.ObservablePromotionalSetActions.Remove(SelectedAction),
				() => CanRemoveAction
			);
		}

		#endregion
		
		private void ConfigureOnlineParameters()
		{
			MobileAppPromotionalSetOnlineParameters = GetPromotionalSetOnlineParameters(GoodsOnlineParameterType.ForMobileApp);
			VodovozWebSitePromotionalSetOnlineParameters = GetPromotionalSetOnlineParameters(GoodsOnlineParameterType.ForVodovozWebSite);
			KulerSaleWebSitePromotionalSetOnlineParameters = GetPromotionalSetOnlineParameters(GoodsOnlineParameterType.ForKulerSaleWebSite);
		}
		
		private PromotionalSetOnlineParameters GetPromotionalSetOnlineParameters(GoodsOnlineParameterType type)
		{
			var parameters = Entity.PromotionalSetOnlineParameters.SingleOrDefault(x => x.Type == type);
			return parameters ?? CreatePromotionalSetOnlineParameters(type);
		}
		
		private PromotionalSetOnlineParameters CreatePromotionalSetOnlineParameters(GoodsOnlineParameterType type)
		{
			PromotionalSetOnlineParameters parameters = null;
			switch(type)
			{
				case GoodsOnlineParameterType.ForMobileApp:
					parameters = new MobileAppPromotionalSetOnlineParameters();
					break;
				case GoodsOnlineParameterType.ForVodovozWebSite:
					parameters = new VodovozWebSitePromotionalSetOnlineParameters();
					break;
				case GoodsOnlineParameterType.ForKulerSaleWebSite:
					parameters = new KulerSaleWebSitePromotionalSetOnlineParameters();
					break;
			}

			parameters.PromotionalSet = Entity;
			Entity.PromotionalSetOnlineParameters.Add(parameters);
			return parameters;
		}
	}
}
