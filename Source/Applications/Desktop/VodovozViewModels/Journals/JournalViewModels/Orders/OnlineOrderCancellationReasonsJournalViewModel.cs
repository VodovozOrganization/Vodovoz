using System;
using NHibernate;
using NHibernate.Transform;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.Services;
using Vodovoz.Domain.Orders;
using Vodovoz.ViewModels.Journals.JournalNodes.Orders;
using Vodovoz.ViewModels.ViewModels.Orders;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.Orders
{
	public class OnlineOrderCancellationReasonsJournalViewModel
		: EntityJournalViewModelBase<
			OnlineOrderCancellationReason,
			OnlineOrderCancellationReasonViewModel,
			OnlineOrderCancellationReasonsJournalNode>
	{
		public OnlineOrderCancellationReasonsJournalViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			IInteractiveService interactiveService,
			INavigationManager navigationManager,
			ICurrentPermissionService currentPermissionService
			) : base(unitOfWorkFactory, interactiveService, navigationManager, currentPermissionService: currentPermissionService)
		{
			if(unitOfWorkFactory is null)
			{
				throw new ArgumentNullException(nameof(unitOfWorkFactory));
			}
			if(navigationManager is null)
			{
				throw new ArgumentNullException(nameof(navigationManager));
			}

			TabName = "Журнал причин отмены онлайн заказов";
			
			VisibleDeleteAction = false;
		}

		protected override IQueryOver<OnlineOrderCancellationReason> ItemsQuery(IUnitOfWork uow)
		{
			OnlineOrderCancellationReasonsJournalNode resultAlias = null;

			var query = uow.Session.QueryOver<OnlineOrderCancellationReason>()
				.SelectList(list => list
					.Select(r => r.Id).WithAlias(() => resultAlias.Id)
					.Select(r => r.IsArchive).WithAlias(() => resultAlias.IsArchive)
					.Select(r => r.Name).WithAlias(() => resultAlias.Name))
				.TransformUsing(Transformers.AliasToBean<OnlineOrderCancellationReasonsJournalNode>());

			return query;
		}
	}
}
