using System;
using System.Linq;
using Autofac;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.SqlCommand;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.DB;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Project.Services;
using QS.Services;
using QS.ViewModels.Dialog;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Operations;
using Vodovoz.ViewModels.Journals.FilterViewModels.Goods;
using Vodovoz.ViewModels.Journals.JournalNodes.Goods;
using Vodovoz.ViewModels.ViewModels.Goods;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.Nomenclatures
{
	public class InventoryInstancesJournalViewModel
		: EntityJournalViewModelBase<InventoryNomenclatureInstance, InventoryInstanceViewModel, InventoryInstancesJournalNode>
	{
		private readonly ILifetimeScope _scope;
		private InventoryInstancesJournalFilterViewModel _filterViewModel;

		public InventoryInstancesJournalViewModel(
			IUnitOfWorkFactory unitOfWorkFactory, 
			ICommonServices commonServices,
			INavigationManager navigationManager,
			ILifetimeScope scope,
			IDeleteEntityService deleteEntityService = null,
			Action<InventoryInstancesJournalFilterViewModel> filterParams = null)
			: base(unitOfWorkFactory, commonServices.InteractiveService, navigationManager, deleteEntityService,
				commonServices.CurrentPermissionService)
		{
			if(unitOfWorkFactory is null)
			{
				throw new ArgumentNullException(nameof(unitOfWorkFactory));
			}
			if(navigationManager is null)
			{
				throw new ArgumentNullException(nameof(navigationManager));
			}

			_scope = scope ?? throw new ArgumentNullException(nameof(scope));
			
			TabName = "Журнал экземпляров номенклатур с инвентарным учетом";

			CreateFilter(filterParams);
		}
		
		protected override void CreateNodeActions()
		{
			NodeActionsList.Clear();
			CreateDefaultSelectAction();

			var canCreate = CurrentPermissionService.ValidateEntityPermission(typeof(InventoryNomenclatureInstance)).CanCreate;
			var canEdit = CurrentPermissionService.ValidateEntityPermission(typeof(InventoryNomenclatureInstance)).CanRead;

			var addAction = new JournalAction("Добавить",
				selected => canCreate,
				selected => VisibleCreateAction,
				selected => CreateEntityDialog(),
				"Insert"
			);
			NodeActionsList.Add(addAction);

			var editAction = new JournalAction("Изменить",
				selected => canEdit && selected.Any(),
				selected => VisibleEditAction,
				selected => selected.Cast<InventoryInstancesJournalNode>().ToList().ForEach(EditEntityDialog)
			);
			NodeActionsList.Add(editAction);
			
			var duplicateEntityAction = new JournalAction("Дублировать экземпляр",
				selected => canEdit && selected.Any(),
				selected => true,
				selected => selected.Cast<InventoryInstancesJournalNode>()
					.ToList()
					.ForEach(OpenEntityDialogForDuplicate)
			);
			NodeActionsList.Add(duplicateEntityAction);

			if(SelectionMode == JournalSelectionMode.None)
			{
				RowActivatedAction = editAction;
			}
		}
		
		protected override IQueryOver<InventoryNomenclatureInstance> ItemsQuery(IUnitOfWork uow)
		{
			Nomenclature nomenclatureAlias = null;
			InventoryNomenclatureInstance instanceAlias = null;
			InstanceGoodsAccountingOperation instanceOperationAlias = null;
			InventoryInstancesJournalNode resultAlias = null;

			IProjection balanceProjection = null;
			
			var query = uow.Session.QueryOver(() => instanceAlias)
				.JoinAlias(ini => ini.Nomenclature, () => nomenclatureAlias);
			
			query.Where(GetSearchCriterion(
				() => instanceAlias.Id,
				() => nomenclatureAlias.Name));
			
			if(!_filterViewModel.ShowArchive)
			{
				query.Where(ini => !ini.IsArchive);
			}

			if(_filterViewModel.Nomenclature != null)
			{
				query.Where(ini => ini.Nomenclature.Id == _filterViewModel.Nomenclature.Id);
			}

			if(!string.IsNullOrWhiteSpace(_filterViewModel.InventoryNumber))
			{
				query.Where(Restrictions.Like(
					Projections.Property(() => instanceAlias.InventoryNumber),
					_filterViewModel.InventoryNumber,
					MatchMode.Anywhere));
			}

			if(_filterViewModel.ExcludedInventoryInstancesIds != null && _filterViewModel.ExcludedInventoryInstancesIds.Any())
			{
				query.WhereRestrictionOn(ini => ini.Id).Not.IsIn(_filterViewModel.ExcludedInventoryInstancesIds);
			}

			if(_filterViewModel.OnlyWithZeroBalance)
			{
				query.JoinEntityAlias(() => instanceOperationAlias,
					() => instanceOperationAlias.InventoryNomenclatureInstance.Id == instanceAlias.Id,
					JoinType.LeftOuterJoin);
				
				balanceProjection = Projections.Sum(() => instanceOperationAlias.Amount);
				
				query.Where(CustomRestrictions.OrHaving(
					Restrictions.Eq(balanceProjection, 0),
					Restrictions.IsNull(balanceProjection)));
			}

			return query.SelectList(list => list
				.SelectGroup(ini => ini.Id).WithAlias(() => resultAlias.Id)
				.Select(ini => ini.InventoryNumber).WithAlias(() => resultAlias.InventoryNumber)
				.Select(ini => ini.IsUsed).WithAlias(() => resultAlias.IsUsed)
				.Select(() => nomenclatureAlias.Id).WithAlias(() => resultAlias.NomenclatureId)
				.Select(() => nomenclatureAlias.Name).WithAlias(() => resultAlias.NomenclatureName)
				.Select(balanceProjection))
				.TransformUsing(Transformers.AliasToBean<InventoryInstancesJournalNode>())
				.OrderBy(() => nomenclatureAlias.Name).Asc
				.ThenBy(ini => ini.InventoryNumber).Asc;
		}

		private void CreateFilter(Action<InventoryInstancesJournalFilterViewModel> filterParams)
		{
			Autofac.Core.Parameter[] parameters = {
				new TypedParameter(typeof(DialogViewModelBase), this),
				new TypedParameter(typeof(Action<InventoryInstancesJournalFilterViewModel>), filterParams)
			};

			_filterViewModel = _scope.Resolve<InventoryInstancesJournalFilterViewModel>(parameters);
			_filterViewModel.OnFiltered += OnFilterViewModelFiltered;
			JournalFilter = _filterViewModel;
		}

		private void OnFilterViewModelFiltered(object sender, EventArgs e)
		{
			Refresh();
		}

		private void OpenEntityDialogForDuplicate(InventoryInstancesJournalNode node)
		{
			NavigationManager.OpenViewModel<InventoryInstanceViewModel, IEntityUoWBuilder, Nomenclature>(
				this,
				EntityUoWBuilder.ForCreate(),
				UoW.GetById<Nomenclature>(node.NomenclatureId));
		}

		public override void Dispose()
		{
			_filterViewModel.OnFiltered -= OnFilterViewModelFiltered;
			base.Dispose();
		}
	}
}
