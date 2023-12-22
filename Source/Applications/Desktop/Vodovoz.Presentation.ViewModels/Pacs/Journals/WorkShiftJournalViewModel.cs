using NHibernate;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.Services;
using Vodovoz.Core.Domain.Pacs;
using Vodovoz.Presentation.ViewModels.Employees;

namespace Vodovoz.Presentation.ViewModels.Pacs.Journals
{
	public class WorkShiftJournalViewModel : EntityJournalViewModelBase<WorkShift, WorkShiftViewModel, WorkShift>
	{
		public WorkShiftJournalViewModel(
			IUnitOfWorkFactory uowFactory,
			IInteractiveService interactiveService,
			INavigationManager navigationManager,
			ICurrentPermissionService currentPermissionService
		) : base(uowFactory, interactiveService, navigationManager, currentPermissionService: currentPermissionService)
		{
			Title = "Рабочие смены";
		}

		protected override IQueryOver<WorkShift> ItemsQuery(IUnitOfWork uow)
		{
			return uow.Session.QueryOver<WorkShift>();
		}

		protected override void CreateEntityDialog()
		{
			NavigationManager.OpenViewModel<WorkShiftViewModel, IEntityIdentifier>(this, EntityIdentifier.NewEntity());
		}

		protected override void EditEntityDialog(WorkShift node)
		{
			NavigationManager.OpenViewModel<WorkShiftViewModel, IEntityIdentifier>(this, EntityIdentifier.OpenEntity(node.Id));
		}
	}
}
