using System;
using System.Linq;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Project.Journal.DataLoader;
using QS.Services;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Suppliers;
using Vodovoz.EntityRepositories.Suppliers;
using Vodovoz.FilterViewModels.Suppliers;
using Vodovoz.Journals.JournalNodes;
using Vodovoz.Services;
using Vodovoz.ViewModels.Suppliers;

namespace Vodovoz.JournalViewModels.Suppliers
{
	public class RequestsToSuppliersJournalViewModel :
		FilterableSingleEntityJournalViewModelBase<
			RequestToSupplier,
			RequestToSupplierViewModel,
			RequestToSupplierJournalNode,
			RequestsToSuppliersFilterViewModel>
	{
		private readonly ISupplierPriceItemsRepository _supplierPriceItemsRepository;
		private readonly ITdiCompatibilityNavigation _navigationManager;
		private readonly IEmployeeService _employeeService;

		public RequestsToSuppliersJournalViewModel(
			RequestsToSuppliersFilterViewModel filterViewModel,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			ITdiCompatibilityNavigation navigationManager,
			IEmployeeService employeeService,
			ISupplierPriceItemsRepository supplierPriceItemsRepository) : base(filterViewModel, unitOfWorkFactory, commonServices, navigation: navigationManager)
		{
			_navigationManager = navigationManager ?? throw new ArgumentNullException(nameof(navigationManager));
			_employeeService = employeeService ?? throw new ArgumentNullException(nameof(employeeService));
			_supplierPriceItemsRepository = supplierPriceItemsRepository ?? throw new ArgumentNullException(nameof(supplierPriceItemsRepository));

			filterViewModel.Journal = this;

			UseSlider = false;
			TabName = "Журнал заявок поставщикам";

			var threadLoader = DataLoader as ThreadDataLoader<RequestToSupplierJournalNode>;
			threadLoader.MergeInOrderBy(x => x.Id, true);

			UpdateOnChanges(typeof(RequestToSupplier));
		}

		protected override void CreateNodeActions()
		{
			NodeActionsList.Clear();
			CreateDefaultSelectAction();
			CreateDefaultAddActions();
			CreateEditAction();
			CreateDefaultDeleteAction();
		}

		private void CreateEditAction()
		{
			var editAction = new JournalAction("Изменить",
				(selected) => {
					var selectedNodes = selected.OfType<RequestToSupplierJournalNode>();
					if(selectedNodes == null || selectedNodes.Count() != 1) {
						return false;
					}
					RequestToSupplierJournalNode selectedNode = selectedNodes.First();
					if(!EntityConfigs.ContainsKey(selectedNode.EntityType)) {
						return false;
					}
					var config = EntityConfigs[selectedNode.EntityType];
					return config.PermissionResult.CanRead;
				},
				(selected) => true,
				(selected) => {
					var selectedNodes = selected.OfType<RequestToSupplierJournalNode>();
					if(selectedNodes == null || selectedNodes.Count() != 1) {
						return;
					}
					RequestToSupplierJournalNode selectedNode = selectedNodes.First();
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

		protected override Func<IUnitOfWork, IQueryOver<RequestToSupplier>> ItemsSourceQueryFunction => (uow) => {
			Employee authorAlias = null;
			Nomenclature nomenclaturesAlias = null;
			RequestToSupplierJournalNode resultAlias = null;

			var authorProjection = Projections.SqlFunction(
				new SQLFunctionTemplate(NHibernateUtil.String, "GET_PERSON_NAME_WITH_INITIALS(?1, ?2, ?3)"),
				NHibernateUtil.String,
				Projections.Property(() => authorAlias.LastName),
				Projections.Property(() => authorAlias.Name),
				Projections.Property(() => authorAlias.Patronymic)
			);

			var query = uow.Session.QueryOver<RequestToSupplier>()
				.Left.JoinAlias(x => x.Creator, () => authorAlias)
				.Left.JoinAlias(x => x.RequestingNomenclatureItems, () => nomenclaturesAlias);

			if(FilterViewModel?.RestrictNomenclature != null) {
				var subquery = QueryOver.Of<RequestToSupplierItem>()
					.Where(r => r.Nomenclature.Id == FilterViewModel.RestrictNomenclature.Id && !r.Transfered)
					.Select(r => r.RequestToSupplier.Id);
				query.WithSubquery.WhereProperty(r => r.Id).In(subquery);
			}

			if(FilterViewModel?.RestrictStartDate != null)
				query.Where(x => x.CreatingDate >= FilterViewModel.RestrictStartDate.Value);

			if(FilterViewModel?.RestrictEndDate != null)
				query.Where(o => o.CreatingDate <= FilterViewModel.RestrictEndDate.Value.AddDays(1).AddTicks(-1));

			if(FilterViewModel?.RestrictStatus != null)
				query.Where(o => o.Status == FilterViewModel.RestrictStatus.Value);

			query.Where(
				GetSearchCriterion<RequestToSupplier>(
					x => x.Id,
					x => x.Name
				)
			);

			var result = query.SelectList(list => list
					.SelectGroup(x => x.Id).WithAlias(() => resultAlias.Id)
					.Select(x => x.Name).WithAlias(() => resultAlias.Name)
					.Select(x => x.CreatingDate).WithAlias(() => resultAlias.Created)
					.Select(authorProjection).WithAlias(() => resultAlias.Author)
					.Select(x => x.Status).WithAlias(() => resultAlias.Status)
				)
				.TransformUsing(Transformers.AliasToBean<RequestToSupplierJournalNode>())
				.OrderBy(x => x.Id)
				.Desc;

			return result;
		};

		protected override Func<RequestToSupplierViewModel> CreateDialogFunction => () => new RequestToSupplierViewModel(
			EntityUoWBuilder.ForCreate(),
			UnitOfWorkFactory,
			commonServices,
			_navigationManager,
			_employeeService,
			_supplierPriceItemsRepository
		);

		protected override Func<RequestToSupplierJournalNode, RequestToSupplierViewModel> OpenDialogFunction => n =>
			new RequestToSupplierViewModel(
				EntityUoWBuilder.ForOpen(n.Id),
				UnitOfWorkFactory,
				commonServices,
				_navigationManager,
				_employeeService,
				_supplierPriceItemsRepository
			);

		protected override void CreatePopupActions()
		{
			PopupActionsList.Add(
				new JournalAction(
					"Скопировать заявку",
					n => EntityConfigs[typeof(RequestToSupplier)].PermissionResult.CanCreate,
					n => true,
					n => {
						var currentRequestId = n.OfType<RequestToSupplierJournalNode>().FirstOrDefault()?.Id;
						if(currentRequestId.HasValue) {
							var currentRequest = UoW.GetById<RequestToSupplier>(currentRequestId.Value);

							RequestToSupplierViewModel newRequestVM = new RequestToSupplierViewModel(
								EntityUoWBuilder.ForCreate(),
								UnitOfWorkFactory,
								commonServices,
								_navigationManager,
								_employeeService,
								_supplierPriceItemsRepository
							);
							newRequestVM.Entity.Name = currentRequest.Name;
							newRequestVM.Entity.WithDelayOnly = currentRequest.WithDelayOnly;

							foreach(ILevelingRequestNode item in currentRequest.ObservableRequestingNomenclatureItems) {
								if(item is RequestToSupplierItem requestItem) {
									var newItem = new RequestToSupplierItem {
										Nomenclature = requestItem.Nomenclature,
										Quantity = requestItem.Quantity,
										RequestToSupplier = newRequestVM.Entity
									};
									newRequestVM.Entity.ObservableRequestingNomenclatureItems.Add(newItem);
								}
							}

							TabParent.AddSlaveTab(this, newRequestVM);
						}
					}
				)
			);

			PopupActionsList.Add(
				new JournalAction(
					"Закрыть заявку",
					n => {
						var currentRequestId = n.OfType<RequestToSupplierJournalNode>().FirstOrDefault()?.Id;
						if(currentRequestId.HasValue) {
							var currentRequest = UoW.GetById<RequestToSupplier>(currentRequestId.Value);
							return currentRequest.Status == RequestStatus.InProcess;
						}
						return false;
					},
					n => true,
					n => {
						var currentRequestId = n.OfType<RequestToSupplierJournalNode>().FirstOrDefault()?.Id;
						if(currentRequestId.HasValue) {
							var currentRequest = UoW.GetById<RequestToSupplier>(currentRequestId.Value);
							currentRequest.Status = RequestStatus.Closed;
							UoW.Save(currentRequest);
							UoW.Commit();
						}
					}
				)
			);
		}
	}
}
