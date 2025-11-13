using System;
using System.Linq;
using QS.Commands;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Services;
using QS.ViewModels;
using Autofac;
using QS.Navigation;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Goods.NomenclaturesOnlineParameters;
using Vodovoz.Domain.Goods.PromotionalSetsOnlineParameters;
using Vodovoz.Domain.Orders;
using Vodovoz.ViewModels.Journals.FilterViewModels.Goods;
using Vodovoz.ViewModels.Journals.JournalNodes.Goods;
using Vodovoz.ViewModels.Journals.JournalViewModels.Goods;
using VodovozInfrastructure.StringHandlers;
using QS.ViewModels.Extension;
using Vodovoz.Core.Domain.Goods;

namespace Vodovoz.ViewModels.Orders
{
	public class PromotionalSetViewModel : EntityTabViewModelBase<PromotionalSet>, IPermissionResult, IAskSaveOnCloseViewModel
	{
		private ILifetimeScope _lifetimeScope;
		private PromotionalSetItem _selectedPromoItem;
		private PromotionalSetActionBase _selectedAction;
		private WidgetViewModelBase _selectedActionViewModel;
		private bool _informationTabActive;
		private bool _sitesAndAppsTabActive;
		private bool _showSpecialBottlesCountForDeliveryPrice;
		private int _currentPage;
		
		public PromotionalSetViewModel(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			INavigationManager navigationManager,
			IStringHandler stringHandler,
			ILifetimeScope lifetimeScope) : base(uowBuilder, unitOfWorkFactory, commonServices, navigationManager)
		{
			StringHandler = stringHandler ?? throw new ArgumentNullException(nameof(stringHandler));
			_lifetimeScope = lifetimeScope ?? throw new ArgumentNullException(nameof(lifetimeScope));
			CanChangeType =
				commonServices.CurrentPermissionService.ValidatePresetPermission(
					Vodovoz.Core.Domain.Permissions.OrderPermissions.PromotionalSet.CanChangeTypePromoSet) && CanUpdate;

			if(!CanRead)
			{
				AbortOpening("У вас недостаточно прав для просмотра");
			}

			_showSpecialBottlesCountForDeliveryPrice = Entity.BottlesCountForCalculatingDeliveryPrice.HasValue;
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
		
		public bool ShowSpecialBottlesCountForDeliveryPrice
		{
			get => _showSpecialBottlesCountForDeliveryPrice;
			set
			{
				if(SetField(ref _showSpecialBottlesCountForDeliveryPrice, value) && !value)
				{
					Entity.BottlesCountForCalculatingDeliveryPrice = null;
				}
			}
		}
		
		public PromotionalSetOnlineParameters MobileAppPromotionalSetOnlineParameters { get; private set; }
		public PromotionalSetOnlineParameters VodovozWebSitePromotionalSetOnlineParameters { get; private set; }
		public PromotionalSetOnlineParameters KulerSaleWebSitePromotionalSetOnlineParameters { get; private set; }
		public IStringHandler StringHandler { get; }

		#region Permissions

		public bool CanCreate => PermissionResult.CanCreate;
		public bool CanRead => PermissionResult.CanRead;
		public bool CanUpdate => PermissionResult.CanUpdate;
		public bool CanDelete => PermissionResult.CanDelete;

		public bool CanCreateOrUpdate => Entity.Id == 0 ? CanCreate : CanUpdate;
		public bool CanRemoveNomenclature => SelectedPromoItem != null && CanCreateOrUpdate;
		public bool CanRemoveAction => _selectedAction != null && CanDelete && Entity.Id == 0;

		public bool CanChangeType { get; }

		public bool AskSaveOnClose => CanCreateOrUpdate;

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
				var nomenJournalViewModel =
					NavigationManager.OpenViewModel<NomenclaturesJournalViewModel, Action<NomenclatureFilterViewModel>>(
						this,
						f =>
						{
							f.AvailableCategories = Nomenclature.GetCategoriesForSaleToOrder();
							f.SelectCategory = NomenclatureCategory.water;
							f.SelectSaleCategory = SaleCategory.forSale;
						},
						OpenPageOptions.AsSlave,
						vm => vm.SelectionMode = JournalSelectionMode.Single)
					.ViewModel;

				nomenJournalViewModel.OnSelectResult += (sender, e) => {
					var selectedNode = e.SelectedObjects.Cast<NomenclatureJournalNode>().FirstOrDefault();
					if(selectedNode == null) {
						return;
					}
					var nomenclature = UoW.GetById<Nomenclature>(selectedNode.Id);
					if(Entity.ObservablePromotionalSetItems.Any(i => i.Nomenclature.Id == nomenclature.Id))
						return;
					Entity.ObservablePromotionalSetItems.Add(new PromotionalSetItem {
						Nomenclature = nomenclature,
						Count = 0,
						Discount = 0,
						PromoSet = Entity
					});
				};
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
					var addFixedPriceViewModel = _lifetimeScope.Resolve<AddFixPriceActionViewModel>(
						new TypedParameter(typeof(IUnitOfWork), UoW),
						new TypedParameter(typeof(PromotionalSet), Entity));

					addFixedPriceViewModel.Container = this;

					SelectedActionViewModel = addFixedPriceViewModel;

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

		public override void Dispose()
		{
			_lifetimeScope = null;
			base.Dispose();
		}
	}
}
