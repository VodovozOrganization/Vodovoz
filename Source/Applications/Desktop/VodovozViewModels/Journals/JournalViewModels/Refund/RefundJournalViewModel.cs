using System;
using System.Linq;
using NHibernate;
using NHibernate.Transform;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.Project.Services;
using QS.Services;
using Vodovoz.Core.Domain.Refunds;
using Vodovoz.Filters.ViewModels;
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
			throw new NotImplementedException();
		}
	}
}
