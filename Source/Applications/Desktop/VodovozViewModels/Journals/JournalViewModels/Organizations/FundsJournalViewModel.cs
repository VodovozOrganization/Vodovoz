using NHibernate;
using NHibernate.Transform;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.Services;
using Vodovoz.Core.Domain.Organizations;
using Vodovoz.Presentation.ViewModels.Organisations;
using Vodovoz.ViewModels.Journals.JournalNodes.Organizations;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.Organizations
{
	public class FundsJournalViewModel :
		EntityJournalViewModelBase<Funds, FundsViewModel, FundsJournalNode>
	{
		public FundsJournalViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			IInteractiveService interactiveService,
			INavigationManager navigationManager,
			ICurrentPermissionService currentPermissionService)
			: base(unitOfWorkFactory, interactiveService, navigationManager, currentPermissionService: currentPermissionService)
		{
			
		}

		protected override IQueryOver<Funds> ItemsQuery(IUnitOfWork uow)
		{
			FundsJournalNode resultAlias = null;

			var query = uow.Session.QueryOver<Funds>()
				.SelectList(list => list
					.Select(f => f.Id).WithAlias(() => resultAlias)
					.Select(f => f.Name).WithAlias(() => resultAlias.Name))
				.TransformUsing(Transformers.AliasToBean<FundsJournalNode>());

			return query;
		}
	}
}
