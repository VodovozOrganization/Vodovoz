using NHibernate;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Services;
using System;
using Autofac;
using NHibernate.SqlCommand;
using QS.Navigation;
using QS.Project.DB;
using Vodovoz.Domain.Employees;
using Vodovoz.EntityRepositories.Permissions;
using Vodovoz.JournalNodes;
using Vodovoz.ViewModels.Journals.FilterViewModels.Users;
using Vodovoz.Core.Domain.Users;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.Users
{
	public class UsersJournalViewModel : FilterableSingleEntityJournalViewModelBase
		<User, UserViewModel, UserJournalNode, UsersJournalFilterViewModel>
	{
		private readonly INavigationManager _navigationManager;
		private readonly IUserRoleRepository _userRoleRepository;
		private readonly IUserRoleService _userRoleService;
		private readonly ILifetimeScope _scope;

		public UsersJournalViewModel(
			UsersJournalFilterViewModel usersJournalFilterViewModel,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			INavigationManager navigationManager,
			IUserRoleRepository userRoleRepository,
			IUserRoleService userRoleService,
			ILifetimeScope scope,
			bool hideJournalForOpenDialog = false,
			bool hideJournalForCreateDialog = false) 
			: base(usersJournalFilterViewModel, unitOfWorkFactory, commonServices, hideJournalForOpenDialog, hideJournalForCreateDialog)
		{
			TabName = "Журнал пользователей";
			_navigationManager = navigationManager ?? throw new ArgumentNullException(nameof(navigationManager));
			_userRoleRepository = userRoleRepository ?? throw new ArgumentNullException(nameof(userRoleRepository));
			_userRoleService = userRoleService ?? throw new ArgumentNullException(nameof(userRoleService));
			_scope = scope ?? throw new ArgumentNullException(nameof(scope));
		}

		protected override void CreateNodeActions()
		{
			NodeActionsList.Clear();
			CreateDefaultSelectAction();
			CreateDefaultEditAction();
		}

		protected override Func<IUnitOfWork, IQueryOver<User>> ItemsSourceQueryFunction => (uow) =>
		{
			UserJournalNode resultAlias = null;
			User userAlias = null;
			Employee employeeAlias = null;

			var query = uow.Session.QueryOver(() => userAlias)
				.JoinEntityAlias(() => employeeAlias, () => employeeAlias.User.Id == userAlias.Id, JoinType.LeftOuterJoin);

			if(FilterViewModel != null)
			{
				if(!FilterViewModel.ShowDeactivatedUsers)
				{
					query.Where(() => !userAlias.Deactivated);
				}
			}
			
			var employeeProjection = CustomProjections.Concat_WS(
				" ",
				() => employeeAlias.LastName,
				() => employeeAlias.Name,
				() => employeeAlias.Patronymic
			);

			query.Where(GetSearchCriterion(
				() => userAlias.Id,
				() => userAlias.Name,
				() => userAlias.Login
			));

			var result = query.SelectList(
				list => list
					.Select(() => userAlias.Id).WithAlias(() => resultAlias.Id)
					.Select(() => userAlias.Name).WithAlias(() => resultAlias.Name)
					.Select(() => userAlias.Login).WithAlias(() => resultAlias.Login)
					.Select(() => userAlias.IsAdmin).WithAlias(() => resultAlias.IsAdmin)
					.Select(() => userAlias.Deactivated).WithAlias(() => resultAlias.Deactivated)
					.Select(() => employeeAlias.Id).WithAlias(() => resultAlias.EmployeeId)
					.Select(employeeProjection).WithAlias(() => resultAlias.EmployeeFIO))
				.OrderBy(u => u.IsAdmin).Desc
				.OrderBy(u => u.Name).Asc
				.TransformUsing(Transformers.AliasToBean<UserJournalNode>());

			return result;
		};

		protected override Func<UserViewModel> CreateDialogFunction => () => new UserViewModel(
			EntityUoWBuilder.ForCreate(),
			UnitOfWorkFactory,
			commonServices,
			_navigationManager,
			_userRoleRepository,
			_userRoleService,
			_scope.BeginLifetimeScope());

		protected override Func<UserJournalNode, UserViewModel> OpenDialogFunction => (node) => new UserViewModel(
			EntityUoWBuilder.ForOpen(node.Id),
			UnitOfWorkFactory,
			commonServices,
			_navigationManager,
			_userRoleRepository,
			_userRoleService,
			_scope.BeginLifetimeScope());
	}
}
