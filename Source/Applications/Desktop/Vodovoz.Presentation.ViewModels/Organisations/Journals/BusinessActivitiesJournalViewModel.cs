using NHibernate;
using NHibernate.Transform;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.Services;
using Vodovoz.Core.Domain.Organizations;

namespace Vodovoz.Presentation.ViewModels.Organisations.Journals
{
	public class BusinessActivitiesJournalViewModel :
		EntityJournalViewModelBase<BusinessActivity, BusinessActivityViewModel, BusinessActivityJournalNode>
	{
		public BusinessActivitiesJournalViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			IInteractiveService interactiveService,
			INavigationManager navigationManager,
			ICurrentPermissionService currentPermissionService)
			: base(unitOfWorkFactory, interactiveService, navigationManager, currentPermissionService: currentPermissionService)
		{
			
		}

		protected override IQueryOver<BusinessActivity> ItemsQuery(IUnitOfWork uow)
		{
			BusinessActivityJournalNode resultAlias = null;

			var query = uow.Session.QueryOver<BusinessActivity>()
				.SelectList(list => list
					.Select(f => f.Id).WithAlias(() => resultAlias.Id)
					.Select(f => f.Name).WithAlias(() => resultAlias.Name))
				.TransformUsing(Transformers.AliasToBean<BusinessActivityJournalNode>());

			return query;
		}
	}
}
