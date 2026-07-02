using NHibernate;
using NHibernate.Transform;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.Project.Services;
using QS.Services;
using System;
using System.Linq;
using Vodovoz.Core.Domain.Payments;
using Vodovoz.Core.Domain.Refunds;
using Vodovoz.Filters.ViewModels;
using Vodovoz.ViewModels.Journals.JournalNodes.Payments;
using Vodovoz.ViewModels.Journals.JournalNodes.Refunds;
using Vodovoz.ViewModels.ViewModels.Refunds;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.Refund
{
	public class RefundJournalViewModel : EntityJournalViewModelBase<RefundEntity, RefundViewModel, RefundJournalNode>
	{
		public RefundJournalViewModel(
			IUnitOfWorkFactory unitOfWorkFactory, 
			IInteractiveService interactiveService, 
			INavigationManager navigationManager, 
			IDeleteEntityService deleteEntityService = null, 
			ICurrentPermissionService currentPermissionService = null) : base(unitOfWorkFactory, interactiveService, navigationManager, deleteEntityService, currentPermissionService)
		{

		}

		protected override IQueryOver<RefundEntity> ItemsQuery(IUnitOfWork uow)
		{
			RefundEntity refund = null;
			RefundJournalNode resultAlias = null;

			var query = uow.Session.QueryOver(() => refund);

			
			query.Where(GetSearchCriterion(
					() => refund.Id,
					() => refund.Order,
					() => refund.OrderOnline,
					() => refund.Date));

			query.SelectList(list => list
					.Select(() => refund.Id).WithAlias(() => resultAlias.Id)
					.Select(() => refund.Order).WithAlias(() => resultAlias.Order)
					.Select(() => refund.OrderOnline).WithAlias(() => resultAlias.OrderOnlineId)
					.Select(() => refund.Date).WithAlias(() => resultAlias.Date)
				)
				.TransformUsing(Transformers.AliasToBean<RefundJournalNode>());

			return query;
		}

		protected override void CreateNodeActions()
		{
			NodeActionsList.Clear();
			CreateDefaultSelectAction();

			var canCreate = CurrentPermissionService.ValidateEntityPermission(typeof(RefundEntity)).CanCreate;
			var canRead = CurrentPermissionService.ValidateEntityPermission(typeof(RefundEntity)).CanUpdate;
			var canDelete = CurrentPermissionService.ValidateEntityPermission(typeof(RefundEntity)).CanDelete;

			var addAction = new JournalAction("Добавить",
				(selected) => canCreate,
				(selected) => VisibleCreateAction,
				(selected) => CreateEntityDialog(),
				"Insert"
			);
			NodeActionsList.Add(addAction);

			var editAction = new JournalAction("Изменить",
				(selected) => canRead && selected.Any(),
				(selected) => VisibleEditAction,
				(selected) => selected.Cast<RefundJournalNode>().ToList().ForEach(EditEntityDialog)
			);
			NodeActionsList.Add(editAction);

			if(SelectionMode == JournalSelectionMode.None)
				RowActivatedAction = editAction;

			var deleteAction = new JournalAction("Удалить",
				(selected) => canDelete && selected.Any(),
				(selected) => VisibleDeleteAction,
				(selected) => DeleteEntities(selected.Cast<RefundJournalNode>().ToArray()),
				"Delete"
			);
			NodeActionsList.Add(deleteAction);
		}
	}
}
