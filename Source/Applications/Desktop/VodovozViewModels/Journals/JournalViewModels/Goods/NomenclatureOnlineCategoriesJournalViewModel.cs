using System;
using Autofac;
using NHibernate;
using NHibernate.Transform;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Project.Services;
using QS.Services;
using Vodovoz.Domain.Goods;
using Vodovoz.ViewModels.Journals.JournalNodes.Goods;
using Vodovoz.ViewModels.ViewModels.Goods;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.Goods
{
	public class NomenclatureOnlineCategoriesJournalViewModel
		: EntityJournalViewModelBase<NomenclatureOnlineCategory, NomenclatureOnlineCategoryViewModel, NomenclatureOnlineCategoriesJournalNode>
	{
		private readonly ILifetimeScope _scope;

		public NomenclatureOnlineCategoriesJournalViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			IInteractiveService interactiveService,
			INavigationManager navigationManager,
			IDeleteEntityService deleteEntityService,
			ICurrentPermissionService currentPermissionService,
			ILifetimeScope scope)
			: base(unitOfWorkFactory, interactiveService, navigationManager, deleteEntityService, currentPermissionService)
		{
			_scope = scope ?? throw new ArgumentNullException(nameof(scope));
			VisibleDeleteAction = false;
			UpdateOnChanges(typeof(NomenclatureOnlineCategory), typeof(NomenclatureOnlineGroup));
		}

		protected override IQueryOver<NomenclatureOnlineCategory> ItemsQuery(IUnitOfWork uow)
		{
			NomenclatureOnlineGroup onlineGroupAlias = null;
			NomenclatureOnlineCategoriesJournalNode resultAlias = null;

			var query = uow.Session.QueryOver<NomenclatureOnlineCategory>()
				.Left.JoinAlias(oc => oc.NomenclatureOnlineGroup, () => onlineGroupAlias);
				
			query.Where(GetSearchCriterion<VodovozWebSiteNomenclatureOnlineCatalog>(
					oc => oc.Id,
					oc => oc.Name
				));
			
			query.SelectList(list => list
					.Select(oc => oc.Id).WithAlias(() => resultAlias.Id)
					.Select(oc => oc.Name).WithAlias(() => resultAlias.Name)
					.Select(() => onlineGroupAlias.Name).WithAlias(() => resultAlias.OnlineGroup))
				.TransformUsing(Transformers.AliasToBean<NomenclatureOnlineCategoriesJournalNode>());

			return query;
		}

		protected override void CreateEntityDialog()
		{
			NavigationManager.OpenViewModel<NomenclatureOnlineCategoryViewModel, IEntityUoWBuilder, ILifetimeScope>(
				this, EntityUoWBuilder.ForCreate(), _scope);
		}

		protected override void EditEntityDialog(NomenclatureOnlineCategoriesJournalNode node)
		{
			NavigationManager.OpenViewModel<NomenclatureOnlineCategoryViewModel, IEntityUoWBuilder, ILifetimeScope>(
				this, EntityUoWBuilder.ForOpen(node.Id), _scope);
		}
	}
}
