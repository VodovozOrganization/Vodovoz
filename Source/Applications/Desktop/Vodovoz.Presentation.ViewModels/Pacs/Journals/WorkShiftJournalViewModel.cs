using NHibernate;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.Project.Services;
using QS.Services;
using Vodovoz.Core.Application.Entity;
using Vodovoz.Core.Domain.Pacs;

namespace Vodovoz.Presentation.ViewModels.Pacs.Journals
{
	public class WorkShiftJournalViewModel : EntityJournalViewModelBase<WorkShift, WorkShiftViewModel, WorkShift>
	{
		public WorkShiftJournalViewModel(
			IUnitOfWorkFactory uowFactory,
			IInteractiveService interactiveService,
			INavigationManager navigationManager,
			IDeleteEntityService deleteEntityService,
			ICurrentPermissionService currentPermissionService
		) : base(uowFactory, interactiveService, navigationManager, currentPermissionService: currentPermissionService, deleteEntityService: deleteEntityService)
		{
			Title = "Рабочие смены";

			VisibleDeleteAction = false;

			UpdateOnChanges(typeof(WorkShift));
		}

		protected override IQueryOver<WorkShift> ItemsQuery(IUnitOfWork uow)
		{
			var query = uow.Session.QueryOver<WorkShift>();
			query.Where(GetSearchCriterion<WorkShift>(
				x => x.Name
			));
			return query;
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
