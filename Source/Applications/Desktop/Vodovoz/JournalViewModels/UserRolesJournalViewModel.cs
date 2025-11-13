using System.Linq;
using NHibernate;
using NHibernate.Transform;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Project.Services;
using QS.Services;
using Vodovoz.Core.Domain.Users;
using Vodovoz.Domain.Permissions;
using Vodovoz.ViewModels.Users;

namespace Vodovoz.JournalViewModels
{
	public class UserRolesJournalViewModel : EntityJournalViewModelBase<UserRole, UserRoleViewModel, UserRolesJournalNode>
	{
		public UserRolesJournalViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			IInteractiveService interactiveService,
			INavigationManager navigationManager,
			IDeleteEntityService deleteEntityService = null,
			ICurrentPermissionService currentPermissionService = null)
			: base(unitOfWorkFactory, interactiveService, navigationManager, deleteEntityService, currentPermissionService)
		{
			TabName = "Пользовательские роли по работе с БД";
			
			UpdateOnChanges(
				typeof(UserRole),
				typeof(PrivilegeBase));
		}
		
		protected override void CreateNodeActions()
		{
			var userRolePermissionResult = CurrentPermissionService.ValidateEntityPermission(typeof(UserRole));
			
			NodeActionsList.Clear();

			var addAction = new JournalAction("Добавить",
				(selected) => userRolePermissionResult.CanCreate,
				(selected) => VisibleCreateAction,
				(selected) => CreateEntityDialog(),
				"Insert"
			);
			NodeActionsList.Add(addAction);

			var editAction = new JournalAction("Изменить",
				(selected) => userRolePermissionResult.CanUpdate && selected.Any(),
				(selected) => VisibleEditAction,
				(selected) => selected.OfType<UserRolesJournalNode>().ToList().ForEach(EditEntityDialog)
			);
			NodeActionsList.Add(editAction);

			if(SelectionMode == JournalSelectionMode.None)
			{
				RowActivatedAction = editAction;
			}
		}
		
		protected override IQueryOver<UserRole> ItemsQuery(IUnitOfWork uow)
		{
			UserRole userRoleAlias = null;
			UserRolesJournalNode resultAlias = null;

			var query = uow.Session.QueryOver(() => userRoleAlias);

			query.Where(GetSearchCriterion(
				() => userRoleAlias.Name
			));
			
			var result = query.SelectList(list => list
					.Select(ur => ur.Id).WithAlias(() => resultAlias.Id)
					.Select(ur => ur.Name).WithAlias(() => resultAlias.Name)
					.Select(ur => ur.Description).WithAlias(() => resultAlias.Description))
				.OrderBy(ur => ur.Name).Asc
				.TransformUsing(Transformers.AliasToBean<UserRolesJournalNode>());
			
			return result;
		}

		protected override void CreateEntityDialog()
		{
			NavigationManager.OpenViewModel<UserRoleViewModel, IEntityUoWBuilder>(this, EntityUoWBuilder.ForCreate());
		}

		protected override void EditEntityDialog(UserRolesJournalNode node)
		{
			NavigationManager.OpenViewModel<UserRoleViewModel, IEntityUoWBuilder>(this, EntityUoWBuilder.ForOpen(DomainHelper.GetId(node)));
		}
	}

	public class UserRolesJournalNode : JournalEntityNodeBase<UserRole>
	{
		public string Name { get; set; }
		public string Description { get; set; }
		public override string Title => Name;
	}
}
