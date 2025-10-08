using NHibernate;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.Project.Services;
using QS.Services;
using Vodovoz.Domain.Employees;
using Vodovoz.ViewModels.Employees;
using Vodovoz.ViewModels.Journals.Nodes.Employees;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.Employees
{
	public class FineCategoryJournalViewModel : EntityJournalViewModelBase<
			FineCategory,
			FineCategoryViewModel,
			FineCategoryJournalNode>
	{
		public FineCategoryJournalViewModel(IUnitOfWorkFactory unitOfWorkFactory, IInteractiveService interactiveService, INavigationManager navigationManager, IDeleteEntityService deleteEntityService = null, ICurrentPermissionService currentPermissionService = null) : base(unitOfWorkFactory, interactiveService, navigationManager, deleteEntityService, currentPermissionService)
		{
		}

		protected override IQueryOver<FineCategory> ItemsQuery(IUnitOfWork uow)
		{
			throw new System.NotImplementedException();
		}
	}
}
