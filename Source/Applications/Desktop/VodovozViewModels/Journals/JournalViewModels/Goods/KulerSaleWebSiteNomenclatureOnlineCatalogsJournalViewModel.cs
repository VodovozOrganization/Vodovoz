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
	[Appellative(NominativePlural = "Онлайн каталоги номенклатур сайта Кулер сэйл")]
	public class KulerSaleWebSiteNomenclatureOnlineCatalogsJournalViewModel
		: EntityJournalViewModelBase<
			KulerSaleWebSiteNomenclatureOnlineCatalog,
			KulerSaleWebSiteNomenclatureOnlineCatalogViewModel,
			NomenclatureOnlineCatalogsJournalNode>
	{
		public KulerSaleWebSiteNomenclatureOnlineCatalogsJournalViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			IInteractiveService interactiveService,
			INavigationManager navigationManager,
			IDeleteEntityService deleteEntityService,
			ICurrentPermissionService currentPermissionService)
			: base(unitOfWorkFactory, interactiveService, navigationManager, deleteEntityService, currentPermissionService)
		{
			
		}

		protected override IQueryOver<KulerSaleWebSiteNomenclatureOnlineCatalog> ItemsQuery(IUnitOfWork uow)
		{
			NomenclatureOnlineCatalogsJournalNode resultAlias = null;

			var query = uow.Session.QueryOver<KulerSaleWebSiteNomenclatureOnlineCatalog>()
				.SelectList(list => list
					.Select(oc => oc.Id).WithAlias(() => resultAlias.Id)
					.Select(oc => oc.Name).WithAlias(() => resultAlias.Name))
				.TransformUsing(Transformers.AliasToBean<NomenclatureOnlineCatalogsJournalNode>());

			return query;
		}
	}
}
