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
	public class BusinessAccountsJournalViewModel :
		EntityJournalViewModelBase<BusinessAccount, BusinessAccountViewModel, BusinessAccountJournalNode>
	{
		public BusinessAccountsJournalViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			IInteractiveService interactiveService,
			INavigationManager navigationManager,
			ICurrentPermissionService currentPermissionService)
			: base(unitOfWorkFactory, interactiveService, navigationManager, currentPermissionService: currentPermissionService)
		{
			
		}

		protected override IQueryOver<BusinessAccount> ItemsQuery(IUnitOfWork uow)
		{
			BusinessActivity businessActivityAlias = null;
			Funds fundsAlias = null;
			BusinessAccountJournalNode resultAlias = null;

			var query = uow.Session.QueryOver<BusinessAccount>()
				.JoinAlias(ba => ba.BusinessActivity, () => businessActivityAlias)
				.JoinAlias(ba => ba.Funds, () => fundsAlias)
				.SelectList(list => list
					.Select(ba => ba.Id).WithAlias(() => resultAlias.Id)
					.Select(ba => ba.Name).WithAlias(() => resultAlias.Name)
					.Select(ba => ba.Bank).WithAlias(() => resultAlias.Bank)
					.Select(ba => ba.Number).WithAlias(() => resultAlias.Number)
					.Select(() => businessActivityAlias.Name).WithAlias(() => resultAlias.BusinessActivity)
					.Select(() => fundsAlias.Name).WithAlias(() => resultAlias.Funds)
				)
				.TransformUsing(Transformers.AliasToBean<BusinessAccountJournalNode>());

			return query;
		}
	}
}
