using NHibernate;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.Services;
using Vodovoz.Core.Application.Entity;
using Vodovoz.Domain.Logistic;

namespace Vodovoz.Presentation.ViewModels.Logistic
{
	public class AcceptBeforeJournalViewModel : EntityJournalViewModelBase<AcceptBefore, AcceptBeforeViewModel, AcceptBefore>
	{
		public AcceptBeforeJournalViewModel(
			IUnitOfWorkFactory uowFactory,
			IInteractiveService interactiveService,
			INavigationManager navigationManager,
			ICurrentPermissionService currentPermissionService
		) : base(uowFactory, interactiveService, navigationManager)
		{
			Title = "Время приема до";

			UpdateOnChanges(typeof(AcceptBefore));
		}

		protected override IQueryOver<AcceptBefore> ItemsQuery(IUnitOfWork uow)
		{
			var query = uow.Session.QueryOver<AcceptBefore>();
			query.Where(GetSearchCriterion<AcceptBefore>(
				x => x.Name
			));

			return query;
		}

		protected override void CreateEntityDialog()
		{
			NavigationManager.OpenViewModel<AcceptBeforeViewModel, IEntityIdentifier>(this, EntityIdentifier.NewEntity());
		}

		protected override void EditEntityDialog(AcceptBefore node)
		{
			NavigationManager.OpenViewModel<AcceptBeforeViewModel, IEntityIdentifier>(this, EntityIdentifier.OpenEntity(node.Id));
		}
	}
}
