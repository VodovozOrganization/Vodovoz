using System;
using NHibernate;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Services;
using Vodovoz.Domain.Employees;
using Vodovoz.EntityRepositories.Permissions;
using Vodovoz.JournalNodes;
using Vodovoz.Journals.FilterViewModels.Employees;
using Vodovoz.ViewModels;

namespace Vodovoz.Journals
{
    public class SelectUserJournalViewModel : FilterableSingleEntityJournalViewModelBase<User, UserViewModel, UserJournalNode, UserJournalFilterViewModel>
    {
		private readonly IPermissionRepository _permissionRepository;

		public SelectUserJournalViewModel(UserJournalFilterViewModel filterViewModel, IUnitOfWorkFactory unitOfWorkFactory, IPermissionRepository permissionRepository, ICommonServices commonServices)
            : base(filterViewModel, unitOfWorkFactory, commonServices)
        {
            TabName = "Журнал выбора пользователей";
			_permissionRepository = permissionRepository ?? throw new ArgumentNullException(nameof(permissionRepository));
		}

        protected override void CreateNodeActions()
        {
            NodeActionsList.Clear();
            CreateDefaultSelectAction();
        }

        protected override Func<IUnitOfWork, IQueryOver<User>> ItemsSourceQueryFunction => (uow) =>
        {
            UserJournalNode userJournalNodeAlias = null;
            User userAlias = null;

            var query = uow.Session.QueryOver<User>(() => userAlias);

            query.Where(GetSearchCriterion(
                () => userAlias.Id,
                () => userAlias.Name,
                () => userAlias.Login
            ));

            var result = query.SelectList(list => list
                .Select(u => u.Id).WithAlias(() => userJournalNodeAlias.Id)
                .Select(u => u.Name).WithAlias(() => userJournalNodeAlias.Name)
                .Select(u => u.Login).WithAlias(() => userJournalNodeAlias.Login)
                .Select(u => u.Deactivated).WithAlias(() => userJournalNodeAlias.Deactivated))
                .TransformUsing(Transformers.AliasToBean<UserJournalNode>());

            return result;
        };

        protected override Func<UserViewModel> CreateDialogFunction => () => new UserViewModel(
			   EntityUoWBuilder.ForCreate(),
			   UnitOfWorkFactory,
			   _permissionRepository,
			   commonServices
		   );

		protected override Func<UserJournalNode, UserViewModel> OpenDialogFunction => (node) => new UserViewModel(
			   EntityUoWBuilder.ForOpen(node.Id),
			   UnitOfWorkFactory,
			   _permissionRepository,
			   commonServices);
    }
}
