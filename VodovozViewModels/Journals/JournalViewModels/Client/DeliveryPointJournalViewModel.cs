using System;
using System.Linq;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using NHibernate.Transform;
using QS.Deletion;
using QS.DomainModel.UoW;
using QS.Osm.Loaders;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Services;
using Vodovoz.Domain;
using Vodovoz.Domain.Client;
using Vodovoz.EntityRepositories;
using Vodovoz.EntityRepositories.Counterparties;
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
		private readonly IDeliveryPointRepository _deliveryPointRepository;
		private readonly INomenclatureSelectorFactory _nomenclatureSelectorFactory;
		private readonly NomenclatureFixedPriceController _nomenclatureFixedPriceController;

		public DeliveryPointJournalViewModel(
			IUserRepository userRepository, IGtkTabsOpener gtkTabsOpener, IPhoneRepository phoneRepository, IContactsParameters contactsParameters,
			ICitiesDataLoader citiesLoader, IStreetsDataLoader streetsLoader, IHousesDataLoader housesLoader,
			IDeliveryPointRepository deliveryPointRepository,
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
			_deliveryPointRepository = deliveryPointRepository ?? throw new ArgumentNullException(nameof(deliveryPointRepository));
			_nomenclatureSelectorFactory = nomenclatureSelectorFactory ?? throw new ArgumentNullException(nameof(nomenclatureSelectorFactory));
			_nomenclatureFixedPriceController = nomenclatureFixedPriceController ??
			                                    throw new ArgumentNullException(nameof(nomenclatureFixedPriceController));

			TabName = "Журнал точек доставки";
			UpdateOnChanges(
				typeof(Counterparty),
				typeof(DeliveryPoint)
			);
		}

		protected override void CreateNodeActions()
		{
			NodeActionsList.Clear();
			CreateDefaultSelectAction();
			CreateDefaultEditAction();
			CreateDeleteAction();
		}

		protected void CreateDeleteAction()
		{
			var deleteAction = new JournalAction("Удалить",
				(selected) => {
					var selectedNodes = selected.OfType<DeliveryPointJournalNode>();
					if(selectedNodes == null || selectedNodes.Count() != 1) {
						return false;
					}
					DeliveryPointJournalNode selectedNode = selectedNodes.First();
					if(!EntityConfigs.ContainsKey(selectedNode.EntityType)) {
						return false;
					}
					var config = EntityConfigs[selectedNode.EntityType];
					return config.PermissionResult.CanDelete 
						&& commonServices.CurrentPermissionService.ValidatePresetPermission("can_delete_counterparty_and_deliverypoint");
				},
				(selected) => true,
				(selected) => {
					var selectedNodes = selected.OfType<DeliveryPointJournalNode>();
					if(selectedNodes == null || selectedNodes.Count() != 1) {
						return;
					}
					DeliveryPointJournalNode selectedNode = selectedNodes.First();
					if(!EntityConfigs.ContainsKey(selectedNode.EntityType)) {
						return;
					}
					var config = EntityConfigs[selectedNode.EntityType];
					if(config.PermissionResult.CanDelete) {
						DeleteHelper.DeleteEntity(selectedNode.EntityType, selectedNode.Id);
					}
				},
				"Delete"
			);
			NodeActionsList.Add(deleteAction);
		}

		protected override Func<IUnitOfWork, IQueryOver<DeliveryPoint>> ItemsSourceQueryFunction => (uow) => {
			DeliveryPoint deliveryPointAlias = null;
			Counterparty counterpartyAlias = null;
			DeliveryPointJournalNode resultAlias = null;

			var query = uow.Session.QueryOver<DeliveryPoint>(() => deliveryPointAlias);

			if(FilterViewModel != null && FilterViewModel.RestrictOnlyActive)
			{
				query = query.Where(() => deliveryPointAlias.IsActive);
			}

			if(FilterViewModel?.Counterparty != null)
			{
				query = query.Where(() => counterpartyAlias.Id == FilterViewModel.Counterparty.Id);
			}

			if(FilterViewModel?.RestrictOnlyNotFoundOsm == true)
			{
				query = query.Where(() => deliveryPointAlias.FoundOnOsm == false);
			}

			if(FilterViewModel?.RestrictOnlyWithoutStreet == true)
			{
				query = query.Where(Restrictions.Eq
					(
						Projections.SqlFunction(new SQLFunctionTemplate(NHibernateUtil.Boolean, "IS_NULL_OR_WHITESPACE(?1)"),
							NHibernateUtil.String, Projections.Property(() => deliveryPointAlias.Street)), true
					)
				);
			}

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
				_deliveryPointRepository,
				EntityUoWBuilder.ForOpen(node.Id), UnitOfWorkFactory, commonServices);
	}
}
