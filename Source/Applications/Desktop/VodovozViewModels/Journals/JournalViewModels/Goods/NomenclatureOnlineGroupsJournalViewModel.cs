using Autofac;
using NHibernate;
using NHibernate.Transform;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.DB;
using QS.Project.Journal;
using QS.Project.Services;
using QS.Services;
using Vodovoz.Domain.Goods;
using Vodovoz.ViewModels.Journals.JournalNodes.Goods;
using Vodovoz.ViewModels.ViewModels.Goods;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.Goods
{
	public class NomenclatureOnlineGroupsJournalViewModel
		: EntityJournalViewModelBase<NomenclatureOnlineGroup, NomenclatureOnlineGroupViewModel, NomenclatureOnlineGroupsJournalNode>
	{
		public NomenclatureOnlineGroupsJournalViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			IInteractiveService interactiveService,
			INavigationManager navigationManager,
			IDeleteEntityService deleteEntityService,
			ICurrentPermissionService currentPermissionService,
			ILifetimeScope scope)
			: base(unitOfWorkFactory, interactiveService, navigationManager, deleteEntityService, currentPermissionService)
		{
			VisibleDeleteAction = false;
			UpdateOnChanges(typeof(NomenclatureOnlineGroup), typeof(NomenclatureOnlineCategory));
		}

		protected override IQueryOver<NomenclatureOnlineGroup> ItemsQuery(IUnitOfWork uow)
		{
			NomenclatureOnlineGroupsJournalNode resultAlias = null;
			NomenclatureOnlineCategory onlineCategoriesAlias = null;

			var onlineCategories = CustomProjections.GroupConcat(
				() => onlineCategoriesAlias.Name,
				orderByExpression: () => onlineCategoriesAlias.Name,
				separator: ", ");

			var query = uow.Session.QueryOver<NomenclatureOnlineGroup>()
				.Left.JoinAlias(og => og.NomenclatureOnlineCategories, () => onlineCategoriesAlias);
				
			query.Where(GetSearchCriterion<VodovozWebSiteNomenclatureOnlineCatalog>(
					og => og.Id,
					og => og.Name
				));
			
			query.SelectList(list => list
					.SelectGroup(og => og.Id).WithAlias(() => resultAlias.Id)
					.Select(og => og.Name).WithAlias(() => resultAlias.Name)
					.Select(onlineCategories).WithAlias(() => resultAlias.OnlineCategories))
				.TransformUsing(Transformers.AliasToBean<NomenclatureOnlineGroupsJournalNode>());

			return query;
		}
	}
}
