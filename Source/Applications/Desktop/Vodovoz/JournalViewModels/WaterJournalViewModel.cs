using System;
using System.Linq;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Transform;
using QS.BusinessCommon.Domain;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Project.Journal.EntitySelector;
using QS.Services;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Operations;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories;
using Vodovoz.EntityRepositories.Goods;
using Vodovoz.Infrastructure.Services;
using Vodovoz.Services;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Dialogs.Goods;
using Vodovoz.ViewModels.Journals.JournalNodes.Goods;
using VodovozInfrastructure.StringHandlers;
using VodovozOrder = Vodovoz.Domain.Orders.Order;

namespace Vodovoz.JournalViewModels
{
	public class WaterJournalViewModel : SingleEntityJournalViewModelBase<Nomenclature, NomenclatureViewModel, WaterJournalNode>
	{
		private readonly IEmployeeService employeeService;
		private readonly INomenclatureJournalFactory _nomenclatureSelectorFactory;
		private readonly ICounterpartyJournalFactory counterpartySelectorFactory;
		private readonly INomenclatureRepository nomenclatureRepository;
		private readonly IUserRepository userRepository;

		public WaterJournalViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			IEmployeeService employeeService,
			INomenclatureJournalFactory nomenclatureSelectorFactory,
			ICounterpartyJournalFactory counterpartySelectorFactory,
			INomenclatureRepository nomenclatureRepository,
			IUserRepository userRepository
		) : base(unitOfWorkFactory, commonServices)
		{
			this.employeeService = employeeService ?? throw new ArgumentNullException(nameof(employeeService));
			_nomenclatureSelectorFactory = nomenclatureSelectorFactory ?? throw new ArgumentNullException(nameof(nomenclatureSelectorFactory));
			this.counterpartySelectorFactory = counterpartySelectorFactory ?? throw new ArgumentNullException(nameof(counterpartySelectorFactory));
			this.nomenclatureRepository = nomenclatureRepository ?? throw new ArgumentNullException(nameof(nomenclatureRepository));
			this.userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));

			TabName = "Выбор номенклатуры воды";
			SetOrder(x => x.Name);
			UpdateOnChanges(
				typeof(Nomenclature),
				typeof(MeasurementUnits),
				typeof(GoodsAccountingOperation),
				typeof(VodovozOrder),
				typeof(OrderItem)
			);
		}

		protected override void CreateNodeActions()
		{
			NodeActionsList.Clear();
			CreateDefaultSelectAction();
			CreateEditAction();
			CreateDefaultDeleteAction();
		}

		private void CreateEditAction()
		{
			var editAction = new JournalAction("Изменить",
				(selected) => {
					var selectedNodes = selected.OfType<WaterJournalNode>();
					if(selectedNodes == null || selectedNodes.Count() != 1) {
						return false;
					}
					WaterJournalNode selectedNode = selectedNodes.First();
					if(!EntityConfigs.ContainsKey(selectedNode.EntityType)) {
						return false;
					}
					var config = EntityConfigs[selectedNode.EntityType];
					return config.PermissionResult.CanRead;
				},
				(selected) => true,
				(selected) => {
					var selectedNodes = selected.OfType<WaterJournalNode>();
					if(selectedNodes == null || selectedNodes.Count() != 1) {
						return;
					}
					WaterJournalNode selectedNode = selectedNodes.First();
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
			WaterJournalNode resultAlias = null;

			var itemsQuery = uow.Session.QueryOver(() => nomenclatureAlias)
				.Left.JoinAlias(() => nomenclatureAlias.Unit, () => unitAlias);

			itemsQuery.Where(
				GetSearchCriterion(
					() => nomenclatureAlias.Name,
					() => nomenclatureAlias.Id
				)
			);

			itemsQuery
				.Where(() => nomenclatureAlias.Category == NomenclatureCategory.water)
				.Where(() => !nomenclatureAlias.IsArchive)
				.SelectList(list => list
					.SelectGroup(() => nomenclatureAlias.Id).WithAlias(() => resultAlias.Id)
					.Select(() => unitAlias.Name).WithAlias(() => resultAlias.UnitName)
					.Select(() => unitAlias.Digits).WithAlias(() => resultAlias.UnitDigits)
					.Select(() => nomenclatureAlias.Name).WithAlias(() => resultAlias.Name)
				)
				.OrderBy(x => x.Name).Asc
				.TransformUsing(Transformers.AliasToBean<WaterJournalNode>());

			return itemsQuery;
		};

		protected override Func<NomenclatureViewModel> CreateDialogFunction => () =>
			throw new NotSupportedException("Не поддерживается создание номенклатуры воды из текущего журнала");

		protected override Func<WaterJournalNode, NomenclatureViewModel> OpenDialogFunction =>
			node => new NomenclatureViewModel(EntityUoWBuilder.ForOpen(node.Id), UnitOfWorkFactory, commonServices,
				employeeService, _nomenclatureSelectorFactory, counterpartySelectorFactory, nomenclatureRepository,
				userRepository, new StringHandler());
	}
}
