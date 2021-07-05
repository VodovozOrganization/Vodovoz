using System;
using NHibernate;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain.Employees;
using Vodovoz.JournalNodes;
using Vodovoz.Journals.FilterViewModels.Employees;
using Vodovoz.ViewModels;

namespace Vodovoz.Journals
{
    public class UserJournalViewModel : FilterableSingleEntityJournalViewModelBase<User, UserViewModel, UserJournalNode, UserJournalFilterViewModel>
    {
        public UserJournalViewModel(
	        EntitiesJournalActionsViewModel journalActionsViewModel,
	        UserJournalFilterViewModel filterViewModel,
	        IUnitOfWorkFactory unitOfWorkFactory,
	        ICommonServices commonServices)
            : base(journalActionsViewModel, filterViewModel, unitOfWorkFactory, commonServices)
        {
            TabName = "Журнал пользователей";
        }

        protected override void InitializeJournalActionsViewModel()
        {
	        EntitiesJournalActionsViewModel.Initialize(
		        SelectionMode, EntityConfigs, this, HideJournal, OnItemsSelected,
		        true, false, false, false);
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
			   CommonServices
		   );

		protected override Func<JournalEntityNodeBase, UserViewModel> OpenDialogFunction => node => new UserViewModel(
			   EntityUoWBuilder.ForOpen(node.Id),
			   UnitOfWorkFactory,
			   CommonServices);
    }
}
