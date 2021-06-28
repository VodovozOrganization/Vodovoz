using NHibernate;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Services;
using System;
using Vodovoz.Domain.Employees;
using Vodovoz.JournalNodes;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.Users
{
	public class UsersJournalViewModel : SingleEntityJournalViewModelBase<User, UserViewModel, UserJournalNode>
	{
		public UsersJournalViewModel(IUnitOfWorkFactory unitOfWorkFactory, ICommonServices commonServices, bool hideJournalForOpenDialog = false, bool hideJournalForCreateDialog = false) : base(unitOfWorkFactory, commonServices, hideJournalForOpenDialog, hideJournalForCreateDialog)
		{
			TabName = "Журнал пользователей";
		}

		protected override void CreateNodeActions()
		{
			NodeActionsList.Clear();
			CreateDefaultSelectAction();
			CreateDefaultAddActions();
			CreateDefaultEditAction();
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
				.Select(() => userAlias.Id).WithAlias(() => userJournalNodeAlias.Id)
				.Select(() => userAlias.Name).WithAlias(() => userJournalNodeAlias.Name)
				.Select(() => userAlias.Login).WithAlias(() => userJournalNodeAlias.Login)
				.Select(() => userAlias.Deactivated).WithAlias(() => userJournalNodeAlias.Deactivated))
				.OrderByAlias(() => userAlias.Name).Asc
				.TransformUsing(Transformers.AliasToBean<UserJournalNode>());

			return result;
		};

		protected override Func<UserViewModel> CreateDialogFunction => () => new UserViewModel(
			   EntityUoWBuilder.ForCreate(),
			   UnitOfWorkFactory,
			   commonServices
		   );

		protected override Func<UserJournalNode, UserViewModel> OpenDialogFunction => (node) => new UserViewModel(
			   EntityUoWBuilder.ForOpen(node.Id),
			   UnitOfWorkFactory,
			   commonServices);
	}
}
