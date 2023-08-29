using NHibernate;
using NHibernate.Transform;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.Project.Services;
using QS.Services;
using Vodovoz.Domain.Goods;
using Vodovoz.ViewModels.Journals.JournalNodes.Goods;
using Vodovoz.ViewModels.ViewModels.Goods;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.Goods
{
	[Appellative(NominativePlural = "Онлайн каталоги номенклатур мобильного приложения")]
	public class MobileAppNomenclatureOnlineCatalogsJournalViewModel
		: EntityJournalViewModelBase<
			MobileAppNomenclatureOnlineCatalog,
			MobileAppNomenclatureOnlineCatalogViewModel,
			NomenclatureOnlineCatalogsJournalNode>
	{
		public MobileAppNomenclatureOnlineCatalogsJournalViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			IInteractiveService interactiveService,
			INavigationManager navigationManager,
			IDeleteEntityService deleteEntityService,
			ICurrentPermissionService currentPermissionService)
			: base(unitOfWorkFactory, interactiveService, navigationManager, deleteEntityService, currentPermissionService)
		{
			
		}

		protected override IQueryOver<MobileAppNomenclatureOnlineCatalog> ItemsQuery(IUnitOfWork uow)
		{
			NomenclatureOnlineCatalogsJournalNode resultAlias = null;

			var query = uow.Session.QueryOver<MobileAppNomenclatureOnlineCatalog>()
				.SelectList(list => list
					.Select(oc => oc.Id).WithAlias(() => resultAlias.Id)
					.Select(oc => oc.Name).WithAlias(() => resultAlias.Name))
				.TransformUsing(Transformers.AliasToBean<NomenclatureOnlineCatalogsJournalNode>());

			return query;
		}
	}
}
