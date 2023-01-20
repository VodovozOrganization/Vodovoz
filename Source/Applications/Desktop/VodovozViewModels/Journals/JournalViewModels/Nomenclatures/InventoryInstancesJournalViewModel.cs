using System;
using System.Linq;
using Autofac;
using NHibernate;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.Project.Services;
using QS.Services;
using QS.ViewModels.Dialog;
using Vodovoz.Domain.Goods;
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
			params Action<InventoryInstancesJournalFilterViewModel>[] filterParams)
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
				(selected) => canCreate,
				(selected) => VisibleCreateAction,
				(selected) => CreateEntityDialog(),
				"Insert"
			);
			NodeActionsList.Add(addAction);

			var editAction = new JournalAction("Изменить",
				(selected) => canEdit && selected.Any(),
				(selected) => VisibleEditAction,
				(selected) => selected.Cast<InventoryInstancesJournalNode>().ToList().ForEach(EditEntityDialog)
			);
			NodeActionsList.Add(editAction);

			if(SelectionMode == JournalSelectionMode.None)
			{
				RowActivatedAction = editAction;
			}
		}
		
		protected override IQueryOver<InventoryNomenclatureInstance> ItemsQuery(IUnitOfWork uow)
		{
			Nomenclature nomenclatureAlias = null;
			InventoryNomenclatureInstance instanceAlias = null;
			InventoryInstancesJournalNode resultAlias = null;
			
			var query = uow.Session.QueryOver(() => instanceAlias)
				.JoinAlias(ini => ini.Nomenclature, () => nomenclatureAlias);
			
			query.Where(GetSearchCriterion(() => instanceAlias.InventoryNumber));

			return query.SelectList(list => list
				.Select(() => instanceAlias.Id).WithAlias(() => resultAlias.Id)
				.Select(() => instanceAlias.InventoryNumber).WithAlias(() => resultAlias.InventoryNumber)
				.Select(() => nomenclatureAlias.Id).WithAlias(() => resultAlias.NomenclatureId)
				.Select(() => nomenclatureAlias.Name).WithAlias(() => resultAlias.NomenclatureName))
				.TransformUsing(Transformers.AliasToBean<InventoryInstancesJournalNode>());
		}

		private void CreateFilter(Action<InventoryInstancesJournalFilterViewModel>[] filterParams)
		{
			Autofac.Core.Parameter[] parameters = {
				new TypedParameter(typeof(DialogViewModelBase), this),
				new TypedParameter(typeof(Action<InventoryInstancesJournalFilterViewModel>[]), filterParams)
			};

			_filterViewModel = _scope.Resolve<InventoryInstancesJournalFilterViewModel>(parameters);
			_filterViewModel.OnFiltered += OnFilterViewModelFiltered;
			JournalFilter = _filterViewModel;
		}

		private void OnFilterViewModelFiltered(object sender, EventArgs e)
		{
			Refresh();
		}

		public override void Dispose()
		{
			_filterViewModel.OnFiltered -= OnFilterViewModelFiltered;
			base.Dispose();
		}
	}
}
