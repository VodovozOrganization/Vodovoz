using System;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QS.Osm;
using QS.Osm.Loaders;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Services;
using Vodovoz.Domain;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.EntityFactories;
using Vodovoz.EntityRepositories;
using Vodovoz.EntityRepositories.Goods;
using Vodovoz.Filters.ViewModels;
using Vodovoz.Parameters;
using Vodovoz.Services;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Journals.JournalNodes;
using Vodovoz.ViewModels.ViewModels.Counterparty;

namespace Vodovoz.ViewModels.Journals.JournalViewModels
{
	/// <summary>
	/// Данный журнал главным образом необходим для выбора точки доставки конкретного клиента в различных entityVMentry без колонок с лишней информацией
	/// </summary>
	public class DeliveryPointByClientJournalViewModel : FilterableSingleEntityJournalViewModelBase
		<DeliveryPoint, DeliveryPointViewModel, DeliveryPointByClientJournalNode, DeliveryPointJournalFilterViewModel>
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

		public DeliveryPointByClientJournalViewModel(
			IUserRepository userRepository, IGtkTabsOpener gtkTabsOpener, IPhoneRepository phoneRepository, IContactsParameters contactsParameters,
			ICitiesDataLoader citiesLoader, IStreetsDataLoader streetsLoader, IHousesDataLoader housesLoader,
			INomenclatureSelectorFactory nomenclatureSelectorFactory, NomenclatureFixedPriceController nomenclatureFixedPriceController,
			DeliveryPointJournalFilterViewModel filterViewModel, IUnitOfWorkFactory unitOfWorkFactory, ICommonServices commonServices)
			: base(filterViewModel, unitOfWorkFactory, commonServices)
		{
			TabName = "Журнал точек доставки клиента";
			if(FilterViewModel.Counterparty == null)
			{
				throw new ArgumentException("Для использования этого журнала необходимо передать клиента в фильтр");
			}

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

			UpdateOnChanges(
				typeof(Counterparty),
				typeof(DeliveryPoint)
			);
		}

		protected override void CreateNodeActions()
		{
			NodeActionsList.Clear();
			CreateDefaultSelectAction();
			CreateDefaultAddActions();
			CreateDefaultEditAction();
		}

		protected override Func<IUnitOfWork, IQueryOver<DeliveryPoint>> ItemsSourceQueryFunction => (uow) =>
		{
			DeliveryPoint deliveryPointAlias = null;
			DeliveryPointByClientJournalNode resultAlias = null;

			var query = uow.Session.QueryOver(() => deliveryPointAlias);

			if(FilterViewModel?.RestrictOnlyActive == true)
			{
				query = query.Where(() => deliveryPointAlias.IsActive);
			}

			if(FilterViewModel?.Counterparty != null)
			{
				query = query.Where(() => deliveryPointAlias.Counterparty.Id == FilterViewModel.Counterparty.Id);
			}

			if(FilterViewModel?.RestrictOnlyNotFoundOsm == true)
			{
				query = query.Where(() => deliveryPointAlias.FoundOnOsm == false);
			}

			if(FilterViewModel?.RestrictOnlyWithoutStreet == true)
			{
				query = query.Where(Restrictions.Eq
					(Projections.SqlFunction(new SQLFunctionTemplate(NHibernateUtil.Boolean, "IS_NULL_OR_WHITESPACE(?1)"),
							NHibernateUtil.String, new IProjection[] {Projections.Property(() => deliveryPointAlias.Street)}
							), true));
			}

			query.Where(GetSearchCriterion(
				() => deliveryPointAlias.Id,
				() => deliveryPointAlias.CompiledAddress
			));

			var resultQuery = query
				.SelectList(list => list
					.Select(() => deliveryPointAlias.Id).WithAlias(() => resultAlias.Id)
					.Select(() => deliveryPointAlias.CompiledAddress).WithAlias(() => resultAlias.CompiledAddress)
					.Select(() => deliveryPointAlias.IsActive).WithAlias(() => resultAlias.IsActive)
				)
				.TransformUsing(Transformers.AliasToBean<DeliveryPointByClientJournalNode>());

			return resultQuery;
		};

		protected override Func<DeliveryPointViewModel> CreateDialogFunction => () =>
			new DeliveryPointViewModel(
				FilterViewModel.Counterparty,
				_userRepository, _gtkTabsOpener, _phoneRepository, _contactsParameters,
				_citiesLoader, _streetsLoader, _housesLoader,
				_nomenclatureSelectorFactory,
				_nomenclatureFixedPriceController,
				EntityUoWBuilder.ForCreate(), UnitOfWorkFactory, commonServices);

		protected override Func<DeliveryPointByClientJournalNode, DeliveryPointViewModel> OpenDialogFunction => (node) =>
			new DeliveryPointViewModel(
				_userRepository, _gtkTabsOpener, _phoneRepository, _contactsParameters,
				_citiesLoader, _streetsLoader, _housesLoader,
				_nomenclatureSelectorFactory,
				_nomenclatureFixedPriceController,
				EntityUoWBuilder.ForOpen(node.Id), UnitOfWorkFactory, commonServices);
	}
}
