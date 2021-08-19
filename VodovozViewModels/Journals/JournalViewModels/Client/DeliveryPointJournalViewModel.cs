using System;
using System.Linq;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QS.Osm.Loaders;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Services;
using Vodovoz.Domain;
using Vodovoz.Domain.Client;
using Vodovoz.EntityRepositories;
using Vodovoz.Filters.ViewModels;
using Vodovoz.Services;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Journals.JournalNodes.Client;
using Vodovoz.ViewModels.ViewModels.Counterparty;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.Client
{
	public class DeliveryPointJournalViewModel : FilterableSingleEntityJournalViewModelBase<DeliveryPoint, DeliveryPointViewModel, DeliveryPointJournalNode, DeliveryPointJournalFilterViewModel>
	{
		private readonly IUserRepository _userRepository;
		private readonly IGtkTabsOpener _gtkTabsOpener;
		private readonly IPhoneRepository _phoneRepository;
		private readonly IContactsParameters _contactsParameters;
		private readonly ICitiesDataLoader _citiesLoader;
		private readonly IStreetsDataLoader _streetsLoader;
		private readonly IHousesDataLoader _housesLoader;
		private readonly INomenclatureSelectorFactory _nomenclatureSelectorFactory;
		private readonly NomenclatureFixedPriceController _nomenclatureFixedPriceController;

		public DeliveryPointJournalViewModel(
			IUserRepository userRepository, IGtkTabsOpener gtkTabsOpener, IPhoneRepository phoneRepository, IContactsParameters contactsParameters,
			ICitiesDataLoader citiesLoader, IStreetsDataLoader streetsLoader, IHousesDataLoader housesLoader,
			INomenclatureSelectorFactory nomenclatureSelectorFactory, NomenclatureFixedPriceController nomenclatureFixedPriceController,
			DeliveryPointJournalFilterViewModel filterViewModel, IUnitOfWorkFactory unitOfWorkFactory, ICommonServices commonServices)
			: base(filterViewModel, unitOfWorkFactory, commonServices)
		{
			_userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
			_gtkTabsOpener = gtkTabsOpener ?? throw new ArgumentNullException(nameof(gtkTabsOpener));
			_phoneRepository = phoneRepository ?? throw new ArgumentNullException(nameof(phoneRepository));
			_contactsParameters = contactsParameters ?? throw new ArgumentNullException(nameof(contactsParameters));
			_citiesLoader = citiesLoader ?? throw new ArgumentNullException(nameof(citiesLoader));
			_streetsLoader = streetsLoader ?? throw new ArgumentNullException(nameof(streetsLoader));
			_housesLoader = housesLoader ?? throw new ArgumentNullException(nameof(housesLoader));
			_nomenclatureSelectorFactory = nomenclatureSelectorFactory ?? throw new ArgumentNullException(nameof(nomenclatureSelectorFactory));
			_nomenclatureFixedPriceController = nomenclatureFixedPriceController ??
			                                    throw new ArgumentNullException(nameof(nomenclatureFixedPriceController));

			TabName = "Журнал точек доставки";
			
			UpdateOnChanges(
				typeof(Counterparty),
				typeof(DeliveryPoint)
			);
		}

		protected override void InitializeJournalActionsViewModel()
		{
			EntitiesJournalActionsViewModel.Initialize(
				SelectionMode, EntityConfigs, this, HideJournal, OnItemsSelected, true,false);
			ConfigureDeleteAction();
		}

		private void ConfigureDeleteAction()
		{
			var deleteAction = EntitiesJournalActionsViewModel.JournalActions.Single(a => a.ActionType == ActionType.Delete);
			
			deleteAction.SensitiveFunc =
				() =>
				{
					var selectedNodes = SelectedItems.OfType<DeliveryPointJournalNode>().ToList();
					
					if(selectedNodes.Count != 1) 
					{
						return false;
					}
					
					DeliveryPointJournalNode selectedNode = selectedNodes.First();
					
					if(!EntityConfigs.ContainsKey(selectedNode.EntityType)) 
					{
						return false;
					}
					var config = EntityConfigs[selectedNode.EntityType];
					
					return config.PermissionResult.CanDelete 
					       && CommonServices.CurrentPermissionService.ValidatePresetPermission("can_delete_counterparty_and_deliverypoint");
				};
		}

		protected override Func<IUnitOfWork, IQueryOver<DeliveryPoint>> ItemsSourceQueryFunction => (uow) => {
			DeliveryPoint deliveryPointAlias = null;
			Counterparty counterpartyAlias = null;
			DeliveryPointJournalNode resultAlias = null;

			var query = uow.Session.QueryOver<DeliveryPoint>(() => deliveryPointAlias);

			if(FilterViewModel != null && FilterViewModel.RestrictOnlyActive)
				query = query.Where(() => deliveryPointAlias.IsActive);

			if(FilterViewModel != null && FilterViewModel.Counterparty != null)
				query = query.Where(() => counterpartyAlias.Id == FilterViewModel.Counterparty.Id);

			if(FilterViewModel != null && FilterViewModel.RestrictOnlyNotFoundOsm)
				query = query.Where(() => deliveryPointAlias.FoundOnOsm == false);

			if(FilterViewModel != null && FilterViewModel.RestrictOnlyWithoutStreet)
				query = query.Where(Restrictions.Eq
					(
						Projections.SqlFunction(new SQLFunctionTemplate(NHibernateUtil.Boolean, "IS_NULL_OR_WHITESPACE(?1)"),
						NHibernateUtil.String, new IProjection[] { Projections.Property(() => deliveryPointAlias.Street) }
					), true
				)
			);

			query.Where(GetSearchCriterion(
				() => deliveryPointAlias.Id,
				() => counterpartyAlias.Name,
				() => deliveryPointAlias.CompiledAddress,
				() => deliveryPointAlias.Address1c
			));

			var resultQuery = query
				.JoinAlias(c => c.Counterparty, () => counterpartyAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
				.SelectList(list => list
				   .Select(() => deliveryPointAlias.Id).WithAlias(() => resultAlias.Id)
				   .Select(() => deliveryPointAlias.CompiledAddress).WithAlias(() => resultAlias.CompiledAddress)
				   .Select(() => deliveryPointAlias.FoundOnOsm).WithAlias(() => resultAlias.FoundOnOsm)
				   .Select(() => deliveryPointAlias.IsFixedInOsm).WithAlias(() => resultAlias.FixedInOsm)
				   .Select(() => deliveryPointAlias.IsActive).WithAlias(() => resultAlias.IsActive)
				   .Select(() => deliveryPointAlias.Address1c).WithAlias(() => resultAlias.Address1c)
				   .Select(() => counterpartyAlias.FullName).WithAlias(() => resultAlias.Counterparty)
				)
				.TransformUsing(Transformers.AliasToBean<DeliveryPointJournalNode>());

			return resultQuery;
		};

		protected override Func<DeliveryPointViewModel> CreateDialogFunction => () => throw new NotImplementedException();

		protected override Func<DeliveryPointJournalNode, DeliveryPointViewModel> OpenDialogFunction => (node) =>
			new DeliveryPointViewModel(
				_userRepository, _gtkTabsOpener, _phoneRepository, _contactsParameters,
				_citiesLoader, _streetsLoader, _housesLoader,
				_nomenclatureSelectorFactory,
				_nomenclatureFixedPriceController,
				EntityUoWBuilder.ForOpen(node.Id), UnitOfWorkFactory, commonServices);
	}
}
