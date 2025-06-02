using System;
using NHibernate;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QS.Project.Journal;
using QS.Services;
using Vodovoz.Core.Domain.Users;
using Vodovoz.JournalNodes;
using Vodovoz.ViewModels;
using Vodovoz.ViewModels.Journals.FilterViewModels.Users;

namespace Vodovoz.Journals
{
	public class SelectUserJournalViewModel : FilterableSingleEntityJournalViewModelBase
		<User, UserViewModel, UserJournalNode, UsersJournalFilterViewModel>
	{
		public SelectUserJournalViewModel(
			UsersJournalFilterViewModel filterViewModel,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices)
			: base(filterViewModel, unitOfWorkFactory, commonServices)
		{
			TabName = "Журнал выбора пользователей";
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

			if(FilterViewModel != null)
			{
				if(!FilterViewModel.ShowDeactivatedUsers)
				{
					query.Where(() => !userAlias.Deactivated);
				}
			}
			
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
				.OrderBy(u => u.Name).Asc
				.TransformUsing(Transformers.AliasToBean<UserJournalNode>());

			return result;
		};

		protected override Func<UserViewModel> CreateDialogFunction =>
			() => throw new InvalidOperationException("Нельзя открывать диалог создания пользователя из этого журнала");

		protected override Func<UserJournalNode, UserViewModel> OpenDialogFunction =>
			n => throw new InvalidOperationException("Нельзя открывать диалог изменения пользователя из этого журнала");
	}
}
