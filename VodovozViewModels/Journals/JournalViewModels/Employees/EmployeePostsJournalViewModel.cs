using System;
using NHibernate;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain.Employees;
using Vodovoz.ViewModels.Journals.JournalNodes;
using Vodovoz.ViewModels.ViewModels.Employees;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.Employees
{
    public class EmployeePostsJournalViewModel : SingleEntityJournalViewModelBase<EmployeePost, EmployeePostViewModel, EmployeePostJournalNode>
    {
        public EmployeePostsJournalViewModel(
	        EntitiesJournalActionsViewModel journalActionsViewModel,
            IUnitOfWorkFactory unitOfWorkFactory,
            ICommonServices commonServices)
            : base(journalActionsViewModel, unitOfWorkFactory, commonServices)
        {
            TabName = "Журнал должностей";
        }

        protected override Func<IUnitOfWork, IQueryOver<EmployeePost>> ItemsSourceQueryFunction => uow => {
            EmployeePostJournalNode resultAlias = null;
            EmployeePost postNameAlias = null;

            var query = uow.Session.QueryOver<EmployeePost>(() => postNameAlias);

            query.Where(GetSearchCriterion(
                () => postNameAlias.Id,
                () => postNameAlias.Name));

            return query
                .SelectList(list => list
                    .Select(x => postNameAlias.Id).WithAlias(() => resultAlias.Id)
                    .Select(x => postNameAlias.Name).WithAlias(() => resultAlias.EmployeePostName))
                    .TransformUsing(Transformers.AliasToBean<EmployeePostJournalNode>());
        };

        protected override Func<EmployeePostViewModel> CreateDialogFunction => () => new EmployeePostViewModel(
            EntityUoWBuilder.ForCreate(),
            UnitOfWorkFactory,
            CommonServices);

        protected override Func<JournalEntityNodeBase, EmployeePostViewModel> OpenDialogFunction => node => new EmployeePostViewModel(
            EntityUoWBuilder.ForOpen(node.Id),
            UnitOfWorkFactory,
            CommonServices
        );

        protected override void InitializeJournalActionsViewModel()
        {
	        EntitiesJournalActionsViewModel.Initialize(SelectionMode, EntityConfigs, this, HideJournal, OnItemsSelected,
		        true, true, true, false);
        }
    }
}