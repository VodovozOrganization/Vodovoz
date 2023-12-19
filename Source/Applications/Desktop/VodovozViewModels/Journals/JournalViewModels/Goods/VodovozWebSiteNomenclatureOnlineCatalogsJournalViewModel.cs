using System;
using NHibernate;
using NHibernate.Transform;
using QS.Dialog;
using QS.DomainModel.Entity;
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
	public class VodovozWebSiteNomenclatureOnlineCatalogsJournalViewModel
		: EntityJournalViewModelBase<
			VodovozWebSiteNomenclatureOnlineCatalog,
			NomenclatureOnlineCatalogViewModel,
			NomenclatureOnlineCatalogsJournalNode>
	{
		public VodovozWebSiteNomenclatureOnlineCatalogsJournalViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			IInteractiveService interactiveService,
			INavigationManager navigationManager,
			IDeleteEntityService deleteEntityService,
			ICurrentPermissionService currentPermissionService)
			: base(unitOfWorkFactory, interactiveService, navigationManager, deleteEntityService, currentPermissionService)
		{
			VisibleDeleteAction = false;
		}

		protected override IQueryOver<VodovozWebSiteNomenclatureOnlineCatalog> ItemsQuery(IUnitOfWork uow)
		{
			NomenclatureOnlineCatalogsJournalNode resultAlias = null;

			var query = uow.Session.QueryOver<VodovozWebSiteNomenclatureOnlineCatalog>();

			query.Where(GetSearchCriterion<VodovozWebSiteNomenclatureOnlineCatalog>(
				oc => oc.Id,
				oc => oc.Name
			));
				
			query.SelectList(list => list
					.Select(oc => oc.Id).WithAlias(() => resultAlias.Id)
					.Select(oc => oc.Name).WithAlias(() => resultAlias.Name)
					.Select(oc => oc.ExternalId).WithAlias(() => resultAlias.ExternalId))
				.TransformUsing(Transformers.AliasToBean<NomenclatureOnlineCatalogsJournalNode>());

			return query;
		}

		protected override void CreateEntityDialog()
		{
			NavigationManager.OpenViewModel<NomenclatureOnlineCatalogViewModel, IEntityUoWBuilder, Type>(
			this, EntityUoWBuilder.ForCreate(), typeof(VodovozWebSiteNomenclatureOnlineCatalog));
		}

		protected override void EditEntityDialog(NomenclatureOnlineCatalogsJournalNode node)
		{
			NavigationManager.OpenViewModel<NomenclatureOnlineCatalogViewModel, IEntityUoWBuilder, Type>(
			this, EntityUoWBuilder.ForOpen(DomainHelper.GetId(node)), typeof(VodovozWebSiteNomenclatureOnlineCatalog));
		}
	}
}
