using System.Linq;
using NHibernate;
using NHibernate.Transform;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.Project.Services;
using QS.Services;
using Vodovoz.Core.Domain.Cash;
using Vodovoz.Tools;
using Vodovoz.ViewModels.Journals.Nodes.Cash;
using Vodovoz.ViewModels.ViewModels.Cash;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.Cash
{
	public class VatRateJournalViewModel: EntityJournalViewModelBase<
		VatRate,
		VatRateViewModel,
		VatRateJournalNode>
	{
		private readonly IPermissionResult _permissionResult;
		
		public VatRateJournalViewModel(
			IUnitOfWorkFactory unitOfWorkFactory, 
			IInteractiveService interactiveService, 
			INavigationManager navigationManager, 
			IDeleteEntityService deleteEntityService = null, 
			ICurrentPermissionService currentPermissionService = null) : base(unitOfWorkFactory, interactiveService, navigationManager, deleteEntityService, currentPermissionService)
		{
			_permissionResult = currentPermissionService.ValidateEntityPermission(typeof(VatRate));

			TabName = $"Журнал {typeof(VatRate).GetClassUserFriendlyName().GenitivePlural}";
		}

		protected override void CreateNodeActions()
		{
			NodeActionsList.Clear();
			CreateDefaultSelectAction();

			var addAction = new JournalAction("Добавить",
				(selected) => _permissionResult.CanCreate,
				(selected) => VisibleCreateAction,
				(selected) => CreateEntityDialog(),
				"Insert"
			);
			NodeActionsList.Add(addAction);

			var editAction = new JournalAction("Изменить",
				(selected) => _permissionResult.CanRead && selected.Any(),
				(selected) => VisibleEditAction,
				(selected) => selected.Cast<VatRateJournalNode>().ToList().ForEach(base.EditEntityDialog)
			);
			NodeActionsList.Add(editAction);

			if(SelectionMode == JournalSelectionMode.None)
			{
				RowActivatedAction = editAction;
			}

			var deleteAction = new JournalAction("Удалить",
				(selected) => _permissionResult.CanDelete && selected.Any(),
				(selected) => VisibleDeleteAction,
				(selected) => DeleteEntities(selected.Cast<VatRateJournalNode>().ToArray()),
				"Delete"
			);
			NodeActionsList.Add(deleteAction);
		}

		protected override IQueryOver<VatRate> ItemsQuery(IUnitOfWork uow)
		{
			VatRate fineCategoryAlias = null;
			VatRateJournalNode resultAlias = null;

			var query = uow.Session.QueryOver(() => fineCategoryAlias);
			return query
				.SelectList(list => list
					.Select(() => fineCategoryAlias.Id).WithAlias(() => resultAlias.Id)
					.Select(() => fineCategoryAlias.VatRateStringValue).WithAlias(() => resultAlias.VatRateValue)
				)
				.TransformUsing(Transformers.AliasToBean<VatRateJournalNode>());
		}
	}
}
