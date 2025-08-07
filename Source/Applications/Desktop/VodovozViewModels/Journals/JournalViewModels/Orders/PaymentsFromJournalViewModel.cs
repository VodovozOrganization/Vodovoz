using System.Linq;
using NHibernate;
using NHibernate.Transform;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.Services;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Organizations;
using Vodovoz.ViewModels.Journals.JournalNodes.Orders;
using Vodovoz.ViewModels.Orders;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.Orders
{
	public class PaymentsFromJournalViewModel : EntityJournalViewModelBase<PaymentFrom, PaymentFromViewModel, PaymentFromJournalNode>
	{
		public PaymentsFromJournalViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			IInteractiveService interactiveService,
			ICurrentPermissionService currentPermissionService,
			INavigationManager navigationManager)
			: base(unitOfWorkFactory,
				interactiveService,
				navigationManager,
				null,
				currentPermissionService)
		{
			TabName = "Источники оплат по картам";
		}

		protected override IQueryOver<PaymentFrom> ItemsQuery(IUnitOfWork uow)
		{
			Organization organizationAlias = null;
			PaymentFromJournalNode resultAlias = null;

			var query = uow.Session.QueryOver<PaymentFrom>()
				.SelectList(list =>
					list.Select(pf => pf.Id).WithAlias(() => resultAlias.Id)
						.Select(pf => pf.Name).WithAlias(() => resultAlias.Name)
						.Select(pf => pf.IsArchive).WithAlias(() => resultAlias.IsArchive))
				.TransformUsing(Transformers.AliasToBean<PaymentFromJournalNode>());

			return query;
		}
		
		protected override void CreateNodeActions()
		{
			NodeActionsList.Clear();

			var paymentFromPermissions = CurrentPermissionService.ValidateEntityPermission(typeof(PaymentFrom));
			
			var addAction = new JournalAction("Добавить",
				(selected) => paymentFromPermissions.CanCreate,
				(selected) => VisibleCreateAction,
				(selected) => CreateEntityDialog(),
				"Insert"
			);
			NodeActionsList.Add(addAction);

			var editAction = new JournalAction("Изменить",
				(selected) => paymentFromPermissions.CanRead && selected.Any(),
				(selected) => VisibleEditAction,
				(selected) => selected.Cast<PaymentFromJournalNode>().ToList().ForEach(EditEntityDialog)
			);
			NodeActionsList.Add(editAction);

			if(SelectionMode == JournalSelectionMode.None)
				RowActivatedAction = editAction;
		}
	}
}
