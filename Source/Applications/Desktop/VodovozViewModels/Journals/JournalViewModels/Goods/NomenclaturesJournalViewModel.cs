using NHibernate;
using NHibernate.Criterion;
using NHibernate.Transform;
using QS.BusinessCommon.Domain;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Services;
using System;
using System.Linq;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Operations;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories;
using Vodovoz.EntityRepositories.Goods;
using Vodovoz.Infrastructure;
using Vodovoz.Services;
using Vodovoz.Settings.Nomenclature;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Dialogs.Goods;
using Vodovoz.ViewModels.Journals.FilterViewModels.Goods;
using Vodovoz.ViewModels.Journals.JournalNodes.Goods;
using VodovozInfrastructure.StringHandlers;
using VodovozOrder = Vodovoz.Domain.Orders.Order;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.Goods
{
	public class NomenclaturesJournalViewModel : FilterableSingleEntityJournalViewModelBase<Nomenclature, NomenclatureViewModel, NomenclatureJournalNode, NomenclatureFilterViewModel>
	{
		private readonly IEmployeeService _employeeService;
		private readonly INomenclatureJournalFactory _nomenclatureSelectorFactory;
		private readonly ICounterpartyJournalFactory _counterpartySelectorFactory;
		private readonly INomenclatureRepository _nomenclatureRepository;
		private readonly IUserRepository _userRepository;
		private readonly INomenclatureOnlineParametersProvider _nomenclatureOnlineParametersProvider;
		private readonly INomenclatureSettings _nomenclatureSettings;

		public NomenclaturesJournalViewModel(
			NomenclatureFilterViewModel filterViewModel,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			IEmployeeService employeeService,
			INomenclatureJournalFactory nomenclatureSelectorFactory,
			ICounterpartyJournalFactory counterpartySelectorFactory,
			INomenclatureRepository nomenclatureRepository,
			IUserRepository userRepository,
			INomenclatureSettings nomenclatureSettings,
			INomenclatureOnlineParametersProvider nomenclatureOnlineParametersProvider,
			Action<NomenclatureFilterViewModel> filterParams = null
		) : base(filterViewModel, unitOfWorkFactory, commonServices)
		{
			_employeeService = employeeService ?? throw new ArgumentNullException(nameof(employeeService));
			_nomenclatureSelectorFactory =
				nomenclatureSelectorFactory ?? throw new ArgumentNullException(nameof(nomenclatureSelectorFactory));
			_counterpartySelectorFactory =
				counterpartySelectorFactory ?? throw new ArgumentNullException(nameof(counterpartySelectorFactory));
			_nomenclatureRepository = nomenclatureRepository ?? throw new ArgumentNullException(nameof(nomenclatureRepository));
			_userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
			_nomenclatureSettings = nomenclatureSettings ?? throw new ArgumentNullException(nameof(nomenclatureSettings));
			_nomenclatureOnlineParametersProvider =
				nomenclatureOnlineParametersProvider ?? throw new ArgumentNullException(nameof(nomenclatureOnlineParametersProvider));
			
			TabName = "Журнал ТМЦ";

			SetOrder(x => x.Name);

			if(filterParams != null)
			{
				FilterViewModel.ConfigureWithoutFiltering(filterParams);
			}

			UpdateOnChanges(
				typeof(Nomenclature),
				typeof(MeasurementUnits),
				typeof(GoodsAccountingOperation),
				typeof(VodovozOrder),
				typeof(OrderItem)
			);
		}

		[Obsolete("Лучше передавать через фильтр")]
		public int[] ExcludingNomenclatureIds { get; set; }

		public bool CalculateQuantityOnStock { get; set; } = false;

		public IAdditionalJournalRestriction<Nomenclature> AdditionalJournalRestriction { get; set; } = null;

		protected override void CreateNodeActions()
		{
			NodeActionsList.Clear();
			CreateDefaultSelectAction();
			CreateDefaultAddActions();
			CreateEditAction();
			CreateDefaultDeleteAction();
		}

		public void HideButtons()
		{
			NodeActionsList.Clear();
			CreateDefaultSelectAction();
		}
		
		private void CreateEditAction()
		{
			var editAction = new JournalAction("Изменить",
				(selected) => {
					var selectedNodes = selected.OfType<NomenclatureJournalNode>();
					if(selectedNodes == null || selectedNodes.Count() != 1) {
						return false;
					}
					NomenclatureJournalNode selectedNode = selectedNodes.First();
					if(!EntityConfigs.ContainsKey(selectedNode.EntityType)) {
						return false;
					}
					var config = EntityConfigs[selectedNode.EntityType];
					return config.PermissionResult.CanRead;
				},
				(selected) => true,
				(selected) => {
					var selectedNodes = selected.OfType<NomenclatureJournalNode>();
					if(selectedNodes == null || selectedNodes.Count() != 1) {
						return;
					}
					NomenclatureJournalNode selectedNode = selectedNodes.First();
					if(!EntityConfigs.ContainsKey(selectedNode.EntityType)) {
						return;
					}
					var config = EntityConfigs[selectedNode.EntityType];
					var foundDocumentConfig = config.EntityDocumentConfigurations.FirstOrDefault(x => x.IsIdentified(selectedNode));

					TabParent.OpenTab(() => foundDocumentConfig.GetOpenEntityDlgFunction().Invoke(selectedNode), this);
					if(foundDocumentConfig.JournalParameters.HideJournalForOpenDialog) {
						HideJournal(TabParent);
					}
				}
			);
			if(SelectionMode == JournalSelectionMode.None) {
				RowActivatedAction = editAction;
			}
			NodeActionsList.Add(editAction);
		}

		protected override Func<IUnitOfWork, IQueryOver<Nomenclature>> ItemsSourceQueryFunction => (uow) => {
			Nomenclature nomenclatureAlias = null;
			MeasurementUnits unitAlias = null;
			NomenclatureJournalNode resultAlias = null;
			WarehouseBulkGoodsAccountingOperation operationAlias = null;
			VodovozOrder orderAlias = null;
			OrderItem orderItemsAlias = null;

			var subQueryBalance = QueryOver.Of(() => operationAlias)
				.Where(() => operationAlias.Nomenclature.Id == nomenclatureAlias.Id)
				.Select(Projections.Sum<GoodsAccountingOperation>(o => o.Amount));

			var subQueryReserved = QueryOver.Of(() => orderAlias)
				.JoinAlias(() => orderAlias.OrderItems, () => orderItemsAlias)
				.Where(() => orderItemsAlias.Nomenclature.Id == nomenclatureAlias.Id)
				.Where(() => nomenclatureAlias.DoNotReserve == false)
				.Where(() => orderAlias.OrderStatus == OrderStatus.Accepted
					   || orderAlias.OrderStatus == OrderStatus.InTravelList
					   || orderAlias.OrderStatus == OrderStatus.OnLoading)
				.Select(Projections.Sum(() => orderItemsAlias.Count));

			var itemsQuery = uow.Session.QueryOver(() => nomenclatureAlias);

			//Хардкодим выборку номенклатур не для инвентарного учета
			itemsQuery.Where(() => !nomenclatureAlias.HasInventoryAccounting);
			
			if(!FilterViewModel.RestrictArchive)
			{
				itemsQuery.Where(() => !nomenclatureAlias.IsArchive);
			}

			if(FilterViewModel.RestrictedExcludedIds != null && FilterViewModel.RestrictedExcludedIds.Any()) {
				itemsQuery.WhereRestrictionOn(() => nomenclatureAlias.Id).Not.IsInG(FilterViewModel.RestrictedExcludedIds);
			}

			if(ExcludingNomenclatureIds != null && ExcludingNomenclatureIds.Any())
			{
				itemsQuery.WhereNot(() => nomenclatureAlias.Id.IsIn(ExcludingNomenclatureIds));
			}

			itemsQuery.Where(
				GetSearchCriterion(
					() => nomenclatureAlias.Name,
					() => nomenclatureAlias.Id,
					() => nomenclatureAlias.OnlineStoreExternalId
				)
			);

			if(!FilterViewModel.RestrictDilers)
			{
				itemsQuery.Where(() => !nomenclatureAlias.IsDiler);
			}

			if(FilterViewModel.RestrictCategory == NomenclatureCategory.water)
			{
				itemsQuery.Where(() => nomenclatureAlias.IsDisposableTare == FilterViewModel.RestrictDisposbleTare);
			}

			if(FilterViewModel.RestrictCategory.HasValue)
			{
				itemsQuery.Where(n => n.Category == FilterViewModel.RestrictCategory.Value);
			}

			if(FilterViewModel.SelectCategory.HasValue && FilterViewModel.SelectSaleCategory.HasValue && Nomenclature.GetCategoriesWithSaleCategory().Contains(FilterViewModel.SelectCategory.Value))
			{
				itemsQuery.Where(n => n.SaleCategory == FilterViewModel.SelectSaleCategory);
			}

			if(AdditionalJournalRestriction != null)
			{
				foreach(var expr in AdditionalJournalRestriction.ExternalRestrictions)
				{
					itemsQuery.Where(expr);
				}
			}

			if(FilterViewModel.IsDefectiveBottle)
			{
				itemsQuery.Where(x => x.IsDefectiveBottle);
			}

			if(FilterViewModel.GlassHolderType.HasValue)
			{
				itemsQuery.Where(x => x.GlassHolderType == FilterViewModel.GlassHolderType.Value);
			}

			if(CalculateQuantityOnStock) {
				itemsQuery.Left.JoinAlias(() => nomenclatureAlias.Unit, () => unitAlias)
					.Where(() => !nomenclatureAlias.IsSerial)
					.SelectList(list => list
						.SelectGroup(() => nomenclatureAlias.Id).WithAlias(() => resultAlias.Id)
						.Select(() => nomenclatureAlias.Name).WithAlias(() => resultAlias.Name)
						.Select(() => nomenclatureAlias.Category).WithAlias(() => resultAlias.Category)
						.Select(() => nomenclatureAlias.GlassHolderType).WithAlias(() => resultAlias.GlassHolderType)
						.Select(() => unitAlias.Name).WithAlias(() => resultAlias.UnitName)
						.Select(() => unitAlias.Digits).WithAlias(() => resultAlias.UnitDigits)
						.Select(() => nomenclatureAlias.OnlineStoreExternalId).WithAlias(() => resultAlias.OnlineStoreExternalId)
						.SelectSubQuery(subQueryBalance).WithAlias(() => resultAlias.InStock)
						.SelectSubQuery(subQueryReserved).WithAlias(() => resultAlias.Reserved))
					.OrderBy(x => x.Name).Asc
					.TransformUsing(Transformers.AliasToBean<NomenclatureJournalNode>());
			}
			else
			{
				itemsQuery.Where(() => !nomenclatureAlias.IsSerial)
					.SelectList(list => list
						.SelectGroup(() => nomenclatureAlias.Id).WithAlias(() => resultAlias.Id)
						.Select(() => nomenclatureAlias.Name).WithAlias(() => resultAlias.Name)
						.Select(() => nomenclatureAlias.Category).WithAlias(() => resultAlias.Category)
						.Select(() => nomenclatureAlias.GlassHolderType).WithAlias(() => resultAlias.GlassHolderType)
						.Select(() => nomenclatureAlias.OnlineStoreExternalId).WithAlias(() => resultAlias.OnlineStoreExternalId)
						.Select(() => false).WithAlias(() => resultAlias.CalculateQtyOnStock))
					.OrderBy(x => x.Name).Asc
					.TransformUsing(Transformers.AliasToBean<NomenclatureJournalNode>());
			}

			return itemsQuery;
		};

		protected override Func<NomenclatureViewModel> CreateDialogFunction =>
			() => new NomenclatureViewModel(EntityUoWBuilder.ForCreate(), UnitOfWorkFactory, commonServices,
				_employeeService, _nomenclatureSelectorFactory, _counterpartySelectorFactory, _nomenclatureRepository,
				_userRepository, new StringHandler(), _nomenclatureOnlineParametersProvider, _nomenclatureSettings);

		protected override Func<NomenclatureJournalNode, NomenclatureViewModel> OpenDialogFunction =>
			node => new NomenclatureViewModel(EntityUoWBuilder.ForOpen(node.Id), UnitOfWorkFactory, commonServices,
				_employeeService, _nomenclatureSelectorFactory, _counterpartySelectorFactory, _nomenclatureRepository,
				_userRepository, new StringHandler(), _nomenclatureOnlineParametersProvider, _nomenclatureSettings);
	}
}
