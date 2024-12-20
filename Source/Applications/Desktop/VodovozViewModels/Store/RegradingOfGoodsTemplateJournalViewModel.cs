using NHibernate;
using NHibernate.Transform;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.Project.Services;
using QS.Services;
using Vodovoz.Domain.Store;

namespace Vodovoz.ViewModels.Store
{
	public class RegradingOfGoodsTemplateJournalViewModel : EntityJournalViewModelBase<
			RegradingOfGoodsTemplate,
			RegradingOfGoodsTemplateViewModel,
			RegradingOfGoodsTemplateJournalNode>
	{
		public RegradingOfGoodsTemplateJournalViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			IInteractiveService interactiveService,
			INavigationManager navigationManager,
			IDeleteEntityService deleteEntityService,
			ICurrentPermissionService currentPermissionService)
			: base(unitOfWorkFactory, interactiveService, navigationManager, deleteEntityService, currentPermissionService)
		{
		}

		protected override IQueryOver<RegradingOfGoodsTemplate> ItemsQuery(IUnitOfWork uow)
		{
			RegradingOfGoodsTemplateJournalNode resultAlias = null;

			return uow.Session.QueryOver<RegradingOfGoodsTemplate>()
				.SelectList(list => list
					.Select(x => x.Id).WithAlias(() => resultAlias.Id)
					.Select(x => x.Name).WithAlias(() => resultAlias.Name))
				.TransformUsing(Transformers.AliasToBean(typeof(RegradingOfGoodsTemplateJournalNode)));
		}
	}
}
